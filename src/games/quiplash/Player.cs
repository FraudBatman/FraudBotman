using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordQuiplash.Games.Quiplash
{
    class Player
    {
        /*MEMBERS*/
        DiscordSocketClient client = null;
        SocketTextChannel responseChannel = null;
        IUser user = null;
        int score = 0;
        int promptsRemaining = 2;
        bool responded = false;
        string response = "";

        /*CONSTRUCTORS*/
        public Player(DiscordSocketClient socketClient, SocketTextChannel socketChannel, IUser socketUser)
        {
            client = socketClient;
            responseChannel = socketChannel;
            user = socketUser;
        }

        /*METHODS*/
        public async Task takeTurn(List<Prompt> prompts, CancellationToken ct, int playerID)
        {
            try
            {
                await user.SendMessageAsync("Please answer the following 2 prompts as best as you can.");

                //connect message being recieved to response checking
                client.MessageReceived += CheckForResponse;
                foreach (Prompt prompt in prompts)
                {
                    responded = false;
                    response = "";
                    if (prompt.PlayerA == playerID)
                    {
                        //send the prompt
                        await user.SendMessageAsync(prompt.Question);

                        //wait for response (see method CheckForResponse)
                        while (!responded)
                        {
                            await Task.Delay(1);
                        }
                        prompt.AnswerA = response;
                    }
                    else if (prompt.PlayerB == playerID)
                    {
                        //send the prompt
                        await user.SendMessageAsync(prompt.Question);

                        //wait for response (see method CheckForResponse)
                        while (!responded)
                        {
                            await Task.Delay(1);
                        }
                        prompt.AnswerB = response;
                    }
                }
                await user.SendMessageAsync("That's all! Please return to the game channel.");
                await Task.CompletedTask;
            }
            catch (OperationCanceledException)
            {
                await user.SendMessageAsync("Time is up! Please return to the game channel.");
                await Task.CompletedTask;
            }
            catch (Exception err)
            {
                await responseChannel.SendMessageAsync(err.ToString());
            }
        }

        private async Task CheckForResponse(SocketMessage msg)
        {
            //did the message recieved come from a dm?
            if (msg.Channel.Id == user.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().Id && !msg.Author.IsBot)
            {
                //that means they responded
                responded = true;
                response = msg.Content;
            }
            await Task.CompletedTask;
        }

        /*PROPERTIES*/
        public SocketTextChannel ResponseChannel
        {
            get { return responseChannel; }
            set { responseChannel = value; }
        }

        public IUser User
        {
            get { return user; }
            set { user = value; }
        }

        public int Score
        {
            get { return score; }
            set { score = value; }
        }

        public int PromptsRemaining
        {
            get { return promptsRemaining; }
            set { promptsRemaining = value; }
        }
    }
}