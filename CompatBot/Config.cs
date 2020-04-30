﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using CompatBot.Utils;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using NLog;
using NLog.Extensions.Logging;
using NLog.Filters;
using NLog.Targets;
using NLog.Targets.Wrappers;
using ILogger = NLog.ILogger;
using LogLevel = NLog.LogLevel;

namespace CompatBot
{
    internal static class Config
    {
        private static IConfigurationRoot config;
        internal static readonly ILogger Log;
        internal static readonly ILoggerFactory LoggerFactory;
        internal static readonly ConcurrentDictionary<string, string> inMemorySettings = new ConcurrentDictionary<string, string>();
        internal static readonly RecyclableMemoryStreamManager MemoryStreamManager = new RecyclableMemoryStreamManager();

        public static readonly CancellationTokenSource Cts = new CancellationTokenSource();
        public static readonly TimeSpan ModerationTimeThreshold = TimeSpan.FromHours(12);
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
        public static readonly TimeSpan LogParsingTimeout = TimeSpan.FromSeconds(30);
        public static readonly TimeSpan BuildTimeDifferenceForOutdatedBuilds = TimeSpan.FromDays(3);
        public static readonly TimeSpan ShutupTimeLimit = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan ForcedNicknamesRecheckTime = TimeSpan.FromHours(3);
        public static readonly Stopwatch Uptime = Stopwatch.StartNew();

        // these settings could be configured either through `$ dotnet user-secrets`, or through environment variables (e.g. launchSettings.json, etc)
        public static string CommandPrefix => config.GetValue(nameof(CommandPrefix), "!");
        public static string AutoRemoveCommandPrefix => config.GetValue(nameof(AutoRemoveCommandPrefix), ".");
        public static ulong BotGuildId => config.GetValue(nameof(BotGuildId), 701209559930503230ul);                  // discord server where the bot is supposed to be
        public static ulong BotGeneralChannelId => config.GetValue(nameof(BotGeneralChannelId), 701209560647467061ul);// #rpcs3; main or general channel where noobs come first thing
        public static ulong BotChannelId => config.GetValue(nameof(BotChannelId), 701209560647467061ul);              // #build-updates; this is used for new build announcements
        public static ulong BotSpamId => config.GetValue(nameof(BotSpamId), 701209560647467061ul);                    // #bot-spam; this is a dedicated channel for bot abuse
        public static ulong BotLogId => config.GetValue(nameof(BotLogId), 701209560647467061ul);                      // #bot-log; a private channel for admin mod queue
        public static ulong BotRulesChannelId => config.GetValue(nameof(BotRulesChannelId), 701209560647467061ul);    // #rules-info; used to give links to rules
        public static ulong ThumbnailSpamId => config.GetValue(nameof(ThumbnailSpamId), 475678410098606100ul);        // #bot-data; used for whatever bot needs to keep (cover embeds, etc)
        public static ulong BotAdminId => config.GetValue(nameof(BotAdminId), 267367850706993152ul);                  // discord user id for a bot admin
        public static int ProductCodeLookupHistoryThrottle => config.GetValue(nameof(ProductCodeLookupHistoryThrottle), 7);
        public static int TopLimit => config.GetValue(nameof(TopLimit), 15);
        public static int AttachmentSizeLimit => config.GetValue(nameof(AttachmentSizeLimit), 8 * 1024 * 1024);
        public static int LogSizeLimit => config.GetValue(nameof(LogSizeLimit), 64 * 1024 * 1024);
        public static int MinimumBufferSize => config.GetValue(nameof(MinimumBufferSize), 512);
        public static int BuildNumberDifferenceForOutdatedBuilds => config.GetValue(nameof(BuildNumberDifferenceForOutdatedBuilds), 10);
        public static int MinimumPiracyTriggerLength => config.GetValue(nameof(MinimumPiracyTriggerLength), 4);
        public static int MaxSyscallResultLines => config.GetValue(nameof(MaxSyscallResultLines), 13);
        public static TimeSpan IncomingMessageCheckIntervalInMinutes => TimeSpan.FromMinutes(config.GetValue(nameof(IncomingMessageCheckIntervalInMinutes), 10));
        public static string Token => config.GetValue(nameof(Token), "");
        public static string AzureDevOpsToken => config.GetValue(nameof(AzureDevOpsToken), "");
        public static string AzureComputerVisionKey => config.GetValue(nameof(AzureComputerVisionKey), "");
        public static string AzureComputerVisionEndpoint => config.GetValue(nameof(AzureComputerVisionEndpoint), "https://westeurope.api.cognitive.microsoft.com/");
        public static Guid AzureDevOpsProjectId => config.GetValue(nameof(AzureDevOpsProjectId), new Guid("3598951b-4d39-4fad-ad3b-ff2386a649de"));
        public static string LogPath => config.GetValue(nameof(LogPath), "./logs/"); // paths are relative to the working directory
        public static string IrdCachePath => config.GetValue(nameof(IrdCachePath), "./ird/");

