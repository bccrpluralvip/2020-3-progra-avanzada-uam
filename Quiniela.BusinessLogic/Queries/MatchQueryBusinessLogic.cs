using Microsoft.EntityFrameworkCore;
using Quiniela.Model;

namespace Quiniela.BusinessLogic.Queries;

/// <summary>
/// Read-only queries that return <see cref="MatchSummaryRecord"/> projections.
/// </summary>
public sealed class MatchQueryBusinessLogic
{
    private readonly QuinielaDbContext _context;

    public MatchQueryBusinessLogic(QuinielaDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns all matches in which <paramref name="teamId"/> participated
    /// (either as home or away team) within the inclusive date interval
    /// [<paramref name="from"/>, <paramref name="to"/>], sorted chronologically ascending.
    /// </summary>
    /// <param name="teamId">The team whose matches are queried.</param>
    /// <param name="from">Start of the date interval (inclusive).</param>
    /// <param name="to">End of the date interval (inclusive).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    public Task<List<MatchSummaryRecord>> GetMatchesByTeamAsync(
        int teamId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        return BuildMatchSummaryQuery()
            .Where(m =>
                (m.HomeTeamId == teamId || m.AwayTeamId == teamId) &&
                m.PlayedAt >= from &&
                m.PlayedAt <= to)
            .OrderBy(m => m.PlayedAt)
            .Select(ToMatchSummaryRecord)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Returns all matches that belong to <paramref name="tournamentId"/>,
    /// sorted chronologically ascending.
    /// </summary>
    /// <param name="tournamentId">The tournament whose matches are queried.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    public Task<List<MatchSummaryRecord>> GetMatchesByTournamentAsync(
        int tournamentId,
        CancellationToken cancellationToken = default)
    {
        return BuildMatchSummaryQuery()
            .Where(m => m.TournamentId == tournamentId)
            .OrderBy(m => m.PlayedAt)
            .Select(ToMatchSummaryRecord)
            .ToListAsync(cancellationToken);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Base queryable with the navigation properties required for the projection,
    /// without any filtering applied (tracking disabled for read-only use).
    /// </summary>
    private IQueryable<Match> BuildMatchSummaryQuery()
    {
        return _context.Matches
            .AsNoTracking()
            .Include(m => m.HomeTeam)
            .Include(m => m.AwayTeam)
            .Include(m => m.Tournament);
    }

    /// <summary>
    /// EF Core-compatible projection expression from <see cref="Match"/>
    /// to <see cref="MatchSummaryRecord"/>.
    /// </summary>
    private static readonly System.Linq.Expressions.Expression<Func<Match, MatchSummaryRecord>>
        ToMatchSummaryRecord = m => new MatchSummaryRecord(
            m.Tournament != null ? m.Tournament.Name       : null,
            m.Tournament != null ? m.Tournament.Season     : null,
            m.Tournament != null ? m.Tournament.Sport.ToString() : null,
            m.MatchId,
            m.MatchKind.ToString(),
            m.HomeTeam.Name,
            m.AwayTeam.Name,
            m.PlayedAt,
            m.Status.ToString(),
            m.HomeScore,
            m.AwayScore);
}
