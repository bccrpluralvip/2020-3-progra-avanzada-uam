using Quiniela.Api.Services;
using Quiniela.BusinessLogic.Queries;

namespace Quiniela.Api.Tests;

public sealed class ForecastsChartChangeMonitorTests
{
    private static readonly DateOnly Today = new(2026, 11, 14);

    private static ForecastCountRecord Count(int matchId = 11, int hour = 15, int result = 1, int count = 1) =>
        new(matchId, Today, hour, result, count);

    [Fact]
    public void Insert_NotifiesOnlyAffectedMatch()
    {
        var existing = Count(matchId: 10);
        Assert.Equal([11], ForecastsChartChangeMonitor.GetChangedMatchIds(
            [existing], [existing, Count()]));
    }

    [Fact]
    public void AdditionalForecastInExistingBucket_NotifiesMatch()
    {
        Assert.Equal([11], ForecastsChartChangeMonitor.GetChangedMatchIds(
            [Count()], [Count(count: 2)]));
    }

    [Fact]
    public void DeleteLastForecast_NotifiesMatch()
    {
        Assert.Equal([11], ForecastsChartChangeMonitor.GetChangedMatchIds([Count()], []));
    }

    [Fact]
    public void MovingForecastToAnotherMatch_NotifiesBothMatchesOnce()
    {
        Assert.Equal([10, 11], ForecastsChartChangeMonitor.GetChangedMatchIds(
            [Count(matchId: 10)], [Count()]).Order());
    }

    [Theory]
    [InlineData(16, 1)]
    [InlineData(15, 0)]
    [InlineData(15, -1)]
    public void ChangedHourOrResult_NotifiesEvenWhenTotalCountIsUnchanged(int hour, int result)
    {
        Assert.Equal([11], ForecastsChartChangeMonitor.GetChangedMatchIds(
            [Count()], [Count(hour: hour, result: result)]));
    }

    [Fact]
    public void UnchangedCountsInDifferentOrder_DoNotNotify()
    {
        Assert.Empty(ForecastsChartChangeMonitor.GetChangedMatchIds(
            [Count(), Count(matchId: 10)], [Count(matchId: 10), Count()]));
    }

    [Fact]
    public void UtcDayRollover_NotifiesEvenWhenNewDayHasSameCounts()
    {
        Assert.Equal([11], ForecastsChartChangeMonitor.GetChangedMatchIds(
            [Count()], [Count() with { Date = Today.AddDays(1) }]));
    }
}
