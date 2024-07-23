using TwitchLib.Client.Events;

namespace TwitchService.Handlers.Commands;

/// <summary>
/// Sample command.
/// </summary>
public class SampleCommand() : Command("sample")
{
    public override void Execute(OnChatCommandReceivedArgs e, ChatHandler chatHandler)
    {
        chatHandler.SendMessage(
            e.Command.ChatMessage.Channel,
            "Sample command.");
    }
}