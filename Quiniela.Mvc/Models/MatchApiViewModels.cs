namespace Quiniela.Mvc.Models;

public sealed record MatchApiDetailsViewModel(
    int MatchId,
    string? TournamentName,
    string MatchKind,
    string Sport,
    string HomeTeamName,
    string AwayTeamName,
    DateTimeOffset PlayedAt,
    string Status,
    int? HomeScore,
    int? AwayScore,
    IReadOnlyList<ForecastApiViewModel> Forecasts);

public sealed record ForecastApiViewModel(
    int UserId,
    int PredictedHomeScore,
    int PredictedAwayScore,
    DateTimeOffset SubmittedAt);

public sealed record MatchApiDeleteViewModel(
    int MatchId,
    string? TournamentName,
    string HomeTeamName,
    string AwayTeamName,
    DateTimeOffset PlayedAt,
    string Status);
