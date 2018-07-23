using System;
using System.Collections.Generic;
namespace DiscordQuiplash.Games
{
    public class DiscordGame
    {
        int minimumPlayers;
        string name;

        /*STATIC*/
        public static bool GameExists(string name)
        {
            name = name.ToUpperInvariant();

            var games = new List<String>();

            games.Add("QUIPLASH");

            if (games.Contains(name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int MinimumPlayers
        {
            get { return minimumPlayers; }
            set { minimumPlayers = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}