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
        UnoCard currentCard = null;

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
        public override async Task Start(List<IUser> users)
        {
            deck = UnoCard.CreateDeck();
            discard = new List<UnoCard>();

            foreach (UnoPlayer player in players)
            {
                player.Draw(deck, discard);
            }
            currentCard = deck[0];
            deck.Remove(deck[0]);

            var embed = new EmbedBuilder();
            embed.Title = "Uno";
            embed.Color = new Color(255, 255, 0);
            string playerList = "";
            foreach (UnoPlayer player in players)
            {
                playerList += player.User.Username + $": {player.Hand.Count}\n";
            }
            string turnIndicator = $"{players[0].User.Username}'s turn!";

            embed.AddField($"Current Card: {currentCard}", playerList);
            embed.AddField(turnIndicator, "");

            var msg = await channel.SendMessageAsync("", false, embed);

            while (true)
            {
                foreach (UnoPlayer player in players)
                {
                    //check if card is draw or skip, and act accordingly
                    //update message
                    //give turn
                    //check if card is reverse, and act accordingly
                }
            }

            await Task.CompletedTask;
        }
    }
}