using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordQuiplash.Games.Uno
{
    public class Uno : DiscordGame
    {
        /*MEMBERS*/
        DiscordSocketClient client = null;
        SocketTextChannel channel = null;
        List<UnoPlayer> players = null;
        List<UnoCard> deck = null;
        List<UnoCard> discard = null;

        /*CONSTRS*/
        public Uno(DiscordSocketClient socketClient, SocketTextChannel gameChannel)
        {
            MinimumPlayers = 2;
            Name = "Uno";
            client = socketClient;
            channel = gameChannel;
            players = new List<UnoPlayer>();
        }

        /*METHODS*/
        public override Task Start(List<IUser> users)
        {


            return Task.CompletedTask;
        }
    }
}