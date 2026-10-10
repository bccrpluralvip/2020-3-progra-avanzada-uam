using Microsoft.EntityFrameworkCore;
using Quiniela.Model;

namespace Quiniela.BusinessLogic.Crud;

public sealed class MatchBusinessLogic : EntityBusinessLogic<Match>
{
	public MatchBusinessLogic(QuinielaDbContext context)
		: base(context, nameof(Match.MatchId))
	{
	}

	protected override IQueryable<Match> IncludeNavigationProperties(IQueryable<Match> query)
	{
		return query
			.Include(match => match.AwayTeam)
			.Include(match => match.Forecasts)
			.Include(match => match.HomeTeam)
			.Include(match => match.Tournament);
	}
}
