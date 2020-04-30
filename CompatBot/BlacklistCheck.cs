using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using System.Threading.Tasks;

namespace CompatBot.EventHandlers
{
    public class BlacklistCheck : CheckBaseAttribute
    {
        override public async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            var id = ctx.User.Id;
            if(Config.Moderation.blacklistedIds.Contains(ctx.User))
            {
                await ctx.Channel.SendMessageAsync("Sorry, you're not allowed to use commands. Contact a moderator if you believe this is incorrect.").ConfigureAwait(false);
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}