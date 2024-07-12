using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace TwitchService;

class Program
{
    static void Main(string[] args)
    {
        var bot = new RecordsBot();
        // Console.ReadLine();
    }
}

internal class RecordsBot
{
    private readonly TwitchClient _client;
    private const string commandPrefix = "!";

    public RecordsBot()
    {
        // TODO: FILL IN DETAILS
        var credentials =
            new ConnectionCredentials(
                Environment.GetEnvironmentVariable("TWITCH_USN"),
                Environment.GetEnvironmentVariable("TWITCH_OAUTH")
                );

        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };

        var customClient = new WebSocketClient(clientOptions);
        _client = new TwitchClient(customClient);
        _client.Initialize(credentials, Environment.GetEnvironmentVariable("TWITCH_USN"));

        _client.OnLog += Client_OnLog;
        _client.OnJoinedChannel += Client_OnJoinedChannel;
        _client.OnMessageReceived += Client_OnMessageReceived;

        _client.Connect();
    }

    private static void Client_OnLog(object sender, OnLogArgs e)
    {
        Console.WriteLine($"{e.BotUsername} has logged in on {e.DateTime}");
    }

    private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
    {
        Console.WriteLine();
        _client.SendMessage(e.Channel, $"Connected to channel {e.Channel}");
    }

    private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
    {
        if (e.ChatMessage.Message.StartsWith(commandPrefix))
        {
            // TODO: Command handler
        }
    }
}

