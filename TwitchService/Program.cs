using System.Globalization;
using DatabaseService;
using MovieHandlerService.Handlers;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace TwitchService;

internal static class Program
{
    private static void Main()
    {
        var shutdownEvent = new AutoResetEvent(false);
        _ = new RecordsBot(shutdownEvent);

        // TODO enable console interactions

        shutdownEvent.WaitOne();
    }
}

internal class RecordsBot
{
    private const char CommandPrefix = '!';
    private readonly TwitchClient _client;
    private readonly QueryHandler _queryHandler = new();
    private readonly AutoResetEvent _shutdownEvent;

    public RecordsBot(AutoResetEvent shutdownEvent)
    {
        _shutdownEvent = shutdownEvent;

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
        _client.AddChatCommandIdentifier(CommandPrefix);

        _client.Initialize(
            credentials,
            Environment.GetEnvironmentVariable("TWITCH_USN")
            );

        _client.OnLog += Client_OnLog;
        _client.OnConnected += Client_OnConnected;
        _client.OnJoinedChannel += Client_OnJoinedChannel;
        _client.OnMessageReceived += Client_OnMessageReceived;
        _client.OnChatCommandReceived += Client_OnChatCommandReceived;

        _client.Connect();
    }

    private static void Client_OnLog(object? sender, OnLogArgs e)
    {
        Console.WriteLine($"{e.BotUsername} :: {e.DateTime.ToString(CultureInfo.InvariantCulture)} :: {e.Data}");
    }

    private void Client_OnConnected(object? sender, OnConnectedArgs e)
    {
        Console.WriteLine($"Connected to {e.AutoJoinChannel}");
    }

    private void Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        _client.SendMessage(e.Channel, $"Connected to channel {e.Channel}");
    }

    private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        // TODO consider functionality
    }

    private void Client_OnChatCommandReceived(object? sender, OnChatCommandReceivedArgs e)
    {
        switch (e.Command.CommandText)
        {
            case "search":
            {
                // TODO replace debug stuff with actual implementation.
                //  - formatting string and list func.
                //  - handle twitch filtering, limits, safety mechanisms
                //  - dynamic command handling
                var values = _queryHandler.MovieNameFindLastWatchedDate(
                    e.Command.ArgumentsAsString,
                    false
                    );
                if (values == null) return;
                var message = string.Concat(values);
                _client.SendMessage(e.Command.ChatMessage.Channel, message);
                break;
            }
            case "done" when
                e.Command.ChatMessage.Username == Environment.GetEnvironmentVariable("TWITCH_OWNER_USN"):
                Shutdown();
                break;
        }
    }

    private void Client_OnDisconnect(object? sender, OnDisconnectedArgs e)
    {
        // TODO handle graceful ending for background services before termination...
    }

    private void Shutdown()
    {
        _client.Disconnect();
        _shutdownEvent.Set();
    }
}

