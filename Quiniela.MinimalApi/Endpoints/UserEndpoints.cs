using Quiniela.Api.Contracts;
using Quiniela.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Quiniela.MinimalApi.Endpoints;

internal static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapCrud<UserRequest, UserResponse>(
            "users",
            "Users");

        group.MapPost("/", async Task<Created<UserResponse>> (
            UserRequest request,
            IEntityApiService<UserRequest, UserResponse> service,
            CancellationToken cancellationToken) =>
        {
            var (id, response) = await service.CreateAsync(request, cancellationToken);
            return TypedResults.Created($"/api/users/{id}", response);
        })
        .AddEndpointFilter<DtoValidationFilter>()
        .WithName("CreateUser")
        .WithSummary("Create a user")
        .WithDescription("Creates a user and returns it with its immediate forecasts.")
        .Produces<UserResponse>(StatusCodes.Status201Created)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", async Task<Results<Ok<UserResponse>, NotFound>> (
            int id,
            UserRequest request,
            IEntityApiService<UserRequest, UserResponse> service,
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
        .WithName("UpdateUser")
        .WithSummary("Replace a user")
        .WithDescription("Replaces a user by ID and returns its immediate related forecasts, or 404 when the ID does not exist.")
        .Produces<UserResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);
    }
}
