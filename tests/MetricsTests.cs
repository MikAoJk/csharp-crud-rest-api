using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Xunit;

public class MetricsTests
{
    [Fact]
    public async Task MetricsReturnsFreshProcessSamplesWithoutDatabaseAccess()
    {
        using var factory = new MetricsApplicationFactory();
        using var client = factory.CreateClient();
        using var process = Process.GetCurrentProcess();
        var cpuBefore = process.TotalProcessorTime.TotalSeconds;
        var started = (DateTimeOffset.UtcNow - DateTimeOffset.UnixEpoch).TotalSeconds;

        using var response = await client.GetAsync("/metrics");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.True(response.Headers.CacheControl?.NoStore);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var sample = document.RootElement;
        Assert.Equal(4, sample.EnumerateObject().Count());
        process.Refresh();
        var cpu = sample.GetProperty("cpu_seconds_total").GetDouble();
        Assert.InRange(cpu, cpuBefore, process.TotalProcessorTime.TotalSeconds);
        Assert.True(sample.GetProperty("memory_rss_bytes").GetInt64() > 0);
        Assert.Equal(Environment.ProcessId, sample.GetProperty("process_id").GetInt32());
        var timestamp = sample.GetProperty("sampled_at_unix_seconds").GetDouble();
        Assert.True(double.IsFinite(timestamp));
        Assert.True(timestamp > 0);
        Assert.InRange(timestamp, started,
            (DateTimeOffset.UtcNow - DateTimeOffset.UnixEpoch).TotalSeconds);

        await Task.Delay(20);
        using var nextResponse = await client.GetAsync("/metrics");
        nextResponse.EnsureSuccessStatusCode();
        using var nextDocument = JsonDocument.Parse(await nextResponse.Content.ReadAsStringAsync());
        var next = nextDocument.RootElement;
        Assert.True(next.GetProperty("cpu_seconds_total").GetDouble() >= cpu);
        var nextTimestamp = next.GetProperty("sampled_at_unix_seconds").GetDouble();
        Assert.True(double.IsFinite(nextTimestamp));
        Assert.True(nextTimestamp > timestamp);
        var cpuPercent = 100 * (next.GetProperty("cpu_seconds_total").GetDouble() - cpu)
            / (nextTimestamp - timestamp);
        Assert.True(double.IsFinite(cpuPercent));
        Assert.True(cpuPercent >= 0);
        Assert.Equal(Environment.ProcessId, next.GetProperty("process_id").GetInt32());
        Assert.Equal(1, factory.Database.MigrationAttempts);
        Assert.Equal(0, factory.Database.QueryAttempts);
    }

    [Fact]
    public async Task DatabaseFailuresStillReturnInternalError()
    {
        using var factory = new MetricsApplicationFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/users");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("Internal error", await response.Content.ReadAsStringAsync());
        Assert.Equal(1, factory.Database.QueryAttempts);
    }

    private sealed class MetricsApplicationFactory : WebApplicationFactory<Program>
    {
        public UnavailableDatabase Database { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IDatabase>();
                services.AddSingleton<IDatabase>(Database);
            });
        }
    }

    private sealed class UnavailableDatabase : IDatabase
    {
        public int MigrationAttempts { get; private set; }
        public int QueryAttempts { get; private set; }

        public void RunMigrations()
        {
            MigrationAttempts++;
            throw new NpgsqlException("Database unavailable");
        }

        public List<UserInDB> GetAllUsers()
        {
            QueryAttempts++;
            throw new NpgsqlException("Database unavailable");
        }

        public void SaveUser(UserRequest user) => throw new InvalidOperationException("Unexpected database access");
        public UserInDB? GetUserById(int id) => throw new InvalidOperationException("Unexpected database access");
        public bool UpdateUser(UserRequest user, int id) => throw new InvalidOperationException("Unexpected database access");
        public bool DeleteUser(int id) => throw new InvalidOperationException("Unexpected database access");
    }
}
