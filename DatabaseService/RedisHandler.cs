using StackExchange.Redis;

namespace DatabaseService;

public static class RedisHandler
{
    private static readonly Lazy<ConnectionMultiplexer> Lazy;

    static RedisHandler()
    {
        var options = new ConfigurationOptions
        {
            EndPoints =
            {
                { "redis", 6379 }
            },
            AbortOnConnectFail = false,
            ConnectRetry = 5,
            ConnectTimeout = 10000,
            SyncTimeout = 5000
        };
        Lazy = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(options));
    }

    public static IDatabase Database => Lazy.Value.GetDatabase();
}