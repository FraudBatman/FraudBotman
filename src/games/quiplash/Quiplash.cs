using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordQuiplash.Games.Quiplash
{
    class Quiplash : DiscordGame
    {
        /*MEMBERS*/
        DiscordSocketClient client = null;
        SocketTextChannel channel = null;
        List<QuiplashPlayer> players = null;

        /*CONSTRUCTORS*/
        public Quiplash(DiscordSocketClient socketClient, SocketTextChannel gameChannel)
        {
            MinimumPlayers = 3;
            Name = "Quiplash";
            client = socketClient;
            channel = gameChannel;
            players = new List<QuiplashPlayer>();
        }

        /*METHODS*/
        public override async Task Start(List<IUser> users)
        {
            foreach (IUser user in users)
            {
                players.Add(new QuiplashPlayer(client, channel, user));
            }
            var embed = new EmbedBuilder();
            embed.Color = new Color(255, 255, 0);
            embed.Title = "Welcome to Quiplash!";
            embed.AddField("Rules", "This bot will DM you two prompts, one at a time. Respond to each of them with whatever you think is funny.Your answer will be pitted against someone else, and you'll get points based on votes! You will have two minutes to respond to both prompts.");
            await channel.SendMessageAsync("", false, embed);
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

            embed = new EmbedBuilder();

            embed.Color = new Color(255, 255, 0);
            embed.Title = players[winningIndex].User.Username + " wins!";

            players.Sort(delegate (QuiplashPlayer x, QuiplashPlayer y)
            {
                var a = y.Score.CompareTo(x.Score);

                if (a == 0)
                    a = x.User.Id.CompareTo(y.User.Id);

                return a;
            });

            string ranking = "";

            for (int i = 0; i < players.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        ranking += "1st: ";
                        break;
                    case 1:
                        ranking += "2nd: ";
                        break;
                    case 2:
                        ranking += "3rd: ";
                        break;
                    default:
                        ranking += (i + 1) + "th: ";
                        break;
                }

                ranking += $"{players[i].User.Username} | {players[i].Score} points\n";
            }

            embed.AddField("FINAL SCORES", ranking);

            await channel.SendMessageAsync("", false, embed);

            await Task.CompletedTask;
        }

        async Task round(int roundNumber)
        {
            try
            {
                var embed = new EmbedBuilder();
                embed.Color = new Color(255, 255, 0);
                switch (roundNumber)
                {
                    case 1:
                        embed.Title = ("Round 1");
                        embed.Description = ("Rounds are worth 1000 points. The winner bonus is 100 points, and the quiplash bonus is 250 points");
                        break;
                    case 2:
                        embed.Title = ("Round 2");
                        embed.Description = ("Rounds are worth 2000 points. The winner bonus is 200 points, and the quiplash bonus is 500 points");
                        break;
                    case 3:
                        embed.Title = ("Round 3");
                        embed.Description = ("Rounds are worth 3000 points. The winner bonus is 300 points, and the quiplash bonus is 750 points. As a bonus rule, you will only see your first prompt, and the second prompt's answer will be the one you filled in for the first prompt. Good luck!");
                        break;
                    default:
                        break;
                }

                await channel.SendMessageAsync("", false, embed);
                await Task.Delay(10000);

                //additional time to read the extra text
                if (roundNumber == 3)
                {
                    await Task.Delay(5000);
                }

                //create a random to use for prompt matching
                var random = new Random();
                //create list of prompts to use
                var prompts = new List<Prompt>();

                //create a list of unique prompts, while also resetting everyone's prompt count
                foreach (QuiplashPlayer p in players)
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
                        //determine who to pick
                        bool twoExists = false;
                        foreach (QuiplashPlayer player in players)
                        {
                            if (player.PromptsRemaining == 2)
                                twoExists = true;
                        }


                        var playerBIndex = random.Next(playerAIndex + 1, players.Count);
                        if (players[playerBIndex].PromptsRemaining == 0 || playerBIndex == lastOpponent)
                        {
                            continue;
                        }

                        if (players[playerBIndex].PromptsRemaining == 1 && twoExists)
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


                //round 3 special turn
                if (roundNumber == 3)
                {
                    for (int i = 0; i < players.Count; i++)
                    {
                        players[i].FinishedTurn = false;
                        players[i].lastTurn(prompts, cts.Token, i);
                    }
                }
                else
                {
                    //Do async so everyone get's their turn at once
                    for (int i = 0; i < players.Count; i++)
                    {
                        players[i].FinishedTurn = false;
                        players[i].takeTurn(prompts, cts.Token, i);
                    }
                }

                //NEW PLAYER PROGRESS SHEET
                embed = new EmbedBuilder();

                embed.Title = $"Round {roundNumber}";
                embed.Color = new Color(255, 255, 0);

                foreach (QuiplashPlayer player in players)
                {
                    embed.Description += $"{player.User.Username}: {(player.FinishedTurn ? ":white_check_mark:" : ":x:")}\n";
                }

                var message = await channel.SendMessageAsync("", false, embed);

                //two minute timer, or one minute for the final round
                int twoSeconds = 0;

                if (roundNumber != 3)
                {
                    while (twoSeconds < 60)
                    {
                        await Task.Delay(2000);
                        twoSeconds++;

                        //refresh the embed
                        embed = new EmbedBuilder();
                        embed.Title = $"Round {roundNumber}";
                        embed.Color = new Color(255, 255, 0);

                        foreach (QuiplashPlayer player in players)
                        {
                            embed.Description += $"{player.User.Username}: {(player.FinishedTurn ? ":white_check_mark:" : ":x:")}\n";
                        }

                        bool allDone = true;

                        foreach (QuiplashPlayer player in players)
                        {
                            if (!player.FinishedTurn)
                            {
                                allDone = false;
                            }
                        }

                        if (twoSeconds >= 30 && !allDone)
                        {
                            embed.AddField("One minute remaining!", "Hurry up!");
                        }

                        if (allDone)
                        {
                            embed.AddField("All Done!", "Prepare to vote!");
                        }

                        if (!allDone && twoSeconds == 60)
                        {
                            //force end of turn
                            cts.Cancel();
                            embed.AddField("Time's Up!", "Prepare to vote!");
                        }
                        await (message as IUserMessage).ModifyAsync(msg => msg.Embed = embed.Build());

                        if (allDone)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    while (twoSeconds < 30)
                    {
                        await Task.Delay(2000);
                        twoSeconds++;

                        //refresh the embed
                        embed = new EmbedBuilder();
                        embed.Title = $"Round {roundNumber}";
                        embed.Color = new Color(255, 255, 0);

                        foreach (QuiplashPlayer player in players)
                        {
                            embed.Description += $"{player.User.Username}: {(player.FinishedTurn ? ":white_check_mark:" : ":x:")}\n";
                        }

                        bool allDone = true;

                        foreach (QuiplashPlayer player in players)
                        {
                            if (!player.FinishedTurn)
                            {
                                allDone = false;
                            }
                        }

                        if (twoSeconds >= 15 && !allDone)
                        {
                            embed.AddField("30 seconds remaining!", "Hurry up!");
                        }

                        if (allDone)
                        {
                            embed.AddField("All Done!", "Prepare to vote!");
                        }

                        if (!allDone && twoSeconds == 30)
                        {
                            //force end of turn
                            cts.Cancel();
                            embed.AddField("Time's Up!", "Prepare to vote!");
                        }
                        await (message as IUserMessage).ModifyAsync(msg => msg.Embed = embed.Build());

                        if (allDone)
                        {
                            break;
                        }
                    }
                }

                await Task.Delay(5000);

                //vote and scores for each prompt
                foreach (Prompt prompt in prompts)
                {
                    embed = new EmbedBuilder();

                    embed.Color = new Color(255, 255, 0);
                    embed.Title = (prompt.Question);
                    embed.Description = ($"A) {prompt.AnswerA}\nB) {prompt.AnswerB}");

                    //send the prompt and let people vote
                    message = await channel.SendMessageAsync("", false, embed);
                    await message.AddReactionAsync(new Emoji("🇦"));
                    await message.AddReactionAsync(new Emoji("🇧"));
                    await Task.Delay(15000);

                    //get votes and determine score
                    await message.UpdateAsync();
                    var reactions = message.Reactions;
                    var aVotes = reactions.GetValueOrDefault(new Emoji("🇦")).ReactionCount - 1;
                    var bVotes = reactions.GetValueOrDefault(new Emoji("🇧")).ReactionCount - 1;

                    //check both responses for author votes
                    IEnumerator<IUser> voters = null;

                    //this is answer a
                    //make sure there are more than 0 votes!
                    if (aVotes > 0)
                    {
                        voters = message.GetReactionUsersAsync("🇦").GetAwaiter().GetResult().GetEnumerator();
                        while (voters.MoveNext())
                        {
                            //check if voter is the owner of response A
                            if (voters.Current.Id == players[prompt.PlayerA].User.Id)
                            {
                                aVotes--;
                                break;
                            }
                        }
                    }

                    //this is answer b
                    //make sure there are more than zero votes!
                    if (bVotes > 0)
                    {
                        voters = message.GetReactionUsersAsync("🇧").GetAwaiter().GetResult().GetEnumerator();
                        while (voters.MoveNext())
                        {
                            //check if voter is the owner of response A
                            if (voters.Current.Id == players[prompt.PlayerB].User.Id)
                            {
                                bVotes--;
                                break;
                            }
                        }
                    }

                    var aPoints = ((double)aVotes / (double)(aVotes + bVotes)) * 1000 * roundNumber;
                    var bPoints = ((double)bVotes / (double)(aVotes + bVotes)) * 1000 * roundNumber;

                    //to prevent div by zero errors affecting scores
                    if (aVotes + bVotes == 0)
                    {
                        aPoints = 0;
                        bPoints = 0;
                    }

                    //show votes
                    embed.AddField("VOTES", $"\"{prompt.AnswerA}\" - {players[prompt.PlayerA].User.Username} ({aVotes} votes)\n"
                    + $"\"{prompt.AnswerB}\" - {players[prompt.PlayerB].User.Username} ({bVotes} votes)");

                    //finish the message
                    string content = "";
                    //a won
                    if (aPoints > bPoints)
                    {
                        //winner bonus
                        aPoints += 100 * roundNumber;

                        //quiplash bonus
                        if (aVotes + bVotes > 7 && ((double)aVotes / (aVotes + bVotes) > .8))
                        {
                            aPoints += 250 * roundNumber;

                            content +=
                                players[prompt.PlayerA].User.Username + " got a quiplash for a total of " + (int)aPoints + " points. (" + (1500 * roundNumber) + " point bonus for quiplash)\n" +
                                players[prompt.PlayerB].User.Username + " earned " + (int)bPoints + " points.";
                        }

                        else
                        {
                            content +=
                                players[prompt.PlayerA].User.Username + " earned " + (int)aPoints + " points. (" + (500 * roundNumber) + " point bonus for winning)\n" +
                                players[prompt.PlayerB].User.Username + " earned " + (int)bPoints + " points.";
                        }
                    }
                    //b won
                    else if (bPoints > aPoints)
                    {
                        //winner bonus
                        bPoints += 100 * roundNumber;

                        //quiplash bonus
                        if (aVotes + bVotes > 7 && ((double)bVotes / (aVotes + bVotes) > .8))
                        {
                            bPoints += 250 * roundNumber;

                            content +=
                                players[prompt.PlayerA].User.Username + " earned " + (int)aPoints + " points.\n" +
                                players[prompt.PlayerB].User.Username + " got a quiplash for a total of " + (int)bPoints + " points. (" + (1500 * roundNumber) + " point bonus for quiplash)";
                        }
                        else
                        {
                            content +=
                                players[prompt.PlayerA].User.Username + " earned " + (int)aPoints + " points.\n" +
                                players[prompt.PlayerB].User.Username + " earned " + (int)bPoints + " points. (" + (500 * roundNumber) + " point bonus for winning)\n";
                        }
                    }
                    //draw
                    else
                    {
                        content +=
                            players[prompt.PlayerA].User.Username + " earned " + (int)aPoints + " points.\n" +
                            players[prompt.PlayerB].User.Username + " earned " + (int)bPoints + " points.";
                    }

                    embed.AddField("SCORES", content);

                    //add points to the players scores
                    players[prompt.PlayerA].Score += (int)aPoints;
                    players[prompt.PlayerB].Score += (int)bPoints;

                    await (message as IUserMessage).ModifyAsync(msg => msg.Embed = embed.Build());
                    await Task.Delay(5000);
                }

                //skip giving scores on final round
                if (roundNumber != 3)
                {
                    string finalMessage = "";

                    foreach (QuiplashPlayer player in players)
                    {
                        finalMessage += player.User.Username + ": " + player.Score + "\n";
                    }

                    embed = new EmbedBuilder();
                    embed.Color = new Color(255, 255, 0);
                    embed.Title = "SCORES";
                    embed.Description = finalMessage;

                    await channel.SendMessageAsync("", false, embed);
                    await Task.Delay(10000);
                }

            }
            catch (Exception err)
            {
                await channel.SendMessageAsync(err.ToString());
            }
        }

        /*PROPERTIES*/
        public SocketTextChannel Channel
        {
            get { return channel; }
        }
    }
}