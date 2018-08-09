using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

public class AllieModPrecondition : PreconditionAttribute
{
    public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        if ((context.User as IGuildUser).GuildPermissions.KickMembers)
        {
            return PreconditionResult.FromSuccess();
        }
        else
        {
            return PreconditionResult.FromError("");
        }
    }
}