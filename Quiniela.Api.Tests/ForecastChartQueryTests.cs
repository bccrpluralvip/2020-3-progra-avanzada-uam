using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Quiniela.BusinessLogic.Queries;
using Quiniela.Model;

namespace Quiniela.Api.Tests;

public sealed class ForecastChartQueryTests
{
    [Fact]
    public async Task DailyCounts_TranslateToPostgresBeforeOpeningConnection()
    {
        var options = new DbContextOptionsBuilder<QuinielaDbContext>();
        options.UseQuinielaPostgres("Host=localhost;Database=unused;Username=unused");
        options.AddInterceptors(new StopBeforeConnecting());
        await using var context = new QuinielaDbContext(options.Options);
        var queries = new ForecastQueryBusinessLogic(context);

        // Query translation must succeed before EF attempts to open a connection.
        // The interceptor prevents network access or use of an actual database.
        await Assert.ThrowsAsync<QueryTranslatedException>(() =>
            queries.GetForecastCountsForDateAsync(new DateOnly(2026, 11, 14)));
    }

    private sealed class QueryTranslatedException : Exception;

    private sealed class StopBeforeConnecting : DbConnectionInterceptor
    {
        public override ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default) => throw new QueryTranslatedException();
    }
}
