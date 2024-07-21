using TwitchLib.Client;
using TwitchLib.Client.Events;

namespace TwitchService.Utils;

/// <summary>
/// Base class for commands.
/// </summary>
public abstract class Command(string commandString)
{
    public string CommandString { get; } = commandString;

    public abstract void Execute(OnChatCommandReceivedArgs e, TwitchClient client);
}