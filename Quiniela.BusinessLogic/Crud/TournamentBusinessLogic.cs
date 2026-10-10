using Microsoft.EntityFrameworkCore;
using Quiniela.Model;

namespace Quiniela.BusinessLogic.Crud;

public sealed class TournamentBusinessLogic : EntityBusinessLogic<Tournament>
{
	public TournamentBusinessLogic(QuinielaDbContext context)
		: base(context, nameof(Tournament.TournamentId))
	{
	}

	protected override IQueryable<Tournament> IncludeNavigationProperties(IQueryable<Tournament> query)
	{
		return query
			.Include(tournament => tournament.Matches)
			.Include(tournament => tournament.TournamentParticipants);
	}
}
