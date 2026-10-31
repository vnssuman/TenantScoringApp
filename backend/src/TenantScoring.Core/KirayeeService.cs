using System.Text.RegularExpressions;
using BCrypt.Net;
using Microsoft.Extensions.Logging;
using TenantScoring.Models;

namespace TenantScoring.Core;

public interface IKirayeeRepository
{
    Task<User?> FindUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<User?> FindUserByMobileAsync(string mobileNumber, CancellationToken cancellationToken);
    Task AddUserAsync(User user, UserProfile profile, CancellationToken cancellationToken);
    Task<UserProfile?> FindProfileAsync(Guid userId, CancellationToken cancellationToken);
    Task<List<BehavioralQuestion>> GetBehavioralQuestionsAsync(CancellationToken cancellationToken);
    Task<List<TenantBehavioralResponse>> GetBehavioralResponsesAsync(Guid userId, CancellationToken cancellationToken);
    Task AddBehavioralResponsesAsync(IEnumerable<TenantBehavioralResponse> responses, CancellationToken cancellationToken);
    Task AddBankAccountAsync(OwnerBankAccount account, CancellationToken cancellationToken);
    Task<IReadOnlyList<OwnerBankAccount>> GetBankAccountsAsync(Guid ownerId, CancellationToken cancellationToken);
    Task AddPropertyAsync(Property property, CancellationToken cancellationToken);
    Task<IReadOnlyList<Property>> GetPropertiesAsync(Guid? ownerId, CancellationToken cancellationToken);
    Task<Property?> FindPropertyAsync(Guid propertyId, CancellationToken cancellationToken);
    Task AddScoreAsync(TenantScore score, CancellationToken cancellationToken);
    Task<TenantScore?> GetLatestScoreAsync(Guid tenantId, CancellationToken cancellationToken);
    Task AddLeaseAsync(Lease lease, CancellationToken cancellationToken);
    Task<Lease?> FindLeaseAsync(Guid leaseId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Lease>> GetLeasesAsync(Guid userId, CancellationToken cancellationToken);
    Task AddPaymentAsync(RentPayment payment, CancellationToken cancellationToken);
    Task<RentPayment?> FindPaymentAsync(Guid paymentId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IKirayeeService
{
    Task<KirayeeRegisterResponse?> RegisterAsync(KirayeeRegisterRequest request, CancellationToken cancellationToken = default);
    Task<ProfileResponse?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
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
    Task<LeaseResponse?> CreateLeaseAsync(Guid tenantId, LeaseRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaseResponse>> GetLeasesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<LeaseResponse?> ChangeLeaseStatusAsync(Guid userId, Guid leaseId, string action, CancellationToken cancellationToken = default);
    Task<PaymentResponse?> PayAsync(Guid userId, Guid paymentId, PaymentRequest request, CancellationToken cancellationToken = default);
}

public sealed class KirayeeService(IKirayeeRepository repository, ILogger<KirayeeService> logger) : IKirayeeService
{
    public async Task<KirayeeRegisterResponse?> RegisterAsync(KirayeeRegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (!Regex.IsMatch(request.MobileNumber.Trim(), "^\\d{10,15}$") || !Regex.IsMatch(request.Email.Trim(), "^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$")) return null;
        if (request.Pin.Length != 4 || request.Pin != request.ConfirmPin || request.Pin.Any(character => character is < '0' or > '9')) return null;
        if (await repository.FindUserByMobileAsync(request.MobileNumber.Trim(), cancellationToken) is not null) return null;

        var user = new User(request.MobileNumber.Trim(), request.Email.Trim().ToLowerInvariant(), BCrypt.Net.BCrypt.HashPassword(request.Pin));
        await repository.AddUserAsync(user, new UserProfile(user.UserId, request.FirstName, request.LastName), cancellationToken);
        logger.LogInformation("Registered Kirayee user {UserId}", user.UserId);
        return new KirayeeRegisterResponse(user.UserId, "Account created. Verify your mobile and email to continue.");
    }

    public async Task<ProfileResponse?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await BuildProfileAsync(userId, cancellationToken);

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
        return new BehavioralAssessmentResponse(profile.BehavioralAssessmentCompleted, profile.BehavioralScore, questions.Select(ToQuestion).ToArray());
    }

    public async Task<BehavioralAssessmentResponse?> SubmitBehavioralAssessmentAsync(Guid userId, BehavioralAssessmentRequest request, CancellationToken cancellationToken = default)
    {
        var profile = await repository.FindProfileAsync(userId, cancellationToken);
        if (profile is null) return null;
        var questions = await repository.GetBehavioralQuestionsAsync(cancellationToken);
        if (questions.Count != 3 || request.Answers.Count != 3 || request.Answers.Select(answer => answer.QuestionId).Distinct().Count() != 3 || request.Answers.Any(answer => answer.Answer is not ("Yes" or "No")) || request.Answers.Any(answer => questions.All(question => question.QuestionId != answer.QuestionId))) return null;
        if ((await repository.GetBehavioralResponsesAsync(userId, cancellationToken)).Count > 0) return null;

        var responses = request.Answers.Select(answer =>
        {
            var question = questions.Single(question => question.QuestionId == answer.QuestionId);
            var points = string.Equals(answer.Answer, question.PreferredAnswer, StringComparison.OrdinalIgnoreCase) ? question.WeightPoints : 0;
            return new TenantBehavioralResponse(userId, question.QuestionId, answer.Answer, points);
        }).ToArray();
        await repository.AddBehavioralResponsesAsync(responses, cancellationToken);
        var score = responses.Sum(response => response.PointsEarned) / (decimal)questions.Sum(question => question.WeightPoints) * 100m;
        profile.SetBehavioralAssessment(score);
        await repository.SaveChangesAsync(cancellationToken);
        return new BehavioralAssessmentResponse(true, score, questions.Select(ToQuestion).ToArray());
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
        if (await repository.FindUserAsync(userId, cancellationToken) is null || request.RentAmount <= 0 || request.AdvanceAmount < 0 || string.IsNullOrWhiteSpace(request.PropertyName) || string.IsNullOrWhiteSpace(request.Location)) return null;
        var property = new Property(userId, request.BankAccountId, request.PropertyName.Trim(), request.Location.Trim(), request.RentAmount, request.AdvanceAmount);
        await repository.AddPropertyAsync(property, cancellationToken);
        return ToProperty(property);
    }

    public async Task<IReadOnlyList<PropertyResponse>> GetPropertiesAsync(Guid? ownerId, CancellationToken cancellationToken = default) =>
        (await repository.GetPropertiesAsync(ownerId, cancellationToken)).Select(ToProperty).ToArray();

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

    public async Task<LeaseResponse?> ChangeLeaseStatusAsync(Guid userId, Guid leaseId, string action, CancellationToken cancellationToken = default)
    {
        var lease = await repository.FindLeaseAsync(leaseId, cancellationToken);
        if (lease is null || lease.OwnerId != userId && lease.TenantId != userId) return null;
        switch (action.Trim().ToLowerInvariant())
        {
            case "accept" when lease.TenantId == userId: lease.Accept(); lease.Property.SetStatus("Leased"); break;
            case "reject" when lease.TenantId == userId: lease.Reject(); break;
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
        if (payment is null || request.Amount != payment.Amount || string.IsNullOrWhiteSpace(request.TransactionReference)) return null;
        payment.MarkPaid(request.TransactionReference.Trim(), DateTime.UtcNow);
        var profile = await repository.FindProfileAsync(userId, cancellationToken);
        if (profile is not null) profile.AddRewardPoints(payment.RewardPointsEarned);
        await repository.SaveChangesAsync(cancellationToken);
        return new PaymentResponse(payment.PaymentId, payment.Amount, payment.DueDate, payment.PaidDate, payment.Status, payment.RewardPointsEarned);
    }

    private async Task<ProfileResponse?> BuildProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await repository.FindUserAsync(userId, cancellationToken);
        var profile = await repository.FindProfileAsync(userId, cancellationToken);
        return user is null || profile is null ? null : new ProfileResponse(user.UserId, profile.FirstName, profile.LastName, user.MobileNumber, user.Email, profile.PanNumber, profile.AadhaarNumber, user.MobileVerified, user.EmailVerified, profile.PanVerified, profile.AadhaarVerified, profile.BehavioralAssessmentCompleted, profile.BehavioralScore, profile.ProfileCompletionScore, profile.RewardPoints, user.CurrentRole, BuildProfileStatus(user, profile));
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
        return new ProfileStatusDto(completedCount == 7, completedCount * 100 / 7, user.MobileVerified, user.EmailVerified, profile.PanVerified, profile.AadhaarVerified, profile.BehavioralAssessmentCompleted, profile.BehavioralScore, missingFields);
    }

    private static BehavioralQuestionDto ToQuestion(BehavioralQuestion question) => new(question.QuestionId, question.Category, question.QuestionText);

    private static bool ValidScore(ScoreRequest request) => new[] { request.CreditScore, request.PaymentHistory, request.OwnerFeedback }.All(value => value is >= 0 and <= 100);
    private static PropertyResponse ToProperty(Property property) => new(property.PropertyId, property.PropertyName, property.Location, property.RentAmount, property.AdvanceAmount, property.Status, property.BankAccountId);
    private static LeaseResponse ToLease(Lease lease) => new(lease.LeaseId, lease.PropertyId, lease.TenantId, lease.OwnerId, lease.AgreedRent, lease.AgreedAdvance, lease.Status, lease.StartDate);
}