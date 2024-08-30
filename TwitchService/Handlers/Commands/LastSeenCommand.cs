using MovieHandlerService.Handlers;
using TwitchLib.Client.Events;

namespace TwitchService.Handlers.Commands;

public class LastSeenCommand() : Command("lastseen")
{
    QueryHandler _queryHandler = new QueryHandler();

    public override void Execute(OnChatCommandReceivedArgs e, ChatHandler chatHandler)
    {
        var queriedMovie = e.Command.ArgumentsAsString;

        if (queriedMovie == null)
        {
            chatHandler.SendMessage(e.Command.ChatMessage.Channel, "No argument provided!");
            return;
        }

        var candidateMovies = _queryHandler.MovieNameFindLastWatchedDate(queriedMovie, false);
        // TODO: complete rest of logic...
    }
}