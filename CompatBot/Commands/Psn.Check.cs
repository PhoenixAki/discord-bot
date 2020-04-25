using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CompatBot.Database;
using CompatBot.Utils.ResultFormatters;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using PsnClient.POCOs;

namespace CompatBot.Commands
{
    internal sealed partial class Psn
    {
        [Group("check")]
        [Description("Commands to check for various stuff on PSN")]
        public sealed class Check: BaseCommandModuleCustom
        {
            private static string latestFwVersion = null;

            [Command("firmware"), Aliases("fw")]
            [Cooldown(1, 10, CooldownBucketType.Channel)]
            [Description("Checks for latest PS3 firmware version")]
            public Task Firmware(CommandContext ctx) => GetFirmwareAsync(ctx);

            internal static async Task GetFirmwareAsync(CommandContext ctx)
            {
                var fwList = await Client.GetHighestFwVersionAsync(Config.Cts.Token).ConfigureAwait(false);
                var embed = fwList.ToEmbed();
                await ctx.RespondAsync(embed: embed).ConfigureAwait(false);
            }

            internal static async Task CheckFwUpdateForAnnouncementAsync(DiscordClient client, List<FirmwareInfo> fwList = null)
            {
                fwList ??= await Client.GetHighestFwVersionAsync(Config.Cts.Token).ConfigureAwait(false);
                if (fwList.Count == 0)
                    return;

                var newVersion = fwList[0].Version;
                using var db = new BotDb();
                var fwVersionState = db.BotState.FirstOrDefault(s => s.Key == "Latest-Firmware-Version");
                latestFwVersion ??= fwVersionState?.Value;
                if (latestFwVersion is null
                    || (Version.TryParse(newVersion, out var newFw)
                        && Version.TryParse(latestFwVersion, out var oldFw)
                        && newFw > oldFw))
                {
                    var embed = fwList.ToEmbed().WithTitle("New PS3 Firmware Information");
                    var announcementChannel = await client.GetChannelAsync(Config.BotChannelId).ConfigureAwait(false);
                    await announcementChannel.SendMessageAsync(embed: embed).ConfigureAwait(false);
                    latestFwVersion = newVersion;
                    if (fwVersionState == null)
                        db.BotState.Add(new BotState {Key = "Latest-Firmware-Version", Value = latestFwVersion});
                    else
                        fwVersionState.Value = latestFwVersion;
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
            }

            internal static async Task MonitorFwUpdates(DiscordClient client, CancellationToken cancellationToken)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await CheckFwUpdateForAnnouncementAsync(client).ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromHours(1), cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
