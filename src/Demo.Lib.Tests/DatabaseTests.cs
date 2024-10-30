using System.Data;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using NUnit.Framework;

namespace Demo.Lib.Tests;

public class DatabaseTests
{
    private const int DatabasePort = 5432;
    private const string Password = "dwskol4j34jm32wdsa";

    private IContainer _container;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _container = CreateTestContainer();
        await _container.StartAsync();

        var host = _container.Hostname;
        var port = _container.GetMappedPublicPort(DatabasePort);

        Environment.SetEnvironmentVariable(Config.HostKey, host);
        Environment.SetEnvironmentVariable(Config.PortKey, $"{port}");
        Environment.SetEnvironmentVariable(Config.UsernameKey, "postgres");
        Environment.SetEnvironmentVariable(Config.PasswordKey, Password);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _container.StopAsync();
    }

    private static IContainer CreateTestContainer()
    {
        var builder = new ContainerBuilder()
            .WithImage("postgres:17")
            .WithPortBinding(DatabasePort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(DatabasePort))
            .WithEnvironment("POSTGRES_PASSWORD", Password)
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
}