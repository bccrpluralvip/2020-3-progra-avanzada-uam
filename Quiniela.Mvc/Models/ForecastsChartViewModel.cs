using Microsoft.AspNetCore.Mvc.Rendering;
using Quiniela.Api.Contracts;
using Quiniela.Model;

namespace Quiniela.Mvc.Models;

public sealed class ForecastsChartViewModel
{
    public required IReadOnlyList<SelectListItem> Sports { get; init; }
    public required IReadOnlyList<SelectListItem> Teams { get; init; }
    public required IReadOnlyList<SelectListItem> Matches { get; init; }
    public Sport? SelectedSport { get; init; }
    public int? SelectedTeamId { get; init; }
    public int? SelectedMatchId { get; init; }
    public required DateOnly SelectedDate { get; init; }
    public required string HubUrl { get; init; }
    public required IReadOnlyList<ForecastCountResponse> DailyForecasts { get; init; }
}
