using System;
using System.Collections.Generic;

namespace Quiniela.Model;

public partial class Tournament
{
    public int TournamentId { get; set; }

    public string Name { get; set; } = null!;

    public string Season { get; set; } = null!;

    public Sport Sport { get; set; }

    public virtual ICollection<Match> Matches { get; set; } = new List<Match>();

    public virtual ICollection<TournamentParticipant> TournamentParticipants { get; set; } = new List<TournamentParticipant>();
}
