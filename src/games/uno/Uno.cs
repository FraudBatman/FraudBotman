using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading;
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
            string updateMessage = "";

            while (true)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    //update message
                    await (msg as IUserMessage).ModifyAsync(message => message.Embed = embedEdit(i, updateMessage).Build());
                    //give turn
                    var cts = new CancellationTokenSource();
                    var played = players[i].TakeTurn(currentCard, deck, discard, cts.Token);

                    var seconds = 60;
                    while (seconds > 0)
                    {
                        await Task.Delay(5000);
                        if (players[i].TurnDone)
                            break;
                        seconds -= 5;
                    }
                    var playedCard = played.GetAwaiter().GetResult();
                    if (playedCard == null)
                    {
                        cts.Cancel();
                        players[i].Draw(deck, discard, 1);
                        updateMessage = players[i].User.Username + " did not play in time and is forced to draw one card.";
                    }
                    if (playedCard.Color == UnoColor.Wild && playedCard.Number == UnoNumber.Draw)
                    {
                        updateMessage = players[i].User.Username + " draws.";
                    }
                    //add question in player for what color the player selects for a wild, THEN
                    //check if card is wild, and act accordingly
                    //check if card is draw or skip, and act accordingly
                    //check if card is reverse, and act accordingly
                }
            }

            await Task.CompletedTask;
        }

        private EmbedBuilder embedEdit(int playerIndex, string updateMessage = "")
        {
            var embed = new EmbedBuilder();
            embed.Title = "Uno";
            embed.Color = new Color(255, 255, 0);
            string playerList = "";
            foreach (UnoPlayer player in players)
            {
                playerList += player.User.Username + $": {player.Hand.Count}\n";
            }
            string turnIndicator = $"{players[playerIndex].User.Username}'s turn!";

            embed.AddField($"Current Card: {currentCard}", playerList);
            embed.AddField(turnIndicator, updateMessage);
            return embed;
        }
    }
}