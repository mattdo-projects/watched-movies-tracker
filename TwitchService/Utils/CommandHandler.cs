using TwitchLib.Client;
using TwitchLib.Client.Events;

namespace TwitchService.Utils;

/// <summary>
/// Handles the command functionality at large.
/// </summary>
public class CommandHandler
{
    private readonly List<Command> _commands = [];
    private readonly TwitchClient _client;

    public CommandHandler(TwitchClient client)
    {
        _client = client;

        Register(new SampleCommand());
    }

    private void Register(Command command)
    {
        _commands.Add(command);
    }

    public int Execute(OnChatCommandReceivedArgs e)
    {
        var matchingCommand = GetMatchingCommand(e.Command.CommandText);
        if (matchingCommand == null) return 1;

        matchingCommand.Execute(e, _client);
        return 0;
    }

    private Command? GetMatchingCommand(string commandString)
    {
        return _commands.FirstOrDefault(command => command.CommandString == commandString);
    }
}