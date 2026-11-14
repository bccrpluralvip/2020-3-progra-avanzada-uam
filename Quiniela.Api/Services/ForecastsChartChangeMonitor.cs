using Microsoft.AspNetCore.SignalR;
using Quiniela.Api.Hubs;
using Quiniela.BusinessLogic.Queries;

namespace Quiniela.Api.Services;

public sealed class ForecastsChartChangeMonitor(
    IServiceScopeFactory scopeFactory,
    IHubContext<ForecastsChartHub> hubContext,
    ILogger<ForecastsChartChangeMonitor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IReadOnlyList<ForecastCountRecord> previous = [];
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        do
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var queries = scope.ServiceProvider.GetRequiredService<ForecastQueryBusinessLogic>();
                var current = await queries.GetForecastCountsForDateAsync(
                    DateOnly.FromDateTime(DateTime.UtcNow), stoppingToken);

                foreach (var matchId in GetChangedMatchIds(previous, current))
                {
                    await hubContext.Clients.Group(ForecastsChartHub.GetMatchGroupName(matchId))
                        .SendAsync("ForecastsChanged", matchId, stoppingToken);
                }

                // Only advance after successful notification; failures retry next time.
                previous = current;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Could not check for forecast chart changes; retrying in five seconds.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    internal static IEnumerable<int> GetChangedMatchIds(
        IReadOnlyList<ForecastCountRecord> previous,
        IReadOnlyList<ForecastCountRecord> current)
    {
        var changes = previous.ToHashSet();
        changes.SymmetricExceptWith(current);
        return changes.Select(record => record.MatchId).Distinct();
    }
}
