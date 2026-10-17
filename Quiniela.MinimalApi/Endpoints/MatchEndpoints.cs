using Quiniela.Api.Contracts;
using Quiniela.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Quiniela.MinimalApi.Endpoints;

internal static class MatchEndpoints
{
    public static void MapMatchEndpoints(this WebApplication app)
    {
        var group = app.MapCrud<MatchRequest, MatchResponse>(
            "matches",
            "Matches");

        group.MapPost("/", async Task<Created<MatchResponse>> (
            MatchRequest request,
            IEntityApiService<MatchRequest, MatchResponse> service,
            CancellationToken cancellationToken) =>
        {
            var (id, response) = await service.CreateAsync(request, cancellationToken);
            return TypedResults.Created($"/api/matches/{id}", response);
        })
        .AddEndpointFilter<DtoValidationFilter>()
        .WithName("CreateMatch")
        .WithSummary("Create a match")
        .WithDescription("Creates a match and returns it with its immediate related teams, tournament, and forecasts.")
        .Produces<MatchResponse>(StatusCodes.Status201Created)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", async Task<Results<Ok<MatchResponse>, NotFound>> (
            int id,
            MatchRequest request,
            IEntityApiService<MatchRequest, MatchResponse> service,
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
        .WithName("UpdateMatch")
        .WithSummary("Replace a match")
        .WithDescription("Replaces a match by ID and returns its immediate related records, or 404 when the ID does not exist.")
        .Produces<MatchResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);
    }
}
