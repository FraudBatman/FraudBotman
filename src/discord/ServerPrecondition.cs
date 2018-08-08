using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

public class AllieServerPrecondition : PreconditionAttribute
{
    public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        if (context.Guild.Id == 421943449437208577)
        {
            return PreconditionResult.FromSuccess();
        }
        else
        {
            return PreconditionResult.FromError("");
        }
    }
}
