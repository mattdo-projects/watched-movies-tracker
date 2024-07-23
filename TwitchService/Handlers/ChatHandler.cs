using TwitchLib.Client;

namespace TwitchService.Handlers;

public class ChatHandler(TwitchClient client)
{
    public void SendMessage(string channel, string message)
    {
        client.SendMessage(channel, message);
    }
}