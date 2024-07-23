using TwitchLib.Client.Events;

namespace TwitchService.Handlers.Commands;

/// <summary>
/// Base class for commands.
/// </summary>
public abstract class Command(string commandString)
{
    public override string ToString()
    {
        return commandString;
    }

    public string CommandString { get; } = commandString;

    public abstract void Execute(OnChatCommandReceivedArgs e, ChatHandler chatHandler);
}