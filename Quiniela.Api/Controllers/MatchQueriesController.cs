using Microsoft.AspNetCore.Mvc;
using Quiniela.Api.Contracts;
using Quiniela.BusinessLogic.Queries;

namespace Quiniela.Api.Controllers;

[ApiController]
[Route("api/queries/matches")]
public sealed class MatchQueriesController(MatchQueryBusinessLogic queries) : ControllerBase
{
    [HttpGet("by-tournament/{tournamentId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<MatchQueryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MatchQueryResponse>>> GetByTournament(
        int tournamentId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var matches = await queries.GetMatchesByTournamentAsync(
            tournamentId,
            pageNumber,
            pageSize,
            cancellationToken);
        return Ok(matches.Select(ApiMappings.ToResponse).ToArray());
    }

    [HttpGet("by-team/{teamId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<MatchQueryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MatchQueryResponse>>> GetByTeam(
        int teamId,
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var matches = await queries.GetMatchesByTeamAsync(
            teamId,
            from.UtcDateTime,
            to.UtcDateTime,
            pageNumber,
            pageSize,
            cancellationToken);
        return Ok(matches.Select(ApiMappings.ToResponse).ToArray());
    }
}
