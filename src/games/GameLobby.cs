using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordQuiplash.Games
{
    public class GameLobby
    {
        /*MEMBERS*/
        IDiscordClient client;
        IMessageChannel gameChannel;
        List<IUser> players;
        DiscordGame game;
        bool joinable;

        /*CONSTRUCTORS*/
        public GameLobby(IDiscordClient iDiscordClient, IMessageChannel channel, DiscordGame chosenGame, bool CanJoin = true)
        {
            client = iDiscordClient;
            gameChannel = channel;
            players = new List<IUser>();
            game = chosenGame;
            joinable = CanJoin;
        }

        /*METHODS*/
        public IDiscordClient Client
        {
            get { return client; }
            set { client = value; }
        }

        public async Task StartGame()
        {
            await game.Start();
        }

        /*PROPERTIES*/
        public IMessageChannel Channel
        {
            get { return gameChannel; }
            set { gameChannel = value; }
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