using TenantScoring.Core;
using TenantScoring.Models;
using Xunit;

namespace TenantScoring.Tests;

public sealed class RegisterTenantTests
{
    [Fact]
    public void Validate_ReturnsExactMessagesForInvalidRequest()
    {
        var request = new RegisterTenantRequest("", "Jane2", "Doe!", "123", "invalid", "12", "13");

        var errors = RegisterTenantValidator.Validate(request);

        Assert.Equal(
            [
                "Please select a country code.",
                "First Name should contain only letters.",
                "Last Name should contain only letters.",
                "Please enter a valid 10-digit mobile number.",
                "Please enter a valid email address.",
                "PIN must be exactly 4 digits.",
                "PIN and Confirm PIN do not match."
            ],
            errors);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsDuplicateWithoutGeneratingOrSaving()
    {
        var store = new FakeStore(true);
        var generator = new FakeTenantIdGenerator();
        var service = new TenantRegistrationService(store, generator);

        var result = await service.RegisterAsync(ValidRequest());

        Assert.True(result.IsDuplicate);
        Assert.Null(result.Response);
        Assert.False(generator.WasCalled);
        Assert.False(store.WasSaved);
    }

    [Fact]
    public async Task RegisterAsync_HashesPinAndReturnsCreatedResponse()
    {
        var store = new FakeStore(false);
        var generator = new FakeTenantIdGenerator();
        var service = new TenantRegistrationService(store, generator);

        var result = await service.RegisterAsync(ValidRequest());

        Assert.False(result.IsDuplicate);
        Assert.Equal("K-1234567890", result.Response!.TenantId);
        Assert.Equal("Tenant Successfully Registered", result.Response.Message);
        Assert.True(BCrypt.Net.BCrypt.Verify("1234", store.Saved!.PinHash));
    }

    private static RegisterTenantRequest ValidRequest() =>
        new("+1", "Jane Doe", "Doe-Smith", "1234567890", "jane@example.com", "1234", "1234");

    private sealed class FakeStore(bool exists) : ITenantApplicationStore
    {
        public TenantApplication? Saved { get; private set; }
        public bool WasSaved => Saved is not null;

        public Task<bool> ExistsAsync(string countryCode, string mobileNumber, CancellationToken cancellationToken = default) =>
            Task.FromResult(exists);

        public Task AddAsync(TenantApplication application, CancellationToken cancellationToken = default)
        {
            Saved = application;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeTenantIdGenerator : ITenantIdGenerator
    {
        public bool WasCalled { get; private set; }

        public Task<string> GenerateAsync(CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult("K-1234567890");
        }
    }
}
