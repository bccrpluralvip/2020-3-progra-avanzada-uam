using Microsoft.AspNetCore.SignalR;

namespace Quiniela.Api.Hubs;

public sealed class ForecastsChartHub : Hub
{
    public static string GetMatchGroupName(int matchId) => $"match:{matchId}";

    public Task SubscribeToMatch(int matchId)
    {
        if (matchId < 1)
        {
            throw new HubException("Match id must be greater than zero.");
        }

        return Groups.AddToGroupAsync(Context.ConnectionId, GetMatchGroupName(matchId));
    }

    public Task UnsubscribeFromMatch(int matchId)
    {
        if (matchId < 1)
        {
            throw new HubException("Match id must be greater than zero.");
        }

        return Groups.RemoveFromGroupAsync(Context.ConnectionId, GetMatchGroupName(matchId));
    }
}
