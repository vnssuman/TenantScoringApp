namespace TenantScoring.Models;

public static class FeedbackTypes
{
    public const string TenantToOwner = "TENANT_TO_OWNER";
    public const string OwnerToTenant = "OWNER_TO_TENANT";
}

public sealed class FeedbackQuestion
{
    private FeedbackQuestion() { }

    public FeedbackQuestion(Guid id, string category, string questionText, int displayOrder, DateTime createdAt)
    {
        Id = id;
        Category = category;
        QuestionText = questionText;
        DisplayOrder = displayOrder;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string Category { get; private set; } = null!;
    public string QuestionText { get; private set; } = null!;
    public int DisplayOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ICollection<FeedbackResponse> Responses { get; private set; } = new List<FeedbackResponse>();
}

public sealed class FeedbackSession
{
    private FeedbackSession() { }

    public FeedbackSession(Guid leaseId, Guid propertyId, Guid evaluatorId, Guid subjectId, string feedbackType)
    {
        Id = Guid.NewGuid();
        LeaseId = leaseId;
        PropertyId = propertyId;
        EvaluatorId = evaluatorId;
        SubjectId = subjectId;
        FeedbackType = feedbackType;
        Status = "PENDING";
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid LeaseId { get; private set; }
    public Guid PropertyId { get; private set; }
    public Guid EvaluatorId { get; private set; }
    public Guid SubjectId { get; private set; }
    public string FeedbackType { get; private set; } = null!;
    public string Status { get; private set; } = "PENDING";
    public double? OverallScore { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Lease Lease { get; private set; } = null!;
    public Property Property { get; private set; } = null!;
    public User Evaluator { get; private set; } = null!;
    public User Subject { get; private set; } = null!;
    public ICollection<FeedbackResponse> Responses { get; private set; } = new List<FeedbackResponse>();

    public void Complete(double overallScore, DateTime completedAt)
    {
        if (Status != "PENDING") throw new InvalidOperationException("Only pending feedback can be completed.");
        if (overallScore is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(overallScore));
        OverallScore = overallScore;
        CompletedAt = completedAt;
        Status = "COMPLETED";
    }
}

public sealed class FeedbackResponse
{
    private FeedbackResponse() { }

    public FeedbackResponse(Guid sessionId, Guid questionId, int rating)
    {
        if (rating is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(rating));
        Id = Guid.NewGuid();
        SessionId = sessionId;
        QuestionId = questionId;
        Rating = rating;
    }

    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public Guid QuestionId { get; private set; }
    public int Rating { get; private set; }
    public FeedbackSession Session { get; private set; } = null!;
    public FeedbackQuestion Question { get; private set; } = null!;
}
