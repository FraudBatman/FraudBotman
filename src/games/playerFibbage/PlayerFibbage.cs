using Discord;
using Discord.WebSocket;
using DiscordQuiplash.Games;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordQuiplash.Games.PlayerFibbage
{
    class PlayerFibbage : DiscordGame
    {
        /*MEMBERS*/
        DiscordSocketClient client = null;
        SocketTextChannel channel = null;
        List<PlayerFibbagePlayer> players = null;

        /*CONSTRUCTORS*/
        public PlayerFibbage(DiscordSocketClient socketClient, SocketTextChannel gameChannel)
        {
            MinimumPlayers = 3;
            Name = "Keep Your Friends Close";
            client = socketClient;
            channel = gameChannel;
            players = new List<PlayerFibbagePlayer>();
        }

        /*METHODS*/
        public override async Task Start(List<IUser> users)
        {
            foreach (IUser user in users)
            {
                players.Add(new PlayerFibbagePlayer(client, channel, user));
            }

            var embed = new EmbedBuilder();
            embed.Color = new Color(255, 255, 0);
            embed.Title = "Welcome to Keep Your Friends Close!";
            embed.AddField("Rules", "The game will start with each player answering questions about themselves. For each round, the other players will make false answers to trick the other players. You'll earn points for picking the right answer and avoiding the wrong ones.");
            await channel.SendMessageAsync("", false, embed);
            await Task.Delay(15000);

            await answerRound();
            foreach (PlayerFibbagePlayer player in players)
            {
                await playerRound(player);
            }
        }

        async Task answerRound()
        {
            try
            {
                //do the opening round announcement
                var embed = new EmbedBuilder();
                embed.Color = new Color(255, 255, 0);
                embed.Title = "Answering Round";
                embed.Description = "This is the answering round! Please answer the following questions as truthfully as you can.";
                await channel.SendMessageAsync("", false, embed);
                await Task.Delay(15000);

                //create a series of random prompts
                var prompts = new List<PlayerFibbagePrompt>();
                if (players.Count == 3)
                {
                    foreach (PlayerFibbagePlayer player in players)
                    {
                        var count = 3;
                        player.Prompts = new PlayerFibbagePrompt[3];
                        while (count != 0)
                        {
                            PlayerFibbagePrompt prompt = null;
                            do
                            {
                                prompt = new PlayerFibbagePrompt();
                            } while (prompts.Contains(prompt));
                            prompts.Add(prompt);
                            count--;
                        }
                    }
                }
                else
                {
                    foreach (PlayerFibbagePlayer player in players)
                    {
                        var count = 2;
                        player.Prompts = new PlayerFibbagePrompt[2];
                        while (count != 0)
                        {
                            PlayerFibbagePrompt prompt = null;
                            do
                            {
                                prompt = new PlayerFibbagePrompt();
                            } while (prompts.Contains(prompt));
                            prompts.Add(prompt);
                            count--;
                        }
                    }
                }

                //randomly assign those prompts to each of the players
                foreach (PlayerFibbagePrompt prompt in prompts)
                {
                    var rnd = new Random();
                    int index;
                    do
                    {
                        index = rnd.Next(players.Count);
                    } while (players[index].PromptsRemaining == 0);

                    //chosen player gets the prompt at the index based on their number of prompts remaining
                    players[index].Prompts[players[index].PromptsRemaining - 1] = prompt;
                    players[index].PromptsRemaining--;
                }

                //give players turns to respond
                var cts = new CancellationTokenSource();
                foreach (PlayerFibbagePlayer player in players)
                {
                    player.AnswerTurn(cts.Token);
                }

                //monitor player progress
                await MonitorProgress("Answering Round", "Prepare to lie!", (players.Count == 3 ? 18000 : 12000), cts);
            }
            catch (Exception err)
            {
                await channel.SendMessageAsync(err.ToString());
            }
        }

        async Task playerRound(PlayerFibbagePlayer roundPlayer)
        {
            var embed = new EmbedBuilder();
            embed.Color = new Color(255, 255, 0);
            embed.Title = $"It's {roundPlayer.User.Username}'s Turn!";
            embed.Description = $"Prepare to create lies about {roundPlayer.User.Mention}! Picking the correct answer is worth 1000 points, and fooling someone with your lie is worth 500 points.";
            await channel.SendMessageAsync("", false, embed);
            await Task.Delay(15000);

            //give every player their turn
            var cts = new CancellationTokenSource();
            foreach (PlayerFibbagePlayer player in players)
            {
                player.LieTurn(roundPlayer.Prompts, roundPlayer.User.Username, cts.Token);
            }
            //track their progress
            await MonitorProgress($"{roundPlayer.User.Username}'s Round", "Prepare to Answer!", (players.Count == 3 ? 18000 : 12000), cts);

            for (int i = 0; i < roundPlayer.Prompts.Length; i++)
            {
                cts = new CancellationTokenSource();
                roundPlayer.Prompts[i].Lies = new string[players.Count];
                //connect responses to prompt
                for (int innerI = 0; i < players.Count; innerI++)
                {
                    //i is for the prompt number, and inner i is for the player's index
                    roundPlayer.Prompts[i].Lies[innerI] = players[innerI].Lies[i];
                }

                //present prompt and wait for votes
                var promptEmbed = roundPlayer.Prompts[i].PresentPrompt(roundPlayer.User.Username);
                await channel.SendMessageAsync("", false, promptEmbed);

                foreach (PlayerFibbagePlayer player in players)
                {

                }

                await MonitorProgress(roundPlayer.Prompts[i].GetLiarQuestion(roundPlayer.User.Username), "Prepare for results!", 30000, cts);
            }
        }

        async Task MonitorProgress(string progressTitle, string closingMessage, int timerCount, CancellationTokenSource cts = null)
        {
            var embed = new EmbedBuilder();
            embed.Title = progressTitle;
            embed.Color = new Color(255, 255, 0);
            embed.Description = "";

            foreach (PlayerFibbagePlayer player in players)
            {
                embed.Description += $"{player.User.Username}: {(player.FinishedTurn ? ":white_check_mark:" : ":x:")}\n";
            }
            embed.AddField("Time Remaining", timerCount);

            var msg = channel.SendMessageAsync("", false, embed);
            var allDone = false;

            while (timerCount < 0 || !allDone)
            {
                await Task.Delay(1000);
                timerCount--;

                //refresh embed
                embed.Description = "";
                foreach (PlayerFibbagePlayer player in players)
                {
                    embed.Description += $"{player.User.Username}: {(player.FinishedTurn ? ":white_check_mark:" : ":x:")}\n";
                }
                embed.Fields[0].Value = timerCount;

                //check if done
                allDone = true;

                foreach (PlayerFibbagePlayer player in players)
                {
                    if (!player.FinishedTurn)
                    {
                        allDone = false;
                    }
                }

                if (timerCount == 0)
                {
                    cts.Cancel();
                    embed.Fields[0].Name = "Times Up!";
                    embed.Fields[0].Value = closingMessage;
                }
                if (allDone)
                {
                    embed.Fields[0].Name = "All Done!";
                    embed.Fields[0].Value = closingMessage;
                }

                await (msg as IUserMessage).ModifyAsync(message => message.Embed = embed.Build());
            }

            await Task.Delay(5000);
        }

        /*PROPERTIES*/
    }
}