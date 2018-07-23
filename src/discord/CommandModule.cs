using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordQuiplash.Games;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordQuiplash.Discord
{
    public class CommandModule : ModuleBase
    {
        /*REMOVED BECAUSE IRRELEVANT
        static Quiplash game = null;
        static ulong lobbyChannel = 0;
        static List<IUser> users = null;
        */

        static List<GameLobby> lobbies;

        [Command("booptheheretic")]
        [Summary("shhh him")]
        public async Task BoopTheHeretic()
        {
            var channel = Context.Channel as SocketTextChannel;
            await channel.SendMessageAsync("boop has nothing nice to say so shhh him\n-Allie, 2018");
        }

        [Command("help")]
        [Summary("Lists all available commands")]
        public async Task Help()
        {

            var commandService = new CommandService();
            await commandService.AddModulesAsync(Assembly.GetEntryAssembly());

            var commands = commandService.Commands;

            var embed = new EmbedBuilder();
            var normies = new EmbedFieldBuilder();

            //normie commands
            normies.Name = "Commands";

            foreach (CommandInfo command in commands)
            {
                normies.Value += $"{command.Name}: {command.Summary}";
            }

            //add the field to the embed
            embed.AddField(normies);

            embed.Color = new Color(255, 255, 0);

            await ReplyAsync("", false, embed);
        }


        /*CURRENTLY WORKING THIS TO SUPPORT MULTIPLE GAMES AT ONCE*/
        [Command("play", RunMode = RunMode.Async)]
        [Summary("Creates a new Quiplash lobby in the channel, or joins one that has not started.")]
        public async Task Play()
        {
            GameLobby lobby = null;

            //check to see if lobby exists
            foreach (GameLobby checkLobby in lobbies)
            {
                if (checkLobby.ChannelID == Context.Channel.Id)
                {
                    lobby = checkLobby;
                }
            }

            //lobby doesn't exist
            if (lobby == null)
            {
                //create lobby and game
                lobbies.Add(new GameLobby(Context.Client, Context.Channel.Id, new DiscordGame()));

                //notify the channel
                await ReplyAsync("A lobby has been started! The game will start in one minute. Type \".play\" if you'd like to join!");

                await Task.Delay(30000);

                await ReplyAsync("The game will start in 30 seconds! Be sure you've joined the game using \".play\"!");

                await Task.Delay(30000);

                //not enough players
                if (lobby.Players.Count < lobby.LobbyGame.MinimumPlayers)
                {
                    await ReplyAsync($"Sorry, you need at least {lobby.LobbyGame.MinimumPlayers} people to play {lobby.LobbyGame.Name}.");
                }

                //get the game started
                else
                {
                    lobby.StartGame();
                    lobbies.Remove(lobby);
                }
            }

            //lobby exists
            else
            {

            }

            /*REMOVED BECAUSE IRRELEVANT
            //create game
            if (lobbyChannel == 0)
            {
                told = true;
                lobbyChannel = Context.Channel.Id;
                await ReplyAsync("A lobby has been started! The game will start in one minute. Type \".play\" if you'd like to join!");

                users = new List<IUser>();
                users.Add(Context.User);

                await Task.Delay(30000);

                await ReplyAsync("The game will start in 30 seconds! Be sure you've joined the game using \".play\"!");

                await Task.Delay(30000);

                if (users.Count < 3)
                {
                    await ReplyAsync("Sorry, you need at least 3 players to play Quiplash.");
                    lobbyChannel = 0;
                    users = null;
                }
                else
                {
                    await ReplyAsync("The lobby is now closed. If you didn't make it in, you can still vote!");

                    await Task.Delay(5000);

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
            if (game != null && game.Channel.Id == Context.Channel.Id && !told)
            {
                told = true;
                await ReplyAsync("Unfortuantely, you can't join an ongoing game. You can still vote during the voting period, however!");
                await Task.CompletedTask;
            }

            //join game
            if (Context.Channel.Id == lobbyChannel && !told)
            {
                told = true;
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
            if (lobbyChannel != Context.Channel.Id && !told)
            {
                await ReplyAsync("Due to Fraud being bad, only one game of Quiplash can be played at a time");
            }
            */

            //just to give it something
            await Task.CompletedTask;
        }

        [Command("reyallie")]
        [Summary("She the king")]
        public async Task PrincesaAllie()
        {
            var channel = Context.Channel as SocketTextChannel;
            await channel.SendMessageAsync("ALL HAIL KING ALLIE :crown:");
        }

        [Command("whogay")]
        [Summary("lol")]
        public async Task WhoGay()
        {
            await ReplyAsync(Context.User.Mention + " is gay lol");
        }
    }
}