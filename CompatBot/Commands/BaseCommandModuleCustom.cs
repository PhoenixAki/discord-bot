﻿using System;
using System.Linq;
using System.Threading.Tasks;
using CompatBot.Commands.Attributes;
using CompatBot.Database.Providers;
using CompatBot.Utils;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace CompatBot.Commands
{
    internal class BaseCommandModuleCustom : BaseCommandModule
    {
        public override async Task BeforeExecutionAsync(CommandContext ctx)
        {
            try
            {
                if (ctx.Prefix == Config.AutoRemoveCommandPrefix && ModProvider.IsMod(ctx.User.Id))
                    await ctx.Message.DeleteAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Config.Log.Warn(e, "Failed to delete command message with the autodelete command prefix");
            }

			if (ctx.Channel.Name == "media" && ctx.Command.QualifiedName != "warn" && ctx.Command.QualifiedName != "report")
			{
				Config.Log.Info($"Ignoring command from {ctx.User.Username} (<@{ctx.User.Id}>) in #media: {ctx.Message.Content}");
				if (ctx.Member is DiscordMember member)
				{
					var dm = await member.CreateDmChannelAsync().ConfigureAwait(false);
					await dm.SendMessageAsync($"Only `{Config.CommandPrefix}warn` and `{Config.CommandPrefix}report` are allowed in {ctx.Channel.Mention}").ConfigureAwait(false);
				}
				throw new DSharpPlus.CommandsNext.Exceptions.ChecksFailedException(ctx.Command, ctx, new CheckBaseAttribute[] { new RequiresNotMedia() });
			}

            var disabledCmds = DisabledCommandsProvider.Get();
            if (disabledCmds.Contains(ctx.Command.QualifiedName) && !disabledCmds.Contains("*"))
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder {Color = Config.Colors.Maintenance, Description = "Command is currently disabled"}).ConfigureAwait(false);
                throw new DSharpPlus.CommandsNext.Exceptions.ChecksFailedException(ctx.Command, ctx, new CheckBaseAttribute[] {new RequiresDm()});
            }

            if (TriggersTyping(ctx))
                await ctx.ReactWithAsync(Config.Reactions.PleaseWait).ConfigureAwait(false);

            await base.BeforeExecutionAsync(ctx).ConfigureAwait(false);
        }

        public override async Task AfterExecutionAsync(CommandContext ctx)
        {
            var qualifiedName = ctx.Command.QualifiedName;
            StatsStorage. CmdStatCache.TryGetValue(qualifiedName, out int counter);
            StatsStorage.CmdStatCache.Set(qualifiedName, ++counter, StatsStorage.CacheTime);

            if (TriggersTyping(ctx))
                await ctx.RemoveReactionAsync(Config.Reactions.PleaseWait).ConfigureAwait(false);

            await base.AfterExecutionAsync(ctx).ConfigureAwait(false);
        }

        private static bool TriggersTyping(CommandContext ctx)
        {
            return ctx.Command.CustomAttributes.OfType<TriggersTyping>().FirstOrDefault() is TriggersTyping a && a.ExecuteCheck(ctx);
        }
    }
}