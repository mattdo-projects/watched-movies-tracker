using TwitchLib.Client.Events;

namespace TwitchService.Handlers.Commands;

/// <summary>
/// Base class for commands.
/// </summary>
public abstract class Command(string commandString)
{
    public abstract void Execute(OnChatCommandReceivedArgs e, ChatHandler chatHandler);

    public override string ToString()
    {
        return commandString;
    }
}