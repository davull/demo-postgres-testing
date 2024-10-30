using System.Data;
using Dapper;
using Npgsql;

namespace Demo.Lib;

public static class Database
{
    public static IDbConnection GetConnection(string? databaseName = null)
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = Config.Host,
            Port = Config.Port,
            Username = Config.Username,
            Password = Config.Password,
            Database = databaseName,
            BrowsableConnectionString = false
        };
        var connectionString = connectionStringBuilder.ConnectionString;
        var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        return connection;
    }

    public static async Task Instantiate(IDbConnection connection,
        string databaseName, string schemaName)
    {
        if (await DatabaseExists(connection, databaseName) is false)
        {
            await CreateDatabase(connection, databaseName);
        }

        connection.ChangeDatabase(databaseName);

        if (await SchemaExists(connection, databaseName, schemaName) is false)
        {
            await CreateSchema(connection, databaseName, schemaName);
        }

        if (await TableExists(connection, databaseName, schemaName, "sensor_values") is false)
        {
            var sql = $"""
                      create table {schemaName}.sensor_values (
                          id serial primary key,
                          sensor_id integer not null,
                          value double precision not null,
                          created_at timestamp with time zone not null
                      );
                      """;
            await connection.ExecuteAsync(sql);
        }
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

    public static async Task<bool> SchemaExists(IDbConnection connection,
        string databaseName, string tableName)
    {
        connection.ChangeDatabase(databaseName);

        const string sql = """
                           SELECT count(*) > 0
                           FROM information_schema.schemata
                           WHERE catalog_name = @databaseName
                             AND schema_name = @tableName;
                           """;

        var result = await connection.ExecuteScalarAsync<bool>(
            sql, new { databaseName, tableName });
        return result;
    }

    public static async Task<bool> TableExists(IDbConnection connection,
        string databaseName, string schemaName, string tableName)
    {
        connection.ChangeDatabase(databaseName);

        const string sql = """
                           SELECT count(*) > 0
                           FROM information_schema.tables
                           WHERE table_catalog = @databaseName
                             AND table_schema = @schemaName
                             AND table_name = @tableName
                             AND table_type = 'BASE TABLE';
                           """;

        var result = await connection.ExecuteScalarAsync<bool>(
            sql, new { databaseName, schemaName, tableName });
        return result;
    }

    public static async Task CreateDatabase(
        IDbConnection connection, string databaseName)
    {
        var sql = $"CREATE DATABASE {databaseName};";
        await connection.ExecuteAsync(sql);
    }

    public static async Task CreateSchema(
        IDbConnection connection, string databaseName, string schemaName)
    {
        connection.ChangeDatabase(databaseName);

        var sql = $"CREATE SCHEMA {schemaName};";
        await connection.ExecuteAsync(sql);
    }

    public static async Task DropDatabase(
        IDbConnection connection, string databaseName)
    {
        var sql = $"DROP DATABASE {databaseName};";
        await connection.ExecuteAsync(sql);
    }
}