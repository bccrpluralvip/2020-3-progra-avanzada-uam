using System.ComponentModel.DataAnnotations.Schema;

namespace Quiniela.Model;

public partial class Match
{
    [NotMapped]
    public string ElapsedTimeSinceMatch
    {
        get
        {
            var now = DateTime.UtcNow;
            if (PlayedAt > now)
            {
                return "0 months, 0 days, 0 hours and 0 minutes";
            }

            var months = (now.Year - PlayedAt.Year) * 12 + now.Month - PlayedAt.Month;
            var monthBoundary = PlayedAt.AddMonths(months);
            if (monthBoundary > now)
            {
                months--;
                monthBoundary = PlayedAt.AddMonths(months);
            }

            var remainder = now - monthBoundary;
            return $"{FormatUnit(months, "month")}, {FormatUnit(remainder.Days, "day")}, {FormatUnit(remainder.Hours, "hour")} and {FormatUnit(remainder.Minutes, "minute")}";
        }
    }

    private static string FormatUnit(int value, string unit)
    {
        return $"{value} {unit}{(value == 1 ? string.Empty : "s")}";
    }
}