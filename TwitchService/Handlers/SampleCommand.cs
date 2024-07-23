using TwitchLib.Client;
using TwitchLib.Client.Events;

namespace TwitchService.Handlers;

/// <summary>
/// Sample command.
/// </summary>
public class SampleCommand() : Command("sample")
{
    public override void Execute(OnChatCommandReceivedArgs e, TwitchClient client)
    {
        client.SendMessage(e.Command.ChatMessage.Channel, "Sample command.");
    }
}