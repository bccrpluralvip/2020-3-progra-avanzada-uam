using System;
using System.Collections.Generic;

namespace Quiniela.Model;

public partial class Team
{
    public int TeamId { get; set; }

    public string Name { get; set; } = null!;

    public string ShortName { get; set; } = null!;

    public Sport Sport { get; set; }

    public virtual ICollection<Match> MatchAwayTeams { get; set; } = new List<Match>();

    public virtual ICollection<Match> MatchHomeTeams { get; set; } = new List<Match>();

    public virtual ICollection<TournamentParticipant> TournamentParticipants { get; set; } = new List<TournamentParticipant>();
}
