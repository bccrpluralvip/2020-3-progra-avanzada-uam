using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Quiniela.Model;
using Quiniela.Mvc.Models;

namespace Quiniela.Mvc.Controllers;

public sealed class MatchesController(QuinielaDbContext context) : Controller
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

        var query = context.Matches
            .AsNoTracking()
            .Include(match => match.Tournament)
            .Include(match => match.HomeTeam)
            .Include(match => match.AwayTeam)
            .Include(match => match.Forecasts)
            .AsQueryable();

        if (tournamentId.HasValue)
        {
            query = query.Where(match => match.TournamentId == tournamentId);
        }

        if (teamId.HasValue)
        {
            query = query.Where(match =>
                match.HomeTeamId == teamId || match.AwayTeamId == teamId);
        }

        var totalMatches = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalMatches / (double)pageSize);
        pageNumber = totalPages == 0 ? 1 : Math.Min(pageNumber, totalPages);

        var matches = await query
            .OrderBy(match => match.PlayedAt)
            .ThenBy(match => match.MatchId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(match => new MatchListItemViewModel(
                match.MatchId,
                match.Tournament == null ? null : match.Tournament.Name,
                match.MatchKind,
                match.Sport,
                match.HomeTeam.Name,
                match.AwayTeam.Name,
                match.PlayedAt,
                match.Status,
                match.HomeScore,
                match.AwayScore,
                match.StatsJson,
                match.Forecasts
                    .OrderBy(forecast => forecast.SubmittedAt)
                    .Select(forecast => new ForecastListItemViewModel(
                        forecast.UserId,
                        forecast.PredictedHomeScore,
                        forecast.PredictedAwayScore,
                        forecast.SubmittedAt))
                    .ToList()))
            .ToListAsync(cancellationToken);

        var model = new MatchListViewModel
        {
            Matches = matches,
            Tournaments = await GetTournamentOptionsAsync(tournamentId, cancellationToken),
            Teams = await GetTeamOptionsAsync(teamId, cancellationToken),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
            TournamentId = tournamentId,
            TeamId = teamId
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var match = await context.Matches
            .AsNoTracking()
            .Include(item => item.Tournament)
            .Include(item => item.HomeTeam)
            .Include(item => item.AwayTeam)
            .Include(item => item.Forecasts)
            .SingleOrDefaultAsync(item => item.MatchId == id, cancellationToken);

        return match is null ? NotFound() : View(match);
    }

    [HttpGet]
    public async Task<IActionResult> Stats(int id, CancellationToken cancellationToken)
    {
        var match = await context.Matches
            .AsNoTracking()
            .Include(item => item.HomeTeam)
            .Include(item => item.AwayTeam)
            .SingleOrDefaultAsync(item => item.MatchId == id, cancellationToken);

        if (match is null)
        {
            return NotFound();
        }

        return View(new MatchStatsViewModel(
            match.MatchId,
            $"{match.HomeTeam.Name} vs {match.AwayTeam.Name}",
            FormatJson(match.StatsJson)));
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new MatchFormViewModel();
        await PopulateFormOptionsAsync(model, cancellationToken);
        return View(model);
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
            return View(model);
        }

        var match = new Match
        {
            TournamentId = model.TournamentId,
            MatchKind = model.MatchKind,
            Sport = model.Sport,
            HomeTeamId = model.HomeTeamId,
            AwayTeamId = model.AwayTeamId,
            PlayedAt = ToUtc(model.PlayedAt),
            Status = model.Status,
            HomeScore = model.HomeScore,
            AwayScore = model.AwayScore,
            StatsJson = model.StatsJson
        };

        context.Matches.Add(match);
        await context.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id = match.MatchId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var match = await context.Matches
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.MatchId == id, cancellationToken);

        if (match is null)
        {
            return NotFound();
        }

        var model = new MatchFormViewModel
        {
            MatchId = match.MatchId,
            TournamentId = match.TournamentId,
            MatchKind = match.MatchKind,
            Sport = match.Sport,
            HomeTeamId = match.HomeTeamId,
            AwayTeamId = match.AwayTeamId,
            PlayedAt = match.PlayedAt.ToLocalTime(),
            Status = match.Status,
            HomeScore = match.HomeScore,
            AwayScore = match.AwayScore,
            StatsJson = FormatJson(match.StatsJson)
        };

        await PopulateFormOptionsAsync(model, cancellationToken);
        return View(model);
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

        var match = await context.Matches
            .SingleOrDefaultAsync(item => item.MatchId == id, cancellationToken);
        if (match is null)
        {
            return NotFound();
        }

        ValidateMatchInput(
            model,
            isNewMatch: false,
            match.Status,
            match.HomeScore,
            match.AwayScore);
        if (!ModelState.IsValid)
        {
            await PopulateFormOptionsAsync(model, cancellationToken);
            return View(model);
        }

        match.TournamentId = model.TournamentId;
        match.MatchKind = model.MatchKind;
        match.Sport = model.Sport;
        match.HomeTeamId = model.HomeTeamId;
        match.AwayTeamId = model.AwayTeamId;
        match.PlayedAt = ToUtc(model.PlayedAt);
        match.Status = model.Status;
        match.HomeScore = model.HomeScore;
        match.AwayScore = model.AwayScore;
        match.StatsJson = model.StatsJson;

        await context.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var match = await context.Matches
            .AsNoTracking()
            .Include(item => item.Tournament)
            .Include(item => item.HomeTeam)
            .Include(item => item.AwayTeam)
            .SingleOrDefaultAsync(item => item.MatchId == id, cancellationToken);

        return match is null ? NotFound() : View(match);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var match = await context.Matches
            .SingleOrDefaultAsync(item => item.MatchId == id, cancellationToken);
        if (match is null)
        {
            return NotFound();
        }

        context.Matches.Remove(match);
        await context.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
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
        CancellationToken cancellationToken) =>
        await context.Tournaments
            .AsNoTracking()
            .OrderBy(tournament => tournament.Name)
            .Select(tournament => new SelectListItem(
                tournament.Name,
                tournament.TournamentId.ToString(),
                tournament.TournamentId == selectedId))
            .ToListAsync(cancellationToken);

    private async Task<IReadOnlyList<SelectListItem>> GetTeamOptionsAsync(
        int? selectedId,
        CancellationToken cancellationToken) =>
        await context.Teams
            .AsNoTracking()
            .OrderBy(team => team.Name)
            .Select(team => new SelectListItem(
                team.Name,
                team.TeamId.ToString(),
                team.TeamId == selectedId))
            .ToListAsync(cancellationToken);

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

    private static DateTime ToUtc(DateTime value) =>
        DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime();

    private static string FormatJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions { WriteIndented = true });
    }
}
