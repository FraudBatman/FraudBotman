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
        static List<GameLobby> lobbies = new List<GameLobby>();

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
                normies.Value += $"{command.Name}: {command.Summary}\n";
            }

            //add the field to the embed
            embed.AddField(normies);

            embed.Color = new Color(255, 255, 0);

            await ReplyAsync("", false, embed);
        }

        [Command("play", RunMode = RunMode.Async)]
        [Summary("Creates a new game lobby in the channel, or joins one that has not started.")]
        public async Task Play(string gameName = "Quiplash")
        {
            try
            {
                if (!DiscordGame.GameExists(gameName) && gameName != null)
                {
                    await ReplyAsync("That game doesn't exist.");
                    return;
                }
                GameLobby lobby = null;

                //check to see if lobby exists
                foreach (GameLobby checkLobby in lobbies)
                {
                    if (checkLobby.Channel.Id == Context.Channel.Id)
                    {
                        lobby = checkLobby;
                    }
                }

                //lobby doesn't exist
                if (lobby == null)
                {
                    //create lobby and game
                    lobby = new GameLobby(Context.Client, Context.Channel, null);
                    lobby.LobbyGame = DiscordGame.ConstructGame(gameName, lobby);
                    lobbies.Add(lobby);

                    lobby.Players.Add(Context.User);

                    //notify the channel
                    await ReplyAsync($"A lobby has been started! The game of {lobby.LobbyGame.Name} will start in one minute. Type \".play\" if you'd like to join!");

                    await Task.Delay(30000);

                    await ReplyAsync("The game will start in 30 seconds! Be sure you've joined the game using \".play\"!");

                    await Task.Delay(30000);

                    lobby.Joinable = false;

                    //not enough players
                    if (lobby.Players.Count < lobby.LobbyGame.MinimumPlayers)
                    {
                        await ReplyAsync($"Sorry, you need at least {lobby.LobbyGame.MinimumPlayers} people to play {lobby.LobbyGame.Name}.");
                        lobbies.Remove(lobby);
                    }

                    //get the game started
                    else
                    {
                        await lobby.StartGame();
                        lobbies.Remove(lobby);
                    }
                }

                //lobby exists
                else
                {
                    if (lobby.Joinable && !lobby.Players.Contains(Context.User))
                    {
                        lobby.Players.Add(Context.User);
                        await ReplyAsync($"{Context.User.Mention} has joined the game!");
                    }
                    else if (lobby.Players.Contains(Context.User))
                    {
                        await ReplyAsync("You already joined this game!");
                    }
                    else
                    {
                        await ReplyAsync("Sorry, you can't join this game.");
                    }
                }

                //just to give it something
                await Task.CompletedTask;
            }
            catch (Exception err)
            {
                await ReplyAsync(err.ToString());
            }
        }

        [Command("shutup", RunMode = RunMode.Async)]
        [Summary("Shuts you up.")]
        public async Task ShutUp()
        {
            var rnd = new Random();
            await Task.Delay(rnd.Next(2, 10) * 1000);
            await ReplyAsync("Shut up.");
        }

        [Command("whogay")]
        [Summary("lol")]
        public async Task WhoGay()
        {
            await ReplyAsync(Context.User.Mention + " is gay lol");
        }
    }
}