        internal static string CurrentLogPath => Path.GetFullPath(Path.Combine(LogPath, "bot.log"));

        public static string GoogleApiConfigPath 
        {
            get
            {
                if (SandboxDetector.Detect() == SandboxType.Docker)
                    return "/bot-config/credentials.json";

                if (Assembly.GetEntryAssembly().GetCustomAttribute<UserSecretsIdAttribute>() is UserSecretsIdAttribute attribute)
                {
                    var path = Path.GetDirectoryName(PathHelper.GetSecretsPathFromSecretsId(attribute.UserSecretsId));
                    path = Path.Combine(path, "credentials.json");
                    if (File.Exists(path))
                        return path;
                }
                
                return "Properties/credentials.json";
            }
        }

        public static class Colors
        {
            public static readonly DiscordColor Help = DiscordColor.Azure;
            public static readonly DiscordColor DownloadLinks = new DiscordColor(0x3b88c3);
            public static readonly DiscordColor Maintenance = new DiscordColor(0xffff00);

            public static readonly DiscordColor CompatStatusNothing = new DiscordColor(0x455556); // colors mimic compat list statuses
            public static readonly DiscordColor CompatStatusLoadable = new DiscordColor(0xe74c3c);
            public static readonly DiscordColor CompatStatusIntro = new DiscordColor(0xe08a1e);
            public static readonly DiscordColor CompatStatusIngame = new DiscordColor(0xf9b32f);
            public static readonly DiscordColor CompatStatusPlayable = new DiscordColor(0x1ebc61);
            public static readonly DiscordColor CompatStatusUnknown = new DiscordColor(0x3198ff);

            public static readonly DiscordColor LogResultFailed = DiscordColor.Gray;

            public static readonly DiscordColor LogAlert = new DiscordColor(0xf04747); // colors mimic discord statuses
            public static readonly DiscordColor LogNotice = new DiscordColor(0xfaa61a);
            public static readonly DiscordColor LogInfo = new DiscordColor(0x43b581);
            public static readonly DiscordColor LogUnknown = new DiscordColor(0x747f8d);

            public static readonly DiscordColor PrOpen = new DiscordColor(0x2cbe4e);
            public static readonly DiscordColor PrMerged = new DiscordColor(0x6f42c1);
            public static readonly DiscordColor PrClosed = new DiscordColor(0xcb2431);

            public static readonly DiscordColor UpdateStatusGood = new DiscordColor(0x3b88c3);
            public static readonly DiscordColor UpdateStatusBad = DiscordColor.Yellow;
        }

        public static class Reactions
        {
            public static readonly DiscordEmoji Success = DiscordEmoji.FromUnicode("👌");
            public static readonly DiscordEmoji Failure = DiscordEmoji.FromUnicode("⛔");
            public static readonly DiscordEmoji Denied = DiscordEmoji.FromUnicode("👮");
            public static readonly DiscordEmoji Starbucks = DiscordEmoji.FromUnicode("☕");
            public static readonly DiscordEmoji Moderated = DiscordEmoji.FromUnicode("🔨");
            public static readonly DiscordEmoji No = DiscordEmoji.FromUnicode("😐");
            public static readonly DiscordEmoji PleaseWait = DiscordEmoji.FromUnicode("👀");
            public static readonly DiscordEmoji PiracyCheck = DiscordEmoji.FromUnicode("🔨");
            public static readonly DiscordEmoji Shutup = DiscordEmoji.FromUnicode("🔇");
            public static readonly DiscordEmoji BadUpdate = DiscordEmoji.FromUnicode("⚠\ufe0f");
        }

