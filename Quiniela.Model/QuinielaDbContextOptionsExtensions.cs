using Microsoft.EntityFrameworkCore;

namespace Quiniela.Model;

public static class QuinielaDbContextOptionsExtensions
{
    public static DbContextOptionsBuilder UseQuinielaPostgres(
        this DbContextOptionsBuilder optionsBuilder,
        string connectionString)
    {
        return optionsBuilder.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions
            .MapEnum<Sport>("sport", "user_25")
            .MapEnum<MatchKind>("match_kind", "user_25")
            .MapEnum<MatchStatus>("match_status", "user_25"));
    }
}
