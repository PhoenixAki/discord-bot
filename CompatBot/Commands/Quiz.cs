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
        [GroupCommand]
        public async Task Verification(CommandContext ctx)
        {
            string[] questions = {
                "For questions that aren't answered in the FAQ, should you only ask for help in the #help channel?",
                "Is it okay to ask or inform where to download pirated copies of games?",
                "Are spoilers ever okay to use within any server chat?",
                "Is it okay to engage in vote manipulation in Reddit content within this server?",
                "Do you agree to abide by RPCS3's rules and community guidelines, and the Discord TOS?"
            };

            string[] answers = {
                "yes",
                "no",
                "yes",
                "no",
                "yes"
            };

            int correct = 0;
            var interact = ctx.Client.GetInteractivity();

            for(int question = 0; question < questions.Length; ++question)
            {
                var botMsg = await ctx.RespondAsync(questions[question]);
                var msg = await interact.WaitForMessageAsync(m => m.Author == ctx.User && m.Channel == ctx.Channel && !string.IsNullOrEmpty(m.Content)).ConfigureAwait(false);
                await botMsg.DeleteAsync().ConfigureAwait(false);

                if(!string.IsNullOrEmpty(msg.Result.Content))
                {
                    if(msg.Result.Content.ToLower() == answers[question])
                    {
                        correct++;
                    }
                }
                else
                {
                    await ctx.RespondAsync("Format issue with your response - start over and try again");
                    return;
                }
            }

            await ctx.RespondAsync("You got " + questions + " out of " + questions.Length + " questions correct!");

            if(correct == questions.Length)
            {
                //TODO: inform user of success and add Verified role to user 
            }
            else
            {
                //TODO: inform user of failure and tell them to try again
            }
        }
    }
}