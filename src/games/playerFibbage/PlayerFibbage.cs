using Discord;
using Discord.WebSocket;
using DiscordQuiplash.Games;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordQuiplash.Games.PlayerFibbage
{
    class PlayerFibbage : DiscordGame
    {
        /*MEMBERS*/
        DiscordSocketClient client = null;
        SocketTextChannel channel = null;
        List<PlayerFibbagePlayer> players = null;

        /*CONSTRUCTORS*/
        public PlayerFibbage(DiscordSocketClient socketClient, SocketTextChannel gameChannel)
        {
            MinimumPlayers = 3;
            Name = "Fibbage (Enough About You)";
            client = socketClient;
            channel = gameChannel;
            players = new List<PlayerFibbagePlayer>();
        }

        /*METHODS*/
        public override async Task Start(List<IUser> users)
        {
            foreach (IUser user in users)
            {
                players.Add(new PlayerFibbagePlayer(client, channel, user));
            }

            var embed = new EmbedBuilder();
        }

        /*PROPERTIES*/
    }
}