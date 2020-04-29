using System.Threading.Tasks;
using CompatBot.EventHandlers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using CompatBot.Commands.Attributes;

namespace CompatBot.Commands
{
    [Group("Blacklist")]
    [BlacklistCheck]
    [Description("Command to add and remove users from the blacklist.")]
    internal sealed class Blacklist : BaseCommandModuleCustom
    {
        [Command("add"), RequiresBotModRole]
        [Description("Adds a user to the blacklist.")]
        public async Task blacklistAdd(CommandContext ctx, [RemainingText] string message)
        {
            var users = ctx.Message.MentionedUsers;
            foreach(DiscordUser user in users)
            {
                bool added = false;
                if(user != ctx.User)
                    added = Config.Moderation.blacklistedIds.Add(user);

                if(added)
                {
                    await ctx.Channel.SendMessageAsync("User " + user.Username + " added to blacklist.").ConfigureAwait(false);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("User " + user.Username + " already in blacklist, or you tried to add yourself.").ConfigureAwait(false);
                }
            }
        }

        [Command("remove"), RequiresBotModRole]
        [Description("Removes a user from the blacklist.")]
        public async Task blacklistRemove(CommandContext ctx, [RemainingText] string message)
        {
            var users = ctx.Message.MentionedUsers;
            foreach(DiscordUser user in users)
            {
                bool removed = Config.Moderation.blacklistedIds.Remove(user);
                if(removed)
                {
                    await ctx.Channel.SendMessageAsync("User " + user.Username + " removed from blacklist.").ConfigureAwait(false);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("User " + user.Username + " not in blacklist.").ConfigureAwait(false);
                }
            }
        }

        [Command("list"), RequiresBotModRole]
        [Description("Lists all of the currently blacklisted users.")]
        public async Task blacklistList(CommandContext context)
        {
            string badPeople = "";
            foreach(DiscordUser user in Config.Moderation.blacklistedIds)
            {
                badPeople += user.Username + ", ID " + user.Id + "\n";
            }
            if(badPeople == "")
            {
                badPeople = "No blacklisted users!";
            }
            await context.Channel.SendMessageAsync("**Blacklisted Users:**\n" + badPeople).ConfigureAwait(false);
        }
    }
}