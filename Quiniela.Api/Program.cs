using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Quiniela.Api.Contracts;
using Quiniela.Api.Hubs;
using Quiniela.Api.Middleware;
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
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
var allowedMvcOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? throw new InvalidOperationException("Configuration section 'Cors:AllowedOrigins' is required.");
builder.Services.AddCors(options =>
    options.AddPolicy("MvcClient", policy =>
        policy.WithOrigins(allowedMvcOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));
builder.Services.AddSignalR();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddScoped<MatchQueryBusinessLogic>();
builder.Services.AddScoped<ForecastQueryBusinessLogic>();
builder.Services.AddHostedService<ForecastsChartChangeMonitor>();
builder.Services.AddEntityApiService<Forecast, ForecastRequest, ForecastResponse, ForecastBusinessLogic>(
    forecast => forecast.ForecastId,
    ApiMappings.ToResponse,
    ApiMappings.ToEntity,
    ApiMappings.Apply);
builder.Services.AddEntityApiService<Match, MatchRequest, MatchResponse, MatchBusinessLogic>(
    match => match.MatchId,
    ApiMappings.ToResponse,
    ApiMappings.ToEntity,
    ApiMappings.Apply);
builder.Services.AddEntityApiService<Team, TeamRequest, TeamResponse, TeamBusinessLogic>(
    team => team.TeamId,
    ApiMappings.ToResponse,
    ApiMappings.ToEntity,
    ApiMappings.Apply);
builder.Services.AddEntityApiService<Tournament, TournamentRequest, TournamentResponse, TournamentBusinessLogic>(
    tournament => tournament.TournamentId,
    ApiMappings.ToResponse,
    ApiMappings.ToEntity,
    ApiMappings.Apply);
builder.Services.AddEntityApiService<TournamentParticipant, TournamentParticipantRequest, TournamentParticipantResponse, TournamentParticipantBusinessLogic>(
    participant => participant.ParticipantId,
    ApiMappings.ToResponse,
    ApiMappings.ToEntity,
    ApiMappings.Apply);
builder.Services.AddEntityApiService<User, UserRequest, UserResponse, UserBusinessLogic>(
    user => user.UserId,
    ApiMappings.ToResponse,
    ApiMappings.ToEntity,
    ApiMappings.Apply);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors("MvcClient");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.MapHub<ForecastsChartHub>("/hubs/forecasts").RequireCors("MvcClient");

app.Run();
