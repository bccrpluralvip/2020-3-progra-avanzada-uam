using Microsoft.EntityFrameworkCore;
using Quiniela.Model;

namespace Quiniela.BusinessLogic.Crud;

public sealed class TournamentParticipantBusinessLogic : EntityBusinessLogic<TournamentParticipant>
{
	public TournamentParticipantBusinessLogic(QuinielaDbContext context)
		: base(context, nameof(TournamentParticipant.ParticipantId))
	{
	}

	protected override IQueryable<TournamentParticipant> IncludeNavigationProperties(
		IQueryable<TournamentParticipant> query)
	{
		return query
			.Include(participant => participant.Team)
			.Include(participant => participant.Tournament);
	}
}
