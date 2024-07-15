using TwitchLib.Client.Events;

namespace TwitchService.Utils;

public static class CommandHandler
{
    private static readonly List<Command> Commands = [];

    public static void Register(Command command)
    {
        Commands.Add(command);
    }

    public static int Execute(string command, OnChatCommandReceivedArgs e)
    {
        var matchingCommand = GetMatchingCommand(command);
        if (matchingCommand == null) return 1;
        matchingCommand.Execute(e);
        return 0;

    }

    private static Command? GetMatchingCommand(string commandString)
    {
        return Commands.FirstOrDefault(command => commandString == command.CommandString);
    }
}