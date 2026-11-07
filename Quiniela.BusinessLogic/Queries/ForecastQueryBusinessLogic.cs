using Microsoft.EntityFrameworkCore;
using Quiniela.Model;

namespace Quiniela.BusinessLogic.Queries;

public sealed class ForecastQueryBusinessLogic(QuinielaDbContext context)
{
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
