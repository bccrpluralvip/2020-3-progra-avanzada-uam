using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Quiniela.Model;

namespace Quiniela.Api.Contracts;

/// <summary>Payload for creating or replacing a forecast.</summary>
public sealed record ForecastRequest
{
    [Range(1, int.MaxValue)]
    public int UserId { get; init; }

    [Range(1, int.MaxValue)]
    public int MatchId { get; init; }

    [Range(0, int.MaxValue)]
    public int PredictedHomeScore { get; init; }

    [Range(0, int.MaxValue)]
    public int PredictedAwayScore { get; init; }

    public required DateTimeOffset SubmittedAt { get; init; }
}

/// <summary>Forecast data returned by the API with its immediate related records.</summary>
public sealed record ForecastResponse(
    int ForecastId,
    int PredictedHomeScore,
    int PredictedAwayScore,
    DateTimeOffset SubmittedAt,
    MatchReferenceResponse Match,
    UserReferenceResponse User);

/// <summary>Payload for creating or replacing a match.</summary>
public sealed record MatchRequest
{
    public int? TournamentId { get; init; }
    public required MatchKind MatchKind { get; init; }
    public required Sport Sport { get; init; }

    [Range(1, int.MaxValue)]
    public int HomeTeamId { get; init; }

    [Range(1, int.MaxValue)]
    public int AwayTeamId { get; init; }

    public required DateTimeOffset PlayedAt { get; init; }
    public required MatchStatus Status { get; init; }
    public int? HomeScore { get; init; }
    public int? AwayScore { get; init; }
    public JsonElement StatsJson { get; init; }
}

/// <summary>Match data returned by the API with its immediate related records.</summary>
public sealed record MatchResponse(
    int MatchId,
    int? TournamentId,
    MatchKind MatchKind,
    Sport Sport,
    int HomeTeamId,
    int AwayTeamId,
    DateTimeOffset PlayedAt,
    MatchStatus Status,
    int? HomeScore,
    int? AwayScore,
    JsonElement StatsJson,
    TeamReferenceResponse HomeTeam,
    TeamReferenceResponse AwayTeam,
    TournamentReferenceResponse? Tournament,
    IReadOnlyList<ForecastReferenceResponse> Forecasts);

/// <summary>A filtered, paged collection of matches.</summary>
public sealed record MatchSearchResponse(
    int PageNumber,
    int PageSize,
    int TotalCount,
    IReadOnlyList<MatchResponse> Matches);

/// <summary>Payload for creating or replacing a team.</summary>
public sealed record TeamRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    [Required, MaxLength(20)]
    public required string ShortName { get; init; }

    public required Sport Sport { get; init; }
}

/// <summary>Team data returned by the API with immediate related records.</summary>
public sealed record TeamResponse(
    int TeamId,
    string Name,
    string ShortName,
    Sport Sport,
    IReadOnlyList<MatchReferenceResponse> MatchAwayTeams,
    IReadOnlyList<MatchReferenceResponse> MatchHomeTeams,
    IReadOnlyList<TournamentParticipantReferenceResponse> TournamentParticipants);

/// <summary>Payload for creating or replacing a tournament.</summary>
public sealed record TournamentRequest
{
    [Required, MaxLength(150)]
    public required string Name { get; init; }

    [Required, MaxLength(50)]
    public required string Season { get; init; }

    public required Sport Sport { get; init; }
}

/// <summary>Tournament data returned by the API with immediate related records.</summary>
public sealed record TournamentResponse(
    int TournamentId,
    string Name,
    string Season,
    Sport Sport,
    IReadOnlyList<MatchReferenceResponse> Matches,
    IReadOnlyList<TournamentParticipantReferenceResponse> TournamentParticipants);

/// <summary>Payload for creating or replacing a tournament participant.</summary>
public sealed record TournamentParticipantRequest
{
    [Range(1, int.MaxValue)]
    public int TournamentId { get; init; }

    [Range(1, int.MaxValue)]
    public int TeamId { get; init; }
}

/// <summary>Tournament participant data returned with its immediate related records.</summary>
public sealed record TournamentParticipantResponse(
    int ParticipantId,
    int TournamentId,
    int TeamId,
    TeamReferenceResponse Team,
    TournamentReferenceResponse Tournament);

/// <summary>Payload for creating or replacing a user.</summary>
public sealed record UserRequest
{
    [Required, MaxLength(50)]
    public required string Username { get; init; }

    [Required, EmailAddress, MaxLength(255)]
    public required string Email { get; init; }
}

/// <summary>User data returned by the API with immediate related records.</summary>
public sealed record UserResponse(
    int UserId,
    string Username,
    string Email,
    IReadOnlyList<ForecastReferenceResponse> Forecasts);

/// <summary>Basic team information included in related resources.</summary>
public sealed record TeamReferenceResponse(int TeamId, string Name, string ShortName, Sport Sport);

/// <summary>Basic tournament information included in related resources.</summary>
public sealed record TournamentReferenceResponse(int TournamentId, string Name, string Season, Sport Sport);

/// <summary>Basic user information included in related resources.</summary>
public sealed record UserReferenceResponse(int UserId, string Username, string Email);

/// <summary>Basic match information included in related resources.</summary>
public sealed record MatchReferenceResponse(
    int MatchId,
    int? TournamentId,
    MatchKind MatchKind,
    Sport Sport,
    int HomeTeamId,
    int AwayTeamId,
    DateTimeOffset PlayedAt,
    MatchStatus Status,
    int? HomeScore,
    int? AwayScore);

/// <summary>Basic forecast information included in related resources.</summary>
public sealed record ForecastReferenceResponse(
    int ForecastId,
    int UserId,
    int MatchId,
    int PredictedHomeScore,
    int PredictedAwayScore,
    DateTimeOffset SubmittedAt);

/// <summary>Basic tournament participant information included in related resources.</summary>
public sealed record TournamentParticipantReferenceResponse(
    int ParticipantId,
    int TournamentId,
    int TeamId);

/// <summary>A paged match query result with tournament and team names.</summary>
public sealed record MatchQueryResponse(
    string? TournamentName,
    string? TournamentSeason,
    string? TournamentSportName,
    int MatchId,
    string MatchKind,
    string HomeTeamName,
    string AwayTeamName,
    DateTimeOffset DatePlayed,
    string StatusName,
    int? HomeScore,
    int? AwayScore);
