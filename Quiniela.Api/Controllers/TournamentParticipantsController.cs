using Microsoft.AspNetCore.Mvc;
using Quiniela.Api.Contracts;
using Quiniela.Api.Services;

namespace Quiniela.Api.Controllers;

[ApiController]
[Route("api/tournament-participants")]
public sealed class TournamentParticipantsController(
    IEntityApiService<TournamentParticipantRequest, TournamentParticipantResponse> service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TournamentParticipantResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TournamentParticipantResponse>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        Ok(await service.GetAllAsync(pageNumber, pageSize, cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TournamentParticipantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TournamentParticipantResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var response = await service.GetByIdAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TournamentParticipantResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TournamentParticipantResponse>> Create(
        [FromBody] TournamentParticipantRequest request,
        CancellationToken cancellationToken)
    {
        var (id, response) = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TournamentParticipantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TournamentParticipantResponse>> Update(
        int id,
        [FromBody] TournamentParticipantRequest request,
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
