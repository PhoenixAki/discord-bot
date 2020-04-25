using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CompatApiClient.Utils;
using CompatBot.Database;
using CompatBot.EventHandlers;
using CompatBot.Utils;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace CompatBot.Commands
{
    internal sealed class Misc: BaseCommandModuleCustom
    {
        private readonly Random rng = new Random();

        private static readonly List<string> EightBallAnswers = new List<string>
        {
            // POSITIVE Original: 45, Added: 21, Total: 66
            "Ya fo sho", "Fo shizzle mah nizzle", "Yuuuup", "Da", "Affirmative",
            "Sure", "Yeah, why not", "Most likely", "Sim", "Oui", "Heck yeah!",
            "Roger that", "Aye!", "Yes without a doubt m8!", ":cell_ok_hand_hd:",
            "Don't be an idiot. YES.", "Mhm!", "Many Yes", "Yiss", "Yah!", "Ja",
            "Sir, yes, Sir!", "Umu!", "Make it so", "Sure thing", "Certainly",
            "Of course", "Definitely", "Indeed", "Much yes", "Totally", "Yup",
            "Consider it done", "You bet", "Yep", "Positive!", "Hmmm, yes!",
            "Yarp", "That's a yes for me", "Aye mate", "Absolutely", "👌", "👍",
            "Totes my goats", "Without fail", "Looks like", "Most def",
            "I guarantee it", "Search your feelings; you know it to be true!",
            "I believe so", "I have a good feeling about it", "You know it",
            "Beyond a doubt", ":wink:", "Most assuredly", "Obviously!", "Obvi",
            "Confirmed", "Does a bear poop in the woods?", "Umm, duh!",
            "According to my calculations, yes.", "It is as it was foretold.",
            "I've never been more sure about anything in my life", "Uh-huh",
            "I don't see why not", "Si", 

            // NEUTRAL Original: 25, Added: 19, Total: 44
            "Maybe", "I don't know", "I don't care", "Who cares", "Ask Neko",
            "Maybe yes, maybe not", "Error 404: answer not found", "Ask Ani",
            "Maybe not, maybe yes", "Ugh", "Probably", "Ask again later", 
            "Don't ask me that again", "You should think twice before asking",
            "Bloody hell, answering that ain't so easy", "You what now?",
            "I'm pretty sure that's illegal!", "What do *you* think?", "🤔",
            "Only on Wednesdays", "Not sure my dude", "Have you googled it?",
            "Look in the mirror, you know the answer already",
            "Don't know, don't care", "_shows signs of complete confusion_",
            "Don't ask me anything until I've had my coffee...", "Unclear",
            "Let me ask you this: Does it spark joy?", "Oof that's a toughie",
            "I don't see how that's any business of yours.", "Perchance",
            "I don't feel comfortable answering that...", "Beats me, kid",
            "Impossible to know for sure", "Anything is possible I guess",
            "I'm tired, ask me another time", "The answer you seek lies within",
            "You're kidding, right?",  "I cannot say with certainty",
            "Zzzzzz...hmm--huh--what...oh, did you say something?", "_shrug_",
            "Well excuuuuuuuuse me, Princess!", "Uhhhhhhh :grimacing:",
            "Are you sure you want to know?", 

            // NEGATIVE Original: 35, Added: 9, Total: 44
            "Nah mate", "Nope", "Njet", "Of course not", "Seriously no",
            "Noooooooooo", "Most likely not", "Não", "Non", "Hell no",
            "Absolutely not", "Nuh-uh!", "Nyet!", "Negatory!", "Heck no",
            "Nein!", "I think not", "I'm afraid not", "Nay", "Yesn't", "👎",
            "No way", "Certainly not", "I must say no", "Nah", "Negative",
            "Definitely not", "No way, Jose", "Not today", "Narp", "Not in a million years", 
            "I'm afraid I can't let you do that Dave.", "Oh, I don't think so",
            "This mission is too important for me to allow you to jeopardize it.",
            "By *no* means", "Ain't gonna happen", "lol no", "Sorry, chief",
            "**Aw hell naw!!**", "Not a chance", "Pshh, yeah right bro",
            "According to my calculations, no.", "Umm...no", "No, no, definitely not!",
        };

        private static readonly List<string> EightBallSnarkyComments = new List<string>
        {
            "Can't answer the question that wasn't asked",
            "Having issues with my mind reading attachment, you'll have to state your question explicitly",
            "Bad reception on your brain waves today, can't read the question",
            "What should the answer be for the question that wasn't asked 🤔",
            "In Discord no one can read your question if you don't type it",
            "In space no one can hear you scream; that's what you're doing right now",
            "Unfortunately there's no technology to transmit your question telepathically just yet",
            "I'd say maybe, but I'd need to see your question first",
        };

        private static readonly List<string> EightBallTimeUnits = new List<string>
        {
            "second", "minute", "hour", "day", "week", "month", "year", "decade", "century", "millennium",
            "night", "moon cycle", "solar eclipse", "blood moon", "complete emulator rewrite",
        };

        private static readonly List<string> RateAnswers = new List<string>
        {
            // POSITIVE Original: 60, Added: 33, Total: 93
            "Not so bad", "I likesss!", "Pretty good", "Guchi gud", "Amazing!",
            "Glorious!", "Very good", "Excellent...", "Magnificent",
            "Rate bot says he likes, so you like too",
            "If you reorganize the words it says \"pretty cool\"", "I approve",
            "<:morgana_sparkle:315899996274688001>　やるじゃねーか！",
            "Not half bad 👍", "Belissimo!", "I am in awe", "Incredible!",
            "Cool. Cool cool cool", "Radiates gloriousness", "A lucky find!",
            "Like a breath of fresh air", "Can't recommend enough", "So good!",
            "Sunshine for my digital soul 🌞", "Fantastic like petrichor 🌦",
            "Joyous like a rainbow 🌈", "Unbelievably good", "💯 approved",
            "Not perfect, but ok", "I don't see any downsides", "Magical ✨",
            "Here's my seal of approval 💮", "As good as it gets", "Fabulous",
            "A benchmark to pursue", "Should make you warm and fuzzy inside",
            "Cool like a cup of good wine 🍷", "Filled with passion!",
            "Wondrous like a unicorn 🦄", "Soothing sight for these tired eyes",
            "Lovely", "So cute!", "It's so nice, I think about it every day!",
            "😊 Never expected to be this pretty!", "Sweet as a candy",
            "It's overflowing with charm!", "A love magnet", "Pretty Fancy",
            "Admirable", "Delightful", "Enchanting as the Sunset", "Shiny!",
            "A beacon of hope!", "Filled with hope!", "Absolute Hope!",
            "The meaning of hope", "Inspiring!", "Marvelous", "Breathtaking",
            "Better than bubble wrap.", "Rad", "Truly iconic", "Ingenius!",
             "Not too hot, not too cold; just right.", "A gift to mankind",
            "The epitome of classy :ok_hand:", "10/10 would recommend",
            "Hard to resist, impossible to forget", ":slight_smile:",
            "Spicy! :wink:", "Otherworldly cool :sparkles:", "Brilliant!",
            "Downright decent", "I'm green with envy", "Blissful",
            "The sound of it makes me want to party!", "Spectacular!",
            "A fitting gift for your new mechanical overlords.", ":ok_hand:",
            "A true gem", "Stupendous!", "Absolutely stunning", "Gnarly",
            "Quite exquisite, if I do say so myself.", "Absolutely priceless", 
            "So cool it makes me jealous!", "Heartwarming", "Top of the line ",
            "It brings warmth to the soul", "Kawaii! :kissing_smiling_eyes:", 
            "Awesome sauce", "Comforting",
            "Refreshing like a cold drink after crossing the desert on a horse with no name.", 
            

            // NEUTRAL Original: 22, Added: 40, Total: 62
            "Ask MsLow", "Could be worse", "I need more time to think about it",
            "It's ok, nothing and no one is perfect", "🆗", "🤔", "Ok-ish?",
            "You already know, my boi", "I don't know, man...", "Passable",
            "Unexpected like a bouquet of sunflowers 🌻", "meh",
            "Hard to measure precisely...", "Requires more data to analyze", 
            "Quite unique 🤔", "Less like an orange, and more like an apple",
            "It is so tiring to grade everything...", "...", "I've seen worse",
            "Bland like porridge", "Not _bad_, but also not _good_",
            "Why would you want to _rate_ this?", ":rolling_eyes:", "_shrug_",
            "Pretty standard, nothing to write home about", "Unremarkable",
            ":yawning_face:", "Uhhhhh, it's okay I guess", "Uninspiring", 
            "So uninteresting the bot fell asleep...:sleeping:", "Tepid",
            "Vapid", ":neutral_face:", "I'd rather not say", "Ordinary", 
            ":expressionless:", "*removes an earbud* What?", "So-so",
            "You could ask a little nicer.", "_blinks_...I don't get it",
            "Do you think I have nothing better to do than to rate stuff?",
            "All right, but not as good as spaghetti", "*gestures vaguely*",
            "I don't feel like rating anything right now", "It is what it is",
            "Reminds me of something I can't recall :thinking:", "_sigh_",
            "I'm sensing...not much", "Could use improvement", "It's...fine",
            "_slightly judgemental stare_", "I would prefer not to.", "5/10",
            "Do you _really_ want to know?", "You want me to rate **what** now",  
            "As captivating as Ann Veal", "I'll permit it", "Tolerable",
            "Rate machine broken, come back later", "It's whatever, man",
            "I have no strong feelings one way or the other",
            "I honestly have no idea what to say to that",
            

            // NEGATIVE Original: 43, Added: 19, Total: 62
            "Bad", "Very bad", "Pretty bad", "Horrible", "Ugly", "Disgusting",
            "Literally the worst", "Not interesting", "Simply ugh", "🤮", "😐",
            "I don't like it! You shouldn't either!", "Just like you, 💩", "😔",
            "Not approved", "Big Mistake", "The opposite of good", "Real shame",
            "Could be better", "Horrendous :scream:", "Not worth it", "Useless",
            "Mediocre at best", "I think you misspelled `poop` there", "Poopy",
            "Nothing special", "Boooooooo!", "Smelly", "Feeling-breaker",
            "Annoying", "It's pretty foolish to want to rate this", 
            "Boring", "Easily forgettable", "An Abomination", "A Monstrosity",
            "Truly horrific", "Filled with despair!", "Eroded by despair",
            "Hopeless…", "Cursed with misfortune", "Nothing but terror",
            "Not good, at all", "A waste of time", "Not a vibe!", "Hard pass",
            "Wack", "A major snore fest", "Unsatisfying", ":thumbsdown:", 
            "If I could give it zero stars I would", "Just sad", "Gross",
            "This is a load of barnacles", "So horrible it ruined my day",
            "Unforgivable. I can't believe you made me rate this.", "Terrible",
            "Oof not a fan", "Lame", "An enormous disappointment", "Bizarre",
            "Ridiculous", "Booooooorrrring",
        };

        private static readonly char[] Separators = { ' ', '　', '\r', '\n' };
        private static readonly char[] Suffixes = {',', '.', ':', ';', '!', '?', ')', '}', ']', '>', '+', '-', '/', '*', '=', '"', '\'', '`'};
        private static readonly char[] Prefixes = {'@', '(', '{', '[', '<', '!', '`', '"', '\'', '#'};
        private static readonly char[] EveryTimable = Separators.Concat(Suffixes).Concat(Prefixes).Distinct().ToArray();

        private static readonly HashSet<string> Me = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "I", "me", "myself", "moi"
        };

        private static readonly HashSet<string> Your = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "your", "you're", "yor", "ur", "yours", "your's",
        };

        private static readonly HashSet<char> Vowels = new HashSet<char> {'a', 'e', 'i', 'o', 'u'};

        private static readonly Regex Instead = new Regex("rate (?<instead>.+) instead", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

        [Command("credits"), Aliases("about")]
        [Description("Author Credit")]
        public async Task About(CommandContext ctx)
        {
            var hcorion = ctx.Client.GetEmoji(":hcorion:", DiscordEmoji.FromUnicode("🍁"));
            var embed = new DiscordEmbedBuilder
                {
                    Title = "RPCS3 Compatibility Bot",
                    Url = "https://github.com/RPCS3/discord-bot",
                    Color = DiscordColor.Purple,
                }.AddField("Made by",
                    "💮 13xforever\n" +
                    "🇭🇷 Roberto Anić Banić aka nicba1010")
                .AddField("People who ~~broke~~ helped test the bot",
                    "🐱 Juhn\n" +
                    $"{hcorion} hcorion\n" +
                    "🙃 TGE\n" +
                    "🍒 Maru\n" +
                    "♋ Tourghool");
            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("roll")]
        [Description("Generates a random number between 1 and maxValue. Can also roll dices like `2d6`. Default is 1d6")]
        public async Task Roll(CommandContext ctx, [Description("Some positive natural number")] int maxValue = 6, [RemainingText, Description("Optional text")] string comment = null)
        {
            string result = null;
            if (maxValue > 1)
                lock (rng) result = (rng.Next(maxValue) + 1).ToString();
            if (string.IsNullOrEmpty(result))
                await ctx.ReactWithAsync(DiscordEmoji.FromUnicode("💩"), $"How is {maxValue} a positive natural number?").ConfigureAwait(false);
            else
                await ctx.RespondAsync(result).ConfigureAwait(false);
        }

        [Command("roll")]
        public async Task Roll(CommandContext ctx, [RemainingText, Description("Dices to roll (i.e. 2d6+1 for two 6-sided dices with a bonus 1)")] string dices)
        {
            var result = "";
            var embed = new DiscordEmbedBuilder();
            if (dices is string dice && Regex.Matches(dice, @"(?<num>\d+)?d(?<face>\d+)(?:\+(?<mod>\d+))?") is MatchCollection matches && matches.Count > 0 && matches.Count <= EmbedPager.MaxFields)
            {
                var grandTotal = 0;
                foreach (Match m in matches)
                {
                    result = "";
                    if (!int.TryParse(m.Groups["num"].Value, out var num))
                        num = 1;
                    if (int.TryParse(m.Groups["face"].Value, out var face)
                        && 0 < num && num < 101
                        && 1 < face && face < 1001)
                    {
                        List<int> rolls;
                        lock (rng) rolls = Enumerable.Range(0, num).Select(_ => rng.Next(face) + 1).ToList();
                        var total = rolls.Sum();
                        var totalStr = total.ToString();
                        int.TryParse(m.Groups["mod"].Value, out var mod);
                        if (mod > 0)
                            totalStr += $" + {mod} = {total + mod}";
                        var rollsStr = string.Join(' ', rolls);
                        if (rolls.Count > 1)
                        {
                            result = "Total: " + totalStr;
                            result += "\nRolls: " + rollsStr;
                        }
                        else
                            result = totalStr;
                        grandTotal += total + mod;
                        var diceDesc = $"{num}d{face}";
                        if (mod > 0)
                            diceDesc += "+" + mod;
                        embed.AddField(diceDesc, result.Trim(EmbedPager.MaxFieldLength), true);
                    }
                }
                if (matches.Count == 1)
                    embed = null;
                else
                {
                    embed.Description = "Grand total: " + grandTotal;
                    embed.Title = $"Result of {matches.Count} dice rolls";
                    embed.Color = Config.Colors.Help;
                    result = null;
                }
            }
            else
            {
                await Roll(ctx).ConfigureAwait(false);
                return;
            }

            if (string.IsNullOrEmpty(result) && embed == null)
                await ctx.ReactWithAsync(DiscordEmoji.FromUnicode("💩"), "Invalid dice description passed").ConfigureAwait(false);
            else if (embed != null)
                await ctx.RespondAsync(embed: embed).ConfigureAwait(false);
            else
                await ctx.RespondAsync(result).ConfigureAwait(false);
        }

        [Command("random"), Aliases("rng"), Hidden, Cooldown(1, 3, CooldownBucketType.Channel)]
        [Description("Provides random stuff")]
        public async Task RandomShit(CommandContext ctx, string stuff)
        {
            stuff = stuff?.ToLowerInvariant() ?? "";
            switch (stuff)
            {
                    case "game":
                    case "serial":
                    case "productcode":
                    case "product code":
                    {
                        using var db = new ThumbnailDb();
                        var count = await db.Thumbnail.CountAsync().ConfigureAwait(false);
                        if (count == 0)
                        {
                            await ctx.RespondAsync("Sorry, I have no information about a single game yet").ConfigureAwait(false);
                            return;
                        }

                        var rng = new Random().Next(count);
                        var productCode = await db.Thumbnail.Skip(rng).Take(1).FirstOrDefaultAsync().ConfigureAwait(false);
                        if (productCode == null)
                        {
                            await ctx.RespondAsync("Sorry, there's something with my brains today. Try again or something").ConfigureAwait(false);
                            return;
                        }

                        await ProductCodeLookup.LookupAndPostProductCodeEmbedAsync(ctx.Client, ctx.Message, new List<string> {productCode.ProductCode}).ConfigureAwait(false);
                        break;
                    }
                    default:
                        await Roll(ctx, comment: stuff).ConfigureAwait(false);
                        break;
            }
        }

        [Command("8ball"), Cooldown(20, 60, CooldownBucketType.Channel)]
        [Description("Provides a ~~random~~ objectively best answer to your question")]
        public async Task EightBall(CommandContext ctx, [RemainingText, Description("A yes/no question")] string question)
        {
            question = question?.ToLowerInvariant() ?? "";
            if (question.StartsWith("when "))
                await When(ctx, question[5..]).ConfigureAwait(false);
            else
            {
                string answer;
                var pool = string.IsNullOrEmpty(question) ? EightBallSnarkyComments : EightBallAnswers;
                lock (rng)
                    answer = pool[rng.Next(pool.Count)];
                if (answer.StartsWith(':') && answer.EndsWith(':'))
                    answer = ctx.Client.GetEmoji(answer, "🔮");
                await ctx.RespondAsync(answer).ConfigureAwait(false);
            }
        }

        [Command("when"), Hidden, Cooldown(20, 60, CooldownBucketType.Channel)]
        [Description("Provides advanced clairvoyance services to predict the time frame for specified event with maximum accuracy")]
        public async Task When(CommandContext ctx, [RemainingText, Description("Something to happen")] string whatever = "")
        {
            var question = whatever?.Trim().TrimEnd('?').ToLowerInvariant() ?? "";
            var prefix = DateTime.UtcNow.ToString("yyyyMMddHH");
            var crng = new Random((prefix + question).GetHashCode());
            var number = crng.Next(100) + 1;
            var unit = EightBallTimeUnits[crng.Next(EightBallTimeUnits.Count)];
            if (number > 1)
            {
                if (unit.EndsWith("ry"))
                    unit = unit[..^1] + "ie";
                unit += "s";
                if (unit == "millenniums")
                    unit = "millennia";
            }
            var willWont = crng.NextDouble() < 0.5 ? "will" : "won't";
            await ctx.RespondAsync($"🔮 My psychic powers tell me it {willWont} happen in the next **{number} {unit}** 🔮").ConfigureAwait(false);
        }

        [Command("rate"), Cooldown(20, 60, CooldownBucketType.Channel)]
        [Description("Gives a ~~random~~ expert judgment on the matter at hand")]
        public async Task Rate(CommandContext ctx, [RemainingText, Description("Something to rate")] string whatever = "")
        {
            try
            {
                var choices = RateAnswers;
                var choiceFlags = new HashSet<char>();
                whatever = whatever.ToLowerInvariant();
                var originalWhatever = whatever;
                var matches = Instead.Matches(whatever);
                if (matches.Any())
                {
                    var insteadWhatever = matches.Last().Groups["instead"].Value.TrimEager();
                    if (!string.IsNullOrEmpty(insteadWhatever))
                        whatever = insteadWhatever;
                }
                foreach (var attachment in ctx.Message.Attachments)
                    whatever += $" {attachment.FileSize}";

                var nekoUser = await ctx.Client.GetUserAsync(272032356922032139ul).ConfigureAwait(false);
                var nekoMember = ctx.Client.GetMember(nekoUser);
                var nekoMatch = new HashSet<string>(new[] {nekoUser.Id.ToString(), nekoUser.Username, nekoMember?.DisplayName ?? "neko", "neko", "nekotekina",});
                var kdUser = await ctx.Client.GetUserAsync(272631898877198337ul).ConfigureAwait(false);
                var kdMember = ctx.Client.GetMember(kdUser);
                var kdMatch = new HashSet<string>(new[] {kdUser.Id.ToString(), kdUser.Username, kdMember?.DisplayName ?? "kd-11", "kd", "kd-11", "kd11", });
                var botMember = ctx.Client.GetMember(ctx.Client.CurrentUser);
                var botMatch = new HashSet<string>(new[] {botMember.Id.ToString(), botMember.Username, botMember.DisplayName, "yourself", "urself", "yoself",});

                var prefix = DateTime.UtcNow.ToString("yyyyMMddHH");
                var words = whatever.Split(Separators);
                var result = new StringBuilder();
                for (var i = 0; i < words.Length; i++)
                {
                    var word = words[i].TrimEager();
                    var suffix = "";
                    var tmp = word.TrimEnd(Suffixes);
                    if (tmp.Length != word.Length)
                    {
                        suffix = word[..tmp.Length];
                        word = tmp;
                    }
                    tmp = word.TrimStart(Prefixes);
                    if (tmp.Length != word.Length)
                    {
                        result.Append(word[..^tmp.Length]);
                        word = tmp;
                    }
                    if (word.EndsWith("'s"))
                    {
                        suffix = "'s" + suffix;
                        word = word[..^2];
                    }

                    void MakeCustomRoleRating(DiscordMember mem)
                    {
                        if (mem != null && !choiceFlags.Contains('f'))
                        {
                            var roleList = mem.Roles.ToList();
                            if (roleList.Any())
                            {

                                var role = roleList[new Random((prefix + mem.Id).GetHashCode()).Next(roleList.Count)].Name?.ToLowerInvariant();
                                if (!string.IsNullOrEmpty(role))
                                {
                                    if (role.EndsWith('s'))
                                        role = role[..^1];
                                    var article = Vowels.Contains(role[0]) ? "n" : "";
                                    choices = RateAnswers.Concat(Enumerable.Repeat($"Pretty fly for a{article} {role} guy", RateAnswers.Count / 20)).ToList();
                                    choiceFlags.Add('f');
                                }
                            }
                        }
                    }

                    var appended = false;
                    DiscordMember member = null;
                    if (Me.Contains(word))
                    {
                        member = ctx.Member;
                        word = ctx.Message.Author.Id.ToString();
                        result.Append(word);
                        appended = true;
                    }
                    else if (word == "my")
                    {
                        result.Append(ctx.Message.Author.Id).Append("'s");
                        appended = true;
                    }
                    else if (botMatch.Contains(word))
                    {
                        word = ctx.Client.CurrentUser.Id.ToString();
                        result.Append(word);
                        appended = true;
                    }
                    else if (Your.Contains(word))
                    {
                        result.Append(ctx.Client.CurrentUser.Id).Append("'s");
                        appended = true;
                    }
                    else if (word.StartsWith("actually") || word.StartsWith("nevermind") || word.StartsWith("nvm"))
                    {
                        result.Clear();
                        appended = true;
                    }
                    if (member == null && i == 0 && await ctx.ResolveMemberAsync(word).ConfigureAwait(false) is DiscordMember m)
                        member = m;
                    if (member != null)
                    {
                        if (suffix.Length == 0)
                            MakeCustomRoleRating(member);
                        if (!appended)
                        {
                            result.Append(member.Id);
                            appended = true;
                        }
                    }
                    if (nekoMatch.Contains(word))
                    {
                        if (i == 0 && suffix.Length == 0)
                        {
                            choices = RateAnswers.Concat(Enumerable.Repeat("Ugh", RateAnswers.Count * 3)).ToList();
                            MakeCustomRoleRating(nekoMember);
                        }
                        result.Append(nekoUser.Id);
                        appended = true;
                    }
                    if (kdMatch.Contains(word))
                    {
                        if (i == 0 && suffix.Length == 0)
                        {
                            choices = RateAnswers.Concat(Enumerable.Repeat("RSX genius", RateAnswers.Count * 3)).ToList();
                            MakeCustomRoleRating(kdMember);
                        }
                        result.Append(kdUser.Id);
                        appended = true;
                    }
                    if (!appended)
                        result.Append(word);
                    result.Append(suffix).Append(" ");
                }
                whatever = result.ToString();
                var cutIdx = whatever.LastIndexOf("never mind");
                if (cutIdx > -1)
                    whatever = whatever[cutIdx..];
                whatever = whatever.Replace("'s's", "'s").TrimStart(EveryTimable).Trim();
                if (whatever.StartsWith("rate "))
                    whatever = whatever[("rate ".Length)..];
                if (originalWhatever == "sonic" || originalWhatever.Contains("sonic the"))
                    choices = RateAnswers.Concat(Enumerable.Repeat("💩 out of 🦔", RateAnswers.Count)).Concat(new[] {"Sonic out of 🦔", "Sonic out of 10"}).ToList();

                if (string.IsNullOrEmpty(whatever))
                    await ctx.RespondAsync("Rating nothing makes _**so much**_ sense, right?").ConfigureAwait(false);
                else
                {
                    var seed = (prefix + whatever).GetHashCode(StringComparison.CurrentCultureIgnoreCase);
                    var seededRng = new Random(seed);
                    var answer = choices[seededRng.Next(choices.Count)];
                    await ctx.RespondAsync(answer).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Config.Log.Warn(e, $"Failed to rate {whatever}");
            }
        }

        [Command("meme"), Aliases("memes"), Cooldown(1, 30, CooldownBucketType.Channel), Hidden]
        [Description("No, memes are not implemented yet")]
        public async Task Memes(CommandContext ctx, [RemainingText] string _ = null)
        {
            var ch = await ctx.GetChannelForSpamAsync().ConfigureAwait(false);
            await ch.SendMessageAsync($"{ctx.User.Mention} congratulations, you're the meme").ConfigureAwait(false);
        }

        [Command("download"), Cooldown(1, 10, CooldownBucketType.Channel)]
        [Description("Find games to download")]
        public Task Download(CommandContext ctx, [RemainingText] string game)
        {
            var invariantTitle = game?.ToLowerInvariant() ?? "";
            if (invariantTitle == "rpcs3")
                return CompatList.UpdatesCheck.CheckForRpcs3Updates(ctx.Client, ctx.Channel);

            if (invariantTitle == "ps3updat.dat" || invariantTitle == "firmware" || invariantTitle == "fw")
                return Psn.Check.GetFirmwareAsync(ctx);

            if (invariantTitle.StartsWith("update")
                && ProductCodeLookup.ProductCode.Match(invariantTitle) is Match m
                && m.Success)
            {
                var checkUpdateCmd = ctx.CommandsNext.FindCommand("psn check update", out _);
                var checkUpdateCtx = ctx.CommandsNext.CreateContext(ctx.Message, ctx.Prefix, checkUpdateCmd, m.Groups[0].Value);
                return checkUpdateCmd.ExecuteAsync(checkUpdateCtx);
            }

            if (invariantTitle == "unnamed")
                game = "Persona 5";
            else if (invariantTitle == "KOT")
                game = invariantTitle;
            return Psn.SearchForGame(ctx, game, 3);
        }

        [Command("firmware"), Aliases("fw"), Cooldown(1, 10, CooldownBucketType.Channel)]
        [Description("Checks for latest PS3 firmware version")]
        public Task Firmware(CommandContext ctx) => Psn.Check.GetFirmwareAsync(ctx);

        [Command("compare"), Hidden]
        [Description("Calculates the similarity metric of two phrases from 0 (completely different) to 1 (identical)")]
        public Task Compare(CommandContext ctx, string strA, string strB)
        {
            var result = strA.GetFuzzyCoefficientCached(strB);
            return ctx.RespondAsync($"Similarity score is {result:0.######}");
        }

        [Command("productcode"), Aliases("pci", "decode")]
        [Description("Describe Playstation product code")]
        public async Task ProductCode(CommandContext ctx, [RemainingText, Description("Product code such as BLUS12345 or SCES")] string productCode)
        {
            productCode = ProductCodeLookup.GetProductIds(productCode).FirstOrDefault() ?? productCode;
            productCode = productCode?.ToUpperInvariant();
            if (productCode?.Length > 3)
            {
                var dsc = ProductCodeDecoder.Decode(productCode);
                var info = string.Join('\n', dsc);
                if (productCode.Length == 9)
                {
                    var embed = await ctx.Client.LookupGameInfoAsync(productCode).ConfigureAwait(false);
                    embed.AddField("Product code info", info);
                    await ctx.RespondAsync(embed: embed).ConfigureAwait(false);
                }
                else
                    await ctx.RespondAsync(info).ConfigureAwait(false);
            }
            else
                await ctx.ReactWithAsync(Config.Reactions.Failure, "Invalid product code").ConfigureAwait(false);
        }

#if DEBUG
        [Command("whitespacetest"), Aliases("wst", "wstest")]
        [Description("Testing discord embeds breakage for whitespaces")]
        public async Task WhitespaceTest(CommandContext ctx)
        {
            var checkMark = "[\u00a0]";
            const int width = 20;
            var result = new StringBuilder($"` 1. Dots:{checkMark.PadLeft(width, '.')}`").AppendLine()
                .AppendLine($"` 2. None:{checkMark,width}`");
            var ln = 3;
            foreach (var c in StringUtils.SpaceCharacters)
                result.AppendLine($"`{ln++,2}. {(int)c:x4}:{checkMark,width}`");
            void addRandomStuff(DiscordEmbedBuilder emb)
            {
                var txt = "😾 lasjdf wqoieyr osdf `Vreoh Sdab` wohe `270`\n" +
                          "🤔 salfhiosfhsero hskfh shufwei oufhwehw e wkihrwe h\n" +
                          "ℹ sakfjas f hs `ASfhewighehw safds` asfw\n" +
                          "🔮 ¯\\\\\\_(ツ)\\_/¯";

                emb.AddField("Random section", txt, false);
            }
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Whitespace embed test")
                .WithDescription("In a perfect world all these lines would look the same, with perfectly formed columns");

            var lines = result.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            var embedList = lines.BreakInEmbeds(embed, lines.Length / 2 + lines.Length % 2, "Normal");
            foreach (var _ in embedList)
            {
                //drain the enumerable
            }
            embed.AddField("-", "-", false);

            lines = result.ToString().Replace(' ', StringUtils.Nbsp).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            embedList = lines.BreakInEmbeds(embed, lines.Length / 2 + lines.Length % 2, "Non-breakable spaces");
            foreach (var _ in embedList)
            {
                //drain the enumerable
            }
            await ctx.RespondAsync(embed: embed).ConfigureAwait(false);
        }
#endif
    }
}
