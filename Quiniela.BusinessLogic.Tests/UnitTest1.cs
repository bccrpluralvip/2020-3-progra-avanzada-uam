using Quiniela.BusinessLogic;

namespace Quiniela.BusinessLogic.Tests;

public class QuinielaScorerTests
{
    [Theory]
    [InlineData(2, 1, 2, 1, 5)]
    [InlineData(1, 2, 1, 2, 5)]
    [InlineData(3, 3, 3, 3, 5)]
    [InlineData(3, 1, 2, 0, 2)]
    [InlineData(1, 3, 0, 2, 2)]
    [InlineData(2, 2, 1, 1, 2)]
    [InlineData(3, 1, 3, 0, 3)]
    [InlineData(3, 1, 2, 1, 3)]
    [InlineData(1, 3, 1, 2, 3)]
    [InlineData(1, 3, 0, 3, 3)]
    [InlineData(3, 1, 1, 3, 0)]
    [InlineData(1, 3, 3, 1, 0)]
    [InlineData(2, 2, 3, 1, 0)]
    public void CalculatePoints_ReturnsExpectedPoints(
        int realTeamAScore,
        int realTeamBScore,
        int guessedTeamAScore,
        int guessedTeamBScore,
        int expectedPoints)
    {
        var scorer = new QuinielaScorer();

        var points = scorer.CalculatePoints(
            realTeamAScore,
            realTeamBScore,
            guessedTeamAScore,
            guessedTeamBScore);

        Assert.Equal(expectedPoints, points);
    }
}
