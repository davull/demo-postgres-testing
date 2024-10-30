using System.Data;
using FluentAssertions;
using NUnit.Framework;

namespace Demo.Lib.Tests;

public class DatabaseTests
{
    [SetUp]
    public void SetUp()
    {
        Environment.SetEnvironmentVariable(Config.HostKey, "localhost");
        Environment.SetEnvironmentVariable(Config.PortKey, "5432");
        Environment.SetEnvironmentVariable(Config.UsernameKey, "postgres");
        Environment.SetEnvironmentVariable(Config.PasswordKey, "AbC123!");
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