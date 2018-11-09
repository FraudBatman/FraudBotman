using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordQuiplash.Games.Uno
{
    class UnoPlayer : Player
    {
        /*MEMBERS*/
        List<UnoCard> hand;
        bool responded = false;
        bool turnDone = false;
        string response = "";
        ulong lastResponse = 0;

        /*CONSTRS*/
        public UnoPlayer(DiscordSocketClient socketClient, SocketTextChannel socketChannel, IUser socketUser)
            : base(socketClient, socketChannel, socketUser)
        {
            hand = new List<UnoCard>();
        }
        /*METHODS*/
        public void Draw(List<UnoCard> deck, List<UnoCard> discard, ushort count = 7)
        {
            while (count != 0)
            {
                if (deck.Count == 0)
                {
                    deck = UnoCard.Shuffle(discard);
                    discard = new List<UnoCard>();
                }
                hand.Add(deck[0]);
                deck.Remove(deck[0]);
                count--;
            }
        }

        public async Task<UnoCard> TakeTurn(UnoCard currentCard, List<UnoCard> deck, List<UnoCard> discard, CancellationToken ct)
        {
            try
            {
                short answer = -1;
                UnoCard chosenCard = null;
                turnDone = false;
                var embed = new EmbedBuilder();
                embed.Title = "Your Turn!";
                embed.Color = new Color(0, 255, 255);
                var playables = new List<UnoCard>();

                Client.MessageReceived += CheckForResponse;

                foreach (UnoCard card in hand)
                {
                    if (UnoCard.CanPlay(card, currentCard))
                    {
                        playables.Add(card);
                    }
                }

                string choices = "0) Draw a card\n";

                for (int i = 0; i < playables.Count; i++)
                {
                    choices += $"{i + 1}) {playables[i].ToString()}";
                }

                embed.AddField($"Current card: {currentCard.ToString()}", choices);
                await User.SendMessageAsync("", false, embed);

                while (answer == -1)
                {
                    while (!responded)
                    {
                        await Task.Delay(200);
                        ct.ThrowIfCancellationRequested();
                    }

                    if (Int16.TryParse(response, out answer))
                    {
                        if (answer < 0 || answer > playables.Count)
                        {
                            answer = -1;
                        }
                    }
                    responded = false;
                }

                if (answer == 0)
                {
                    Draw(deck, discard, 1);
                    if (UnoCard.CanPlay(hand[(hand.Count - 1)], currentCard))
                    {
                        chosenCard = hand[(hand.Count - 1)];
                        hand.RemoveAt((hand.Count - 1));
                    }
                    else
                    {
                        chosenCard.Color = UnoColor.Wild;
                        chosenCard.Number = UnoNumber.Draw;
                    }
                    turnDone = true;
                }
                else
                {
                    chosenCard = playables[(answer - 1)];
                    hand.Remove(chosenCard);
                    if (chosenCard.Color == UnoColor.Wild)
                    {
                        //FUCK
                    }
                    turnDone = true;
                }

                await User.SendMessageAsync("That's all! Please return to the game channel.");
                return chosenCard;
            }
            catch (OperationCanceledException)
            {
                await User.SendMessageAsync("Time is up! Please return to the game channel.");
                return null;
            }
            catch (Exception err)
            {
                await ResponseChannel.SendMessageAsync(err.ToString());
                return null;
            }
        }

        private async Task CheckForResponse(SocketMessage msg)
        {
            //did the message recieved come from a dm, is not from botman, and isn't the last response?
            if (msg.Channel.Id == User.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().Id && !msg.Author.IsBot && msg.Id != lastResponse)
            {
                //that means they responded
                responded = true;
                response = msg.Content;

                //prevent answer duplication bug
                lastResponse = msg.Id;
            }
            await Task.CompletedTask;
        }

        /*PROPERS*/
        public List<UnoCard> Hand
        {
            get { return hand; }
            set { hand = value; }
        }

        public bool TurnDone
        {
            get { return turnDone; }
            set { turnDone = value; }
        }
    }
}