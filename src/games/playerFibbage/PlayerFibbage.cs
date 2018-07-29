using Discord.WebSocket;
using System.Collections.Generic;

namespace DiscordQuiplash.Games.PlayerFibbage
{
    class PlayerFibbage : DiscordGame
    {
        /*MEMBERS*/
        DiscordSocketClient client = null;
        SocketTextChannel channel = null;
        List<Player> players = null;

        /*CONSTRUCTORS*/
        public PlayerFibbage(DiscordSocketClient socketClient, SocketTextChannel gameChannel)
        {
            MinimumPlayers = 3;
            Name = "Fibbage (Enough About You)";
            client = socketClient;
            channel = gameChannel;
            players = new List<Player>();
        }

        /*METHODS*/
        /*PROPERTIES*/
    }
}