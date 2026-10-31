using Microsoft.AspNetCore.Mvc;
using Quiniela.Api.Contracts;
using Quiniela.Api.Services;
using Quiniela.BusinessLogic.Crud;

namespace Quiniela.Api.Controllers;

[ApiController]
[Route("api/matches")]
public sealed class MatchesController(
    IEntityApiService<MatchRequest, MatchResponse> service,
    MatchBusinessLogic matches) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MatchResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MatchResponse>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        Ok(await service.GetAllAsync(pageNumber, pageSize, cancellationToken));

    [HttpGet("search")]
    [ProducesResponseType(typeof(MatchSearchResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MatchSearchResponse>> Search(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? tournamentId = null,
        [FromQuery] int? teamId = null,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await matches.SearchAsync(
            tournamentId,
            teamId,
            pageNumber,
            pageSize,
            cancellationToken);

        return Ok(new MatchSearchResponse(
            pageNumber,
            pageSize,
            totalCount,
            items.Select(ApiMappings.ToResponse).ToArray()));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MatchResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await service.GetByIdAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(MatchResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MatchResponse>> Create(
        [FromBody] MatchRequest request,
        CancellationToken cancellationToken)
    {
        var (id, response) = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(MatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MatchResponse>> Update(
        int id,
        [FromBody] MatchRequest request,
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
