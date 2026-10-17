Éste es el repositorio para los archivos del curso de Programación Avanzada del III Cuatrimestre de 2026 en UAM.

## Quiniela.AppConsole

Console application for comparing a real match score with a user's prediction. The scoring rules live in the referenced `Quiniela.BusinessLogic` class library, so they can be reused independently of the terminal application.

## Quiniela.BusinessLogic

The business-logic project provides asynchronous CRUD operations for `Forecast`, `Match`, `Team`, `Tournament`, `TournamentParticipant`, and `User` through entity-specific business-logic classes. Each class inherits the shared operations from the abstract `EntityBusinessLogic<TEntity>` class and receives a configured `QuinielaDbContext`.

Each service provides `InsertAsync`, `UpdateAsync`, `DeleteAsync` (by primary key), `GetByIdAsync`, and a paged `GetAllAsync` (page 1, 10 records by default). Primary-key and paged queries include only the entity's immediate navigation properties. Collection elements and referenced entities are not expanded further, preventing recursive navigation loading.

### Queries

The `Quiniela.BusinessLogic.Queries` namespace contains read-only, projection-based queries that go beyond single-entity lookups.

#### `MatchQueryBusinessLogic`

Provides two queries that both return `Task<List<MatchSummaryRecord>>`, sorted chronologically ascending by the date the match was played.

| Method | Parameters | Description |
|--------|-----------|-------------|
| `GetMatchesByTeamAsync` | `teamId`, `from`, `to`, `pageNumber = 1`, `pageSize = 10` | A page of matches in which the team participated (home **or** away) within the inclusive date interval `[from, to]`. |
| `GetMatchesByTournamentAsync` | `tournamentId`, `pageNumber = 1`, `pageSize = 10` | A page of matches that belong to the given tournament. |

#### `MatchSummaryRecord`

The shared return type for both queries. Each record contains:

| Field | Type | Notes |
|-------|------|-------|
| `TournamentName` | `string?` | `null` when the match is not part of any tournament. |
| `TournamentSeason` | `string?` | `null` when the match is not part of any tournament. |
| `TournamentSportName` | `string?` | `null` when the match is not part of any tournament. |
| `MatchId` | `int` | |
| `MatchKind` | `string` | `"Friendly"` or `"Official"`. |
| `HomeTeamName` | `string` | |
| `AwayTeamName` | `string` | |
| `DatePlayed` | `DateTime` | |
| `StatusName` | `string` | `"Scheduled"`, `"Live"`, or `"Finished"`. |
| `HomeScore` | `int?` | `null` when the match has not been played yet. |
| `AwayScore` | `int?` | `null` when the match has not been played yet. |

## Quiniela APIs

The solution includes two independently runnable implementations of the same REST API:

- `Quiniela.Api` uses ASP.NET Core controllers. Resource-specific controllers are in `Quiniela.Api/Controllers`; shared CRUD action methods live in `CrudControllerBase`. Custom match-query actions are in `MatchQueriesController`.
- `Quiniela.MinimalApi` uses ASP.NET Core Minimal APIs, with endpoint maps in `Quiniela.MinimalApi/Endpoints`.

Both expose CRUD for forecasts, matches, teams, tournaments, tournament participants, and users, plus these custom query routes:

- `GET /api/queries/matches/by-tournament/{tournamentId}?pageNumber=1&pageSize=10`
- `GET /api/queries/matches/by-team/{teamId}?from={dateTimeOffset}&to={dateTimeOffset}&pageNumber=1&pageSize=10`

Each CRUD resource supports paged `GET /api/{resource}?pageNumber=1&pageSize=10`, `GET /api/{resource}/{id}`, `POST`, `PUT /api/{resource}/{id}`, and `DELETE /api/{resource}/{id}`. Paging defaults to page 1 with 10 records. POST returns `201 Created`, DELETE returns `204 No Content`, and unknown IDs return `404`. Responses contain direct related records without recursively expanding navigation graphs.

Both projects share DTOs and entity mappings through `Quiniela.Api.Contracts`. Enum values are represented by name and date/time inputs and outputs use ISO 8601 offsets. In Development, each provides an OpenAPI document at `/openapi/v1.json`.

Configure PostgreSQL for both projects using the `ConnectionStrings__MySecretPostgresConnection` setting. The projects include an empty setting in `appsettings.json`; supply its value through .NET user secrets or an environment variable. They use separate default HTTP ports, so both can run at once. Start each from its own PowerShell terminal and set the connection string in both:

```powershell
$env:ConnectionStrings__MySecretPostgresConnection = "<your PostgreSQL connection string>"
dotnet run --project .\Quiniela.Api\Quiniela.Api.csproj
```

```powershell
$env:ConnectionStrings__MySecretPostgresConnection = "<your PostgreSQL connection string>"
dotnet run --project .\Quiniela.MinimalApi\Quiniela.MinimalApi.csproj
```

The controller API listens on port 5053; the Minimal API listens on port 5052. Sample requests for each are in [Quiniela.Api.http](./Quiniela.Api/Quiniela.Api.http) and [Quiniela.MinimalApi.http](./Quiniela.MinimalApi/Quiniela.MinimalApi.http).

Run it with the real scores followed by the guessed scores:

```powershell
dotnet run --project .\Quiniela.AppConsole\Quiniela.AppConsole.csproj -- <real-team-a> <real-team-b> <guess-team-a> <guess-team-b>
```

Scoring:

- Exact score: 5 points.
- Correct result only (team A win, team B win, or tie): 2 points.
- Correct result and one exact team score: 3 points.
- Incorrect result: 0 points.

## Tests

Run the business-logic unit tests with:

```powershell
dotnet test .\Quiniela.BusinessLogic.Tests\Quiniela.BusinessLogic.Tests.csproj
```

The tests cover exact scores, wins for both teams, ties, correct results with one exact score, and incorrect results.
