using FluentAssertions;
using NUnit.Framework;

namespace Demo.Lib.Tests;

public class ConfigTests
{
    [Test]
    public void Config_Should_Read_EnvironmentVariables()
    {
        Environment.SetEnvironmentVariable(Config.HostKey, "127.0.0.1");
        Environment.SetEnvironmentVariable(Config.PortKey, "1234");
        Environment.SetEnvironmentVariable(Config.UsernameKey, "dummy-user");
        Environment.SetEnvironmentVariable(Config.PasswordKey, "abcdefg");

        Config.Host.Should().Be("127.0.0.1");
        Config.Port.Should().Be(1234);
        Config.Username.Should().Be("dummy-user");
        Config.Password.Should().Be("abcdefg");
    }

    [Test]
    public void Config_Should_Throw_When_EnvironmentVariables_Are_Not_Set()
    {
        Environment.SetEnvironmentVariable(Config.HostKey, null);

        var action = new Action(() => _ = Config.Host);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Environment variable DEMO_POSTGRES_CONFIG_HOST is not set.");
    }
}