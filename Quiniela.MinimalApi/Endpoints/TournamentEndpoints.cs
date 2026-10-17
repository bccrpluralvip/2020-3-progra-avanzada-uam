using Quiniela.Api.Contracts;
using Quiniela.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Quiniela.MinimalApi.Endpoints;

internal static class TournamentEndpoints
{
    public static void MapTournamentEndpoints(this WebApplication app)
    {
        var group = app.MapCrud<TournamentRequest, TournamentResponse>(
            "tournaments",
            "Tournaments");

        group.MapPost("/", async Task<Created<TournamentResponse>> (
            TournamentRequest request,
            IEntityApiService<TournamentRequest, TournamentResponse> service,
            CancellationToken cancellationToken) =>
        {
            var (id, response) = await service.CreateAsync(request, cancellationToken);
            return TypedResults.Created($"/api/tournaments/{id}", response);
        })
        .AddEndpointFilter<DtoValidationFilter>()
        .WithName("CreateTournament")
        .WithSummary("Create a tournament")
        .WithDescription("Creates a tournament and returns its immediate related matches and participants.")
        .Produces<TournamentResponse>(StatusCodes.Status201Created)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", async Task<Results<Ok<TournamentResponse>, NotFound>> (
            int id,
            TournamentRequest request,
            IEntityApiService<TournamentRequest, TournamentResponse> service,
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
        .WithName("UpdateTournament")
        .WithSummary("Replace a tournament")
        .WithDescription("Replaces a tournament by ID and returns its immediate related records, or 404 when the ID does not exist.")
        .Produces<TournamentResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);
    }
}
