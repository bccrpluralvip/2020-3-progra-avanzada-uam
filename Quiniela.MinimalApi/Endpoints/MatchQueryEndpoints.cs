using Quiniela.Api.Contracts;
using Quiniela.BusinessLogic.Queries;

namespace Quiniela.MinimalApi.Endpoints;

internal static class MatchQueryEndpoints
{
    public static void MapMatchQueryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/queries/matches")
            .WithTags("Match queries");

        group.MapGet("/by-tournament/{tournamentId:int}", async Task<List<MatchQueryResponse>> (
            int tournamentId,
            MatchQueryBusinessLogic queries,
            CancellationToken cancellationToken,
            int pageNumber = 1,
            int pageSize = 10) =>
        {
            var matches = await queries.GetMatchesByTournamentAsync(
                tournamentId,
                pageNumber,
                pageSize,
                cancellationToken);
            return matches.Select(ApiMappings.ToResponse).ToList();
        })
        .WithName("GetMatchesByTournament")
        .WithSummary("Get a page of matches in a tournament")
        .WithDescription("Returns matches in chronological order. Page number defaults to 1 and page size defaults to 10.")
        .Produces<List<MatchQueryResponse>>();

        group.MapGet("/by-team/{teamId:int}", async Task<List<MatchQueryResponse>> (
            int teamId,
            DateTimeOffset from,
            DateTimeOffset to,
            MatchQueryBusinessLogic queries,
            CancellationToken cancellationToken,
            int pageNumber = 1,
            int pageSize = 10) =>
        {
            var matches = await queries.GetMatchesByTeamAsync(
                teamId,
                from.UtcDateTime,
                to.UtcDateTime,
                pageNumber,
                pageSize,
                cancellationToken);
            return matches.Select(ApiMappings.ToResponse).ToList();
        })
        .WithName("GetMatchesByTeam")
        .WithSummary("Get a team's matches within a date range")
        .WithDescription("Returns a page of matches involving the team during the inclusive date interval, in chronological order. Page number defaults to 1 and page size defaults to 10.")
        .Produces<List<MatchQueryResponse>>();
    }
}
