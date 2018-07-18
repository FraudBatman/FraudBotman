using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace DiscordQuiplash
{
    public class CommandModule : ModuleBase
    {
        [Command("play")]
        [Summary("creates a new Quiplash lobby in the channel, or joins one that has not started")]
        public async Task Play()
        {
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