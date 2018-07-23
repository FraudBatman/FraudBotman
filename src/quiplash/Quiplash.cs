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
            await channel.SendMessageAsync("Welcome to Quiplash! This bot will DM you two prompts, one at a time. Respond to each of them with whatever you think is funny. Your answer will be pitted against someone else, and you'll get points based on votes!\nYou will have two minutes to respond to both prompts.");
            await Task.Delay(15000);

            await round(1);
            await round(2);
            await round(3);

            //determine winner
            var winningIndex = 0;
            var highScore = 0;

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].Score > highScore)
                {
                    highScore = players[i].Score;
                    winningIndex = i;
                }
            }

            await channel.SendMessageAsync(players[winningIndex].User.Username + " wins!");

            var message = "FINAL SCORES\n\n";

            foreach (Player player in players)
            {
                message += player.User.Username + ": " + player.Score + " points\n";
            }

            await channel.SendMessageAsync(message);

            await Task.CompletedTask;
        }

        async Task round(int roundNumber)
        {
            switch (roundNumber)
            {
                case 1:
                    await channel.SendMessageAsync("This is round one! Prompts are worth 1000 points, wins are worth 500 points, and quiplashes are worth 1500 points.");
                    break;
                case 2:
                    await channel.SendMessageAsync("This is round 2! Prompts are worth 2000 points, wins are worth 1000 points, and quiplashes are worth 3000 points.");
                    break;
                case 3:
                    await channel.SendMessageAsync("This is round 3! This is the final round. Prompts are worth 3000 points, wins are worth 1500 points, and quiplashes are worth 4500 points.");
                    break;
                default:
                    break;
            }

            await Task.Delay(10000);

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
            int lastOpponent = -1;
            foreach (Prompt prompt in prompts)
            {
                //make sure playerA has a prompt left
                while (players[playerAIndex].PromptsRemaining == 0)
                {
                    playerAIndex++;
                    lastOpponent = -1;
                }

                prompt.PlayerA = playerAIndex;
                players[playerAIndex].PromptsRemaining--;

                //does this process till the prompt is full
                while (prompt.PlayerB == -1)
                {
                    var playerBIndex = random.Next(playerAIndex + 1, players.Count);
                    if (players[playerBIndex].PromptsRemaining == 0 || playerBIndex == lastOpponent)
                    {
                        continue;
                    }

                    prompt.PlayerB = playerBIndex;
                    lastOpponent = playerBIndex;
                    players[playerBIndex].PromptsRemaining--;
                }
            }

            //stuff for canceling when the time runs out
            var cts = new CancellationTokenSource();

            //Do async so everyone get's their turn at once
            for (int i = 0; i < players.Count; i++)
            {
                players[i].takeTurn(prompts, cts.Token, i);
            }

            //two minute timer
            await Task.Delay(60000);

            await channel.SendMessageAsync("One minute remaining!");

            await Task.Delay(60000);

            //force end of turn
            cts.Cancel();

            await channel.SendMessageAsync("Time is up! Prepare to vote!");

            await Task.Delay(5000);

            //vote and scores for each prompt
            foreach (Prompt prompt in prompts)
            {
                await channel.SendMessageAsync("----------");
                //send the prompt and let people vote
                var message = await channel.SendMessageAsync(prompt.ToString());
                await message.AddReactionAsync(new Emoji("🇦"));
                await message.AddReactionAsync(new Emoji("🇧"));
                await Task.Delay(15000);

                //get votes and determine score
                await message.UpdateAsync();
                var reactions = message.Reactions;
                var aVotes = reactions.GetValueOrDefault(new Emoji("🇦")).ReactionCount - 1;
                var bVotes = reactions.GetValueOrDefault(new Emoji("🇧")).ReactionCount - 1;

                var aPoints = ((double)aVotes / (double)(aVotes + bVotes)) * 1000 * roundNumber;
                var bPoints = ((double)bVotes / (double)(aVotes + bVotes)) * 1000 * roundNumber;

                //to prevent div by zero errors affecting scores
                if (aVotes + bVotes == 0)
                {
                    aPoints = 0;
                    bPoints = 0;
                }

                string resultMessage =
                    "\"" + prompt.AnswerA + "\" -" + players[prompt.PlayerA].User.Username + " | " + aVotes + " votes\n" +
                    "\"" + prompt.AnswerB + "\" -" + players[prompt.PlayerB].User.Username + " | " + bVotes + " votes\n\n";

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
                            players[prompt.PlayerA].User.Username + " got a quiplash for a total of " + (int)aPoints + " points. (" + (1500 * roundNumber) + " point bonus for quiplash)\n" +
                            players[prompt.PlayerB].User.Username + " earned " + (int)bPoints + " points.";
                    }

                    else
                    {
                        resultMessage +=
                            players[prompt.PlayerA].User.Username + " earned " + (int)aPoints + " points. (" + (500 * roundNumber) + " point bonus for winning)\n" +
                            players[prompt.PlayerB].User.Username + " earned " + (int)bPoints + " points.";
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
                            players[prompt.PlayerA].User.Username + " earned " + (int)aPoints + " points.\n" +
                            players[prompt.PlayerB].User.Username + " got a quiplash for a total of " + (int)bPoints + " points. (" + (1500 * roundNumber) + " point bonus for quiplash)";
                    }
                    else
                    {
                        resultMessage +=
                            players[prompt.PlayerA].User.Username + " earned " + (int)aPoints + " points.\n" +
                            players[prompt.PlayerB].User.Username + " earned " + (int)bPoints + " points. (" + (500 * roundNumber) + " point bonus for winning)\n";
                    }
                }
                //draw
                else
                {
                    resultMessage +=
                        players[prompt.PlayerA].User.Username + " earned " + (int)aPoints + " points.\n" +
                        players[prompt.PlayerB].User.Username + " earned " + (int)bPoints + " points.";
                }

                //add points to the players scores
                players[prompt.PlayerA].Score += (int)aPoints;
                players[prompt.PlayerB].Score += (int)bPoints;

                await channel.SendMessageAsync(resultMessage);
                await Task.Delay(5000);
            }

            //skip giving scores on final round
            if (roundNumber != 3)

            {
                string finalMessage = "Current Scores:\n\n";

                foreach (Player player in players)
                {
                    finalMessage += player.User.Username + ": " + player.Score + "\n";
                }

                await channel.SendMessageAsync(finalMessage);
                await Task.Delay(10000);
            }
        }

        /*PROPERTIES*/
        public SocketTextChannel Channel
        {
            get { return channel; }
        }
    }
}