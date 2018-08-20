using System.Collections.Generic;

namespace DiscordQuiplash.Games.Uno
{
    public class UnoCard
    {
        /*MEMBERS*/
        UnoColor color;
        ushort number;
        /*CONSTRS*/
        /*STATICS*/
        /*METHODS*/
        /*PROPERS*/
        public UnoColor Color
        {
            get { return color; }
            set { color = value; }
        }

        public ushort Number
        {
            get { return number; }
            set { number = value; }
        }
    }
}