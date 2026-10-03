using System;
using System.Collections.Generic;

namespace Quiniela.Model;

public partial class Match
{
    public int MatchId { get; set; }

    public int? TournamentId { get; set; }

    public MatchKind MatchKind { get; set; }

    public Sport Sport { get; set; }

    public int HomeTeamId { get; set; }

    public int AwayTeamId { get; set; }

    public DateTime PlayedAt { get; set; }

    public MatchStatus Status { get; set; }

    public int? HomeScore { get; set; }

    public int? AwayScore { get; set; }

    public string StatsJson { get; set; } = null!;

    public virtual Team AwayTeam { get; set; } = null!;

    public virtual ICollection<Forecast> Forecasts { get; set; } = new List<Forecast>();

    public virtual Team HomeTeam { get; set; } = null!;

    public virtual Tournament? Tournament { get; set; }
}
