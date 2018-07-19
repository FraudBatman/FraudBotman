using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordQuiplash
{
    class Quiplash
    {
        /*MEMBERS*/
        DiscordSocketClient client = null;
        SocketTextChannel channel = null;
        List<Player> players = null;

        /*CONSTRUCTORS*/
        public Quiplash(DiscordSocketClient socketClient, SocketTextChannel gameChannel, List<SocketGuildUser> users)
        {
            client = socketClient;
            channel = gameChannel;

            //convert users to players
            players = new List<Player>();

            foreach (SocketGuildUser user in users)
            {
                players.Add(new Player(client, channel, user));
            }
        }

        /*METHODS*/
        public async Task gameStart()
        {
            await round(1);
            await round(2);
            await round(3);
            await Task.CompletedTask;
        }

        async Task round(int roundNumber)
        {
            //create a random to use for prompt matching
            var random = new Random();
            //create list of prompts to use
            var prompts = new List<Prompt>();

            //create a list of unique prompts, while also resetting everyone's prompt count
            foreach (Player p in players)
            {
                p.PromptsRemaining = 2;
                Prompt prompt = null;
                do
                {
                    prompt = new Prompt();
                } while (prompts.Contains(prompt));

                prompts.Add(prompt);
            }

            //assign prompts to players
            int playerAIndex = 0;
            foreach (Prompt prompt in prompts)
            {
                //make sure playerA has a prompt left
                while (players[playerAIndex].PromptsRemaining == 0)
                {
                    playerAIndex++;
                }

                prompt.PlayerA = playerAIndex;
                players[playerAIndex].PromptsRemaining--;

                //does this process till the prompt is full
                while (prompt.PlayerB == -1)
                {
                    var playerBIndex = random.Next(playerAIndex + 1, players.Count);
                    if (players[playerBIndex].PromptsRemaining == 0)
                    {
                        continue;
                    }

                    prompt.PlayerB = playerBIndex;
                }
            }

            //give players their turns
            var responses = new List<Tuple<SocketGuildUser, string, string>>();

            //stuff for canceling when the time runs out
            var cts = new CancellationTokenSource();

            for (int i = 0; i < players.Count; i++)
            {
                players[i].takeTurn(prompts, cts.Token, i).GetAwaiter().GetResult();
            }

            //wait 2 minutes for players to respond
            await Task.Delay(120000);

            //end everyone's turn if they didn't finish
            cts.Cancel();

            //vote and scores for each prompt
            foreach (Prompt prompt in prompts)
            {
                //send the prompt and let people vote
                var message = await channel.SendMessageAsync(prompt.ToString());
                await message.AddReactionAsync(new Emoji("🅰️"));
                await message.AddReactionAsync(new Emoji("🅱️"));
                await Task.Delay(30000);

                //get votes and determine score
                var reactions = message.Reactions;
                var aVotes = reactions.GetValueOrDefault(new Emoji("🅰️")).ReactionCount - 1;
                var bVotes = reactions.GetValueOrDefault(new Emoji("🅱️")).ReactionCount - 1;

                var aPoints = aVotes / (aVotes + bVotes) * 1000 * roundNumber;
                var bPoints = bVotes / (aVotes + bVotes) * 1000 * roundNumber;

                string resultMessage =
                    "\"" + prompt.AnswerA + "\" -" + players[prompt.PlayerA].User.Nickname + " | " + aVotes + " votes (" + (aVotes / aVotes + bVotes) + "%)\n" +
                    "\"" + prompt.AnswerB + "\" -" + players[prompt.PlayerB].User.Nickname + " | " + bVotes + " votes (" + (bVotes / aVotes + bVotes) + "%)\n\n";

                //finish the message

                //a won
                if (aPoints > bPoints)
                {
                    //winner bonus
                    aPoints += 500 * roundNumber;

                    //quiplash bonus
                    if (aVotes + bVotes > 7 && ((double)aVotes / (aVotes + bVotes) > .8))
                    {
                        aPoints += 1000 * roundNumber;

                        resultMessage +=
                            players[prompt.PlayerA].User.Nickname + " got a quiplash for a total of " + aPoints + " points. (" + (1500 * roundNumber) + " point bonus for quiplash)\n" +
                            players[prompt.PlayerB].User.Nickname + " earned " + bPoints + " points.";
                    }

                    else
                    {
                        resultMessage +=
                            players[prompt.PlayerA].User.Nickname + " earned " + aPoints + " points. (" + (500 * roundNumber) + " point bonus for winning)\n" +
                            players[prompt.PlayerB].User.Nickname + " earned " + bPoints + " points.";
                    }
                }
                //b won
                else if (bPoints > aPoints)
                {
                    //winner bonus
                    bPoints += 500 * roundNumber;

                    //quiplash bonus
                    if (aVotes + bVotes > 7 && ((double)bVotes / (aVotes + bVotes) > .8))
                    {
                        bPoints += 1000 * roundNumber;

                        resultMessage +=
                                                    players[prompt.PlayerA].User.Nickname + " earned " + aPoints + "points.\n" +
                                                    players[prompt.PlayerB].User.Nickname + " got a quiplash for a total of " + bPoints + " points. (" + (1500 * roundNumber) + " point bonus for quiplash)";
                    }
                    else
                    {
                        resultMessage +=
                            players[prompt.PlayerA].User.Nickname + " earned " + aPoints + "points.\n" +
                            players[prompt.PlayerB].User.Nickname + " earned " + bPoints + " points. (" + (500 * roundNumber) + " point bonus for winning)\n";
                    }
                }
                //draw
                else
                {
                    resultMessage +=
                        players[prompt.PlayerA].User.Nickname + " earned " + aPoints + "points.\n" +
                        players[prompt.PlayerB].User.Nickname + " earned " + bPoints + " points.";
                }

                //add points to the players scores
                players[prompt.PlayerA].Score += aPoints;
                players[prompt.PlayerB].Score += bPoints;

                await channel.SendMessageAsync(resultMessage);
                await Task.Delay(10000);
            }

            string finalMessage = "Current Scores:\n\n";

            foreach (Player player in players)
            {
                finalMessage += player.User.Nickname + ": " + player.Score + "\n";
            }

            await channel.SendMessageAsync(finalMessage);
            await Task.Delay(10000);
        }

        /*PROPERTIES*/
        public SocketTextChannel Channel
        {
            get { return channel; }
        }
    }
}