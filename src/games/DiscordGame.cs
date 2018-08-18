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

        /*CONSTRUCTOR*/
        public static DiscordGame ConstructGame(string name, GameLobby lobby)
        {
            name = name.ToUpperInvariant();
            switch (name)
            {
                case "QUIPLASH":
                case "Q":
                    return new Quiplash.Quiplash(lobby.Client as DiscordSocketClient, lobby.Channel as SocketTextChannel);
                /* another time maybe
                case "KEEP YOUR FRIENDS CLOSE":
                case "K":
                    return new PlayerFibbage.PlayerFibbage(lobby.Client as DiscordSocketClient, lobby.Channel as SocketTextChannel);
                */
                default:
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