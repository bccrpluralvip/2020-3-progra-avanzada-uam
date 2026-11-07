using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Quiniela.Api.Contracts;
using Quiniela.BusinessLogic.Queries;

namespace Quiniela.Api.Controllers;

[ApiController]
[Route("api/queries/forecasts")]
public sealed class ForecastQueriesController(ForecastQueryBusinessLogic queries) : ControllerBase
{
    [HttpGet("by-match/{matchId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<ForecastCountResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ForecastCountResponse>>> GetByMatch(
        int matchId,
        CancellationToken cancellationToken)
    {
        var records = await queries.GetForecastCountsByMatchAsync(matchId, cancellationToken);
        return Ok(records.Select(record => new ForecastCountResponse(
            record.MatchId,
            record.Date.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture),
            record.Hour,
            record.ResultType,
            record.Count)).ToArray());
    }
}
