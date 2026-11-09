namespace TenantScoring.Models;

public sealed class User
{
    private User() { }

    public User(string mobileNumber, string email, string pinHash)
    {
        UserId = Guid.NewGuid();
        MobileNumber = mobileNumber;
        Email = email;
        PinHash = pinHash;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public string MobileNumber { get; private set; } = null!;
    public bool MobileVerified { get; private set; }
    public string Email { get; private set; } = null!;
    public bool EmailVerified { get; private set; }
    public string CurrentRole { get; private set; } = "Tenant";
    public bool IsActive { get; private set; } = true;
    public string PinHash { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public UserProfile? Profile { get; private set; }
    public ICollection<OwnerBankAccount> BankAccounts { get; private set; } = new List<OwnerBankAccount>();
    public ICollection<Property> Properties { get; private set; } = new List<Property>();

    public void SetRole(string role) => CurrentRole = role;
    public void UpdateContact(string mobileNumber, string email)
    {
        var normalizedMobileNumber = mobileNumber.Trim();
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (!string.Equals(MobileNumber, normalizedMobileNumber, StringComparison.Ordinal)) MobileVerified = false;
        if (!string.Equals(Email, normalizedEmail, StringComparison.OrdinalIgnoreCase)) EmailVerified = false;
        MobileNumber = normalizedMobileNumber;
        Email = normalizedEmail;
    }
    public void VerifyMobile() => MobileVerified = true;
    public void VerifyEmail() => EmailVerified = true;
}

public sealed class UserProfile
{
    private UserProfile() { }

    public UserProfile(Guid userId, string? firstName, string? lastName)
    {
        ProfileId = Guid.NewGuid();
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid ProfileId { get; private set; }
    public string ProfileCode { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? PanNumber { get; private set; }
    public bool PanVerified { get; private set; }
    public string? AadhaarNumber { get; private set; }
    public byte[]? ProfileImageData { get; private set; }
    public string? ProfileImageContentType { get; private set; }
    public bool AadhaarVerified { get; private set; }
    public bool BehavioralAssessmentCompleted { get; private set; }
    public decimal BehavioralScore { get; private set; }
    public int ProfileCompletionScore { get; private set; }
    public int RewardPoints { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public User User { get; private set; } = null!;

    public void UpdateIdentity(string? firstName, string? lastName, string? panNumber, string? aadhaarNumber)
    {
        FirstName = firstName?.Trim();
        LastName = lastName?.Trim();
        PanNumber = panNumber?.Trim().ToUpperInvariant();
        AadhaarNumber = aadhaarNumber?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetProfileImage(byte[] imageData, string contentType)
    {
        ProfileImageData = imageData;
        ProfileImageContentType = contentType;
        UpdatedAt = DateTime.UtcNow;
    }

    public void VerifyPan() { PanVerified = true; RecalculateCompletion(); }
    public void VerifyAadhaar() { AadhaarVerified = true; RecalculateCompletion(); }
    public void SetBehavioralAssessment(decimal score)
    {
        BehavioralAssessmentCompleted = true;
        BehavioralScore = Math.Clamp(score, 0, 100);
        RecalculateCompletion();
    }
    public void AddRewardPoints(int points) => RewardPoints += Math.Max(0, points);

    public void RecalculateCompletion()
    {
        var identityScore = (PanVerified && AadhaarVerified) ? 100m : ((PanVerified ? 50m : 0m) + (AadhaarVerified ? 50m : 0m));
        ProfileCompletionScore = (int)Math.Round(identityScore * 0.5m + (BehavioralAssessmentCompleted ? BehavioralScore : 0m) * 0.5m);
    }
}

public sealed class OwnerBankAccount
{
    private OwnerBankAccount() { }

    public OwnerBankAccount(Guid ownerId, string accountHolderName, string bankName, string accountNumber, string ifscCode)
    {
        BankAccountId = Guid.NewGuid();
        OwnerId = ownerId;
        AccountHolderName = accountHolderName;
        BankName = bankName;
        AccountNumber = accountNumber;
        IFSCCode = ifscCode;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid BankAccountId { get; private set; }
    public Guid OwnerId { get; private set; }
    public string AccountHolderName { get; private set; } = null!;
    public string BankName { get; private set; } = null!;
    public string AccountNumber { get; private set; } = null!;
    public string IFSCCode { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public User Owner { get; private set; } = null!;
    public ICollection<Property> Properties { get; private set; } = new List<Property>();
}

public sealed class Property
{
    private Property() { }

    public Property(Guid ownerId, Guid? bankAccountId, string propertyName, string location, decimal rentAmount, decimal advanceAmount)
    {
        PropertyId = Guid.NewGuid();
        OwnerId = ownerId;
        BankAccountId = bankAccountId;
        PropertyName = propertyName;
        Location = location;
        RentAmount = rentAmount;
        AdvanceAmount = advanceAmount;
        Status = "Available";
        PropertyType = "Apartment";
        Address = location;
        City = string.Empty;
        State = string.Empty;
        IsVacant = true;
        CreatedAt = DateTime.UtcNow;
    }

    public Property(Guid ownerId, string propertyName, string propertyType, string address, string city, string state, decimal monthlyRent)
        : this(ownerId, null, propertyName, FormatLocation(address, city, state), monthlyRent, 0)
    {
        PropertyType = propertyType;
        Address = address;
        City = city;
        State = state;
    }

    public Guid PropertyId { get; private set; }
    public Guid OwnerId { get; private set; }
    public Guid? BankAccountId { get; private set; }
    public string PropertyName { get; private set; } = null!;
    public string Location { get; private set; } = null!;
    public string PropertyType { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public string State { get; private set; } = null!;
    public decimal RentAmount { get; private set; }
    public decimal AdvanceAmount { get; private set; }
    public bool IsVacant { get; private set; }
    public string Status { get; private set; } = "Available";
    public DateTime CreatedAt { get; private set; }
    public User Owner { get; private set; } = null!;
    public OwnerBankAccount? BankAccount { get; private set; }
    public ICollection<Lease> Leases { get; private set; } = new List<Lease>();

    public bool HasLiveLeaseOn(DateTime date) => Leases.Any(lease => lease.IsLiveOn(date));

    public void UpdateDetails(string propertyName, string propertyType, string address, string city, string state, decimal monthlyRent)
    {
        PropertyName = propertyName;
        PropertyType = propertyType;
        Address = address;
        City = city;
        State = state;
        Location = FormatLocation(address, city, state);
        RentAmount = monthlyRent;
    }

    public void SetStatus(string status)
    {
        Status = status;
        if (status is "Available" or "Leased") IsVacant = status == "Available";
    }

    private static string FormatLocation(string address, string city, string state) => string.Join(", ", new[] { address, city, state }.Where(part => !string.IsNullOrWhiteSpace(part)));
}

public sealed class TenantScore
{
    private TenantScore() { }

    public TenantScore(Guid tenantId, decimal profileScore, decimal creditScore, decimal paymentHistory, decimal ownerFeedback)
    {
        ScoreId = Guid.NewGuid();
        TenantId = tenantId;
        ProfileScoreWeight = 45;
        CreditScoreWeight = 15;
        PaymentHistoryWeight = 20;
        OwnerFeedbackWeight = 20;
        FinalScore = Math.Round(profileScore * .45m + creditScore * .15m + paymentHistory * .20m + ownerFeedback * .20m, 2);
        CalculatedAt = DateTime.UtcNow;
        ExpiresAt = CalculatedAt.AddDays(90);
    }

    public Guid ScoreId { get; private set; }
    public Guid TenantId { get; private set; }
    public decimal ProfileScoreWeight { get; private set; }
    public decimal CreditScoreWeight { get; private set; }
    public decimal PaymentHistoryWeight { get; private set; }
    public decimal OwnerFeedbackWeight { get; private set; }
    public decimal FinalScore { get; private set; }
    public DateTime CalculatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public User Tenant { get; private set; } = null!;
}

public sealed class Lease
{
    private Lease() { }

    public Lease(Guid propertyId, Guid tenantId, Guid ownerId, decimal agreedRent, decimal agreedAdvance)
    {
        LeaseId = Guid.NewGuid();
        PropertyId = propertyId;
        TenantId = tenantId;
        OwnerId = ownerId;
        AgreedRent = agreedRent;
        AgreedAdvance = agreedAdvance;
        Status = "Pending";
        CreatedAt = DateTime.UtcNow;
    }

    public Guid LeaseId { get; private set; }
    public Guid PropertyId { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid OwnerId { get; private set; }
    public decimal AgreedRent { get; private set; }
    public decimal AgreedAdvance { get; private set; }
    public string Status { get; private set; } = "Pending";
    public string? TenantComments { get; private set; }
    public string? AgreementDocumentUrl { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Property Property { get; private set; } = null!;
    public User Tenant { get; private set; } = null!;
    public User Owner { get; private set; } = null!;
    public ICollection<RentPayment> Payments { get; private set; } = new List<RentPayment>();
    public ICollection<OwnerFeedback> Feedback { get; private set; } = new List<OwnerFeedback>();

    public bool IsLiveOn(DateTime date) =>
        Status is "Active" or "CancellationRequested"
        && (StartDate is null || StartDate.Value.Date <= date.Date)
        && (EndDate is null || EndDate.Value.Date >= date.Date);

    public void Accept() { Status = "Active"; StartDate ??= DateTime.UtcNow; }
    public void SetRentalTerms(DateTime startDate, DateTime endDate, string? agreementDocumentUrl)
    {
        StartDate = startDate;
        EndDate = endDate;
        AgreementDocumentUrl = agreementDocumentUrl;
    }
    public void SendBack(string comments)
    {
        TenantComments = comments;
        Status = "SentBack";
    }

    public void Withdraw() => Status = "Withdrawn";
    public void RequestCancellation() => Status = "CancellationRequested";
    public void Terminate() => Status = "Terminated";
}

public sealed class RentPayment
{
    private RentPayment() { }

    public RentPayment(Guid leaseId, decimal amount, DateTime dueDate)
    {
        PaymentId = Guid.NewGuid();
        LeaseId = leaseId;
        Amount = amount;
        DueDate = dueDate;
        Status = "Pending";
    }

    public Guid PaymentId { get; private set; }
    public Guid LeaseId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? PaidDate { get; private set; }
    public string Status { get; private set; } = "Pending";
    public int RewardPointsEarned { get; private set; }
    public string? TransactionReference { get; private set; }
    public Lease Lease { get; private set; } = null!;

    public void MarkPaid(string transactionReference, DateTime paidDate)
    {
        PaidDate = paidDate;
        TransactionReference = transactionReference;
        Status = "Paid";
        RewardPointsEarned = paidDate.Date <= DueDate.Date ? 10 : 0;
    }
}

public sealed class OwnerFeedback
{
    private OwnerFeedback() { }

    public OwnerFeedback(Guid leaseId, Guid tenantId, Guid ownerId, int rating, string? reviewText)
    {
        FeedbackId = Guid.NewGuid();
        LeaseId = leaseId;
        TenantId = tenantId;
        OwnerId = ownerId;
        Rating = rating;
        ReviewText = reviewText;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid FeedbackId { get; private set; }
    public Guid LeaseId { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid OwnerId { get; private set; }
    public int Rating { get; private set; }
    public string? ReviewText { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Lease Lease { get; private set; } = null!;
    public User Tenant { get; private set; } = null!;
    public User Owner { get; private set; } = null!;
}

public sealed class BehavioralQuestion
{
    private BehavioralQuestion() { }

    public BehavioralQuestion(Guid questionId, string category, string questionText, string preferredAnswer, int weightPoints)
    {
        QuestionId = questionId;
        Category = category;
        QuestionText = questionText;
        PreferredAnswer = preferredAnswer;
        WeightPoints = weightPoints;
        IsActive = true;
    }

    public Guid QuestionId { get; private set; }
    public string Category { get; private set; } = null!;
    public string QuestionText { get; private set; } = null!;
    public string PreferredAnswer { get; private set; } = null!;
    public int WeightPoints { get; private set; }
    public bool IsActive { get; private set; }
    public ICollection<TenantBehavioralResponse> Responses { get; private set; } = new List<TenantBehavioralResponse>();
}

public sealed class TenantBehavioralResponse
{
    private TenantBehavioralResponse() { }

    public TenantBehavioralResponse(Guid userId, Guid questionId, string userAnswer, int pointsEarned)
    {
        ResponseId = Guid.NewGuid();
        UserId = userId;
        QuestionId = questionId;
        UserAnswer = userAnswer;
        PointsEarned = pointsEarned;
        AnsweredAt = DateTime.UtcNow;
    }

    public Guid ResponseId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid QuestionId { get; private set; }
    public string UserAnswer { get; private set; } = null!;
    public int PointsEarned { get; private set; }
    public DateTime AnsweredAt { get; private set; }
    public User User { get; private set; } = null!;
    public BehavioralQuestion Question { get; private set; } = null!;

    public void UpdateAnswer(string userAnswer, int pointsEarned)
    {
        UserAnswer = userAnswer;
        PointsEarned = pointsEarned;
        AnsweredAt = DateTime.UtcNow;
    }
}