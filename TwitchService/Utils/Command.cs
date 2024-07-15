using TwitchLib.Client.Events;

namespace TwitchService.Utils;

public class Command(string commandString, Action<OnChatCommandReceivedArgs> action, bool isEnabled)
{
    public bool Enabled { get; set; } = isEnabled;
    public string CommandString { get; set; } = commandString;
    public Action<OnChatCommandReceivedArgs> Action { get; set; } = action;

    public Command(string commandString, Action<OnChatCommandReceivedArgs> action) : this(commandString, action, true)
    {
    }

    private void Init(string commandString, Action<OnChatCommandReceivedArgs> action)
    {
        CommandString = commandString;
        Action = action;
    }

    public void Execute(OnChatCommandReceivedArgs e)
    {
        if (!Enabled) return;

        try
        {
            Action(e);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An exception was caught running " +
                              $"{e.Command.CommandText} :: " +
                              $"{e.Command.ChatMessage.Message} :: " +
                              $"{ex.Message}"
                              );
        }
    }
}