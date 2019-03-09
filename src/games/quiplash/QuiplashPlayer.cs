using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordQuiplash.Games.Quiplash
{
    class QuiplashPlayer : Player
    {
        /*MEMBERS*/
        int score = 0;
        int promptsRemaining = 2;
        bool responded = false;
        string response = "";
        ulong lastResponse = 0;
        bool finishedTurn = false;

        /*CONSTRUCTORS*/
        public QuiplashPlayer(DiscordSocketClient socketClient, SocketTextChannel socketChannel, IUser socketUser) : base(socketClient, socketChannel, socketUser)
        {

        }

        /*METHODS*/
        public async Task takeTurn(List<Prompt> prompts, CancellationToken ct, int playerID)
        {
            try
            {
                await User.SendMessageAsync("Please answer the following 2 prompts as best as you can.");

                finishedTurn = false;

                //connect message being recieved to response checking
                Client.MessageReceived += CheckForResponse;
                foreach (Prompt prompt in prompts)
                {
                    responded = false;
                    response = "";
                    if (prompt.PlayerA == playerID)
                    {
                        //send the prompt
                        await User.SendMessageAsync(prompt.Question);

                        //wait for response (see method CheckForResponse)
                        while (!responded)
                        {
                            await Task.Delay(1);
                            ct.ThrowIfCancellationRequested();
                        }
                        prompt.AnswerA = response;
                    }
                    else if (prompt.PlayerB == playerID)
                    {
                        //send the prompt
                        await User.SendMessageAsync(prompt.Question);

                        //wait for response (see method CheckForResponse)
                        while (!responded)
                        {
                            await Task.Delay(1);
                            ct.ThrowIfCancellationRequested();
                        }
                        prompt.AnswerB = response;
                    }
                }
                await User.SendMessageAsync("That's all! Please return to the game channel.");
                finishedTurn = true;
                await Task.CompletedTask;
            }
            catch (OperationCanceledException)
            {
                await User.SendMessageAsync("Time is up! Please return to the game channel.");
                await Task.CompletedTask;
            }
            catch (Exception err)
            {
                await ResponseChannel.SendMessageAsync(err.ToString());
            }
        }

        public async Task lastTurn(List<Prompt> prompts, CancellationToken ct, int playerID)
        {
            try
            {
                await User.SendMessageAsync("Please answer the following prompts as best as you can. Your answer will be used for both prompts.");

                //set responded off and clear response
                responded = false;
                response = "";

                var text = "";

                //connect message being recieved to response checking
                Client.MessageReceived += CheckForResponse;
                foreach (Prompt prompt in prompts)
                {
                    if (prompt.PlayerA == playerID)
                    {
                        if (!responded)
                        {
                            text += prompt.Question + "\n";
                            //send the prompt
                            //await User.SendMessageAsync(prompt.Question);
                        }
                        /*
                        //wait for response (see method CheckForResponse)
                        while (!responded)
                        {
                            await Task.Delay(1);
                            ct.ThrowIfCancellationRequested();
                        }
                        prompt.AnswerA = response;
                        responded = true;
                        */
                    }
                    else if (prompt.PlayerB == playerID)
                    {
                        if (!responded)
                        {
                            text += prompt.Question + "\n";
                            //send the prompt
                            //await User.SendMessageAsync(prompt.Question);
                        }
                        /*
                        //wait for response (see method CheckForResponse)
                        while (!responded)
                        {
                            await Task.Delay(1);
                            ct.ThrowIfCancellationRequested();
                        }
                        prompt.AnswerB = response;
                        responded = true;
                        */
                    }
                }

                //send the prompts
                await User.SendMessageAsync(text);
                while (!responded)
                {
                    await Task.Delay(50);
                    ct.ThrowIfCancellationRequested();
                }

                foreach (Prompt prompt in prompts)
                {
                    if (prompt.PlayerA == playerID)
                    {
                        prompt.AnswerA = response;
                    }
                    if (prompt.PlayerB == playerID)
                    {
                        prompt.AnswerB = response;
                    }
                }

                await User.SendMessageAsync("That's all! Please return to the game channel.");
                finishedTurn = true;
                await Task.CompletedTask;
            }
            catch (OperationCanceledException)
            {
                await User.SendMessageAsync("Time is up! Please return to the game channel.");
                await Task.CompletedTask;
            }
            catch (Exception err)
            {
                await ResponseChannel.SendMessageAsync(err.ToString());
            }
        }

        private async Task CheckForResponse(SocketMessage msg)
        {
            //did the message recieved come from a dm, is not from botman, and isn't the last response?
            if (msg.Channel.Id == User.GetOrCreateDMChannelAsync().GetAwaiter().GetResult().Id && !msg.Author.IsBot && msg.Id != lastResponse)
            {
                //that means they responded
                responded = true;
                response = msg.Content;

                //prevent answer duplication bug
                lastResponse = msg.Id;
            }
            await Task.CompletedTask;
        }

        /*PROPERTIES*/
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

        public bool FinishedTurn
        {
            get { return finishedTurn; }
            set { finishedTurn = value; }
        }
    }
}