namespace Quiniela.Mvc.Models;

public class QuinielaScoreViewModel
{
    public int RealTeamAScore { get; set; }
    public int RealTeamBScore { get; set; }
    public int GuessedTeamAScore { get; set; }
    public int GuessedTeamBScore { get; set; }
    public int? TotalPoints { get; set; }
}
