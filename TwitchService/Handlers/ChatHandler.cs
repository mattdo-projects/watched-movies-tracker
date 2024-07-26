using TwitchLib.Client;

namespace TwitchService.Handlers;

public class ChatHandler(TwitchClient client)
{
    public void SendMessage(string channel, string message)
    {
        // TODO message order of operations
        //  0) get initial message
        //  1) self censor, sanitize
        //  2) split message into chunks if exceeds length threshold (500 chars)
        //  3) post message
        client.SendMessage(channel, message);
    }
}