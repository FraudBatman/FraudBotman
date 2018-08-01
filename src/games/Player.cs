using Discord;
using Discord.WebSocket;

namespace DiscordQuiplash.Games
{
    class Player
    {
        DiscordSocketClient client = null;
        SocketTextChannel responseChannel = null;
        IUser user = null;

        public Player(DiscordSocketClient socketClient, SocketTextChannel socketChannel, IUser socketUser)
        {
            client = socketClient;
            responseChannel = socketChannel;
            user = socketUser;
        }

        protected DiscordSocketClient Client
        {
            get { return client; }
            set { client = value; }
        }

        protected SocketTextChannel ResponseChannel
        {
            get { return responseChannel; }
            set { responseChannel = value; }
        }

        public IUser User
        {
            get { return user; }
            set { user = value; }
        }
    }
}