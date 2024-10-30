using System.Data;
using Dapper;
using Npgsql;

namespace Demo.Lib;

public static class Database
{
    public static IDbConnection GetConnection()
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = Config.Host,
            Port = Config.Port,
            Username = Config.Username,
            Password = Config.Password,
            BrowsableConnectionString = false
        };
        var connectionString = connectionStringBuilder.ConnectionString;
        var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        return connection;
    }

    public static async Task<bool> DatabaseExists(
        IDbConnection connection, string databaseName)
    {
        const string sql = """
                           SELECT count(*) > 0
                           FROM pg_database
                           WHERE datname = @databaseName;
                           """;

        var result = await connection.ExecuteScalarAsync<bool>(
            sql, new { databaseName });
        return result;

    }

    public static async Task CreateDatabase(
        IDbConnection connection, string databaseName)
    {
        var sql = $"CREATE DATABASE {databaseName};";
        await connection.ExecuteAsync(sql);
    }

    public static async Task DropDatabase(
        IDbConnection connection, string databaseName)
    {
        var sql = $"DROP DATABASE {databaseName};";
        await connection.ExecuteAsync(sql);
    }
}