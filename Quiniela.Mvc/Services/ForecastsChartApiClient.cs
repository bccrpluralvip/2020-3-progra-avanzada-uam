using System.Net.Http.Json;
using Quiniela.Api.Contracts;

namespace Quiniela.Mvc.Services;

public sealed class ForecastsChartApiClient(HttpClient httpClient)
{
    public string HubUrl => new Uri(httpClient.BaseAddress
        ?? throw new InvalidOperationException("The forecasts API base address is not configured."),
        "hubs/forecasts").ToString();

    public async Task<IReadOnlyList<ForecastCountResponse>> GetByMatchAsync(
        int matchId,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            $"api/queries/forecasts/by-match/{matchId}",
            cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<ForecastCountResponse>>(cancellationToken)
            ?? throw new InvalidOperationException("The forecasts API returned an empty response.");
    }
}
