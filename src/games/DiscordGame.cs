using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        /*CONSTRUCTOR*/
        public static DiscordGame ConstructGame(string name, GameLobby lobby)
        {
            name = name.ToUpperInvariant();
            if (name == "QUIPLASH")
            {
                return new Quiplash.Quiplash(lobby.Client as DiscordSocketClient, lobby.Channel as SocketTextChannel);
            }
            else
            {
                return null;
            }
        }

        /*METHODS*/
        public virtual Task Start(List<IUser> users)
        {
            return Task.CompletedTask;
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