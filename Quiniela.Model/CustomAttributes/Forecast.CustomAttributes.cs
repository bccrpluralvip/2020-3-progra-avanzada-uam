using System.ComponentModel.DataAnnotations.Schema;

namespace Quiniela.Model;

public partial class Forecast
{
    [NotMapped]
    public int ResultType =>
        PredictedHomeScore > PredictedAwayScore ? 1 :
        PredictedHomeScore < PredictedAwayScore ? -1 : 0;
}
