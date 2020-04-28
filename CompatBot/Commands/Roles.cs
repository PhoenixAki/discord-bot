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
    [Description("Description")]
    internal sealed class SyncRoles: BaseCommandModuleCustom
    {
        [GroupCommand]
        public async Task getRoles(CommandContext ctx)
        {
            var userRoles = ctx.Member.Roles.ToList();
            Console.WriteLine(userRoles);
            if(userRoles.Any())
            {
                var ids = "";
                var names = "";
                for (int i = 0; i < userRoles.Count; i++)
                {
                    Console.WriteLine(userRoles[i]);
                    var id_line = userRoles[i].Id + "\n";
                    var name_line = userRoles[i].Name + "\n";
                    ids += id_line;
                    names += name_line;
                }
                await ctx.RespondAsync(ids);
                await ctx.RespondAsync(names);
                //await ctx.RespondAsync(GetAllMemberRoles(Config.Cts.Token));
            }
        }

        internal static async Task GetAllMemberRoles(DiscordClient client) 
        {
            {
                var chan = await client.GetChannelAsync(Config.BotChannelId).ConfigureAwait(false);
                var guilds = await client.GetGuildAsync(Config.BotGuildId).ConfigureAwait(false);
                var users = await guilds.GetAllMembersAsync().ConfigureAwait(false);

                for(int i = 0; i < users.Count; i++)
                {
                    var user = users.ElementAt(i).DisplayName;
                    var roles = users.ElementAt(i).Roles.ToList();
                    var stringRoles = "";
                    foreach(var role in roles)
                    {
                        stringRoles += role.Name + "; ";
                    }
                    await chan.SendMessageAsync(user + " - " + stringRoles);
                }
            }
        }
    }
}