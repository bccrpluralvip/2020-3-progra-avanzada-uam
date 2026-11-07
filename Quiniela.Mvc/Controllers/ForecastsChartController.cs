using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Quiniela.Api.Contracts;
using Quiniela.Model;
using Quiniela.Mvc.Models;
using Quiniela.Mvc.Services;

namespace Quiniela.Mvc.Controllers;

[Route("ForecastsChart")]
public sealed class ForecastsChartController(
    QuinielaDbContext context,
    ForecastsChartApiClient forecastsApi) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        Sport? sport,
        int? teamId,
        int? matchId,
        DateOnly? date,
        CancellationToken cancellationToken)
    {
        var selectedDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var teams = sport.HasValue
            ? await context.Teams.AsNoTracking()
                .Where(team => team.Sport == sport.Value)
                .OrderBy(team => team.Name)
                .Select(team => new { team.TeamId, team.Name })
                .ToListAsync(cancellationToken)
            : [];

        if (!teams.Any(team => team.TeamId == teamId))
        {
            teamId = null;
        }

        var matches = teamId.HasValue && sport.HasValue
            ? await context.Matches.AsNoTracking()
                .Where(match =>
                    match.Sport == sport.Value &&
                    (match.HomeTeamId == teamId.Value || match.AwayTeamId == teamId.Value))
                .OrderByDescending(match => match.PlayedAt)
                .ThenByDescending(match => match.MatchId)
                .Select(match => new
                {
                    match.MatchId,
                    match.PlayedAt,
                    HomeTeamName = match.HomeTeam.Name,
                    AwayTeamName = match.AwayTeam.Name
                })
                .ToListAsync(cancellationToken)
            : [];

        if (!matches.Any(match => match.MatchId == matchId))
        {
            matchId = null;
        }

        var dailyForecasts = matchId.HasValue
            ? await GetDailyForecastsAsync(matchId.Value, selectedDate, cancellationToken)
            : [];

        var model = new ForecastsChartViewModel
        {
            Sports = Enum.GetValues<Sport>()
                .Select(value => new SelectListItem(value.ToString(), value.ToString(), value == sport))
                .ToArray(),
            Teams = teams
                .Select(team => new SelectListItem(team.Name, team.TeamId.ToString(CultureInfo.InvariantCulture), team.TeamId == teamId))
                .ToArray(),
            Matches = matches
                .Select(match => new SelectListItem(
                    $"{match.HomeTeamName} vs {match.AwayTeamName} — {match.PlayedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)} UTC",
                    match.MatchId.ToString(CultureInfo.InvariantCulture),
                    match.MatchId == matchId))
                .ToArray(),
            SelectedSport = sport,
            SelectedTeamId = teamId,
            SelectedMatchId = matchId,
            SelectedDate = selectedDate,
            HubUrl = forecastsApi.HubUrl,
            DailyForecasts = dailyForecasts
        };

        return View(model);
    }

    [HttpGet("teams")]
    public async Task<ActionResult<IReadOnlyList<FilterOption>>> GetTeams(
        Sport sport,
        CancellationToken cancellationToken)
    {
        var teams = await context.Teams.AsNoTracking()
            .Where(team => team.Sport == sport)
            .OrderBy(team => team.Name)
            .Select(team => new { team.TeamId, team.Name })
            .ToListAsync(cancellationToken);

        return Ok(teams.Select(team => new FilterOption(
            team.TeamId.ToString(CultureInfo.InvariantCulture),
            team.Name))
            .ToArray());
    }

    [HttpGet("matches")]
    public async Task<ActionResult<IReadOnlyList<FilterOption>>> GetMatches(
        Sport sport,
        int teamId,
        CancellationToken cancellationToken)
    {
        var matches = await context.Matches.AsNoTracking()
            .Where(match =>
                match.Sport == sport &&
                (match.HomeTeamId == teamId || match.AwayTeamId == teamId))
            .OrderByDescending(match => match.PlayedAt)
            .ThenByDescending(match => match.MatchId)
            .Select(match => new
            {
                match.MatchId,
                match.PlayedAt,
                HomeTeamName = match.HomeTeam.Name,
                AwayTeamName = match.AwayTeam.Name
            })
            .ToListAsync(cancellationToken);

        return Ok(matches.Select(match => new FilterOption(
            match.MatchId.ToString(CultureInfo.InvariantCulture),
            $"{match.HomeTeamName} vs {match.AwayTeamName} — {match.PlayedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)} UTC"))
            .ToArray());
    }

    [HttpGet("data")]
    public async Task<ActionResult<IReadOnlyList<ForecastCountResponse>>> GetChartData(
        int matchId,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        if (matchId < 1)
        {
            return BadRequest("Match id must be greater than zero.");
        }

        return Ok(await GetDailyForecastsAsync(matchId, date, cancellationToken));
    }

    private async Task<IReadOnlyList<ForecastCountResponse>> GetDailyForecastsAsync(
        int matchId,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var allForecasts = await forecastsApi.GetByMatchAsync(matchId, cancellationToken);
        var dateText = date.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
        return allForecasts.Where(forecast => forecast.Date == dateText).ToArray();
    }

    public sealed record FilterOption(string Value, string Text);
}
