using Quiniela.Api.Contracts;
using Quiniela.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Quiniela.MinimalApi.Endpoints;

internal static class ForecastEndpoints
{
    public static void MapForecastEndpoints(this WebApplication app)
    {
        var group = app.MapCrud<ForecastRequest, ForecastResponse>(
            "forecasts",
            "Forecasts");

        group.MapPost("/", async Task<Created<ForecastResponse>> (
            ForecastRequest request,
            IEntityApiService<ForecastRequest, ForecastResponse> service,
            CancellationToken cancellationToken) =>
        {
            var (id, response) = await service.CreateAsync(request, cancellationToken);
            return TypedResults.Created($"/api/forecasts/{id}", response);
        })
        .AddEndpointFilter<DtoValidationFilter>()
        .WithName("CreateForecast")
        .WithSummary("Create a forecast")
        .WithDescription("Creates a forecast and returns it with the referenced match and user.")
        .Produces<ForecastResponse>(StatusCodes.Status201Created)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", async Task<Results<Ok<ForecastResponse>, NotFound>> (
            int id,
            ForecastRequest request,
            IEntityApiService<ForecastRequest, ForecastResponse> service,
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
        .WithName("UpdateForecast")
        .WithSummary("Replace a forecast")
        .WithDescription("Replaces a forecast by ID and returns its referenced match and user, or 404 when the ID does not exist.")
        .Produces<ForecastResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);
    }
}
