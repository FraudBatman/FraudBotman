using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace DiscordQuiplash
{
    class Program
    {
        /*MEMBERS*/
        DiscordSocketClient client = null;
        CommandHandler comhand = null;
        string token = "MzA3Njc1MzI1NTAyODQ5MDI0.DjF0Fg.3k3E3jN0pMbDTPgNdEAkKIxCIH0";

        /*FUNCTIONS*/

        /// <summary>
        /// Main function. Used to start the program asynchronus
        /// </summary>
        public static void Main(string[] args)
                    => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Console.Clear();
            client = new DiscordSocketClient();

            //log into discord
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            //prepare the command handler
            comhand = new CommandHandler();
            await comhand.Install(client);

            /*ACTIONS*/
            client.Log += Log;

            await Task.Delay(-1);
        }

        async Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            await Task.CompletedTask;
        }
    }
}
