using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Quiniela.Api.Contracts;
using Quiniela.Model;
using Quiniela.Mvc.Models;
using Quiniela.Mvc.Services;

namespace Quiniela.Mvc.Controllers;

public sealed class MatchesApiController(MatchesApiClient matchesApi) : Controller
{
    private const int DefaultPageSize = 10;
    private const int MaximumPageSize = 100;

    [HttpGet]
    public async Task<IActionResult> Index(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        int? tournamentId = null,
        int? teamId = null,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, MaximumPageSize);

        var result = await matchesApi.SearchAsync(
            pageNumber, pageSize, tournamentId, teamId, cancellationToken)
            ?? throw new InvalidOperationException("The matches API returned no search result.");

        var totalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize);
        if (totalPages > 0 && pageNumber > totalPages)
        {
            pageNumber = totalPages;
            result = await matchesApi.SearchAsync(
                pageNumber, pageSize, tournamentId, teamId, cancellationToken)
                ?? throw new InvalidOperationException("The matches API returned no search result.");
        }

        var model = new MatchListViewModel
        {
            Matches = result.Matches.Select(match => new MatchListItemViewModel(
                match.MatchId,
                match.Tournament?.Name,
                match.MatchKind,
                match.Sport,
                match.HomeTeam.Name,
                match.AwayTeam.Name,
                match.PlayedAt.UtcDateTime,
                match.Status,
                match.HomeScore,
                match.AwayScore,
                match.StatsJson.GetRawText(),
                match.Forecasts
                    .OrderBy(forecast => forecast.SubmittedAt)
                    .Select(forecast => new ForecastListItemViewModel(
                        forecast.UserId,
                        forecast.PredictedHomeScore,
                        forecast.PredictedAwayScore,
                        forecast.SubmittedAt.UtcDateTime))
                    .ToArray()))
                .ToArray(),
            Tournaments = await GetTournamentOptionsAsync(tournamentId, cancellationToken),
            Teams = await GetTeamOptionsAsync(teamId, cancellationToken),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
            TournamentId = tournamentId,
            TeamId = teamId
        };

        return View("~/Views/Matches/Index.cshtml", model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var match = await matchesApi.GetMatchAsync(id, cancellationToken);
        return match is null ? NotFound() : View(ToDetailsViewModel(match));
    }

    [HttpGet]
    public async Task<IActionResult> Stats(int id, CancellationToken cancellationToken)
    {
        var match = await matchesApi.GetMatchAsync(id, cancellationToken);
        if (match is null)
        {
            return NotFound();
        }

        return View("~/Views/Matches/Stats.cshtml", new MatchStatsViewModel(
            match.MatchId,
            $"{match.HomeTeam.Name} vs {match.AwayTeam.Name}",
            JsonSerializer.Serialize(match.StatsJson, new JsonSerializerOptions { WriteIndented = true })));
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new MatchFormViewModel();
        await PopulateFormOptionsAsync(model, cancellationToken);
        return View("~/Views/Matches/Create.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        MatchFormViewModel model,
        CancellationToken cancellationToken)
    {
        ValidateMatchInput(model, isNewMatch: true);
        if (!ModelState.IsValid)
        {
            await PopulateFormOptionsAsync(model, cancellationToken);
            return View("~/Views/Matches/Create.cshtml", model);
        }

        var created = await matchesApi.CreateMatchAsync(ToRequest(model), cancellationToken);
        return RedirectToAction(nameof(Details), new { id = created.MatchId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var match = await matchesApi.GetMatchAsync(id, cancellationToken);
        if (match is null)
        {
            return NotFound();
        }

        var model = ToFormViewModel(match);
        await PopulateFormOptionsAsync(model, cancellationToken);
        return View("~/Views/Matches/Edit.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        MatchFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (id != model.MatchId)
        {
            return BadRequest();
        }

        var existingMatch = await matchesApi.GetMatchAsync(id, cancellationToken);
        if (existingMatch is null)
        {
            return NotFound();
        }

        ValidateMatchInput(
            model,
            isNewMatch: false,
            existingMatch.Status,
            existingMatch.HomeScore,
            existingMatch.AwayScore);
        if (!ModelState.IsValid)
        {
            await PopulateFormOptionsAsync(model, cancellationToken);
            return View("~/Views/Matches/Edit.cshtml", model);
        }

        var updated = await matchesApi.UpdateMatchAsync(id, ToRequest(model), cancellationToken);
        return updated is null ? NotFound() : RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var match = await matchesApi.GetMatchAsync(id, cancellationToken);
        return match is null ? NotFound() : View(ToDeleteViewModel(match));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var deleted = await matchesApi.DeleteMatchAsync(id, cancellationToken);
        return deleted ? RedirectToAction(nameof(Index)) : NotFound();
    }

    private async Task PopulateFormOptionsAsync(
        MatchFormViewModel model,
        CancellationToken cancellationToken)
    {
        model.Tournaments = await GetTournamentOptionsAsync(model.TournamentId, cancellationToken);
        model.Teams = await GetTeamOptionsAsync(null, cancellationToken);
    }

    private async Task<IReadOnlyList<SelectListItem>> GetTournamentOptionsAsync(
        int? selectedId,
        CancellationToken cancellationToken)
    {
        var tournaments = await matchesApi.GetTournamentsAsync(cancellationToken);
        return tournaments
            .OrderBy(tournament => tournament.Name)
            .Select(tournament => new SelectListItem(
                tournament.Name,
                tournament.TournamentId.ToString(),
                tournament.TournamentId == selectedId))
            .ToArray();
    }

    private async Task<IReadOnlyList<SelectListItem>> GetTeamOptionsAsync(
        int? selectedId,
        CancellationToken cancellationToken)
    {
        var teams = await matchesApi.GetTeamsAsync(cancellationToken);
        return teams
            .OrderBy(team => team.Name)
            .Select(team => new SelectListItem(
                team.Name,
                team.TeamId.ToString(),
                team.TeamId == selectedId))
            .ToArray();
    }

    private void ValidateMatchInput(
        MatchFormViewModel model,
        bool isNewMatch,
        MatchStatus? previousStatus = null,
        int? previousHomeScore = null,
        int? previousAwayScore = null)
    {
        if (!Enum.IsDefined(model.MatchKind) || !Enum.IsDefined(model.Sport) || !Enum.IsDefined(model.Status))
        {
            ModelState.AddModelError(string.Empty, "Select valid match kind, sport, and status values.");
        }

        if (model.HomeTeamId == model.AwayTeamId)
        {
            ModelState.AddModelError(nameof(model.AwayTeamId), "Home and away teams must be different.");
        }

        if (string.IsNullOrWhiteSpace(model.StatsJson))
        {
            ModelState.AddModelError(nameof(model.StatsJson), "Statistics JSON is required.");
        }
        else
        {
            try
            {
                using var document = JsonDocument.Parse(model.StatsJson);
                model.StatsJson = document.RootElement.GetRawText();
            }
            catch (JsonException)
            {
                ModelState.AddModelError(nameof(model.StatsJson), "Enter valid JSON statistics.");
            }
        }

        if (isNewMatch)
        {
            if (model.Status != MatchStatus.Scheduled)
            {
                ModelState.AddModelError(nameof(model.Status), "New matches must be scheduled.");
            }

            if (model.HomeScore.HasValue || model.AwayScore.HasValue)
            {
                ModelState.AddModelError(string.Empty, "New matches cannot have scores.");
            }
        }
        else if (previousStatus.HasValue)
        {
            if (previousStatus == MatchStatus.Finished &&
                (model.Status != previousStatus ||
                 model.HomeScore != previousHomeScore ||
                 model.AwayScore != previousAwayScore))
            {
                ModelState.AddModelError(string.Empty, "Finished matches cannot have their result or status changed.");
            }
            else if (model.Status != previousStatus &&
                     (previousStatus == MatchStatus.Scheduled
                         ? model.Status is not (MatchStatus.Live or MatchStatus.Finished)
                         : previousStatus == MatchStatus.Live && model.Status != MatchStatus.Finished))
            {
                ModelState.AddModelError(nameof(model.Status), "The requested match status transition is not allowed.");
            }
        }

        if (model.Status == MatchStatus.Finished &&
            (!model.HomeScore.HasValue || !model.AwayScore.HasValue))
        {
            ModelState.AddModelError(string.Empty, "Finished matches must include both scores.");
        }

        if (model.Status != MatchStatus.Finished &&
            (model.HomeScore.HasValue || model.AwayScore.HasValue))
        {
            ModelState.AddModelError(string.Empty, "Only finished matches may have scores.");
        }
    }

    private static MatchRequest ToRequest(MatchFormViewModel model)
    {
        using var stats = JsonDocument.Parse(model.StatsJson);
        return new MatchRequest
        {
            TournamentId = model.TournamentId,
            MatchKind = model.MatchKind,
            Sport = model.Sport,
            HomeTeamId = model.HomeTeamId,
            AwayTeamId = model.AwayTeamId,
            PlayedAt = new DateTimeOffset(ToUtc(model.PlayedAt)),
            Status = model.Status,
            HomeScore = model.HomeScore,
            AwayScore = model.AwayScore,
            StatsJson = stats.RootElement.Clone()
        };
    }

    private static MatchFormViewModel ToFormViewModel(MatchResponse match) => new()
    {
        MatchId = match.MatchId,
        TournamentId = match.TournamentId,
        MatchKind = match.MatchKind,
        Sport = match.Sport,
        HomeTeamId = match.HomeTeamId,
        AwayTeamId = match.AwayTeamId,
        PlayedAt = match.PlayedAt.LocalDateTime,
        Status = match.Status,
        HomeScore = match.HomeScore,
        AwayScore = match.AwayScore,
        StatsJson = JsonSerializer.Serialize(match.StatsJson, new JsonSerializerOptions { WriteIndented = true })
    };

    private static MatchApiDetailsViewModel ToDetailsViewModel(MatchResponse match) => new(
        match.MatchId,
        match.Tournament?.Name,
        match.MatchKind.ToString(),
        match.Sport.ToString(),
        match.HomeTeam.Name,
        match.AwayTeam.Name,
        match.PlayedAt,
        match.Status.ToString(),
        match.HomeScore,
        match.AwayScore,
        match.Forecasts
            .OrderBy(forecast => forecast.SubmittedAt)
            .Select(forecast => new ForecastApiViewModel(
                forecast.UserId,
                forecast.PredictedHomeScore,
                forecast.PredictedAwayScore,
                forecast.SubmittedAt))
            .ToArray());

    private static MatchApiDeleteViewModel ToDeleteViewModel(MatchResponse match) => new(
        match.MatchId,
        match.Tournament?.Name,
        match.HomeTeam.Name,
        match.AwayTeam.Name,
        match.PlayedAt,
        match.Status.ToString());

    private static DateTime ToUtc(DateTime value) =>
        DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime();
}
