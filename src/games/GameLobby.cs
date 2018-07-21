using Discord;
using System.Collections.Generic;

namespace DiscordQuiplash.Games
{
    class GameLobby
    {
        /*MEMBERS*/
        ulong channelId;
        List<IUser> players;
        Game game;
        bool joinable;

        /*CONSTRUCTORS*/
        public GameLobby(ulong channelID, Game chosenGame, bool CanJoin = true)
        {
            channelId = channelID;
            players = new List<IUser>();
            game = chosenGame;
            joinable = CanJoin;
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

        public Game LobbyGame
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