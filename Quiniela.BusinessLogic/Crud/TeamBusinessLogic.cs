using Microsoft.EntityFrameworkCore;
using Quiniela.Model;

namespace Quiniela.BusinessLogic.Crud;

public sealed class TeamBusinessLogic : EntityBusinessLogic<Team>
{
	public TeamBusinessLogic(QuinielaDbContext context)
		: base(context, nameof(Team.TeamId))
	{
	}

	protected override IQueryable<Team> IncludeNavigationProperties(IQueryable<Team> query)
	{
		return query
			.Include(team => team.MatchAwayTeams)
			.Include(team => team.MatchHomeTeams)
			.Include(team => team.TournamentParticipants);
	}
}
