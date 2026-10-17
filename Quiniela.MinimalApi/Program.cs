using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Quiniela.Api.Contracts;
using Quiniela.MinimalApi.Endpoints;
using Quiniela.MinimalApi.Middleware;
using Quiniela.Api.Services;
using Quiniela.BusinessLogic.Crud;
using Quiniela.BusinessLogic.Queries;
using Quiniela.Model;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("MySecretPostgresConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'MySecretPostgresConnection' is not configured.");
}

builder.Services.AddDbContext<QuinielaDbContext>(
    options => options.UseQuinielaPostgres(connectionString));
builder.Services.AddEntityApiServices();
builder.Services.AddScoped<MatchQueryBusinessLogic>();
builder.Services.AddScoped<DtoValidationFilter>();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapForecastEndpoints();
app.MapMatchEndpoints();
app.MapTeamEndpoints();
app.MapTournamentEndpoints();
app.MapTournamentParticipantEndpoints();
app.MapUserEndpoints();
app.MapMatchQueryEndpoints();

app.Run();

internal static class ApiServiceRegistration
{
    public static IServiceCollection AddEntityApiServices(this IServiceCollection services)
    {
        services.AddEntityApiService<Forecast, ForecastRequest, ForecastResponse, ForecastBusinessLogic>(
            forecast => forecast.ForecastId,
            ApiMappings.ToResponse,
            ApiMappings.ToEntity,
            ApiMappings.Apply);
        services.AddEntityApiService<Match, MatchRequest, MatchResponse, MatchBusinessLogic>(
            match => match.MatchId,
            ApiMappings.ToResponse,
            ApiMappings.ToEntity,
            ApiMappings.Apply);
        services.AddEntityApiService<Team, TeamRequest, TeamResponse, TeamBusinessLogic>(
            team => team.TeamId,
            ApiMappings.ToResponse,
            ApiMappings.ToEntity,
            ApiMappings.Apply);
        services.AddEntityApiService<Tournament, TournamentRequest, TournamentResponse, TournamentBusinessLogic>(
            tournament => tournament.TournamentId,
            ApiMappings.ToResponse,
            ApiMappings.ToEntity,
            ApiMappings.Apply);
        services.AddEntityApiService<TournamentParticipant, TournamentParticipantRequest, TournamentParticipantResponse, TournamentParticipantBusinessLogic>(
            participant => participant.ParticipantId,
            ApiMappings.ToResponse,
            ApiMappings.ToEntity,
            ApiMappings.Apply);
        services.AddEntityApiService<User, UserRequest, UserResponse, UserBusinessLogic>(
            user => user.UserId,
            ApiMappings.ToResponse,
            ApiMappings.ToEntity,
            ApiMappings.Apply);

        return services;
    }
}
