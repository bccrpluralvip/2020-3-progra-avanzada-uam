using System.Text.Json;
using Quiniela.BusinessLogic.Queries;
using Quiniela.Model;

namespace Quiniela.Api.Contracts;

public static class ApiMappings
{
    public static Forecast ToEntity(ForecastRequest request) => new()
    {
        UserId = request.UserId,
        MatchId = request.MatchId,
        PredictedHomeScore = request.PredictedHomeScore,
        PredictedAwayScore = request.PredictedAwayScore,
        SubmittedAt = request.SubmittedAt.UtcDateTime
    };

    public static void Apply(Forecast entity, ForecastRequest request)
    {
        entity.UserId = request.UserId;
        entity.MatchId = request.MatchId;
        entity.PredictedHomeScore = request.PredictedHomeScore;
        entity.PredictedAwayScore = request.PredictedAwayScore;
        entity.SubmittedAt = request.SubmittedAt.UtcDateTime;
    }

    public static ForecastResponse ToResponse(Forecast entity) => new(
        entity.ForecastId,
        entity.PredictedHomeScore,
        entity.PredictedAwayScore,
        ToOffset(entity.SubmittedAt),
        ToReference(entity.Match),
        ToReference(entity.User));

    public static Match ToEntity(MatchRequest request) => new()
    {
        TournamentId = request.TournamentId,
        MatchKind = request.MatchKind,
        Sport = request.Sport,
        HomeTeamId = request.HomeTeamId,
        AwayTeamId = request.AwayTeamId,
        PlayedAt = request.PlayedAt.UtcDateTime,
        Status = request.Status,
        HomeScore = request.HomeScore,
        AwayScore = request.AwayScore,
        StatsJson = GetStatsJson(request.StatsJson)
    };

    public static void Apply(Match entity, MatchRequest request)
    {
        entity.TournamentId = request.TournamentId;
        entity.MatchKind = request.MatchKind;
        entity.Sport = request.Sport;
        entity.HomeTeamId = request.HomeTeamId;
        entity.AwayTeamId = request.AwayTeamId;
        entity.PlayedAt = request.PlayedAt.UtcDateTime;
        entity.Status = request.Status;
        entity.HomeScore = request.HomeScore;
        entity.AwayScore = request.AwayScore;
        entity.StatsJson = GetStatsJson(request.StatsJson);
    }

    public static MatchResponse ToResponse(Match entity) => new(
        entity.MatchId,
        entity.TournamentId,
        entity.MatchKind,
        entity.Sport,
        entity.HomeTeamId,
        entity.AwayTeamId,
        ToOffset(entity.PlayedAt),
        entity.Status,
        entity.HomeScore,
        entity.AwayScore,
        JsonDocument.Parse(entity.StatsJson).RootElement.Clone(),
        ToReference(entity.HomeTeam),
        ToReference(entity.AwayTeam),
        entity.Tournament is null ? null : ToReference(entity.Tournament),
        entity.Forecasts.Select(ToReference).ToArray());

    public static Team ToEntity(TeamRequest request) => new()
    {
        Name = request.Name,
        ShortName = request.ShortName,
        Sport = request.Sport
    };

    public static void Apply(Team entity, TeamRequest request)
    {
        entity.Name = request.Name;
        entity.ShortName = request.ShortName;
        entity.Sport = request.Sport;
    }

    public static TeamResponse ToResponse(Team entity) => new(
        entity.TeamId,
        entity.Name,
        entity.ShortName,
        entity.Sport,
        entity.MatchAwayTeams.Select(ToReference).ToArray(),
        entity.MatchHomeTeams.Select(ToReference).ToArray(),
        entity.TournamentParticipants.Select(ToReference).ToArray());

    public static Tournament ToEntity(TournamentRequest request) => new()
    {
        Name = request.Name,
        Season = request.Season,
        Sport = request.Sport
    };

    public static void Apply(Tournament entity, TournamentRequest request)
    {
        entity.Name = request.Name;
        entity.Season = request.Season;
        entity.Sport = request.Sport;
    }

    public static TournamentResponse ToResponse(Tournament entity) => new(
        entity.TournamentId,
        entity.Name,
        entity.Season,
        entity.Sport,
        entity.Matches.Select(ToReference).ToArray(),
        entity.TournamentParticipants.Select(ToReference).ToArray());

    public static TournamentParticipant ToEntity(TournamentParticipantRequest request) => new()
    {
        TournamentId = request.TournamentId,
        TeamId = request.TeamId
    };

    public static void Apply(TournamentParticipant entity, TournamentParticipantRequest request)
    {
        entity.TournamentId = request.TournamentId;
        entity.TeamId = request.TeamId;
    }

    public static TournamentParticipantResponse ToResponse(TournamentParticipant entity) => new(
        entity.ParticipantId,
        entity.TournamentId,
        entity.TeamId,
        ToReference(entity.Team),
        ToReference(entity.Tournament));

    public static User ToEntity(UserRequest request) => new()
    {
        Username = request.Username,
        Email = request.Email
    };

    public static void Apply(User entity, UserRequest request)
    {
        entity.Username = request.Username;
        entity.Email = request.Email;
    }

    public static UserResponse ToResponse(User entity) => new(
        entity.UserId,
        entity.Username,
        entity.Email,
        entity.Forecasts.Select(ToReference).ToArray());

    public static MatchQueryResponse ToResponse(MatchSummaryRecord record) => new(
        record.TournamentName,
        record.TournamentSeason,
        record.TournamentSportName,
        record.MatchId,
        record.MatchKind,
        record.HomeTeamName,
        record.AwayTeamName,
        ToOffset(record.DatePlayed),
        record.StatusName,
        record.HomeScore,
        record.AwayScore);

    private static MatchReferenceResponse ToReference(Match entity) => new(
        entity.MatchId,
        entity.TournamentId,
        entity.MatchKind,
        entity.Sport,
        entity.HomeTeamId,
        entity.AwayTeamId,
        ToOffset(entity.PlayedAt),
        entity.Status,
        entity.HomeScore,
        entity.AwayScore);

    private static ForecastReferenceResponse ToReference(Forecast entity) => new(
        entity.ForecastId,
        entity.UserId,
        entity.MatchId,
        entity.PredictedHomeScore,
        entity.PredictedAwayScore,
        ToOffset(entity.SubmittedAt));

    private static TournamentParticipantReferenceResponse ToReference(TournamentParticipant entity) => new(
        entity.ParticipantId,
        entity.TournamentId,
        entity.TeamId);

    private static TeamReferenceResponse ToReference(Team entity) => new(
        entity.TeamId,
        entity.Name,
        entity.ShortName,
        entity.Sport);

    private static TournamentReferenceResponse ToReference(Tournament entity) => new(
        entity.TournamentId,
        entity.Name,
        entity.Season,
        entity.Sport);

    private static UserReferenceResponse ToReference(User entity) => new(
        entity.UserId,
        entity.Username,
        entity.Email);

    private static string GetStatsJson(JsonElement value) =>
        value.ValueKind == JsonValueKind.Undefined ? "{}" : value.GetRawText();

    private static DateTimeOffset ToOffset(DateTime value) =>
        value.Kind == DateTimeKind.Unspecified
            ? new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc))
            : new DateTimeOffset(value);
}
