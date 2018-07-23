using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordQuiplash.Games
{
    class GameLobby
    {
        /*MEMBERS*/
        IDiscordClient client;
        ulong channelId;
        List<IUser> players;
        DiscordGame game;
        bool joinable;

        /*CONSTRUCTORS*/
        public GameLobby(IDiscordClient iDiscordClient, ulong channelID, DiscordGame chosenGame, bool CanJoin = true)
        {
            client = iDiscordClient;
            channelId = channelID;
            players = new List<IUser>();
            game = chosenGame;
            joinable = CanJoin;
        }

        /*METHODS*/
        public void StartGame()
        {
            game.Start();
        }

        /*PROPERTIES*/
        public ulong ChannelID
        {
            get { return channelId; }
            set { channelId = value; }
        }

        public List<IUser> Players
        {
            get { return players; }
            set { players = value; }
        }

        public DiscordGame LobbyGame
        {
            get { return game; }
            set { game = value; }
        }

        public bool Joinable
        {
            get { return joinable; }
            set { joinable = value; }
        }

    }
}