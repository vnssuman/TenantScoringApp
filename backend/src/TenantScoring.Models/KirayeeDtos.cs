using System.Text.Json.Serialization;

namespace TenantScoring.Models;

public sealed record KirayeeRegisterRequest(string MobileNumber, string Email, string Pin, string ConfirmPin, string? FirstName, string? LastName);
public sealed record KirayeeRegisterResponse(Guid UserId, string Message);
public sealed record KirayeeRegistrationResult(KirayeeRegisterResponse? Response, string? ErrorMessage);
public sealed record VerificationRequest(string Channel, string Code);
public sealed record ProfileUpdateRequest(string? FirstName, string? LastName, string? PanNumber, string? AadhaarNumber, string? MobileNumber, string? Email, string? Role);
public sealed record ProfileStatusDto(bool IsProfileComplete, int CompletionPercentage, bool IsMobileVerified, bool IsEmailVerified, bool IsPanVerified, bool IsAadhaarVerified, bool IsPanProvided, bool IsAadhaarProvided, bool BehavioralAssessmentCompleted, decimal BehavioralScore, IReadOnlyList<string> MissingFields);
public sealed record BehavioralQuestionDto(Guid QuestionId, string Category, string QuestionText);
public sealed record BehavioralAnswerRequest(Guid QuestionId, string Answer);
public sealed record BehavioralAssessmentRequest(IReadOnlyList<BehavioralAnswerRequest> Answers);
public sealed record BehavioralAssessmentResponse(bool Completed, decimal Score, IReadOnlyList<BehavioralQuestionDto> Questions, IReadOnlyList<BehavioralAnswerRequest> Answers);
public sealed record AuthResponseDto(Guid UserId, string? FirstName, string CurrentRole, string Token, ProfileStatusDto ProfileStatus);
public sealed record ProfileResponse(Guid UserId, Guid ProfileId, string ProfileCode, DateTime CreatedAt, string? FirstName, string? LastName, string? MobileNumber, string? Email, string? PanNumber, string? AadhaarNumber, bool MobileVerified, bool EmailVerified, bool PanVerified, bool AadhaarVerified, bool BehavioralAssessmentCompleted, decimal BehavioralScore, int ProfileCompletionScore, int RewardPoints, string CurrentRole, ProfileStatusDto ProfileStatus);
public sealed record RoleUpdateRequest(string Role);
public sealed record ScoreResponse(Guid ScoreId, decimal FinalScore, int ProfileScore, decimal CreditScore, decimal PaymentHistory, decimal OwnerFeedback, DateTime CalculatedAt, DateTime ExpiresAt, bool IsExpired);
public sealed record ScoreRequest(decimal CreditScore, decimal PaymentHistory, decimal OwnerFeedback);
public sealed record BankAccountRequest(string AccountHolderName, string BankName, string AccountNumber, string IFSCCode);
public sealed record BankAccountResponse(Guid BankAccountId, string AccountHolderName, string BankName, string AccountNumber, string IFSCCode);
public sealed record PropertyRequest(Guid? BankAccountId, string PropertyName, string Location, decimal RentAmount, decimal AdvanceAmount);
public sealed record PropertyResponse(Guid PropertyId, string PropertyName, string Location, decimal RentAmount, decimal AdvanceAmount, string Status, Guid? BankAccountId);
public sealed record PropertyManagementRequest(
	[property: JsonPropertyName("property_name")] string PropertyName,
	[property: JsonPropertyName("property_type")] string PropertyType,
	[property: JsonPropertyName("address")] string Address,
	[property: JsonPropertyName("city")] string City,
	[property: JsonPropertyName("state")] string State,
	[property: JsonPropertyName("monthly_rent")] decimal MonthlyRent);
public sealed record PropertyManagementResponse(
	[property: JsonPropertyName("id")] Guid Id,
	[property: JsonPropertyName("property_name")] string PropertyName,
	[property: JsonPropertyName("property_type")] string PropertyType,
	[property: JsonPropertyName("address")] string Address,
	[property: JsonPropertyName("city")] string City,
	[property: JsonPropertyName("state")] string State,
	[property: JsonPropertyName("monthly_rent")] decimal MonthlyRent,
	[property: JsonPropertyName("is_vacant")] bool IsVacant);
public sealed record LeaseRequest(Guid PropertyId, decimal? AgreedRent, decimal? AgreedAdvance);
public sealed record LeaseResponse(Guid LeaseId, Guid PropertyId, Guid TenantId, Guid OwnerId, decimal AgreedRent, decimal AgreedAdvance, string Status, DateTime? StartDate);
public sealed record TenantLookupResponse(Guid TenantId, string MobileNumber, string FullName);
public sealed record RentalRequestResponse(Guid LeaseId, Guid PropertyId, string PropertyName, string Location, Guid TenantId, string ProfileCode, string TenantName, decimal AgreedRent, DateTime? StartDate, DateTime? EndDate, string Status, string? TenantComments, bool HasAgreementDocument);
public sealed record TenantRentalRequestResponse(Guid LeaseId, string OwnerName, string PropertyName, string PropertyType, string Address, string City, string State, decimal AgreedRent, decimal AgreedAdvance, DateTime? StartDate, DateTime? EndDate, DateTime CreatedAt, string Status, string? TenantComments, bool HasAgreementDocument);
public sealed record TenantRequestCommentRequest(string Comments);
public sealed record PaymentRequest(decimal Amount, string TransactionReference);
public sealed record PaymentResponse(Guid PaymentId, decimal Amount, DateTime DueDate, DateTime? PaidDate, string Status, int RewardPointsEarned);
public sealed record TenantRentPaymentRecord(Guid PaymentId, DateTime DueDate, DateTime? PaidDate, decimal Amount, string Status, int RewardPointsEarned);
public sealed record TenantPaymentPropertyRecord(Guid LeaseId, Guid PropertyId, string PropertyName, string PropertyType, string Location, DateTime? StartDate, DateTime? EndDate, string LeaseStatus, IReadOnlyList<TenantRentPaymentRecord> Payments);