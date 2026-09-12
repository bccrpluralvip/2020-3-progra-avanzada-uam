using Quiniela.BusinessLogic;

if (args.Length != 4 || !args.All(score => int.TryParse(score, out var parsedScore) && parsedScore >= 0))
{
	Console.Error.WriteLine("Usage: Quiniela.AppConsole <real-team-a> <real-team-b> <guess-team-a> <guess-team-b>");
	return 1;
}

var realTeamAScore = int.Parse(args[0]);
var realTeamBScore = int.Parse(args[1]);
var guessedTeamAScore = int.Parse(args[2]);
var guessedTeamBScore = int.Parse(args[3]);

var scorer = new QuinielaScorer();
var points = scorer.CalculatePoints(
	realTeamAScore,
	realTeamBScore,
	guessedTeamAScore,
	guessedTeamBScore);

Console.WriteLine(points);
return 0;
