using System.Text.RegularExpressions;
using BCrypt.Net;
using Microsoft.Extensions.Logging;
using TenantScoring.Models;

namespace TenantScoring.Core;

public interface IKirayeeRepository
{
    Task<User?> FindUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<User?> FindUserByMobileAsync(string mobileNumber, CancellationToken cancellationToken);
    Task<User?> FindUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task AddUserAsync(User user, UserProfile profile, CancellationToken cancellationToken);
    Task<UserProfile?> FindProfileAsync(Guid userId, CancellationToken cancellationToken);
    Task<List<BehavioralQuestion>> GetBehavioralQuestionsAsync(CancellationToken cancellationToken);
    Task<List<TenantBehavioralResponse>> GetBehavioralResponsesAsync(Guid userId, CancellationToken cancellationToken);
    Task ReplaceBehavioralResponsesAsync(Guid userId, IEnumerable<TenantBehavioralResponse> responses, CancellationToken cancellationToken);
    Task AddBankAccountAsync(OwnerBankAccount account, CancellationToken cancellationToken);
    Task<IReadOnlyList<OwnerBankAccount>> GetBankAccountsAsync(Guid ownerId, CancellationToken cancellationToken);
    Task AddPropertyAsync(Property property, CancellationToken cancellationToken);
    Task<IReadOnlyList<Property>> GetPropertiesAsync(Guid? ownerId, CancellationToken cancellationToken);
    Task<Property?> FindPropertyAsync(Guid propertyId, CancellationToken cancellationToken);
    Task<IReadOnlyList<User>> SearchTenantsAsync(string query, CancellationToken cancellationToken);
    Task<IReadOnlyList<Lease>> GetOwnerLeasesAsync(Guid ownerId, string? status, CancellationToken cancellationToken);
    Task<IReadOnlyList<Lease>> GetTenantRentalRequestsAsync(Guid tenantId, string? status, CancellationToken cancellationToken);
    Task AddScoreAsync(TenantScore score, CancellationToken cancellationToken);
    Task<TenantScore?> GetLatestScoreAsync(Guid tenantId, CancellationToken cancellationToken);
    Task AddLeaseAsync(Lease lease, CancellationToken cancellationToken);
    Task<Lease?> FindLeaseAsync(Guid leaseId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Lease>> GetLeasesAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Lease>> GetTenantPaymentLeasesAsync(Guid tenantId, CancellationToken cancellationToken);
    Task AddPaymentsAsync(IEnumerable<RentPayment> payments, CancellationToken cancellationToken);
    Task AddPaymentAsync(RentPayment payment, CancellationToken cancellationToken);
    Task<RentPayment?> FindPaymentAsync(Guid paymentId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IKirayeeService
{
    Task<KirayeeRegistrationResult> RegisterAsync(KirayeeRegisterRequest request, CancellationToken cancellationToken = default);
    Task<ProfileResponse?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<string?> GetProfileImageFileNameAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(bool Updated, string? PreviousFileName)> UpdateProfileImageAsync(Guid userId, string fileName, CancellationToken cancellationToken = default);
    Task<ProfileStatusDto?> GetProfileStatusAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<BehavioralAssessmentResponse?> GetBehavioralAssessmentAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<BehavioralAssessmentResponse?> SubmitBehavioralAssessmentAsync(Guid userId, BehavioralAssessmentRequest request, CancellationToken cancellationToken = default);
    Task<bool> VerifyAsync(Guid userId, VerificationRequest request, CancellationToken cancellationToken = default);
    Task<ProfileResponse?> UpdateProfileAsync(Guid userId, ProfileUpdateRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateRoleAsync(Guid userId, RoleUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ScoreResponse?> RegenerateScoreAsync(Guid userId, ScoreRequest request, CancellationToken cancellationToken = default);
    Task<BankAccountResponse?> AddBankAccountAsync(Guid userId, BankAccountRequest request, CancellationToken cancellationToken = default);
    Task<PropertyResponse?> AddPropertyAsync(Guid userId, PropertyRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PropertyResponse>> GetPropertiesAsync(Guid? ownerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PropertyManagementResponse>?> GetOwnerPropertiesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PropertyManagementResponse?> CreateOwnerPropertyAsync(Guid userId, PropertyManagementRequest request, CancellationToken cancellationToken = default);
    Task<PropertyManagementResponse?> UpdateOwnerPropertyAsync(Guid userId, Guid propertyId, PropertyManagementRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantLookupResponse>?> SearchTenantsAsync(Guid ownerId, string query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RentalRequestResponse>?> GetOwnerRentalRequestsAsync(Guid ownerId, string? status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantRentalRequestResponse>?> GetTenantRentalRequestsAsync(Guid tenantId, string? status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantRentalRequestResponse>?> GetTenantRentalHistoryAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<RentalRequestResponse?> CreateRentalRequestAsync(Guid ownerId, Guid propertyId, Guid tenantId, decimal rentAmount, DateTime startDate, DateTime endDate, string? documentPath, CancellationToken cancellationToken = default);
    Task<(string? DocumentPath, bool IsAuthorized)> GetRentalRequestDocumentAsync(Guid userId, Guid leaseId, CancellationToken cancellationToken = default);
    Task<LeaseResponse?> CreateLeaseAsync(Guid tenantId, LeaseRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaseResponse>> GetLeasesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantPaymentPropertyRecord>?> GetTenantPaymentRecordsAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<LeaseResponse?> ChangeLeaseStatusAsync(Guid userId, Guid leaseId, string action, CancellationToken cancellationToken = default);
    Task<PaymentResponse?> PayAsync(Guid userId, Guid paymentId, PaymentRequest request, CancellationToken cancellationToken = default);
}

public sealed class KirayeeService(IKirayeeRepository repository, ILogger<KirayeeService> logger) : IKirayeeService
{
    public async Task<KirayeeRegistrationResult> RegisterAsync(KirayeeRegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (!Regex.IsMatch(request.MobileNumber.Trim(), "^\\d{10,15}$") || !Regex.IsMatch(request.Email.Trim(), "^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$"))
            return new KirayeeRegistrationResult(null, "Registration details are invalid.");
        if (request.Pin.Length != 4 || request.Pin != request.ConfirmPin || request.Pin.Any(character => character is < '0' or > '9'))
            return new KirayeeRegistrationResult(null, "Registration details are invalid.");
        var email = request.Email.Trim().ToLowerInvariant();
        if (await repository.FindUserByMobileAsync(request.MobileNumber.Trim(), cancellationToken) is not null)
            return new KirayeeRegistrationResult(null, "Mobile number is already registered.");
        if (await repository.FindUserByEmailAsync(email, cancellationToken) is not null)
            return new KirayeeRegistrationResult(null, "Email ID is already registered.");

        var user = new User(request.MobileNumber.Trim(), email, BCrypt.Net.BCrypt.HashPassword(request.Pin));
        await repository.AddUserAsync(user, new UserProfile(user.UserId, request.FirstName, request.LastName), cancellationToken);
        logger.LogInformation("Registered Kirayee user {UserId}", user.UserId);
        return new KirayeeRegistrationResult(
            new KirayeeRegisterResponse(user.UserId, "Account created. Verify your mobile and email to continue."),
            null);
    }

    public async Task<ProfileResponse?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await BuildProfileAsync(userId, cancellationToken);

    public async Task<string?> GetProfileImageFileNameAsync(Guid userId, CancellationToken cancellationToken = default) =>
        (await repository.FindProfileAsync(userId, cancellationToken))?.ProfileImageFileName;

    public async Task<(bool Updated, string? PreviousFileName)> UpdateProfileImageAsync(Guid userId, string fileName, CancellationToken cancellationToken = default)
    {
        var profile = await repository.FindProfileAsync(userId, cancellationToken);
        if (profile is null) return (false, null);
        var previousFileName = profile.ProfileImageFileName;
        profile.SetProfileImage(fileName);
        await repository.SaveChangesAsync(cancellationToken);
        return (true, previousFileName);
    }

    public async Task<ProfileStatusDto?> GetProfileStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await repository.FindUserAsync(userId, cancellationToken);
        var profile = await repository.FindProfileAsync(userId, cancellationToken);
        return user is null || profile is null ? null : BuildProfileStatus(user, profile);
    }

    public async Task<BehavioralAssessmentResponse?> GetBehavioralAssessmentAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await repository.FindProfileAsync(userId, cancellationToken);
        if (await repository.FindUserAsync(userId, cancellationToken) is null || profile is null) return null;
        var questions = await repository.GetBehavioralQuestionsAsync(cancellationToken);
        var responses = await repository.GetBehavioralResponsesAsync(userId, cancellationToken);
        return new BehavioralAssessmentResponse(
            profile.BehavioralAssessmentCompleted,
            profile.BehavioralScore,
            questions.Select(ToQuestion).ToArray(),
            responses.Select(response => new BehavioralAnswerRequest(response.QuestionId, response.UserAnswer)).ToArray());
    }

    public async Task<BehavioralAssessmentResponse?> SubmitBehavioralAssessmentAsync(Guid userId, BehavioralAssessmentRequest request, CancellationToken cancellationToken = default)
    {
        var profile = await repository.FindProfileAsync(userId, cancellationToken);
        if (profile is null) return null;
        var questions = await repository.GetBehavioralQuestionsAsync(cancellationToken);
        if (questions.Count == 0
            || request.Answers is null
            || request.Answers.Count != questions.Count
            || request.Answers.Any(answer => answer is null)
            || request.Answers.Select(answer => answer.QuestionId).Distinct().Count() != questions.Count
            || request.Answers.Any(answer => answer.Answer is not ("Yes" or "No"))
            || request.Answers.Any(answer => questions.All(question => question.QuestionId != answer.QuestionId))) return null;
        var totalWeight = questions.Sum(question => question.WeightPoints);
        if (totalWeight <= 0) return null;

        var responses = request.Answers.Select(answer =>
        {
            var question = questions.Single(question => question.QuestionId == answer.QuestionId);
            var points = string.Equals(answer.Answer, question.PreferredAnswer, StringComparison.OrdinalIgnoreCase) ? question.WeightPoints : 0;
            return new TenantBehavioralResponse(userId, question.QuestionId, answer.Answer, points);
        }).ToArray();
        await repository.ReplaceBehavioralResponsesAsync(userId, responses, cancellationToken);
        var score = responses.Sum(response => response.PointsEarned) / (decimal)totalWeight * 100m;
        profile.SetBehavioralAssessment(score);
        await repository.SaveChangesAsync(cancellationToken);
        return new BehavioralAssessmentResponse(true, score, questions.Select(ToQuestion).ToArray(), request.Answers);
    }

    public async Task<bool> VerifyAsync(Guid userId, VerificationRequest request, CancellationToken cancellationToken = default)
    {
        var user = await repository.FindUserAsync(userId, cancellationToken);
        if (user is null || request.Code != "123456") return false;
        switch (request.Channel.Trim().ToLowerInvariant())
        {
            case "mobile": user.VerifyMobile(); break;
            case "email": user.VerifyEmail(); break;
            case "pan":
                var panProfile = await repository.FindProfileAsync(userId, cancellationToken);
                if (panProfile is null) return false;
                panProfile.VerifyPan();
                break;
            case "aadhaar":
                var aadhaarProfile = await repository.FindProfileAsync(userId, cancellationToken);
                if (aadhaarProfile is null) return false;
                aadhaarProfile.VerifyAadhaar();
                break;
            default: return false;
        }
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ProfileResponse?> UpdateProfileAsync(Guid userId, ProfileUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var user = await repository.FindUserAsync(userId, cancellationToken);
        var profile = await repository.FindProfileAsync(userId, cancellationToken);
        if (user is null || profile is null || request.Role is not ("Owner" or "Tenant" or "Both") || string.IsNullOrWhiteSpace(request.MobileNumber) || !Regex.IsMatch(request.MobileNumber.Trim(), "^\\d{10,15}$") || string.IsNullOrWhiteSpace(request.Email) || !Regex.IsMatch(request.Email.Trim(), "^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$") || string.IsNullOrWhiteSpace(request.PanNumber) || !Regex.IsMatch(request.PanNumber.Trim(), "^[A-Za-z]{5}\\d{4}[A-Za-z]$") || string.IsNullOrWhiteSpace(request.AadhaarNumber) || !Regex.IsMatch(request.AadhaarNumber.Trim(), "^\\d{12}$")) return null;
        user.UpdateContact(request.MobileNumber, request.Email);
        user.SetRole(request.Role);
        profile.UpdateIdentity(request.FirstName, request.LastName, request.PanNumber, request.AadhaarNumber);
        await repository.SaveChangesAsync(cancellationToken);
        return await BuildProfileAsync(userId, cancellationToken);
    }

    public async Task<bool> UpdateRoleAsync(Guid userId, RoleUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var user = await repository.FindUserAsync(userId, cancellationToken);
        if (user is null || request.Role is not ("Owner" or "Tenant" or "Both")) return false;
        user.SetRole(request.Role);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ScoreResponse?> RegenerateScoreAsync(Guid userId, ScoreRequest request, CancellationToken cancellationToken = default)
    {
        var profile = await repository.FindProfileAsync(userId, cancellationToken);
        if (profile is null || !ValidScore(request)) return null;
        var score = new TenantScore(userId, profile.ProfileCompletionScore, request.CreditScore, request.PaymentHistory, request.OwnerFeedback);
        await repository.AddScoreAsync(score, cancellationToken);
        return new ScoreResponse(score.ScoreId, score.FinalScore, profile.ProfileCompletionScore, request.CreditScore, request.PaymentHistory, request.OwnerFeedback, score.CalculatedAt, score.ExpiresAt, score.IsExpired);
    }

    public async Task<BankAccountResponse?> AddBankAccountAsync(Guid userId, BankAccountRequest request, CancellationToken cancellationToken = default)
    {
        if (await repository.FindUserAsync(userId, cancellationToken) is null || string.IsNullOrWhiteSpace(request.AccountNumber) || string.IsNullOrWhiteSpace(request.IFSCCode)) return null;
        var account = new OwnerBankAccount(userId, request.AccountHolderName.Trim(), request.BankName.Trim(), request.AccountNumber.Trim(), request.IFSCCode.Trim().ToUpperInvariant());
        await repository.AddBankAccountAsync(account, cancellationToken);
        return new BankAccountResponse(account.BankAccountId, account.AccountHolderName, account.BankName, account.AccountNumber[^4..].PadLeft(account.AccountNumber.Length, '*'), account.IFSCCode);
    }

    public async Task<PropertyResponse?> AddPropertyAsync(Guid userId, PropertyRequest request, CancellationToken cancellationToken = default)
    {
        var user = await repository.FindUserAsync(userId, cancellationToken);
        if (user is null || user.CurrentRole is not ("Owner" or "Both") || request.RentAmount <= 0 || request.AdvanceAmount < 0 || string.IsNullOrWhiteSpace(request.PropertyName) || string.IsNullOrWhiteSpace(request.Location)) return null;
        var property = new Property(userId, request.BankAccountId, request.PropertyName.Trim(), request.Location.Trim(), request.RentAmount, request.AdvanceAmount);
        await repository.AddPropertyAsync(property, cancellationToken);
        return ToProperty(property);
    }

    public async Task<IReadOnlyList<PropertyResponse>> GetPropertiesAsync(Guid? ownerId, CancellationToken cancellationToken = default) =>
        (await repository.GetPropertiesAsync(ownerId, cancellationToken)).Select(ToProperty).ToArray();

    public async Task<IReadOnlyList<PropertyManagementResponse>?> GetOwnerPropertiesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await repository.FindUserAsync(userId, cancellationToken);
        if (user is null || user.CurrentRole is not ("Owner" or "Both")) return null;
        return (await repository.GetPropertiesAsync(userId, cancellationToken)).Select(ToPropertyManagement).ToArray();
    }

    public async Task<PropertyManagementResponse?> CreateOwnerPropertyAsync(Guid userId, PropertyManagementRequest request, CancellationToken cancellationToken = default)
    {
        var user = await repository.FindUserAsync(userId, cancellationToken);
        if (user is null || user.CurrentRole is not ("Owner" or "Both") || !ValidPropertyRequest(request)) return null;

        var property = new Property(userId, request.PropertyName.Trim(), request.PropertyType, request.Address.Trim(), request.City.Trim(), request.State.Trim(), request.MonthlyRent, request.IsVacant);
        await repository.AddPropertyAsync(property, cancellationToken);
        return ToPropertyManagement(property);
    }

    public async Task<PropertyManagementResponse?> UpdateOwnerPropertyAsync(Guid userId, Guid propertyId, PropertyManagementRequest request, CancellationToken cancellationToken = default)
    {
        var user = await repository.FindUserAsync(userId, cancellationToken);
        var property = await repository.FindPropertyAsync(propertyId, cancellationToken);
        if (user is null || user.CurrentRole is not ("Owner" or "Both") || property is null || property.OwnerId != userId || !ValidPropertyRequest(request)) return null;

        property.UpdateDetails(request.PropertyName.Trim(), request.PropertyType, request.Address.Trim(), request.City.Trim(), request.State.Trim(), request.MonthlyRent, request.IsVacant);
        await repository.SaveChangesAsync(cancellationToken);
        return ToPropertyManagement(property);
    }

    public async Task<IReadOnlyList<TenantLookupResponse>?> SearchTenantsAsync(Guid ownerId, string query, CancellationToken cancellationToken = default)
    {
        var owner = await repository.FindUserAsync(ownerId, cancellationToken);
        if (owner is null || owner.CurrentRole is not ("Owner" or "Both")) return null;
        var tenants = await repository.SearchTenantsAsync(query.Trim(), cancellationToken);
        return tenants.Select(user => new TenantLookupResponse(user.UserId, user.Profile!.ProfileId, string.Join(" ", new[] { user.Profile.FirstName, user.Profile.LastName }.Where(name => !string.IsNullOrWhiteSpace(name))))).ToArray();
    }

    public async Task<IReadOnlyList<RentalRequestResponse>?> GetOwnerRentalRequestsAsync(Guid ownerId, string? status, CancellationToken cancellationToken = default)
    {
        var owner = await repository.FindUserAsync(ownerId, cancellationToken);
        if (owner is null || owner.CurrentRole is not ("Owner" or "Both")) return null;
        var requests = await repository.GetOwnerLeasesAsync(ownerId, status, cancellationToken);
        return requests.Select(ToRentalRequest).ToArray();
    }

    public async Task<IReadOnlyList<TenantRentalRequestResponse>?> GetTenantRentalRequestsAsync(Guid tenantId, string? status, CancellationToken cancellationToken = default)
    {
        var tenant = await repository.FindUserAsync(tenantId, cancellationToken);
        if (tenant is null || tenant.CurrentRole is not ("Tenant" or "Both")) return null;
        var requests = await repository.GetTenantRentalRequestsAsync(tenantId, status, cancellationToken);
        return requests.Select(lease => new TenantRentalRequestResponse(
            lease.LeaseId,
            string.Join(" ", new[] { lease.Owner.Profile?.FirstName, lease.Owner.Profile?.LastName }.Where(name => !string.IsNullOrWhiteSpace(name))),
            lease.Property.PropertyName,
            lease.Property.PropertyType,
            lease.Property.Address,
            lease.Property.City,
            lease.Property.State,
            lease.AgreedRent,
            lease.AgreedAdvance,
            lease.StartDate,
            lease.EndDate,
            lease.CreatedAt,
            lease.Status,
            !string.IsNullOrWhiteSpace(lease.AgreementDocumentUrl))).ToArray();
    }

    public async Task<IReadOnlyList<TenantRentalRequestResponse>?> GetTenantRentalHistoryAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var rentals = await GetTenantRentalRequestsAsync(tenantId, null, cancellationToken);
        return rentals?.Where(rental => rental.Status is "Active" or "CancellationRequested" or "Terminated").ToArray();
    }

    public async Task<RentalRequestResponse?> CreateRentalRequestAsync(Guid ownerId, Guid propertyId, Guid tenantId, decimal rentAmount, DateTime startDate, DateTime endDate, string? documentPath, CancellationToken cancellationToken = default)
    {
        var owner = await repository.FindUserAsync(ownerId, cancellationToken);
        var tenant = await repository.FindUserAsync(tenantId, cancellationToken);
        var property = await repository.FindPropertyAsync(propertyId, cancellationToken);
        if (owner is null || owner.CurrentRole is not ("Owner" or "Both") || tenant is null || tenant.CurrentRole is not ("Tenant" or "Both") || tenant.Profile is null
            || property is null || property.OwnerId != ownerId || !property.IsVacant || rentAmount <= 0 || startDate.Date < DateTime.UtcNow.Date || endDate.Date <= startDate.Date) return null;

        var lease = new Lease(property.PropertyId, tenantId, ownerId, rentAmount, property.AdvanceAmount);
        lease.SetRentalTerms(startDate, endDate, documentPath);
        await repository.AddLeaseAsync(lease, cancellationToken);
        return new RentalRequestResponse(lease.LeaseId, property.PropertyId, property.PropertyName, property.Location, tenant.UserId, tenant.Profile!.ProfileId, string.Join(" ", new[] { tenant.Profile.FirstName, tenant.Profile.LastName }.Where(name => !string.IsNullOrWhiteSpace(name))), lease.AgreedRent, lease.StartDate, lease.EndDate, lease.Status, !string.IsNullOrWhiteSpace(lease.AgreementDocumentUrl));
    }

    public async Task<(string? DocumentPath, bool IsAuthorized)> GetRentalRequestDocumentAsync(Guid userId, Guid leaseId, CancellationToken cancellationToken = default)
    {
        var lease = await repository.FindLeaseAsync(leaseId, cancellationToken);
        return lease is null || lease.OwnerId != userId && lease.TenantId != userId
            ? (null, false)
            : (lease.AgreementDocumentUrl, true);
    }

    public async Task<LeaseResponse?> CreateLeaseAsync(Guid tenantId, LeaseRequest request, CancellationToken cancellationToken = default)
    {
        var property = await repository.FindPropertyAsync(request.PropertyId, cancellationToken);
        if (property is null || property.Status != "Available") return null;
        var lease = new Lease(property.PropertyId, tenantId, property.OwnerId, request.AgreedRent ?? property.RentAmount, request.AgreedAdvance ?? property.AdvanceAmount);
        await repository.AddLeaseAsync(lease, cancellationToken);
        return ToLease(lease);
    }

    public async Task<IReadOnlyList<LeaseResponse>> GetLeasesAsync(Guid userId, CancellationToken cancellationToken = default) =>
        (await repository.GetLeasesAsync(userId, cancellationToken)).Select(ToLease).ToArray();

    public async Task<IReadOnlyList<TenantPaymentPropertyRecord>?> GetTenantPaymentRecordsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await repository.FindUserAsync(tenantId, cancellationToken);
        if (tenant is null || tenant.CurrentRole is not ("Tenant" or "Both")) return null;
        var leases = await repository.GetTenantPaymentLeasesAsync(tenantId, cancellationToken);
        var missingPayments = leases.SelectMany(CreateMissingMonthlyPayments).ToArray();
        if (missingPayments.Length > 0)
        {
            await repository.AddPaymentsAsync(missingPayments, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            leases = await repository.GetTenantPaymentLeasesAsync(tenantId, cancellationToken);
        }
        var firstDayOfNextMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1);
        return leases.Select(lease => new TenantPaymentPropertyRecord(
            lease.LeaseId,
            lease.PropertyId,
            lease.Property.PropertyName,
            lease.Property.PropertyType,
            lease.Property.Location,
            lease.StartDate,
            lease.EndDate,
            lease.Status,
            lease.Payments.Where(payment => payment.DueDate < firstDayOfNextMonth)
                .OrderByDescending(payment => payment.DueDate)
                .Select(payment => new TenantRentPaymentRecord(
                payment.PaymentId,
                payment.DueDate,
                payment.PaidDate,
                payment.Amount,
                payment.Status,
                payment.RewardPointsEarned)).ToArray())).ToArray();
    }

    public async Task<LeaseResponse?> ChangeLeaseStatusAsync(Guid userId, Guid leaseId, string action, CancellationToken cancellationToken = default)
    {
        var lease = await repository.FindLeaseAsync(leaseId, cancellationToken);
        if (lease is null || lease.OwnerId != userId && lease.TenantId != userId) return null;
        switch (action.Trim().ToLowerInvariant())
        {
            case "accept" when lease.TenantId == userId:
                lease.Accept();
                lease.Property.SetStatus("Leased");
                await repository.AddPaymentsAsync(CreateMissingMonthlyPayments(lease), cancellationToken);
                break;
            case "reject" when lease.TenantId == userId: lease.Reject(); break;
            case "withdraw" when lease.OwnerId == userId && lease.Status == "Pending": lease.Withdraw(); break;
            case "cancel" when lease.Status is "Pending" or "Active": lease.RequestCancellation(); break;
            case "terminate" when lease.OwnerId == userId && lease.Status == "CancellationRequested": lease.Terminate(); lease.Property.SetStatus("Available"); break;
            default: return null;
        }
        await repository.SaveChangesAsync(cancellationToken);
        return ToLease(lease);
    }

    public async Task<PaymentResponse?> PayAsync(Guid userId, Guid paymentId, PaymentRequest request, CancellationToken cancellationToken = default)
    {
        var payment = await repository.FindPaymentAsync(paymentId, cancellationToken);
        if (payment is null || payment.Lease.TenantId != userId || payment.Status != "Pending" || request.Amount != payment.Amount || string.IsNullOrWhiteSpace(request.TransactionReference)) return null;
        payment.MarkPaid(request.TransactionReference.Trim(), DateTime.UtcNow);
        var profile = await repository.FindProfileAsync(userId, cancellationToken);
        if (profile is not null) profile.AddRewardPoints(payment.RewardPointsEarned);
        await repository.SaveChangesAsync(cancellationToken);
        return new PaymentResponse(payment.PaymentId, payment.Amount, payment.DueDate, payment.PaidDate, payment.Status, payment.RewardPointsEarned);
    }

    private static IReadOnlyList<RentPayment> CreateMissingMonthlyPayments(Lease lease)
    {
        if (lease.StartDate is not { } startDate || lease.EndDate is not { } endDate || endDate.Date < startDate.Date) return [];
        var payments = new List<RentPayment>();
        var existingDueDates = lease.Payments.Select(payment => payment.DueDate.Date).ToHashSet();
        for (var month = 0; ; month++)
        {
            DateTime dueDate;
            try
            {
                dueDate = startDate.Date.AddMonths(month);
            }
            catch (ArgumentOutOfRangeException)
            {
                break;
            }
            if (dueDate > endDate.Date) break;
            if (!existingDueDates.Contains(dueDate)) payments.Add(new RentPayment(lease.LeaseId, lease.AgreedRent, dueDate));
        }
        return payments;
    }

    private async Task<ProfileResponse?> BuildProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await repository.FindUserAsync(userId, cancellationToken);
        var profile = await repository.FindProfileAsync(userId, cancellationToken);
        return user is null || profile is null ? null : new ProfileResponse(user.UserId, profile.ProfileId, profile.FirstName, profile.LastName, user.MobileNumber, user.Email, profile.PanNumber, profile.AadhaarNumber, user.MobileVerified, user.EmailVerified, profile.PanVerified, profile.AadhaarVerified, profile.BehavioralAssessmentCompleted, profile.BehavioralScore, profile.ProfileCompletionScore, profile.RewardPoints, user.CurrentRole, BuildProfileStatus(user, profile));
    }

    private static ProfileStatusDto BuildProfileStatus(User user, UserProfile profile)
    {
        var missingFields = new List<string>();
        if (string.IsNullOrWhiteSpace(profile.FirstName)) missingFields.Add("FirstName");
        if (string.IsNullOrWhiteSpace(profile.LastName)) missingFields.Add("LastName");
        if (!user.EmailVerified) missingFields.Add("Email");
        if (!user.MobileVerified) missingFields.Add("Mobile");
        if (!profile.PanVerified) missingFields.Add("PAN");
        if (!profile.AadhaarVerified) missingFields.Add("Aadhaar");
        if (!profile.BehavioralAssessmentCompleted) missingFields.Add("Behavioral assessment");
        var completedCount = 7 - missingFields.Count;
        return new ProfileStatusDto(completedCount == 7, completedCount * 100 / 7, user.MobileVerified, user.EmailVerified, profile.PanVerified, profile.AadhaarVerified, !string.IsNullOrWhiteSpace(profile.PanNumber), !string.IsNullOrWhiteSpace(profile.AadhaarNumber), profile.BehavioralAssessmentCompleted, profile.BehavioralScore, missingFields);
    }

    private static BehavioralQuestionDto ToQuestion(BehavioralQuestion question) => new(question.QuestionId, question.Category, question.QuestionText);

    private static bool ValidScore(ScoreRequest request) => new[] { request.CreditScore, request.PaymentHistory, request.OwnerFeedback }.All(value => value is >= 0 and <= 100);
    private static PropertyResponse ToProperty(Property property) => new(property.PropertyId, property.PropertyName, property.Location, property.RentAmount, property.AdvanceAmount, property.Status, property.BankAccountId);
    private static PropertyManagementResponse ToPropertyManagement(Property property) => new(property.PropertyId, property.PropertyName, property.PropertyType, property.Address, property.City, property.State, property.RentAmount, property.IsVacant);
    private static RentalRequestResponse ToRentalRequest(Lease lease) => new(lease.LeaseId, lease.PropertyId, lease.Property.PropertyName, lease.Property.Location, lease.TenantId, lease.Tenant.Profile!.ProfileId, string.Join(" ", new[] { lease.Tenant.Profile.FirstName, lease.Tenant.Profile.LastName }.Where(name => !string.IsNullOrWhiteSpace(name))), lease.AgreedRent, lease.StartDate, lease.EndDate, lease.Status, !string.IsNullOrWhiteSpace(lease.AgreementDocumentUrl));
    private static bool ValidPropertyRequest(PropertyManagementRequest request) =>
        !string.IsNullOrWhiteSpace(request.PropertyName) && request.PropertyName.Trim().Length <= 150
        && request.PropertyType is "Apartment" or "Villa" or "Independent House"
        && !string.IsNullOrWhiteSpace(request.Address) && request.Address.Trim().Length <= 255
        && !string.IsNullOrWhiteSpace(request.City) && request.City.Trim().Length <= 100
        && !string.IsNullOrWhiteSpace(request.State) && request.State.Trim().Length <= 100
        && string.Join(", ", new[] { request.Address.Trim(), request.City.Trim(), request.State.Trim() }).Length <= 512
        && request.MonthlyRent > 0;
    private static LeaseResponse ToLease(Lease lease) => new(lease.LeaseId, lease.PropertyId, lease.TenantId, lease.OwnerId, lease.AgreedRent, lease.AgreedAdvance, lease.Status, lease.StartDate);
}