using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading;
using DSharpPlus;


namespace CompatBot.Commands
{
    [Group("allroles")]
    [Description("Lists all of the roles a member has.")]
    internal sealed class SyncRoles: BaseCommandModuleCustom
    {
        [GroupCommand]
        public async Task getRoles(CommandContext ctx)
        {
            var userRoles = ctx.Member.Roles.ToList();
            if(userRoles.Any())
            {
                await ctx.RespondAsync("**Roles Names (and IDs):**");
                string final_output = "";
                for (int i = 0; i < userRoles.Count; i++)
                {
                    var role_info = ", " + userRoles[i].Name + " (" + userRoles[i].Id + ")";
                    final_output += role_info;
                }
                await ctx.RespondAsync(final_output.Substring(2));
            }
        }

        internal static async Task GetAllMemberRoles(DiscordClient client) 
        {
            {
                var chan = await client.GetChannelAsync(Config.BotChannelId).ConfigureAwait(false);
                var guilds = await client.GetGuildAsync(Config.BotGuildId).ConfigureAwait(false);
                var users = await guilds.GetAllMembersAsync().ConfigureAwait(false);
                var final_output = "**Users (and their roles)**:\n";

                for(int i = 0; i < users.Count; i++)
                {
                    var user = users.ElementAt(i).DisplayName;
                    var roles = users.ElementAt(i).Roles.ToList();
                    var stringRoles = "";
                    foreach(var role in roles)
                    {
                        stringRoles += ", " + role.Name;
                    }
                    if(stringRoles == "")
                    {
                        stringRoles = "No Roles";
                    }
                    final_output += user + " - " + stringRoles.Substring(2) + "\n";
                }
                await chan.SendMessageAsync(final_output);
            }
        }
    }
}