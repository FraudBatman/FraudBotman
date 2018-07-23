namespace DiscordQuiplash.Games
{
    public class DiscordGame
    {
        int minimumPlayers;
        string name;

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