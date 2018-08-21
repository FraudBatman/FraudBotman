using Discord;
using Discord.WebSocket;
using System.Collections.Generic;

namespace DiscordQuiplash.Games.Uno
{
    class UnoPlayer : Player
    {
        /*MEMBERS*/
        List<UnoCard> hand;

        /*CONSTRS*/
        public UnoPlayer(DiscordSocketClient socketClient, SocketTextChannel socketChannel, IUser socketUser)
            : base(socketClient, socketChannel, socketUser)
        {
            hand = new List<UnoCard>();
        }
        /*METHODS*/
        public void Draw(List<UnoCard> deck, ushort count = 7)
        {
            while (count != 0)
            {
                hand.Add(deck[0]);
                deck.Remove(deck[0]);
                count--;
            }
        }

        /*PROPERS*/
        public List<UnoCard> Hand
        {
            get { return hand; }
            set { hand = value; }
        }
    }
}