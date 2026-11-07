using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Quiniela.Api.Contracts;
using Quiniela.Api.Hubs;
using Quiniela.Api.Services;

namespace Quiniela.Api.Controllers;

[ApiController]
[Route("api/forecasts")]
public sealed class ForecastsController(
    IEntityApiService<ForecastRequest, ForecastResponse> service,
    IHubContext<ForecastsChartHub> hubContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ForecastResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ForecastResponse>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        Ok(await service.GetAllAsync(pageNumber, pageSize, cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ForecastResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ForecastResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await service.GetByIdAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ForecastResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ForecastResponse>> Create(
        [FromBody] ForecastRequest request,
        CancellationToken cancellationToken)
    {
        var (id, response) = await service.CreateAsync(request, cancellationToken);
        await NotifyMatchChangedAsync(response.Match.MatchId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ForecastResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ForecastResponse>> Update(
        int id,
        [FromBody] ForecastRequest request,
        CancellationToken cancellationToken)
    {
        var previous = await service.GetByIdAsync(id, cancellationToken);
        var response = await service.UpdateAsync(id, request, cancellationToken);
        if (response is null)
        {
            return NotFound();
        }

        if (previous is not null && previous.Match.MatchId != response.Match.MatchId)
        {
            await NotifyMatchChangedAsync(previous.Match.MatchId, cancellationToken);
        }

        await NotifyMatchChangedAsync(response.Match.MatchId, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var forecast = await service.GetByIdAsync(id, cancellationToken);
        var deleted = await service.DeleteAsync(id, cancellationToken);
        if (deleted && forecast is not null)
        {
            await NotifyMatchChangedAsync(forecast.Match.MatchId, cancellationToken);
        }

        return deleted ? NoContent() : NotFound();
    }

    private Task NotifyMatchChangedAsync(int matchId, CancellationToken cancellationToken) =>
        hubContext.Clients
            .Group(ForecastsChartHub.GetMatchGroupName(matchId))
            .SendAsync("ForecastsChanged", matchId, cancellationToken);
}
