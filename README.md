Éste es el repositorio para los archivos del curso de Programación Avanzada del III Cuatrimestre de 2026 en UAM.

## Quiniela.AppConsole

Console application for comparing a real match score with a user's prediction. The scoring rules live in the referenced `Quiniela.BusinessLogic` class library, so they can be reused independently of the terminal application.

## Quiniela.BusinessLogic

The business-logic project provides asynchronous CRUD operations for `Forecast`, `Match`, `Team`, `Tournament`, `TournamentParticipant`, and `User` through entity-specific business-logic classes. Each class inherits the shared operations from the abstract `EntityBusinessLogic<TEntity>` class and receives a configured `QuinielaDbContext`.

Each service provides `InsertAsync`, `UpdateAsync`, `DeleteAsync` (by primary key), and `GetByIdAsync`. Primary-key queries include only the entity's immediate navigation properties. Collection elements and referenced entities are not expanded further, preventing recursive navigation loading.

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
