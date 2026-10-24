using Microsoft.AspNetCore.Mvc.Rendering;
using Quiniela.Model;

namespace Quiniela.Mvc.Models;

public sealed class MatchListViewModel
{
    public IReadOnlyList<MatchListItemViewModel> Matches { get; init; } = [];
    public IReadOnlyList<SelectListItem> Tournaments { get; init; } = [];
    public IReadOnlyList<SelectListItem> Teams { get; init; } = [];
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public int? TournamentId { get; init; }
    public int? TeamId { get; init; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

public sealed record MatchListItemViewModel(
    int MatchId,
    string? TournamentName,
    MatchKind MatchKind,
    Sport Sport,
    string HomeTeamName,
    string AwayTeamName,
    DateTime PlayedAt,
    MatchStatus Status,
    int? HomeScore,
    int? AwayScore,
    string StatsJson,
    IReadOnlyList<ForecastListItemViewModel> Forecasts);

public sealed record ForecastListItemViewModel(
    int UserId,
    int PredictedHomeScore,
    int PredictedAwayScore,
    DateTime SubmittedAt);
