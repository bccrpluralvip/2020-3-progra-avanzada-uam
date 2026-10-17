using Quiniela.Api.Contracts;
using Quiniela.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Quiniela.MinimalApi.Endpoints;

internal static class TeamEndpoints
{
    public static void MapTeamEndpoints(this WebApplication app)
    {
        var group = app.MapCrud<TeamRequest, TeamResponse>(
            "teams",
            "Teams");

        group.MapPost("/", async Task<Created<TeamResponse>> (
            TeamRequest request,
            IEntityApiService<TeamRequest, TeamResponse> service,
            CancellationToken cancellationToken) =>
        {
            var (id, response) = await service.CreateAsync(request, cancellationToken);
            return TypedResults.Created($"/api/teams/{id}", response);
        })
        .AddEndpointFilter<DtoValidationFilter>()
        .WithName("CreateTeam")
        .WithSummary("Create a team")
        .WithDescription("Creates a team and returns its immediate related matches and tournament participants.")
        .Produces<TeamResponse>(StatusCodes.Status201Created)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", async Task<Results<Ok<TeamResponse>, NotFound>> (
            int id,
            TeamRequest request,
            IEntityApiService<TeamRequest, TeamResponse> service,
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
        .WithName("UpdateTeam")
        .WithSummary("Replace a team")
        .WithDescription("Replaces a team by ID and returns its immediate related records, or 404 when the ID does not exist.")
        .Produces<TeamResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);
    }
}