        public static class Moderation
        {
            public static readonly int StarbucksThreshold = 5;

            public static readonly IReadOnlyList<ulong> Channels = new List<ulong>
            {
                272875751773306881,
                319224795785068545,
            }.AsReadOnly();

            public static readonly IReadOnlyCollection<ulong> OcrChannels = new HashSet<ulong>(Channels)
            {
                272035812277878785,
                277227681836302338,
                564846659109126244,
                534749301797158914,
            };

            public static readonly IReadOnlyCollection<string> RoleWhiteList = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                "Administrator",
                "Community Manager",
                "Web Developer",
                "Moderator",
                "Lead Graphics Developer",
                "Lead Core Developer",
                "Developers",
                "Affiliated",
                "Contributors",
            };

            public static readonly IReadOnlyCollection<string> RoleSmartList = new HashSet<string>(RoleWhiteList, StringComparer.InvariantCultureIgnoreCase)
            {
                "Testers",
                "Helpers"
            };

            public static readonly IReadOnlyCollection<string> SupporterRoleList = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                "Fans",
                "Supporters",
                "Spectators",
                "Nitro Booster",
            };
        }

        static Config()
        {
            try
            {
                RebuildConfiguration();
                Log = GetLog();
                LoggerFactory = new NLogLoggerFactory();
                Log.Info("Log path: " + CurrentLogPath);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error initializing settings: " + e.Message);
                Console.ResetColor();
            }
        }

        internal static void RebuildConfiguration()
        {
            config = new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly()) // lower priority
                .AddEnvironmentVariables()
                .AddInMemoryCollection(inMemorySettings)     // higher priority
                .Build();
        }

        private static ILogger GetLog()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var fileTarget = new FileTarget("logfile") {
                FileName = CurrentLogPath,
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                KeepFileOpen = true,
                ConcurrentWrites = false,
                AutoFlush = false,
                OpenFileFlushTimeout = 1,
                Layout = "${longdate} ${sequenceid:padding=6} ${level:uppercase=true:padding=-5} ${message} ${onexception:" +
                            "${newline}${exception:format=ToString}" +
                            ":when=not contains('${exception:format=ShortType}','TaskCanceledException')}",
            };
            var asyncFileTarget = new AsyncTargetWrapper(fileTarget)
            {
                TimeToSleepBetweenBatches = 0,
                OverflowAction = AsyncTargetWrapperOverflowAction.Block,
                BatchSize = 500,
            };
            var logTarget = new ColoredConsoleTarget("logconsole") {
                Layout = "${longdate} ${level:uppercase=true:padding=-5} ${message} ${onexception:" +
                            "${newline}${exception:format=Message}" +
                            ":when=not contains('${exception:format=ShortType}','TaskCanceledException')}",
            };
#if DEBUG
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logTarget, "default"); // only echo messages from default logger to the console
#else
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logTarget, "default");
#endif
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, asyncFileTarget);

            var filter = new ConditionBasedFilter { Condition = "contains('${message}','TaskCanceledException')", Action = FilterResult.Ignore, };
            foreach (var rule in config.LoggingRules)
                rule.Filters.Add(filter);
            LogManager.Configuration = config;
            return LogManager.GetLogger("default");
        }

        public static BuildHttpClient GetAzureDevOpsClient()
        {
            if (string.IsNullOrEmpty(AzureDevOpsToken))
                return null;

            var azureCreds = new VssBasicCredential("bot", AzureDevOpsToken);
            var azureConnection = new VssConnection(new Uri("https://dev.azure.com/nekotekina"), azureCreds);
            return azureConnection.GetClient<BuildHttpClient>();
        }
    }
}