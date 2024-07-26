using System.Globalization;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;
using DatabaseService;
using MovieHandlerService.Handlers;
using TwitchService.Handlers;

namespace TwitchService;

internal static class Program
{
    private static void Main()
    {
        var shutdownEvent = new AutoResetEvent(false);
        _ = new RecordsBot(shutdownEvent);

        Console.WriteLine("Bot initialized, entering wait state.");

        // Wait for the shutdown signal
        shutdownEvent.WaitOne();

        // Adding logging to confirm program termination
        Console.WriteLine("Program terminated.");
    }
}

internal class RecordsBot
{
    private const char CommandPrefix = '!';
    private readonly TwitchClient _client;
    private readonly AutoResetEvent _shutdownEvent;
    private readonly QueryHandler _queryHandler;
    private readonly ChatHandler _chatHandler;
    private readonly CommandHandler _commandHandler;

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

        _client.Initialize(
            credentials,
            Environment.GetEnvironmentVariable("TWITCH_USN"),
            CommandPrefix, // default is '!' and is configured as such,
                           // but good for future config to leave it here.
            CommandPrefix
            );

        DbHandler.TestConnection(); // lazy initialization should handle the rest

        _client.OnLog += Client_OnLog;
        _client.OnConnected += Client_OnConnected;
        _client.OnJoinedChannel += Client_OnJoinedChannel;
        _client.OnMessageReceived += Client_OnMessageReceived;
        _client.OnChatCommandReceived += Client_OnChatCommandReceived;
        _client.OnDisconnected += Client_OnDisconnected;

        _queryHandler = new QueryHandler();
        _chatHandler = new ChatHandler(_client);
        _commandHandler = new CommandHandler(_chatHandler);

        _client.Connect();
    }

    private static void Client_OnLog(object? sender, OnLogArgs e)
    {
        Console.WriteLine($"{e.BotUsername} :: " +
                          $"{e.DateTime.ToString(CultureInfo.InvariantCulture)} :: " +
                          $"{e.Data}");
    }

    private void Client_OnConnected(object? sender, OnConnectedArgs e)
    {
        Console.WriteLine($"Connected to {e.AutoJoinChannel}");
    }

    private void Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        _chatHandler.SendMessage(
            e.Channel,
            $"Now operating assistance on {e.Channel}!");
    }

    private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        // TODO consider functionality
    }

    private void Client_OnChatCommandReceived(object? sender, OnChatCommandReceivedArgs e)
    {
        // TODO replace debug stuff with actual implementation.
        //  - formatting string and list func.
        //  - handle twitch filtering, limits, safety mechanisms
        //  - dynamic command handling

        // For now, termination process will be kept a separate case,
        // since it handles sensitive functionality.
        if (e.Command.CommandText == "exit") Shutdown(e.Command.ChatMessage.Channel);

        if (_commandHandler.Execute(e) == 1)
        {
            _chatHandler.SendMessage(
                e.Command.ChatMessage.Channel,
                "PoroSad Command not found...");
        }
    }

    private void Client_OnDisconnected(object? sender, OnDisconnectedEventArgs e)
    {
        // TODO handle graceful ending for background services before termination...
        //  or whatever the proper process is
        Console.WriteLine("Client is disconnecting from twitch...");
    }

    private void Shutdown(string channel)
    {
        _chatHandler.SendMessage(
            channel,
            "Initiating shutdown...");
        _client.Disconnect();
        DbHandler.DisconnectDatabase();
        _shutdownEvent.Set();
    }
}

