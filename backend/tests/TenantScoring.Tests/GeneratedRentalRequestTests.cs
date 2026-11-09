using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using TenantScoring.Core;
using TenantScoring.Models;
using Xunit;

namespace TenantScoring.Tests;

public sealed class GeneratedRentalRequestTests
{
    [Fact]
    public async Task CreateAndResendGenerateMatchingDocumentsAndRestrictAccess()
    {
        var (service, repository, documents, owner, tenant, property) = Setup();
        var terms = Terms();
        var created = await service.CreateGeneratedRentalRequestAsync(owner.UserId, new(property.PropertyId, tenant.UserId, terms));

        Assert.NotNull(created);
        Assert.Equal(terms.AdvanceAmount, created.AgreedAdvance);
        Assert.Equal(terms.RentAmount, documents.Details[0].RentAmount);
        Assert.Equal(terms.AdvanceAmount, documents.Details[0].AdvanceAmount);
        Assert.Equal(terms.OwnerAddress, documents.Details[0].OwnerAddress);
        Assert.Equal(terms.UnitNumber, documents.Details[0].UnitNumber);
        var lease = Assert.IsType<Lease>(repository.Lease);
        SetNavigation(lease, nameof(Lease.Property), property);
        SetNavigation(lease, nameof(Lease.Tenant), tenant);
        lease.SendBack("Please change the rent.");

        var revised = terms with { RentAmount = 25000, AdvanceAmount = 40000, UnitNumber = "B-2" };
        var resent = await service.UpdateSentBackRentalRequestAsync(owner.UserId, lease.LeaseId, revised);

        Assert.NotNull(resent);
        Assert.Equal("Pending", lease.Status);
        Assert.Equal(25000, lease.AgreedRent);
        Assert.Equal(40000, lease.AgreedAdvance);
        Assert.Equal("document-2.pdf", lease.AgreementDocumentUrl);
        Assert.Equal(revised.RentAmount, documents.Details[1].RentAmount);
        Assert.Equal(revised.UnitNumber, documents.Details[1].UnitNumber);
        Assert.True((await service.GetRentalRequestDocumentAsync(tenant.UserId, lease.LeaseId)).IsAuthorized);
        Assert.True((await service.GetRentalRequestDocumentAsync(owner.UserId, lease.LeaseId)).IsAuthorized);
        Assert.False((await service.GetRentalRequestDocumentAsync(Guid.NewGuid(), lease.LeaseId)).IsAuthorized);
    }

    [Fact]
    public async Task InvalidTermsAndUnauthorizedResendDoNotGenerateDocuments()
    {
        var (service, repository, documents, owner, tenant, property) = Setup();
        Assert.Null(await service.CreateGeneratedRentalRequestAsync(owner.UserId, new(property.PropertyId, tenant.UserId, Terms() with { AdvanceAmount = -1 })));
        Assert.Empty(documents.Details);
        repository.Lease = new Lease(property.PropertyId, tenant.UserId, owner.UserId, 100, 0);
        repository.Lease.SendBack("Changes needed");
        Assert.Null(await service.UpdateSentBackRentalRequestAsync(Guid.NewGuid(), repository.Lease.LeaseId, Terms()));
        Assert.Empty(documents.Details);
    }

    [Fact]
    public async Task DatabaseFailureRemovesNewDocument()
    {
        var (service, repository, documents, owner, tenant, property) = Setup();
        repository.FailSave = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateGeneratedRentalRequestAsync(owner.UserId, new(property.PropertyId, tenant.UserId, Terms())));
        Assert.Equal("document-1.pdf", Assert.Single(documents.Deleted));
    }

    private static RentalAgreementTerms Terms() => new(20000, 30000, DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddMonths(12), DateTime.UtcNow.Date, "Owner address", "A-1");

    private static (KirayeeService, RepositoryProxy, DocumentStub, User, User, Property) Setup()
    {
        var owner = new User("1111111111", "owner@example.test", "hash");
        owner.SetRole("Owner");
        var tenant = new User("2222222222", "tenant@example.test", "hash");
        var ownerProfile = new UserProfile(owner.UserId, "Asha", "Owner");
        var tenantProfile = new UserProfile(tenant.UserId, "Ravi", "Tenant");
        SetNavigation(tenantProfile, nameof(UserProfile.ProfileCode), "KID123");
        SetNavigation(owner, nameof(User.Profile), ownerProfile);
        SetNavigation(tenant, nameof(User.Profile), tenantProfile);
        var property = new Property(owner.UserId, "Home", "Apartment", "Road", "Pune", "Maharashtra", 20000);
        var repository = DispatchProxy.Create<IKirayeeRepository, RepositoryProxy>();
        var proxy = (RepositoryProxy)repository;
        proxy.Owner = owner;
        proxy.Tenant = tenant;
        proxy.Property = property;
        var documents = new DocumentStub();
        return (new KirayeeService(repository, NullLogger<KirayeeService>.Instance, documents), proxy, documents, owner, tenant, property);
    }

    private static void SetNavigation(object target, string name, object value) => target.GetType().GetProperty(name)!.SetValue(target, value);

    public class RepositoryProxy : DispatchProxy
    {
        public User Owner { get; set; } = null!;
        public User Tenant { get; set; } = null!;
        public Property Property { get; set; } = null!;
        public Lease? Lease { get; set; }
        public bool FailSave { get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => targetMethod!.Name switch
        {
            nameof(IKirayeeRepository.FindUserAsync) => Task.FromResult<User?>((Guid)args![0]! == Owner.UserId ? Owner : (Guid)args[0]! == Tenant.UserId ? Tenant : null),
            nameof(IKirayeeRepository.FindPropertyAsync) => Task.FromResult<Property?>((Guid)args![0]! == Property.PropertyId ? Property : null),
            nameof(IKirayeeRepository.FindLeaseAsync) => Task.FromResult<Lease?>(Lease?.LeaseId == (Guid)args![0]! ? Lease : null),
            nameof(IKirayeeRepository.AddLeaseAsync) => AddLease((Lease)args![0]!),
            nameof(IKirayeeRepository.SaveChangesAsync) => Task.CompletedTask,
            _ => throw new NotSupportedException(targetMethod.Name),
        };

        private Task AddLease(Lease lease)
        {
            if (FailSave) throw new InvalidOperationException("Database failure");
            Lease = lease;
            return Task.CompletedTask;
        }
    }

    private sealed class DocumentStub : IRentalAgreementDocuments
    {
        public List<RentalAgreementDetails> Details { get; } = [];
        public List<string> Deleted { get; } = [];
        public Task<string> CreateAsync(RentalAgreementDetails details, CancellationToken cancellationToken)
        {
            Details.Add(details);
            return Task.FromResult($"document-{Details.Count}.pdf");
        }
        public void Delete(string documentPath) => Deleted.Add(documentPath);
    }
}
