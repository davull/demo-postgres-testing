namespace Demo.Lib;

public static class Config
{
    public const string HostKey = "DEMO_POSTGRES_CONFIG_HOST";
    public const string PortKey = "DEMO_POSTGRES_CONFIG_PORT";
    public const string UsernameKey = "DEMO_POSTGRES_CONFIG_USERNAME";
    public const string PasswordKey = "DEMO_POSTGRES_CONFIG_PASSWORD";

    public static string Host => GetEnvironmentVariable(HostKey);
    public static int Port => int.Parse(GetEnvironmentVariable(PortKey));
    public static string Username => GetEnvironmentVariable(UsernameKey);
    public static string Password => GetEnvironmentVariable(PasswordKey);

    private static string GetEnvironmentVariable(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Environment variable {key} is not set.");
        }

        return value;
    }
}