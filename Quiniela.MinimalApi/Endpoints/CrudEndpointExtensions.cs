using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Quiniela.Api.Services;

namespace Quiniela.MinimalApi.Endpoints;

internal static class CrudEndpointExtensions
{
    public static RouteGroupBuilder MapCrud<TRequest, TResponse>(
        this WebApplication app,
        string route,
        string resourceName)
        where TRequest : notnull
    {
        var group = app.MapGroup($"/api/{route}")
            .WithTags(resourceName);

        group.MapGet("/", async Task<IReadOnlyList<TResponse>> (
            IEntityApiService<TRequest, TResponse> service,
            CancellationToken cancellationToken,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10) =>
            await service.GetAllAsync(pageNumber, pageSize, cancellationToken))
        .WithName($"Get{resourceName}")
        .WithSummary($"Get a page of {resourceName.ToLowerInvariant()}")
        .WithDescription($"Returns a page of {resourceName.ToLowerInvariant()}, including their immediate related records.")
        .Produces<IReadOnlyList<TResponse>>();

        group.MapGet("/{id:int}", async Task<IResult> (
            int id,
            IEntityApiService<TRequest, TResponse> service,
            CancellationToken cancellationToken) =>
        {
            var response = await service.GetByIdAsync(id, cancellationToken);
            if (response is null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(response);
        })
        .WithName($"Get{resourceName}ById")
        .WithSummary($"Get a {resourceName.ToLowerInvariant()} by ID")
        .WithDescription($"Returns the {resourceName.ToLowerInvariant()} and its immediate related records, or 404 when the ID does not exist.")
        .Produces<TResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (
            int id,
            IEntityApiService<TRequest, TResponse> service,
            CancellationToken cancellationToken) =>
        {
            var deleted = await service.DeleteAsync(id, cancellationToken);
            return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
        })
        .WithName($"Delete{resourceName}")
        .WithSummary($"Delete a {resourceName.ToLowerInvariant()}")
        .WithDescription($"Deletes a {resourceName.ToLowerInvariant()} by ID, or returns 404 when the ID does not exist.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
