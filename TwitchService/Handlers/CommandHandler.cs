using TwitchLib.Client.Events;
using TwitchService.Handlers.Commands;

namespace TwitchService.Handlers;

/// <summary>
/// Handles the command functionality at large.
/// </summary>
public class CommandHandler
{
    private readonly List<Command> _commands = [];
    private readonly ChatHandler _chatHandler;

    public CommandHandler(ChatHandler chatHandler)
    {
        _chatHandler = chatHandler;

        Register(new SampleCommand());
        Register(new HelpCommand());
    }

    private void Register(Command command)
    {
        _commands.Add(command);
    }

    public int Execute(OnChatCommandReceivedArgs e)
    {
        var matchingCommand = GetMatchingCommand(e.Command.CommandText);
        if (matchingCommand == null) return 1;

        matchingCommand.Execute(e, _chatHandler);
        return 0;
    }

    public List<Command> GetRegisteredCommands()
    {
        return _commands;
    }

    private Command? GetMatchingCommand(string commandString)
    {
        return _commands.FirstOrDefault(command => command.CommandString == commandString);
    }
}