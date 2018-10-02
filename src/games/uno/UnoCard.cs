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
        /// <summary>
        /// Checks if a card can be played
        /// </summary>
        /// <param name="test">Card to attempt to play</param>
        /// <param name="currentCard">Current card in play</param>
        /// <returns>true if playable, false if not</returns>
        public static bool CanPlay(UnoCard test, UnoCard currentCard)
        {
            //for wild cards
            if (test.Color == UnoColor.Wild)
            {
                return true;
            }

            //for regular cards
            if (test.Color == currentCard.Color || test.Number == currentCard.Number)
            {
                return true;
            }

            return false;
        }

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
                deck.Add(new UnoCard(UnoColor.Wild, UnoNumber.Wild));
                deck.Add(new UnoCard(UnoColor.Wild, UnoNumber.WildDraw));
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
        public override string ToString()
        {
            string returnValue = "";

            switch (color)
            {
                case UnoColor.Blue:
                    returnValue += "Blue ";
                    break;
                case UnoColor.Green:
                    returnValue += "Green ";
                    break;
                case UnoColor.Red:
                    returnValue += "Red ";
                    break;
                case UnoColor.Yellow:
                    returnValue += "Yellow ";
                    break;
                case UnoColor.Wild:
                    returnValue += "Wild ";
                    break;
            }

            switch (number)
            {
                case UnoNumber.Zero:
                    returnValue += "0";
                    break;
                case UnoNumber.One:
                    returnValue += "1";
                    break;
                case UnoNumber.Two:
                    returnValue += "2";
                    break;
                case UnoNumber.Three:
                    returnValue += "3";
                    break;
                case UnoNumber.Four:
                    returnValue += "4";
                    break;
                case UnoNumber.Five:
                    returnValue += "5";
                    break;
                case UnoNumber.Six:
                    returnValue += "6";
                    break;
                case UnoNumber.Seven:
                    returnValue += "7";
                    break;
                case UnoNumber.Eight:
                    returnValue += "8";
                    break;
                case UnoNumber.Nine:
                    returnValue += "9";
                    break;
                case UnoNumber.Draw:
                    returnValue += "Draw 2";
                    break;
                case UnoNumber.Skip:
                    returnValue += "Skip";
                    break;
                case UnoNumber.Reverse:
                    returnValue += "Reverse";
                    break;
                case UnoNumber.Wild:
                    break;
                case UnoNumber.WildDraw:
                    returnValue += "Draw 4";
                    break;
            }

            return returnValue;
        }

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