using FluentAssertions;
using NUnit.Framework;

namespace Demo.Lib.Tests;

public class EnvironmentTests
{
    [Test]
    public void Platform_Should_BeWindowsOrUnix()
    {
        var actual = Environment.OSVersion.Platform;
        TestContext.WriteLine($"Platform: {actual}");

        PlatformID[] expected = [PlatformID.Win32NT, PlatformID.Unix];

        expected.Should().Contain(actual);
    }
}