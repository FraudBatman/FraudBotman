using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordQuiplash
{
    public class CommandModule : ModuleBase
    {
        Quiplash game = null;
        ulong lobbyChannel = 0;
        List<IUser> users = null;

        [Command("play", RunMode = RunMode.Async)]
        [Summary("creates a new Quiplash lobby in the channel, or joins one that has not started")]
        public async Task Play()
        {
            //create game
            if (lobbyChannel == 0)
            {
                lobbyChannel = Context.Channel.Id;
                await ReplyAsync("A lobby has been started! The game will start in one minute. Type \".play\" if you'd like to join!");

                users = new List<IUser>();
                users.Add(Context.User);

                await Task.Delay(60000);

                if (users.Count < 3)
                {
                    await ReplyAsync("Sorry, you need at least 3 players to play Quiplash.");
                    lobbyChannel = 0;
                    users = null;
                }
                else
                {
                    var players = new List<SocketGuildUser>();
                    foreach (IUser user in users)
                    {
                        players.Add(user as SocketGuildUser);
                    }
                    game = new Quiplash(Context.Client as DiscordSocketClient, Context.Channel as SocketTextChannel, players);
                    await game.gameStart();

                    //clean
                    game = null;
                    lobbyChannel = 0;
                    users = null;
                }
            }

            //game already started
            if (game != null && game.Channel.Id == Context.Channel.Id)
            {
                await ReplyAsync("Unfortuantely, you can't join an ongoing game.");
                await Task.CompletedTask;
            }

            //join game
            if (Context.Channel.Id == lobbyChannel)
            {
                if (users.Contains(Context.User))
                {
                    await ReplyAsync("You are already in this game!");
                }
                else if (users.Count == 8)
                {
                    await ReplyAsync("Sorry, there can only be 8 total players.");
                }
                else
                {
                    await ReplyAsync(Context.User.Mention + " has joined the game!");
                    users.Add(Context.User);
                }
            }

            //stay in the channel
            if (lobbyChannel != Context.Channel.Id)
            {
                await ReplyAsync("Due to Fraud being bad, only one game of quiplash can be played at a time");
            }

            //just to give it something
            await Task.CompletedTask;
        }

        [Command("princesaallie")]
        [Summary("She the princess")]
        public async Task PrincesaAllie()
        {
            var channel = Context.Channel as SocketTextChannel;
            await channel.SendMessageAsync("ALL HAIL PRINCESS @Alliex92#5761 :crown:");
        }

        [Command("booptheheretic")]
        [Summary("He not")]
        public async Task BoopTheHeretic()
        {
            var channel = Context.Channel as SocketTextChannel;
            await channel.SendMessageAsync("boop has nothing nice to say so shhh him");
        }
    }
}