using System.Data;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using NUnit.Framework;

namespace Demo.Lib.Tests;

public class DatabaseTests
{
    private const int DatabasePort = 5432;
    private const string ContainerPassword = "dwskol4j34jm32wdsa";

    private IContainer? _container;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        if(IsWindows)
        {
            SetupWindowsEnvironment();
        }
        else
        {
            await SetupLinuxEnvironment();
        }
    }

    private async Task SetupLinuxEnvironment()
    {
        _container = CreateTestContainer();
        await _container.StartAsync();

        var host = _container.Hostname;
        var port = _container.GetMappedPublicPort(DatabasePort);

        Environment.SetEnvironmentVariable(Config.HostKey, host);
        Environment.SetEnvironmentVariable(Config.PortKey, $"{port}");
        Environment.SetEnvironmentVariable(Config.UsernameKey, "postgres");
        Environment.SetEnvironmentVariable(Config.PasswordKey, ContainerPassword);
    }

    private static void SetupWindowsEnvironment()
    {
        // See https://github.com/actions/runner-images/blob/main/images/windows/Windows2022-Readme.md

        Environment.SetEnvironmentVariable(Config.HostKey, "localhost");
        Environment.SetEnvironmentVariable(Config.PortKey, $"{DatabasePort}");
        Environment.SetEnvironmentVariable(Config.UsernameKey, "postgres");
        Environment.SetEnvironmentVariable(Config.PasswordKey, "root");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if(_container != null)
        {
            await _container.StopAsync();
        }
    }

    private static bool IsWindows => Environment.OSVersion.Platform == PlatformID.Win32NT;

    private static IContainer CreateTestContainer()
    {
        var builder = new ContainerBuilder()
            //.WithImage("postgres:17")
            .WithImage("timescale/timescaledb:latest-pg16")
            .WithPortBinding(DatabasePort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(DatabasePort))
            .WithEnvironment("POSTGRES_PASSWORD", ContainerPassword)
            .WithAutoRemove(true);
        return builder.Build();
    }

    [Test]
    public void GetConnection_ReturnsConnection()
    {
        using var connection = Database.GetConnection();

        connection.State.Should().Be(ConnectionState.Open);
    }

    [TestCase("postgres", true)]
    [TestCase("unknown", false)]
    public async Task DatabaseExists_ReturnsExpected(string databaseName, bool expected)
    {
        using var connection = Database.GetConnection();

        var actual = await Database.DatabaseExists(connection, databaseName);

        actual.Should().Be(expected);
    }

    [Test]
    public async Task CreateDatabase_CreatesDatabase()
    {
        using var connection = Database.GetConnection();

        var databaseName = $"test_database_{Guid.NewGuid():N}";

        var exists = await Database.DatabaseExists(connection, databaseName);
        exists.Should().BeFalse();

        await Database.CreateDatabase(connection, databaseName);

        exists = await Database.DatabaseExists(connection, databaseName);
        exists.Should().BeTrue();
    }

    [Test]
    public async Task DropDatabase_DropsDatabase()
    {
        using var connection = Database.GetConnection();

        var databaseName = $"test_database_{Guid.NewGuid():N}";

        await Database.CreateDatabase(connection, databaseName);

        var exists = await Database.DatabaseExists(connection, databaseName);
        exists.Should().BeTrue();

        await Database.DropDatabase(connection, databaseName);

        exists = await Database.DatabaseExists(connection, databaseName);
        exists.Should().BeFalse();
    }

    [TestCase("postgres", "public", true)]
    [TestCase("postgres", "unknown", false)]
    public async Task SchemaExists_ReturnsExpected(string databaseName,
        string schemaName, bool expected)
    {
        using var connection = Database.GetConnection();

        var actual = await Database.SchemaExists(connection, databaseName, schemaName);

        actual.Should().Be(expected);
    }

    [Test]
    public async Task CreateSchema_CreatesSchema()
    {
        using var connection = Database.GetConnection();

        var databaseName = $"test_database_{Guid.NewGuid():N}";
        var schemaName = $"test_schema_{Guid.NewGuid():N}";

        await Database.CreateDatabase(connection, databaseName);

        var exists = await Database.SchemaExists(connection, databaseName, schemaName);
        exists.Should().BeFalse();

        await Database.CreateSchema(connection, databaseName, schemaName);

        exists = await Database.SchemaExists(connection, databaseName, schemaName);
        exists.Should().BeTrue();
    }

    [TestCase("postgres", "pg_catalog", "pg_statistic", true)]
    [TestCase("postgres", "pg_catalog", "pg_roles", false)]
    [TestCase("postgres", "pg_catalog", "unknown", false)]
    [TestCase("postgres", "unknown", "pg_statistic", false)]
    public async Task TableExists_ReturnsExpected(string databaseName,
        string schemaName, string tableName, bool expected)
    {
        using var connection = Database.GetConnection();

        var actual = await Database.TableExists(connection,
            databaseName, schemaName, tableName);

        actual.Should().Be(expected);
    }

    [Test]
    public async Task Instantiate_Should_CreateDatabaseSchemaAndTable()
    {
        using var connection = Database.GetConnection();

        var databaseName = $"test_database_{Guid.NewGuid():N}";
        var schemaName = $"test_schema_{Guid.NewGuid():N}";
        await Database.Instantiate(connection,databaseName,schemaName);

        var exists = await Database.DatabaseExists(connection, databaseName);
        exists.Should().BeTrue();

        exists = await Database.SchemaExists(connection, databaseName, schemaName);
        exists.Should().BeTrue();

        exists = await Database.TableExists(connection, databaseName, schemaName, "sensor_values");
        exists.Should().BeTrue();
    }

    [Test]
    public async Task Calling_Instantiate_Multiple_Times()
    {
        using var connection = Database.GetConnection();

        var databaseName = $"test_database_{Guid.NewGuid():N}";
        var schemaName = $"test_schema_{Guid.NewGuid():N}";
        await Database.Instantiate(connection,databaseName,schemaName);
        await Database.Instantiate(connection,databaseName,schemaName);
        await Database.Instantiate(connection,databaseName,schemaName);
    }

    [TestCase("plpgsql", true)]
    [TestCase("unknown", false)]
    [TestCase("timescaledb", true)]
    public async Task ExtensionExists_ReturnsExpected(
        string extensionName, bool expected)
    {
        using var connection = Database.GetConnection();

        var actual = await Database.ExtensionExists(connection, extensionName);

        actual.Should().Be(expected);
    }
}