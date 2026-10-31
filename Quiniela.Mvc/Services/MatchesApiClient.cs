using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Quiniela.Api.Contracts;

namespace Quiniela.Mvc.Services;

public sealed class MatchesApiClient(HttpClient httpClient)
{
    private const int OptionPageSize = 100;
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    public Task<MatchSearchResponse?> SearchAsync(
        int pageNumber,
        int pageSize,
        int? tournamentId,
        int? teamId,
        CancellationToken cancellationToken)
    {
        var requestUri = $"api/matches/search?pageNumber={pageNumber}&pageSize={pageSize}";
        if (tournamentId.HasValue)
        {
            requestUri += $"&tournamentId={tournamentId.Value}";
        }

        if (teamId.HasValue)
        {
            requestUri += $"&teamId={teamId.Value}";
        }

        return GetOptionalAsync<MatchSearchResponse>(requestUri, cancellationToken);
    }

    public Task<MatchResponse?> GetMatchAsync(int id, CancellationToken cancellationToken) =>
        GetOptionalAsync<MatchResponse>($"api/matches/{id}", cancellationToken);

    public Task<MatchResponse> CreateMatchAsync(
        MatchRequest request,
        CancellationToken cancellationToken) =>
        SendForResponseAsync<MatchResponse>(
            httpClient.PostAsJsonAsync("api/matches", request, JsonOptions, cancellationToken),
            cancellationToken);

    public Task<MatchResponse?> UpdateMatchAsync(
        int id,
        MatchRequest request,
        CancellationToken cancellationToken) =>
        SendOptionalForResponseAsync<MatchResponse>(
            httpClient.PutAsJsonAsync($"api/matches/{id}", request, JsonOptions, cancellationToken),
            cancellationToken);

    public async Task<bool> DeleteMatchAsync(int id, CancellationToken cancellationToken)
    {
        using var response = await httpClient.DeleteAsync($"api/matches/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();
        return true;
    }

    public Task<IReadOnlyList<TeamResponse>> GetTeamsAsync(CancellationToken cancellationToken) =>
        GetAllPagesAsync<TeamResponse>("api/teams", cancellationToken);

    public Task<IReadOnlyList<TournamentResponse>> GetTournamentsAsync(CancellationToken cancellationToken) =>
        GetAllPagesAsync<TournamentResponse>("api/tournaments", cancellationToken);

    private async Task<T?> GetOptionalAsync<T>(string requestUri, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException($"The API returned an empty response for '{requestUri}'.");
    }

    private async Task<T> SendForResponseAsync<T>(
        Task<HttpResponseMessage> responseTask,
        CancellationToken cancellationToken)
    {
        using var response = await responseTask;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("The API returned an empty response.");
    }

    private async Task<T?> SendOptionalForResponseAsync<T>(
        Task<HttpResponseMessage> responseTask,
        CancellationToken cancellationToken)
    {
        using var response = await responseTask;
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("The API returned an empty response.");
    }

    private async Task<IReadOnlyList<T>> GetAllPagesAsync<T>(
        string endpoint,
        CancellationToken cancellationToken)
    {
        var items = new List<T>();
        var pageNumber = 1;
        while (true)
        {
            var page = await GetOptionalAsync<List<T>>(
                $"{endpoint}?pageNumber={pageNumber}&pageSize={OptionPageSize}",
                cancellationToken);
            if (page is null)
            {
                throw new InvalidOperationException($"The API returned no data for '{endpoint}'.");
            }

            items.AddRange(page);
            if (page.Count < OptionPageSize)
            {
                return items;
            }

            pageNumber = checked(pageNumber + 1);
        }
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
        return options;
    }
}
