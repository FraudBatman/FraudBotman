using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordQuiplash.Games.PlayerFibbage
{
    class PlayerFibbagePlayer : Player
    {
        /*MEMBERS*/
        PlayerFibbagePrompt[] prompts;
        string[] responses;
        int promptsRemaining = -1;
        bool finishedTurn = false;
        bool responded = false;
        string response = "";
        ulong lastResponse = 0;
        int answer = 0;

        /*CONSTRUCTORS*/
        public PlayerFibbagePlayer(DiscordSocketClient client, SocketTextChannel channel, IUser user)
            : base(client, channel, user)
        {

        }
        /*METHODS*/
        public async Task AnswerTurn(CancellationToken ct)
        {
            try
            {
                await User.SendMessageAsync("Please answer the following questions as accurately as you can.");
                finishedTurn = false;
                Client.MessageReceived += CheckForResponse;
                foreach (PlayerFibbagePrompt prompt in prompts)
                {
                    responded = false;
                    response = "";
                    await User.SendMessageAsync(prompt.AnswererQuestion);

                    while (!responded)
                    {
                        await Task.Delay(500);
                        ct.ThrowIfCancellationRequested();
                    }
                    prompt.Truth = response;
                }
                await User.SendMessageAsync("That's all! Please return to the game channel.");
                finishedTurn = true;
            }
            catch (OperationCanceledException)
            {
                await User.SendMessageAsync("Time is up! Please return to the game channel.");
            }
            catch (Exception err)
            {
                await ResponseChannel.SendMessageAsync($"IN TURN {User.Id}\n{err.ToString()}");
            }
        }

        /// <summary>
        /// Fills the player's Lies with answers for the current round
        /// </summary>
        /// <param name="prompts">The round's prompts</param>
        /// <param name="roundUsername">The round's subject's username. For giving the player the correct username</param>
        /// <param name="ct">To cancel the process when time runs up</param>
        /// <returns></returns>
        public async Task LieTurn(PlayerFibbagePrompt[] prompts, string roundUsername, CancellationToken ct)
        {
            try
            {
                responses = new string[prompts.Length];
                await User.SendMessageAsync("Please answer the following prompts with answers that may fool your other players into believing they are true");
                finishedTurn = false;
                Client.MessageReceived += CheckForResponse;
                for (int i = 0; i < prompts.Length; i++)
                {
                    responded = false;
                    response = "";
                    await User.SendMessageAsync(prompts[i].GetLiarQuestion(roundUsername));

                    while (!responded)
                    {
                        await Task.Delay(500);
                        ct.ThrowIfCancellationRequested();
                    }
                    if (response == prompts[i].Truth)
                    {
                        await User.SendMessageAsync("You entered the truth! Please answer again, with something different this time.");
                        i--;
                        continue;
                    }
                    responses[i] = response;

                    await User.SendMessageAsync("That's all! Please return to the game channel.");
                    finishedTurn = true;
                }
            }
            catch (OperationCanceledException)
            {
                await User.SendMessageAsync("Time is up! Please return to the game channel.");
            }
            catch (Exception err)
            {
                await ResponseChannel.SendMessageAsync($"IN TURN {User.Id}\n{err.ToString()}");
            }
        }

        public async Task GetAnswer(EmbedBuilder prompt, ulong subjectID, int promptIndex, int playerCount, int playerIndex, CancellationToken ct)
        {
            try
            {

                finishedTurn = false;
                if (subjectID == User.Id)
                {
                    if (promptIndex == 0)
                    {
                        await User.SendMessageAsync("This is your round! Kick back, and wait for the others to answer.");
                    }
                    finishedTurn = true;
                }
                else
                {
                    var sr = new StringReader(prompt.Description);
                    string newDesc = "";
                    string line = "";
                    int counter = 0;

                    //remove the player's answer from the list
                    while (true)
                    {
                        counter++;
                        line = sr.ReadLine();

                        //okay we're done
                        if (line == null || line == "Answer via the DMs!")
                        {
                            break;
                        }
                        if (line == Lies[promptIndex])
                        {
                            continue;
                        }
                        newDesc += $"{counter}) {line}\n";
                    }

                    newDesc += "Answer by typing the number for the answer you want to pick!";
                    prompt.Description = newDesc;

                    Client.MessageReceived += CheckForResponse;

                    await User.SendMessageAsync("", false, prompt);

                    while (true)
                    {
                        responded = false;
                        response = "";
                        answer = 0;
                        while (!responded)
                        {
                            await Task.Delay(500);
                            ct.ThrowIfCancellationRequested();
                        }
                        if (!Int32.TryParse(response, out answer) || answer < 1 || answer > 9)
                        {
                            await User.SendMessageAsync("Send a numeral between 1-9");
                        }
                        else if (answer == playerIndex + 1)
                        {
                            await User.SendMessageAsync("That number is reserved for your answer! Please try another number.");
                        }
                        else
                        {
                            break;
                        }
                    }

                    await User.SendMessageAsync("That's all! Please return to the game channel.");
                }
            }
            catch (OperationCanceledException)
            {
                await User.SendMessageAsync("Time is up! Please return to the game channel.");
            }
            catch (Exception err)
            {
                await ResponseChannel.SendMessageAsync($"IN TURN {User.Id}\n{err.ToString()}");
            }
        }
        private async Task CheckForResponse(SocketMessage msg)
        {
            //did the message recieved come from a dm, is not from botman, and is this not the last response?
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
        public PlayerFibbagePrompt[] Prompts
        {
            get { return prompts; }
            set { prompts = value; }
        }

        /// <summary>
        /// The player's lies for the round
        /// </summary>
        /// <value></value>
        public string[] Lies
        {
            get { return Lies; }
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