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

	public async Task<(IReadOnlyList<Match> Matches, int TotalCount)> SearchAsync(
		int? tournamentId,
		int? teamId,
		int pageNumber,
		int pageSize,
		CancellationToken cancellationToken = default)
	{
		if (pageNumber < 1)
		{
			throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
		}

		if (pageSize < 1)
		{
			throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
		}

		var query = IncludeNavigationProperties(Entities.AsNoTracking());
		if (tournamentId.HasValue)
		{
			query = query.Where(match => match.TournamentId == tournamentId);
		}

		if (teamId.HasValue)
		{
			query = query.Where(match =>
				match.HomeTeamId == teamId || match.AwayTeamId == teamId);
		}

		var totalCount = await query.CountAsync(cancellationToken);
		var offset = checked((pageNumber - 1) * pageSize);
		var matches = await query
			.OrderBy(match => match.PlayedAt)
			.ThenBy(match => match.MatchId)
			.Skip(offset)
			.Take(pageSize)
			.ToListAsync(cancellationToken);

		return (matches, totalCount);
	}
}
