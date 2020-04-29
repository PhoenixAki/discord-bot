﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompatBot.Commands.Attributes;
using CompatBot.Utils;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using CompatBot.EventHandlers;

namespace CompatBot.Commands
{
    [Cooldown(1, 5, CooldownBucketType.Channel)]
    internal sealed class Vision: BaseCommandModuleCustom
    {
        internal static IEnumerable<DiscordAttachment> GetImageAttachment(DiscordMessage message)
            => message.Attachments.Where(a =>
                                             a.FileName.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase)
                                             || a.FileName.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)
                                             || a.FileName.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase)
                                             //|| a.FileName.EndsWith(".webp", StringComparison.InvariantCultureIgnoreCase)
            );

        [Command("describe"), RequiresSupporterRole]
        [BlacklistCheck]
        [Description("Generates an image description from the attached image, or from the url")]
        public async Task Describe(CommandContext ctx)
        {
            if (GetImageAttachment(ctx.Message).FirstOrDefault() is DiscordAttachment attachment)
                await Describe(ctx, attachment.Url).ConfigureAwait(false);
            else
                await ctx.ReactWithAsync(Config.Reactions.Failure, "No images detected").ConfigureAwait(false);
        }

        [Command("describe"), RequiresSupporterRole]
        public async Task Describe(CommandContext ctx, [RemainingText] string imageUrl)
        {
            try
            {
                var reactMsg = ctx.Message;
                if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
                {
                    var str = imageUrl.ToLowerInvariant();
                    if ((str.StartsWith("this")
                         || str.StartsWith("that")
                         || str.StartsWith("last")
                         || str.StartsWith("previous"))
                        && ctx.Channel.PermissionsFor(ctx.Client.GetMember(ctx.Guild, ctx.Client.CurrentUser)).HasPermission(Permissions.ReadMessageHistory))
                        try
                        {
                            var previousMessages = await ctx.Channel.GetMessagesBeforeAsync(ctx.Message.Id, 10).ConfigureAwait(false);
                            var (selectedMsg, selectedAttachment) = (
                                from m in previousMessages
                                where m.Attachments?.Count > 0
                                from a in GetImageAttachment(m)
                                select (m, a)
                            ).FirstOrDefault();
                            if (selectedMsg != null)
                                reactMsg = selectedMsg;
                            imageUrl = selectedAttachment?.Url;
                            if (string.IsNullOrEmpty(imageUrl))
                            {

                                var (selectedMsg2, selectedUrl) = (
                                    from m in previousMessages
                                    where m.Embeds?.Count > 0
                                    from e in m.Embeds
                                    let url = e.Image?.Url ?? e.Image?.ProxyUrl ?? e.Thumbnail?.Url ?? e.Thumbnail?.ProxyUrl
                                    select (m, url)
                                ).FirstOrDefault();
                                if (selectedMsg2 != null)
                                    reactMsg = selectedMsg2;
                                imageUrl = selectedUrl?.ToString();
                            }
                        }
                        catch (Exception e)
                        {
                            Config.Log.Warn(e, "Failed to get link to the previously posted image");
                            await ctx.RespondAsync("Sorry chief, can't find any images in the recent posts").ConfigureAwait(false);
                            return;
                        }
                }

                if (string.IsNullOrEmpty(imageUrl) || !Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
                {
                    await ctx.ReactWithAsync(Config.Reactions.Failure, "No proper image url was found").ConfigureAwait(false);
                    return;
                }

                var client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(Config.AzureComputerVisionKey)) {Endpoint = Config.AzureComputerVisionEndpoint};
                var result = await client.AnalyzeImageAsync(imageUrl, new List<VisualFeatureTypes> {VisualFeatureTypes.Description}, cancellationToken: Config.Cts.Token).ConfigureAwait(false);
                var captions = result.Description.Captions.OrderByDescending(c => c.Confidence).ToList();
                string msg;
                if (captions.Any())
                {
                    var confidence = captions[0].Confidence switch
                    {
                        double v when v > 0.98 => "It is",
                        double v when v > 0.95 => "I'm pretty sure it is",
                        double v when v > 0.9 => "I'm quite sure it is",
                        double v when v > 0.8 => "I think it's",
                        double v when v > 0.5 => "I'm not very smart, so my best guess it's",
                        _ => "Ugh, idk? Might be",
                    };
                    msg = $"{confidence} {captions[0].Text}";
#if DEBUG
                    msg += $" [{captions[0].Confidence * 100:0.00}%]";
                    if (captions.Count > 1)
                    {
                        msg += "\nHowever, here are more guesses:\n";
                        msg += string.Join('\n', captions.Skip(1).Select(c => $"{c.Text} [{c.Confidence*100:0.00}%]"));
                    }
#endif
                }
                else
                    msg = "An image so weird, I have no words to describe it";
                await ctx.RespondAsync(msg).ConfigureAwait(false);
                if (result.Description.Tags.Count > 0)
                {
                    if (result.Description.Tags.Any(t => t == "cat"))
                        await reactMsg.ReactWithAsync(DiscordEmoji.FromUnicode(BotStats.GoodKot[new Random().Next(BotStats.GoodKot.Length)])).ConfigureAwait(false);
                    if (result.Description.Tags.Any(t => t == "dog"))
                        await reactMsg.ReactWithAsync(DiscordEmoji.FromUnicode(BotStats.GoodDog[new Random().Next(BotStats.GoodDog.Length)])).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Config.Log.Warn(e, "Failed to get image description");
                await ctx.RespondAsync("Failed to generate image description, probably because image is too large (dimensions or file size)").ConfigureAwait(false);
            }
        }
    }
}
