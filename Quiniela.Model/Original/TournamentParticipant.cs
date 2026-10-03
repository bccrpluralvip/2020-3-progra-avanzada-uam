using System;
using System.Collections.Generic;

namespace Quiniela.Model;

public partial class TournamentParticipant
{
    public int ParticipantId { get; set; }

    public int TournamentId { get; set; }

    public int TeamId { get; set; }

    public virtual Team Team { get; set; } = null!;

    public virtual Tournament Tournament { get; set; } = null!;
}
