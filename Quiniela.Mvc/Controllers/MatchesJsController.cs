using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Quiniela.Api.Contracts;
using Quiniela.Mvc.Models;
using Quiniela.Mvc.Services;

namespace Quiniela.Mvc.Controllers;

public sealed class MatchesJsController(MatchesApiClient matchesApi) : Controller
{
    private const int DefaultPageSize = 10;
    private const int MaximumPageSize = 100;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var tournaments = await matchesApi.GetTournamentsAsync(cancellationToken);
        var teams = await matchesApi.GetTeamsAsync(cancellationToken);
        var model = new MatchesJsIndexViewModel(
            tournaments
                .OrderBy(tournament => tournament.Name)
                .Select(tournament => new SelectListItem(
                    tournament.Name,
                    tournament.TournamentId.ToString()))
                .ToArray(),
            teams
                .OrderBy(team => team.Name)
                .Select(team => new SelectListItem(
                    team.Name,
                    team.TeamId.ToString()))
                .ToArray(),
            DefaultPageSize,
            Url.Action(nameof(GetMatches))!,
            Url.Action(nameof(GetMatch))!);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetMatches(
        int pageNumber = 1,
        int pageSize = DefaultPageSize,
        int? tournamentId = null,
        int? teamId = null,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, MaximumPageSize);

        var result = await matchesApi.SearchAsync(
            pageNumber,
            pageSize,
            tournamentId,
            teamId,
            cancellationToken);

        return result is null
            ? Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "The matches API returned no search result.")
            : Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetMatch(int id, CancellationToken cancellationToken)
    {
        var match = await matchesApi.GetMatchAsync(id, cancellationToken);
        return match is null ? NotFound() : Json(match);
    }
}
