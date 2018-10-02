using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordQuiplash.Discord;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DiscordQuiplash
{
    class Program
    {
        /*MEMBERS*/
        DiscordSocketClient client = null;
        CommandHandler comhand = null;
        string token = new StreamReader(new FileStream("data/token.txt", FileMode.Open)).ReadLine();
        bool shutdownFlag = false;

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

            await client.SetGameAsync(".help");
            await client.SetStatusAsync(UserStatus.Online);

            /*ACTIONS*/
            client.Log += Log;
            client.MessageReceived += MessageReceived;

            while (!shutdownFlag)
            {
                await Task.Delay(5000);
            }

            await client.SetStatusAsync(UserStatus.Invisible);

            await client.StopAsync();
            await client.LogoutAsync();
        }

        async Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            await Task.CompletedTask;
        }

        async Task MessageReceived(SocketMessage msg)
        {
            //doot doot
            if (msg.Content == "thank mr. skeltal")
            {
                await msg.Channel.SendMessageAsync("doot doot");
            }
            //shutdown function
            if (msg.Author.Id == 289869983691833344 && msg.Content == "164519888029745152")
            {
                shutdownFlag = true;
                await msg.Channel.SendMessageAsync("Shutdown command received, please hold...");
            }
        }
    }
}
