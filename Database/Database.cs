using Dapper;
using DbUp;
using Npgsql;

public class Database : IDatabase
{
    private readonly string _connectionString;

    public Database(string connectionString)
    {
        _connectionString = connectionString;
    }

    private NpgsqlConnection GetConnection() => new(_connectionString);

    public void RunMigrations()
    {
        EnsureDatabase.For.PostgresqlDatabase(_connectionString);
        DeployChanges.To
            .PostgresqlDatabase(_connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(Database).Assembly)
            .Build()
            .PerformUpgrade();
    }

    public void SaveUser(UserRequest user)
    {
        using var conn = GetConnection();
        conn.Execute("INSERT INTO users(name, email) VALUES (@Name, @Email)", user);
    }

    public List<UserInDB> GetAllUsers()
    {
        using var conn = GetConnection();
        return conn.Query<UserInDB>("SELECT id, name, email FROM users").ToList();
    }

    public UserInDB? GetUserById(int id)
    {
        using var conn = GetConnection();
        return conn.QueryFirstOrDefault<UserInDB>("SELECT id, name, email FROM users WHERE id = @Id", new { Id = id });
    }

    public bool UpdateUser(UserRequest user, int id)
    {
        using var conn = GetConnection();
        var rows = conn.Execute("UPDATE users SET name = @Name, email = @Email WHERE id = @Id",
            new { user.Name, user.Email, Id = id });
        return rows > 0;
    }

    public bool DeleteUser(int id)
    {
        using var conn = GetConnection();
        var rows = conn.Execute("DELETE FROM users WHERE id = @Id", new { Id = id });
        return rows > 0;
    }
}
