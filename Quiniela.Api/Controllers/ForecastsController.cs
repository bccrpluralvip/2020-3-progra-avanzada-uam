using Microsoft.AspNetCore.Mvc;
using Quiniela.Api.Contracts;
using Quiniela.Api.Services;

namespace Quiniela.Api.Controllers;

[ApiController]
[Route("api/forecasts")]
public sealed class ForecastsController(
    IEntityApiService<ForecastRequest, ForecastResponse> service) : ControllerBase
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
        var response = await service.UpdateAsync(id, request, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
