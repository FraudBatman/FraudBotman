using System;
using System.Collections.Generic;

namespace DiscordQuiplash.Games.Uno
{
    public class UnoCard
    {
        /*MEMBERS*/
        UnoColor color;
        UnoNumber number;

        /*CONSTRS*/
        public UnoCard(UnoColor cardColor, UnoNumber cardNumber)
        {
            color = cardColor;
            number = cardNumber;
        }

        /*STATICS*/
        public static List<UnoCard> CreateDeck()
        {
            var deck = new List<UnoCard>();
            for (int i = 0; i < 4; i++)
            {
                UnoColor color = (UnoColor)i;

                deck.Add(new UnoCard(color, 0));

                for (int innerI = 1; i < 13; i++)
                {
                    deck.Add(new UnoCard(color, (UnoNumber)innerI));
                    deck.Add(new UnoCard(color, (UnoNumber)innerI));
                }
            }

            for (int i = 0; i < 4; i++)
            {
                deck.Add(new UnoCard(UnoColor.Wild, UnoNumber.Zero));
                deck.Add(new UnoCard(UnoColor.Wild, UnoNumber.Draw));
            }

            deck = UnoCard.Shuffle(deck);
            return deck;
        }

        public static List<UnoCard> Shuffle(List<UnoCard> toShuffle)
        {
            var returnValue = new List<UnoCard>();
            var rand = new Random();
            while (toShuffle.Count != 0)
            {
                var card = toShuffle[rand.Next(toShuffle.Count)];
                returnValue.Add(card);
                toShuffle.Remove(card);
            }
            return returnValue;
        }

        /*METHODS*/
        /*PROPERS*/
        public UnoColor Color
        {
            get { return color; }
            set { color = value; }
        }

        public UnoNumber Number
        {
            get { return number; }
            set { number = value; }
        }
    }
}