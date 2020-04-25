using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompatApiClient.Utils;
using CompatBot.Commands.Attributes;
using CompatBot.EventHandlers;
using CompatBot.ThumbScrapper;
using CompatBot.Utils;
using CompatBot.Utils.ResultFormatters;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using IrdLibraryClient;

namespace CompatBot.Commands
{
    [Group("Check")]
    [Description("Command to just test the creation of commands")]
    internal sealed class Check: BaseCommandModuleCustom
    {
        private static readonly PsnClient.Client Client = new PsnClient.Client();
        private static readonly IrdClient iClient = new IrdClient();

        [Command("updates"), Aliases("update")]
        [Description("Checks if specified product has any updates")]
        public async Task Updates(CommandContext ctx, [RemainingText, Description("Product code such as `BLUS12345`")] string productCode)
        {

            var id = ProductCodeLookup.GetProductIds(productCode).FirstOrDefault();
            if (string.IsNullOrEmpty(id))
            {
                var botMsg = await ctx.RespondAsync("Please specify a valid product code (e.g. BLUS12345 or NPEB98765):").ConfigureAwait(false);
                var interact = ctx.Client.GetInteractivity();
                var msg = await interact.WaitForMessageAsync(m => m.Author == ctx.User && m.Channel == ctx.Channel && !string.IsNullOrEmpty(m.Content)).ConfigureAwait(false);
                await botMsg.DeleteAsync().ConfigureAwait(false);

                if (string.IsNullOrEmpty(msg.Result?.Content))
                    return;

                if (msg.Result.Content.StartsWith(Config.CommandPrefix) || msg.Result.Content.StartsWith(Config.AutoRemoveCommandPrefix))
                    return;

                id = ProductCodeLookup.GetProductIds(msg.Result.Content).FirstOrDefault();
                if (string.IsNullOrEmpty(id))
                {
                    await ctx.ReactWithAsync(Config.Reactions.Failure, $"`{msg.Result.Content.Trim(10).Sanitize(replaceBackTicks: true)}` is not a valid product code").ConfigureAwait(false);
                    return;
                }
            }

            List<DiscordEmbedBuilder> embeds;
            try
            {
                var updateInfo = await Client.GetTitleUpdatesAsync(id, Config.Cts.Token).ConfigureAwait(false);
                embeds = await updateInfo.AsEmbedAsync(ctx.Client, id).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Config.Log.Warn(e, "Failed to get title update info");
                embeds = new List<DiscordEmbedBuilder>
                {
                    new DiscordEmbedBuilder
                    {
                        Color = Config.Colors.Maintenance,
                        Title = "Service is unavailable",
                        Description = "There was an error communicating with the service. Try again in a few minutes.",
                    }
                };
            }

            if (!ctx.Channel.IsPrivate
                && ctx.Message.Author.Id == 197163728867688448
                && (
                    embeds[0].Title.Contains("africa", StringComparison.InvariantCultureIgnoreCase) ||
                    embeds[0].Title.Contains("afrika", StringComparison.InvariantCultureIgnoreCase)
                ))
            {
                foreach (var embed in embeds)
                {
                    var newTitle = "(๑•ิཬ•ั๑)";
                    var partStart = embed.Title.IndexOf(" [Part");
                    if (partStart > -1)
                        newTitle += embed.Title[partStart..];
                    embed.Title = newTitle;
                    if (!string.IsNullOrEmpty(embed.ThumbnailUrl))
                        embed.ThumbnailUrl = "https://cdn.discordapp.com/attachments/417347469521715210/516340151589535745/onionoff.png";
                }
                var sqvat = ctx.Client.GetEmoji(":sqvat:", Config.Reactions.No);
                await ctx.Message.ReactWithAsync(sqvat).ConfigureAwait(false);
            }
            if (embeds.Count > 1 || embeds[0].Fields?.Count > 0)
                embeds[^1] = embeds.Last().WithFooter("Note that you need to install ALL listed updates, one by one");
            foreach (var embed in embeds)
                await ctx.RespondAsync(embed: embed).ConfigureAwait(false);
        }

        [Command("content")]
        [Description("Adds PSN content id to the scraping queue")]
        public async Task Content(CommandContext ctx, [RemainingText, Description("Content IDs to scrape, such as `UP0006-NPUB30592_00-MONOPOLYPSNNA000`")] string contentIds)
        {
            if (string.IsNullOrEmpty(contentIds))
            {
                await ctx.ReactWithAsync(Config.Reactions.Failure, "No IDs were specified").ConfigureAwait(false);
                return;
            }

            var matches = PsnScraper.ContentIdMatcher.Matches(contentIds.ToUpperInvariant());
            var itemsToCheck = matches.Select(m => m.Groups["content_id"].Value).ToList();
            if (itemsToCheck.Count == 0)
            {
                await ctx.ReactWithAsync(Config.Reactions.Failure, "No IDs were specified").ConfigureAwait(false);
                return;
            }

            foreach (var id in itemsToCheck)
                PsnScraper.CheckContentIdAsync(ctx, id, Config.Cts.Token);

            await ctx.ReactWithAsync(Config.Reactions.Success, $"Added {itemsToCheck.Count} ID{StringUtils.GetSuffix(itemsToCheck.Count)} to the scraping queue").ConfigureAwait(false);
        }

        [Command("ird"), TriggersTyping]
        [Description("Searches IRD Library for the matching .ird files")]
        public async Task Search(CommandContext ctx, [RemainingText, Description("Product code or game title to look up")] string query)
        {
            var result = await iClient.SearchAsync(query, Config.Cts.Token).ConfigureAwait(false);
            var embed = result.AsEmbed();
            await ctx.RespondAsync(embed: embed).ConfigureAwait(false);
        }
    }
}