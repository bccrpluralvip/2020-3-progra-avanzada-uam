using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Quiniela.Model;

namespace Quiniela.Mvc.Models;

public sealed class MatchFormViewModel
{
    public int? MatchId { get; set; }
    public int? TournamentId { get; set; }

    [Required]
    public MatchKind MatchKind { get; set; } = MatchKind.Official;

    [Required]
    public Sport Sport { get; set; } = Sport.Soccer;

    [Range(1, int.MaxValue)]
    public int HomeTeamId { get; set; }

    [Range(1, int.MaxValue)]
    public int AwayTeamId { get; set; }

    [Required]
    public DateTime PlayedAt { get; set; } = DateTime.Now;

    [Required]
    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;

    [Range(0, int.MaxValue)]
    public int? HomeScore { get; set; }

    [Range(0, int.MaxValue)]
    public int? AwayScore { get; set; }

    [Required]
    public string StatsJson { get; set; } = "{}";

    public IReadOnlyList<SelectListItem> Tournaments { get; set; } = [];
    public IReadOnlyList<SelectListItem> Teams { get; set; } = [];
}
