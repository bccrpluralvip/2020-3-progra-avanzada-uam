namespace Quiniela.BusinessLogic.Queries;

/// <summary>
/// A flattened, read-only projection of a match together with its tournament context.
/// Returned by both <see cref="MatchQueryBusinessLogic.GetMatchesByTeamAsync"/> and
/// <see cref="MatchQueryBusinessLogic.GetMatchesByTournamentAsync"/>.
/// </summary>
public sealed record MatchSummaryRecord(
    // Tournament context (nullable – a match may not belong to any tournament)
    string?   TournamentName,
    string?   TournamentSeason,
    string?   TournamentSportName,

    // Match details
    int       MatchId,
    string    MatchKind,
    string    HomeTeamName,
    string    AwayTeamName,
    DateTime  DatePlayed,
    string    StatusName,
    int?      HomeScore,
    int?      AwayScore);
