using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordQuiplash
{
    class Player
    {
        /*MEMBERS*/
        DiscordSocketClient client = null;
        SocketTextChannel responseChannel = null;
        SocketGuildUser user = null;
        int score = 0;
        int promptsRemaining = 2;
        bool responded = false;
        string response = "";

        /*CONSTRUCTORS*/
        public Player(DiscordSocketClient socketClient, SocketTextChannel socketChannel, SocketGuildUser socketUser)
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
                //connect message being recieved to response checking
                client.MessageReceived += CheckForResponse;
                foreach (Prompt prompt in prompts)
                {
                    responded = false;
                    response = "";
                    if (prompt.PlayerA == playerID)
                    {
                        //send the prompt
                        await responseChannel.SendMessageAsync(prompt.Question);

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
                        await responseChannel.SendMessageAsync(prompt.Question);

                        //wait for response (see method CheckForResponse)
                        while (!responded)
                        {
                            await Task.Delay(1);
                        }
                        prompt.AnswerB = response;
                    }
                }
                await Task.CompletedTask;
            }
            catch (OperationCanceledException)
            {
                await Task.CompletedTask;
            }
        }

        private async Task CheckForResponse(SocketMessage msg)
        {
            //did the message recieved come from a dm?
            if (msg.Channel.Id == responseChannel.Id)
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

        public SocketGuildUser User
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