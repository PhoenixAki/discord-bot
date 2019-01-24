﻿using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CompatBot.Utils;
using DSharpPlus.EventArgs;

namespace CompatBot.EventHandlers
{
    internal sealed class PostLogHelpHandler
    {
        private const RegexOptions DefaultOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.ExplicitCapture;
        private static readonly Regex UploadLogMention = new Regex(@"\b(post|upload)\s+(a|the|rpcs3('s)?|your|you're|ur|my)?\blogs?\b", DefaultOptions);
        private static readonly SemaphoreSlim TheDoor = new SemaphoreSlim(1, 1);
        private static readonly TimeSpan ThrottlingThreshold = TimeSpan.FromSeconds(5);
        private static DateTime lastMention = DateTime.UtcNow.AddHours(-1);

        public static async Task OnMessageCreated(MessageCreateEventArgs args)
        {
            if (args.Author.IsBot)
                return;

            if (!args.Channel.Name.Equals("help", StringComparison.InvariantCultureIgnoreCase))
                return;

            if (DateTime.UtcNow - lastMention < ThrottlingThreshold)
                return;

            if (string.IsNullOrEmpty(args.Message.Content) || args.Message.Content.StartsWith(Config.CommandPrefix))
                return;

            if (!UploadLogMention.IsMatch(args.Message.Content))
                return;

            if (!TheDoor.Wait(0))
                return;

            try
            {
                var lastBotMessages = await args.Channel.GetMessagesBeforeAsync(args.Message.Id, 20, DateTime.UtcNow.AddSeconds(-30)).ConfigureAwait(false);
                foreach (var msg in lastBotMessages)
                    if (BotShutupHandler.NeedToSilence(msg).needToChill)
                        return;

                await args.Channel.SendMessageAsync("To upload log, completely close RPCS3 then drag and drop rpcs3.log.gz from the RPCS3 folder into Discord. The file may have a zip or rar icon.").ConfigureAwait(false);
                lastMention = DateTime.UtcNow;
            }
            finally
            {
                TheDoor.Release();
            }
        }
    }
}