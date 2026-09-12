namespace Quiniela.BusinessLogic;

public class QuinielaScorer
{
	public int CalculatePoints(
		int realTeamAScore,
		int realTeamBScore,
		int guessedTeamAScore,
		int guessedTeamBScore)
	{
		if (realTeamAScore == guessedTeamAScore && realTeamBScore == guessedTeamBScore)
		{
			return 5;
		}

		var sameResult = GetResult(realTeamAScore, realTeamBScore) ==
						 GetResult(guessedTeamAScore, guessedTeamBScore);

		if (!sameResult)
		{
			return 0;
		}

		var correctlyGuessedScore = realTeamAScore == guessedTeamAScore ||
									realTeamBScore == guessedTeamBScore;

		return correctlyGuessedScore ? 3 : 2;
	}

	private int GetResult(int teamAScore, int teamBScore)
	{
		return teamAScore.CompareTo(teamBScore);
	}
}