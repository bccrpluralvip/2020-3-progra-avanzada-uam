using NpgsqlTypes;

namespace Quiniela.Model;

public enum Sport
{
    [PgName("soccer")]
    Soccer,
    [PgName("basketball")]
    Basketball,
    [PgName("baseball")]
    Baseball,
    [PgName("volleyball")]
    Volleyball,
    [PgName("rugby")]
    Rugby,
    [PgName("hockey")]
    Hockey
}

public enum MatchKind
{
    [PgName("friendly")]
    Friendly,
    [PgName("official")]
    Official
}

public enum MatchStatus
{
    [PgName("scheduled")]
    Scheduled,
    [PgName("live")]
    Live,
    [PgName("finished")]
    Finished
}