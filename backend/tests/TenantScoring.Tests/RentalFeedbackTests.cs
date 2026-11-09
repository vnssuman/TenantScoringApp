using TenantScoring.Models;
using Xunit;

namespace TenantScoring.Tests;

public sealed class RentalFeedbackTests
{
    [Fact]
    public void SeedContainsTenOrderedQuestionsForBothFeedbackDirections()
    {
        Assert.Equal(20, FeedbackQuestionSeed.All.Length);
        Assert.Equal(20, FeedbackQuestionSeed.All.Select(question => question.Id).Distinct().Count());
        Assert.Collection(
            FeedbackQuestionSeed.All.Where(question => question.Category == FeedbackTypes.TenantToOwner).OrderBy(question => question.DisplayOrder),
            Enumerable.Range(1, 10).Select(order => (Action<FeedbackQuestion>)(question => Assert.Equal(order, question.DisplayOrder))).ToArray());
        Assert.Collection(
            FeedbackQuestionSeed.All.Where(question => question.Category == FeedbackTypes.OwnerToTenant).OrderBy(question => question.DisplayOrder),
            Enumerable.Range(1, 10).Select(order => (Action<FeedbackQuestion>)(question => Assert.Equal(order, question.DisplayOrder))).ToArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void FeedbackResponseRejectsRatingsOutsideOneToFive(int rating)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FeedbackResponse(Guid.NewGuid(), Guid.NewGuid(), rating));
    }

    [Fact]
    public void FeedbackSessionCanBeCompletedOnceWithCalculatedScore()
    {
        var session = new FeedbackSession(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FeedbackTypes.TenantToOwner);
        var completedAt = DateTime.UtcNow;

        session.Complete(4.5, completedAt);

        Assert.Equal("COMPLETED", session.Status);
        Assert.Equal(4.5, session.OverallScore);
        Assert.Equal(completedAt, session.CompletedAt);
        Assert.Throws<InvalidOperationException>(() => session.Complete(4, completedAt));
    }
}
