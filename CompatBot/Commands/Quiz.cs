using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;

namespace CompatBot.Commands
{
    [Group("Quiz")]
    [Description("Quiz a user undergoes when joining a server")]
    internal sealed class Quiz: BaseCommandModuleCustom
    {
        [GroupCommand]
        public async Task QuestionOne(CommandContext ctx)
        {
            bool repeat = true;
            var interact = ctx.Client.GetInteractivity();
            while(repeat) 
            {
                var botMsg = await ctx.RespondAsync("Please type your name.");
                var msg = await interact.WaitForMessageAsync(m => m.Author == ctx.User && m.Channel == ctx.Channel && !string.IsNullOrEmpty(m.Content)).ConfigureAwait(false);
                await botMsg.DeleteAsync().ConfigureAwait(false);

                if(!string.IsNullOrEmpty(msg.Result.Content))
                {
                    Console.WriteLine(msg.Result.Content);
                    var check = "Is " + msg.Result.Content.ToString() + " your name?";
                    botMsg = await ctx.RespondAsync(check);
                    msg = await interact.WaitForMessageAsync(m => m.Author == ctx.User && m.Channel == ctx.Channel && !string.IsNullOrEmpty(m.Content)).ConfigureAwait(false);
                    if(msg.Result.Content.ToString().ToLower() == "yes") 
                    {
                        Console.WriteLine("Valid Name");
                        var embeddedAssignment = new DiscordEmbedBuilder
                        {
                            Title = "Do you agree to the ToS?"
                        };
                        var ToSmsg = await ctx.Channel.SendMessageAsync(embed: embeddedAssignment).ConfigureAwait(false);

                        var thumbsUp = DiscordEmoji.FromName(ctx.Client, ":thumbsup:");
                        var thumbsDown = DiscordEmoji.FromName(ctx.Client, ":thumbsdown:");
                        await ToSmsg.CreateReactionAsync(thumbsUp).ConfigureAwait(false);
                        await ToSmsg.CreateReactionAsync(thumbsDown).ConfigureAwait(false);

                        var emojiResult = await interact.WaitForReactionAsync(x => x.Message == ToSmsg && x.User == ctx.User && (x.Emoji == thumbsUp || x.Emoji == thumbsDown)).ConfigureAwait(false);
                    
                        if(emojiResult.Result.Emoji == thumbsUp) 
                        {
                            Console.WriteLine("Reacted to emoji");
                            var roleId = ctx.Guild.GetRole(704089126844104866);
                            await ctx.Member.GrantRoleAsync(roleId).ConfigureAwait(false);
                            await ToSmsg.DeleteAsync().ConfigureAwait(false);
                        }
                        else
                        {
                            var roleId = ctx.Guild.GetRole(704089126844104866);
                            await ctx.Member.RevokeRoleAsync(roleId).ConfigureAwait(false);
                            await ToSmsg.DeleteAsync().ConfigureAwait(false);
                        }
                        repeat = false;
                    }
                    else
                    {
                        await ctx.RespondAsync("Try again.").ConfigureAwait(false);
                    }
                }
                else
                {
                    return;
                }
            }
        }
    }
}