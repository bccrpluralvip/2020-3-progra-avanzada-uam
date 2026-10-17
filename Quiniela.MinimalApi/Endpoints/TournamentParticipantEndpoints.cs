using Quiniela.Api.Contracts;
using Quiniela.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Quiniela.MinimalApi.Endpoints;

internal static class TournamentParticipantEndpoints
{
    public static void MapTournamentParticipantEndpoints(this WebApplication app)
    {
        var group = app.MapCrud<TournamentParticipantRequest, TournamentParticipantResponse>(
            "tournament-participants",
            "TournamentParticipants");

        group.MapPost("/", async Task<Created<TournamentParticipantResponse>> (
            TournamentParticipantRequest request,
            IEntityApiService<TournamentParticipantRequest, TournamentParticipantResponse> service,
            CancellationToken cancellationToken) =>
        {
            var (id, response) = await service.CreateAsync(request, cancellationToken);
            return TypedResults.Created($"/api/tournament-participants/{id}", response);
        })
        .AddEndpointFilter<DtoValidationFilter>()
        .WithName("CreateTournamentParticipant")
        .WithSummary("Add a team to a tournament")
        .WithDescription("Creates a tournament participant and returns its related team and tournament.")
        .Produces<TournamentParticipantResponse>(StatusCodes.Status201Created)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", async Task<Results<Ok<TournamentParticipantResponse>, NotFound>> (
            int id,
            TournamentParticipantRequest request,
            IEntityApiService<TournamentParticipantRequest, TournamentParticipantResponse> service,
            CancellationToken cancellationToken) =>
        {
            var response = await service.UpdateAsync(id, request, cancellationToken);
            if (response is null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(response);
        })
        .AddEndpointFilter<DtoValidationFilter>()
        .WithName("UpdateTournamentParticipant")
        .WithSummary("Replace a tournament participant")
        .WithDescription("Replaces a tournament participant by ID and returns its related team and tournament, or 404 when it does not exist.")
        .Produces<TournamentParticipantResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);
    }
}
