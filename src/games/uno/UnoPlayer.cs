using Discord;
using Discord.WebSocket;

namespace DiscordQuiplash.Games.Uno
{
    class UnoPlayer : Player
    {
        /*MEMBERS*/
        /*CONSTRS*/
        public UnoPlayer(DiscordSocketClient socketClient, SocketTextChannel socketChannel, IUser socketUser)
            : base(socketClient, socketChannel, socketUser)
        {

        }
        /*METHODS*/
        /*PROPERS*/
    }
}