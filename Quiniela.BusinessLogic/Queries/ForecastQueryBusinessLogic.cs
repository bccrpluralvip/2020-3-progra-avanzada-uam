using Microsoft.EntityFrameworkCore;
using Quiniela.Model;

namespace Quiniela.BusinessLogic.Queries;

public sealed class ForecastQueryBusinessLogic(QuinielaDbContext context)
{
    public async Task<IReadOnlyList<ForecastCountRecord>> GetForecastCountsForDateAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = start.AddDays(1);

        // Aggregate in PostgreSQL so the live-update check only loads chart counts.
        var counts = await context.Forecasts.AsNoTracking()
            .Where(forecast => forecast.SubmittedAt >= start && forecast.SubmittedAt < end)
            .GroupBy(forecast => new
            {
                forecast.MatchId,
                Hour = forecast.SubmittedAt.Hour,
                ResultType = forecast.PredictedHomeScore > forecast.PredictedAwayScore ? 1
                    : forecast.PredictedHomeScore < forecast.PredictedAwayScore ? -1 : 0
            })
            .Select(group => new
            {
                group.Key.MatchId,
                group.Key.Hour,
                group.Key.ResultType,
                Count = group.Count()
            })
            .ToListAsync(cancellationToken);

        return counts.Select(count => new ForecastCountRecord(
            count.MatchId, date, count.Hour, count.ResultType, count.Count)).ToArray();
    }

    public async Task<IReadOnlyList<ForecastCountRecord>> GetForecastCountsByMatchAsync(
        int matchId,
        CancellationToken cancellationToken = default)
    {
        if (matchId < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(matchId), "Match id must be greater than zero.");
        }

        var forecasts = await context.Forecasts
            .AsNoTracking()
            .Where(forecast => forecast.MatchId == matchId)
            .ToListAsync(cancellationToken);

        return forecasts
            .GroupBy(forecast => new
            {
                Date = DateOnly.FromDateTime(forecast.SubmittedAt),
                Hour = forecast.SubmittedAt.Hour,
                forecast.ResultType
            })
            .Select(group => new ForecastCountRecord(
                matchId,
                group.Key.Date,
                group.Key.Hour,
                group.Key.ResultType,
                group.Count()))
            .OrderBy(record => record.Date)
            .ThenBy(record => record.Hour)
            .ThenByDescending(record => record.ResultType)
            .ToArray();
    }
}

public sealed record ForecastCountRecord(
    int MatchId,
    DateOnly Date,
    int Hour,
    int ResultType,
    int Count);
