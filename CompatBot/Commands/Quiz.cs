using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;

namespace CompatBot.Commands
{
    [Group("Quiz")]
    [Description("Quiz a user undergoes when joining a server")]
    internal sealed class Quiz: BaseCommandModuleCustom
    {
        int validCheck = 0;
        [GroupCommand]
        public async Task QuestionOne(CommandContext ctx)
        {
            bool repeat = true;
            while(repeat) 
            {
                var botMsg = await ctx.RespondAsync("Please type your name.");
                var interact = ctx.Client.GetInteractivity();
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
                        validCheck += 1;
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