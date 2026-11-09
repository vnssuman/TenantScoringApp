namespace TenantScoring.Models;

public static class FeedbackQuestionSeed
{
    private static readonly DateTime CreatedAt = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static readonly FeedbackQuestion[] All =
    [
        new(Guid.Parse("a1000001-0000-0000-0000-000000000001"), FeedbackTypes.TenantToOwner, "Responsiveness to Maintenance: How quickly and effectively did the owner address maintenance requests?", 1, CreatedAt),
        new(Guid.Parse("a1000001-0000-0000-0000-000000000002"), FeedbackTypes.TenantToOwner, "Clarity of Communication: How clear, transparent, and professional was the owner in their communication?", 2, CreatedAt),
        new(Guid.Parse("a1000001-0000-0000-0000-000000000003"), FeedbackTypes.TenantToOwner, "Property Condition at Move-In: How well did the initial property condition match what was advertised?", 3, CreatedAt),
        new(Guid.Parse("a1000001-0000-0000-0000-000000000004"), FeedbackTypes.TenantToOwner, "Respect for Privacy: How well did the owner respect your privacy and provide adequate notice before entering?", 4, CreatedAt),
        new(Guid.Parse("a1000001-0000-0000-0000-000000000005"), FeedbackTypes.TenantToOwner, "Fairness in Financial Dealings: How fair and transparent was the owner regarding deposit returns and fees?", 5, CreatedAt),
        new(Guid.Parse("a1000001-0000-0000-0000-000000000006"), FeedbackTypes.TenantToOwner, "General Habitability & Upkeep: How well-maintained are the building's common areas and shared amenities?", 6, CreatedAt),
        new(Guid.Parse("a1000001-0000-0000-0000-000000000007"), FeedbackTypes.TenantToOwner, "Problem-Solving & Conflict Resolution: How cooperative was the owner in resolving issues or disagreements?", 7, CreatedAt),
        new(Guid.Parse("a1000001-0000-0000-0000-000000000008"), FeedbackTypes.TenantToOwner, "Lease Renewal & Flexibility: How reasonable was the owner regarding lease terms and moving out?", 8, CreatedAt),
        new(Guid.Parse("a1000001-0000-0000-0000-000000000009"), FeedbackTypes.TenantToOwner, "Overall Value for Money: How satisfied were you with the overall quality of living relative to rent paid?", 9, CreatedAt),
        new(Guid.Parse("a1000001-0000-0000-0000-000000000010"), FeedbackTypes.TenantToOwner, "Likelihood of Re-renting: How likely are you to rent from this owner again or recommend them?", 10, CreatedAt),
        new(Guid.Parse("a2000001-0000-0000-0000-000000000001"), FeedbackTypes.OwnerToTenant, "Timeliness of Rent Payments: How consistently did the tenant pay rent on or before the due date?", 1, CreatedAt),
        new(Guid.Parse("a2000001-0000-0000-0000-000000000002"), FeedbackTypes.OwnerToTenant, "Care of Property & Cleanliness: How well did the tenant maintain the interior condition of the property?", 2, CreatedAt),
        new(Guid.Parse("a2000001-0000-0000-0000-000000000003"), FeedbackTypes.OwnerToTenant, "Communication & Responsiveness: How responsive was the tenant regarding notices and administrative matters?", 3, CreatedAt),
        new(Guid.Parse("a2000001-0000-0000-0000-000000000004"), FeedbackTypes.OwnerToTenant, "Adherence to Lease Rules: How well did the tenant follow building rules, pet policies, and lease terms?", 4, CreatedAt),
        new(Guid.Parse("a2000001-0000-0000-0000-000000000005"), FeedbackTypes.OwnerToTenant, "Prompt Reporting of Issues: Did the tenant report maintenance issues early before they escalated?", 5, CreatedAt),
        new(Guid.Parse("a2000001-0000-0000-0000-000000000006"), FeedbackTypes.OwnerToTenant, "Neighbor Relations & Conduct: How respectful was the tenant toward neighbors and noise levels?", 6, CreatedAt),
        new(Guid.Parse("a2000001-0000-0000-0000-000000000007"), FeedbackTypes.OwnerToTenant, "Handling of Move-Out / Handover: How clean and undamaged was the property returned at move-out?", 7, CreatedAt),
        new(Guid.Parse("a2000001-0000-0000-0000-000000000008"), FeedbackTypes.OwnerToTenant, "Self-Sufficiency / Minor Upkeep: How capable was the tenant of handling basic, day-to-day minor tasks?", 8, CreatedAt),
        new(Guid.Parse("a2000001-0000-0000-0000-000000000009"), FeedbackTypes.OwnerToTenant, "Trustworthiness & Honesty: How reliable and honest was the tenant in all interactions?", 9, CreatedAt),
        new(Guid.Parse("a2000001-0000-0000-0000-000000000010"), FeedbackTypes.OwnerToTenant, "Likelihood of Renting Again: How likely would you be to rent to this tenant again in the future?", 10, CreatedAt),
    ];
}
