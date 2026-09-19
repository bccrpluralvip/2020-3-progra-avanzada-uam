using Microsoft.AspNetCore.Mvc;
using Quiniela.BusinessLogic;
using Quiniela.Mvc.Models;

namespace Quiniela.Mvc.Controllers;

public class QuinielaController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Quiniela";
        return View();
    }

    [HttpGet]
    public IActionResult WebUnitTesting()
    {
        return View(new QuinielaScoreViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult WebUnitTesting(QuinielaScoreViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var scorer = new QuinielaScorer();
        model.TotalPoints = scorer.CalculatePoints(
            model.RealTeamAScore,
            model.RealTeamBScore,
            model.GuessedTeamAScore,
            model.GuessedTeamBScore);

        return View(model);
    }
}
