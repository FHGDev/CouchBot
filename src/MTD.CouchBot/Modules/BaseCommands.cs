using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using MTD.CouchBot.Domain.Models.Bot;
using MTD.CouchBot.Domain.Utilities;
using MTD.CouchBot.Managers;
using MTD.CouchBot.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MTD.CouchBot.Modules
{
    public class BaseCommands : BaseModule
    {
        private readonly IStatisticsManager _statisticsManager;
        private readonly IYouTubeManager _youtubeManager;
        private readonly IApiAiManager _apiAiManager;
        private readonly DiscordShardedClient _discord;
        private readonly DiscordService _discordService;
        private readonly BotSettings _botSettings;
        private readonly FileService _fileService;

        public BaseCommands(IStatisticsManager statisticsManager, IYouTubeManager youtubeManager, 
            IApiAiManager apiAiManager, DiscordService discordService, DiscordShardedClient discord,
            IOptions<BotSettings> botSettings, FileService fileService) : base(botSettings)
        {
            _statisticsManager = statisticsManager;
            _youtubeManager = youtubeManager;
            _apiAiManager = apiAiManager;
            _discordService = discordService;
            _discord = discord;
            _botSettings = botSettings.Value;
            _fileService = fileService;
        }

        [Command("info"), Summary("Get CouchBot Information.")]
        public async Task Info()
        {
            var serverFiles = Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory);
            var userFiles = Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.UserDirectory);
            var serverTwitchCount = 0;
            var serverYouTubeCount = 0;
            var serverBeamCount = 0;
            var serverHitboxCount = 0;
            var serverPicartoCount = 0;
            var twitchGameCount = 0;
            var twitchTeamCount = 0;
            var vidMeCount = 0;

            foreach (var s in serverFiles)
            {
                var server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(s));

                if (server.PicartoChannels != null)
                {
                    foreach (var u in server.PicartoChannels)
                    {
                        serverPicartoCount++;
                    }
                }

                if(!string.IsNullOrEmpty(server.OwnerPicartoChannel))
                {
                    serverPicartoCount++;
                }

                if (server.ServerBeamChannels != null)
                {
                    foreach (var u in server.ServerBeamChannels)
                    {
                        serverBeamCount++;
                    }
                }

                if (!string.IsNullOrEmpty(server.OwnerBeamChannel))
                {
                    serverBeamCount++;
                }

                if (server.ServerTwitchChannelIds != null)
                {
                    foreach (var u in server.ServerTwitchChannelIds)
                    {
                        serverTwitchCount++;
                    }
                }

                if (!string.IsNullOrEmpty(server.OwnerTwitchChannel))
                {
                    serverTwitchCount++;
                }

                if (server.ServerYouTubeChannelIds != null)
                {
                    foreach (var u in server.ServerYouTubeChannelIds)
                    {
                        serverYouTubeCount++;
                    }
                }

                if (!string.IsNullOrEmpty(server.OwnerYouTubeChannelId))
                {
                    serverYouTubeCount++;
                }

                if (server.ServerHitboxChannels != null)
                {
                    foreach (var u in server.ServerHitboxChannels)
                    {
                        serverHitboxCount++;
                    }
                }

                if (!string.IsNullOrEmpty(server.OwnerHitboxChannel))
                {
                    serverHitboxCount++;
                }

                if (server.ServerVidMeChannelIds != null)
                {
                    foreach (var u in server.ServerVidMeChannelIds)
                    {
                        vidMeCount++;
                    }
                }

                if (!string.IsNullOrEmpty(server.OwnerVidMeChannel))
                {
                    vidMeCount++;
                }

                if (server.TwitchTeams != null)
                {
                    foreach (var t in server.TwitchTeams)
                    {
                        twitchTeamCount++;
                    }
                }

                if (server.ServerGameList != null)
                {
                    foreach (var g in server.ServerGameList)
                    {
                        twitchGameCount++;
                    }
                }
            }

            int serverCount = 0;

            foreach (var shard in _discord.Shards)
            {
                serverCount += shard.Guilds.Count;
            }

            string info = "```Markdown\r\n" +
                          "# " + _discord.CurrentUser.Username + "\r\n" +
                          "- Configured Servers: " + serverFiles.Length + "\r\n" +
                          "- Connected Shards: " + _discord.Shards.Count + "\r\n" +
                          "- Connected Servers: " + serverCount + "\r\n" +
                          "Platforms: \r\n" +
                          "-- Mixer: " + serverBeamCount + "\r\n" +
                          "-- Picarto: " + serverPicartoCount + "\r\n" +
                          "-- Smashcast: " + serverHitboxCount + "\r\n" +
                          "-- Twitch: " + serverTwitchCount + "\r\n" +
                          "-- Twitch Games: " + twitchGameCount + "\r\n" +
                          "-- Twitch Teams: " + twitchTeamCount + "\r\n" +
                          "-- Vid.me: " + vidMeCount + "\r\n" +
                          "-- YouTube: " + serverYouTubeCount + "\r\n" +
                          "-- Total Channels Checked: " + (serverYouTubeCount + serverTwitchCount + serverBeamCount + serverHitboxCount + serverPicartoCount) + "\r\n" +
                          "- Current Memory Usage: " + ((System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1024) / 1024) + "MB \r\n" +
                          "- Built on Discord.Net - (https://github.com/RogueException/Discord.Net)\r\n" +
                          "- Developed and Maintained by Dawgeth - (http://twitter.com/dawgeth)\r\n" +
                          "- Want to submit feedback, a bug, or suggestion? - (http://github.com/dawgeth/couchbot/issues)\r\n" +
                          "- Join us on Discord!-(http://discord.couchbot.io)\r\n" +
                          "```\r\n";

            await Context.Channel.SendMessageAsync(info);
        }

        [Command("servers"), Summary("Get Server Statistic Information.")]
        public async Task Servers()
        {
            var serverFiles = Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory);

            int serverCount = 0;

            foreach (var shard in _discord.Shards)
            {
                serverCount += shard.Guilds.Count;
            }

            List<ulong> serversUsingNone = new List<ulong>();
            List<ulong> serversUsingGoLive = new List<ulong>();
            List<ulong> serversUsingAnnouncements = new List<ulong>();
            List<ulong> serversUsingPublished = new List<ulong>();
            List<ulong> serversUsingGreetings = new List<ulong>();
            List<ulong> serversUsingTeams = new List<ulong>();
            List<ulong> serversUsingGames = new List<ulong>();
            List<ulong> serversUsingFeeds = new List<ulong>();

            foreach (var s in serverFiles)
            {
                var server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(s));

                if (server.GoLiveChannel != 0 || server.OwnerLiveChannel != 0)
                {
                    serversUsingGoLive.Add(server.Id);
                }

                if (server.PublishedChannel != 0 || server.OwnerPublishedChannel != 0)
                {
                    serversUsingPublished.Add(server.Id);
                }

                if(server.TwitchFeedChannel != 0 || server.OwnerTwitchFeedChannel != 0)
                {
                    serversUsingFeeds.Add(server.Id);
                }

                if (server.GreetingsChannel != 0)
                {
                    serversUsingGreetings.Add(server.Id);
                }

                if(server.TwitchTeams != null && server.TwitchTeams.Count > 0)
                {
                    serversUsingTeams.Add(server.Id);
                }

                if (server.ServerGameList != null && server.ServerGameList.Count > 0)
                {
                    serversUsingGames.Add(server.Id);
                }

                if (server.GoLiveChannel == 0 && server.OwnerLiveChannel == 0
                    && server.PublishedChannel == 0 && server.OwnerPublishedChannel == 0
                    && server.GreetingsChannel == 0 && server.OwnerTwitchFeedChannel == 0 && server.TwitchFeedChannel == 0)
                {
                    serversUsingNone.Add(server.Id);
                }
            }

            string info = "```Markdown\r\n" +
                          "# " + _discord.CurrentUser.Username + "\r\n" +
                          "- Connected Servers: " + serverCount + "\r\n" +
                          "- Server Configurations: " + serverFiles.Length + "\r\n" +
                          "- Servers Announcing Livestreams: " + serversUsingGoLive.Count + "\r\n" +
                          "- Servers Announcing Published Content: " + serversUsingPublished.Count + "\r\n" +
                          "- Servers Announcing Greetings: " + serversUsingGreetings.Count + "\r\n" +
                          "- Servers Announcing Twitch Channel Feeds: " + serversUsingFeeds.Count + "\r\n" +
                          "- Servers Announcing Twitch Games: " + serversUsingGames.Count + "\r\n" +
                          "- Servers Announcing Twitch Teams: " + serversUsingTeams.Count + "\r\n" +
                          "- Servers Announcing Nothing: " + serversUsingNone.Count + "\r\n" +
                          "```\r\n";

            await Context.Channel.SendMessageAsync(info);
        }

        [Command("help"), Summary("Provides a link to the CouchBot Website.")]
        public async Task Help()
        {
            string info = "Need help? Ask me a question! ie: !cb help \"How can I add my Twitch account?\". Note: Please include quotes around your question.";

            await Context.Channel.SendMessageAsync(info);
        }

        [Command("help"), Summary("Provides a link to the CouchBot Website.")]
        public async Task Help(string question)
        {
            var response = await _apiAiManager.AskAQuestion(question);

            await Context.Channel.SendMessageAsync(response.result.speech);
        }

        [Command("invite"), Summary("Provide an invite link via DM.")]
        public async Task Invite()
        {
            await (await ((IGuildUser)(Context.Message.Author)).GetOrCreateDMChannelAsync()).SendMessageAsync("Want me to join your server? Click here: <https://discordapp.com/oauth2/authorize?client_id=308371905667137536&scope=bot&permissions=158720>");
        }

        [Command("uptime"), Summary("Get Uptime Statistic Information.")]
        public async Task Uptime()
        {
            var botStats = await _statisticsManager.GetBotStats();
            var minutesSinceStart = Math.Abs((int)DateTime.UtcNow.Subtract(botStats.LoggingStartDate).TotalMinutes);
            TimeSpan ts = new TimeSpan(0, botStats.UptimeMinutes, 0);

            decimal percentage = ((decimal)botStats.UptimeMinutes / minutesSinceStart) * (decimal)100;

            var info = "```Markdown\r\n" +
                    "# Uptime since initial tracking - " + botStats.LoggingStartDate.ToString("MM/dd/yyyy HH:mm:ss UTC") + "\r\n" +
                    "" + ts.Days + " day(s) " + ts.Hours + " hour(s)" + (ts.Minutes > 0 ? " and " + ts.Minutes + " minutes" : "") + ".\r\n" +
                    "Uptime Percentage: " + percentage.ToString("#.##") + "%\r\n" +
                    "Last Restart: " + botStats.LastRestart.ToString("MM/dd/yyyy HH:mm:ss UTC") + "\r\n" +
                    "```\r\n";

            await Context.Channel.SendMessageAsync(info);
        }

        [Command("alerts"), Summary("Get Alert Statistic Information.")]
        public async Task Alerts()
        {
            var botStats = await _statisticsManager.GetBotStats();

            var info = "```Markdown\r\n" +
                    "# Alerts since initial tracking - " + botStats.LoggingStartDate.ToString("MM/dd/yyyy hh:mm:ss UTC") + "\r\n" +
                    "Mixer - " + botStats.BeamAlertCount + "\r\n" +
                    "Mobcrush - " + botStats.MobcrushAlertCount + "\r\n" +
                    "Picarto - " + botStats.PicartoAlertCount + "\r\n" +
                    "Smashcast - " + botStats.HitboxAlertCount + "\r\n" +
                    "Twitch - " + botStats.TwitchAlertCount + "\r\n" +
                    "Vid.me - " + botStats.VidMeAlertCount + "\r\n" +
                    "YouTube - " + botStats.YouTubeAlertCount + "\r\n" +
                    "Total Alerts Sent - " + (botStats.YouTubeAlertCount + 
                                              botStats.MobcrushAlertCount + 
                                              botStats.BeamAlertCount + 
                                              botStats.TwitchAlertCount + 
                                              botStats.HitboxAlertCount + 
                                              botStats.VidMeAlertCount + 
                                              botStats.PicartoAlertCount) + "\r\n" +
                    "```\r\n";

            await Context.Channel.SendMessageAsync(info);
        }

        [Command("supporters"), Summary("Get Supporter Information")]
        public async Task Supporters()
        {
            var supporters = (File.ReadAllText(_botSettings.DirectorySettings.ConfigRootDirectory + "Supporters.txt")).Split(',');

            string info = "```Markdown\r\n" +
                          "# " + _discord.CurrentUser.Username + " Supporters\r\n" +
                          "I wanted to create a place to show my thanks to everyone that supports the development and growth of " + _discord.CurrentUser.Username + ", whether it be via <http://patreon.com/dawgeth>, or " +
                          "just in it's use. Thanks to all those out there that have used it, provided feedback, found bugs, and supported me through Patreon.\r\n\r\n" +
                          "Patron List:\r\n";

            foreach (var s in supporters)
            {
                info += "- " + s + "\r\n";
            }

            info += "- Your Name Could Be Here. Visit <http://patreon.com/dawgeth> today <3" +
                     "- Want to be a one time supporter? <http://paypal.me/dawgeth>" +
                     "```\r\n";

            await Context.Channel.SendMessageAsync(info);
        }

        [Command("ytidlookup"), Summary("Query YouTube API to find a Channel ID.")]
        public async Task YtIdLookup([Summary("Username to Query")]string name)
        {
            var channels = await _youtubeManager.GetYouTubeChannelByQuery(name);

            await Context.Channel.SendMessageAsync("YouTube Id Lookup Results: ");

            if (channels.items == null || channels.items.Count < 1)
            {
                await Context.Channel.SendMessageAsync("No Results Found.");
            }
            else
            {

                string channelInfo = "```\r\n";
                foreach (var channel in channels.items)
                {
                    channelInfo += "\r\n" +
                        "Id (Use This for " + _discord.CurrentUser.Username + " Settings): " + channel.snippet.channelId + "\r\n" +
                        "Channel Name: " + channel.snippet.channelTitle + "\r\n" +
                        "Description: " + channel.snippet.description + "\r\n";
                }

                channelInfo += "```";

                await Context.Channel.SendMessageAsync(channelInfo);
            }
        }

        [Command("haibai")]
        public async Task HaiBai()
        {
            await _statisticsManager.AddToHaiBaiCount();

            EmbedBuilder builder = new EmbedBuilder();
            builder.ImageUrl = "http://couchbot.io/img/bot/haibai.gif";
            builder.AddField(async f =>
            {
                f.Name = "Hai Bai Count!";
                f.Value = await _statisticsManager.GetHaiBaiCount();
                f.IsInline = false;
            });

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command("flip")]
        public async Task Flip()
        {
            await _statisticsManager.AddToFlipCount();

            EmbedBuilder builder = new EmbedBuilder();

            builder.Description = "(╯°□°）╯︵ <:CouchBotSemi10:312758619475279893>\r\n\r\nFlip Count: " + await _statisticsManager.GetFlipCount();

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command("unflip")]
        public async Task Unflip()
        {
            await _statisticsManager.AddToUnflipCount();

            EmbedBuilder builder = new EmbedBuilder();

            builder.Description = "<:couchbot:312752764247736320> ノ( ゜-゜ノ)\r\n\r\nUnflip Count: " + await _statisticsManager.GetUnflipCount();

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command("setbotgame")]
        public async Task SetBotGame(string game)
        {
            if (Context.User.Id != 93015586698727424)
            {
                await Context.Channel.SendMessageAsync("*Bbbbbzzztttt* You are not *zzzzt* Dawgeth. Acc *bbrrrttt* Access Denied.");

                return;
            }

            await _discord.SetGameAsync(game, "", StreamType.NotStreaming);
        }

        [Command("permissions"), Summary("Check bot permissions.")]
        public async Task Permissions(IGuildChannel channel)
        {
            var botUser = await Context.Guild.GetCurrentUserAsync();
            var channelPermissions = botUser.GetPermissions(channel);

            string info = "```Markdown\r\n" +
              "# " + _discord.CurrentUser.Username + " Permissions in " + botUser.Username + "\r\n" +
              "- Add Reactions: " + channelPermissions.AddReactions + "\r\n" +
              "- Attach Files: " + channelPermissions.AttachFiles + "\r\n" +
              "- Connect: " + channelPermissions.Connect + "\r\n" +
              "- Create Invite: " + channelPermissions.CreateInstantInvite + "\r\n" +
              "- Deafen: " + channelPermissions.DeafenMembers + "\r\n" +
              "- **Embed**: " + channelPermissions.EmbedLinks + "\r\n" +
              "- Manage Channel: " + channelPermissions.ManageChannel + "\r\n" +
              "- Manage Messages: " + channelPermissions.ManageMessages + "\r\n" +
              "- Manage Permissions: " + channelPermissions.ManagePermissions + "\r\n" +
              "- Manage Webhooks: " + channelPermissions.ManageWebhooks + "\r\n" +
              "- **Mention Everyone**: " + channelPermissions.MentionEveryone + "\r\n" +
              "- Move: " + channelPermissions.MoveMembers + "\r\n" +
              "- Mute: " + channelPermissions.MuteMembers + "\r\n" +
              "- **Read History**: " + channelPermissions.ReadMessageHistory + "\r\n" +
              "- **Read**: " + channelPermissions.ReadMessages + "\r\n" +
              "- **Send**: " + channelPermissions.SendMessages + "\r\n" +
              "- Send TTS: " + channelPermissions.SendTTSMessages + "\r\n" +
              "- Speak: " + channelPermissions.Speak + "\r\n" +
              "- External Emojis: " + channelPermissions.UseExternalEmojis + "\r\n" +
              "- Use VAD: " + channelPermissions.UseVAD + "\r\n" +
              "**Required Permissions are Bold.**\r\n" +
              "```";

            await Context.Channel.SendMessageAsync(info);
        }

        [Command("echo")]
        public async Task Echo(string message)
        {
            if(message.ToLower().Contains("@everyone") || message.ToLower().Contains("@here"))
            {
                message = message.Replace("@everyone", "@ everyone").Replace("@here", "@ here");
            }

            await Context.Channel.SendMessageAsync(StringUtilities.ScrubChatMessage(message));
        }

        [Command("echoembed")]
        public async Task EchoEmbed(string message)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.AddField("ECHO!", StringUtilities.ScrubChatMessage(message));

            await Context.Channel.SendMessageAsync("", false, eb.Build(), null);
        }

        [Command("ping")]
        public async Task Ping()
        {
            await Context.Channel.SendMessageAsync("Pong!");
        }

        [Command("purge")]
        public async Task Purge(IGuildUser user)
        {
            var deleteList = new List<IMessage>();

            if (!IsAdmin)
            {
                return;
            }

            var messages = Context.Channel.GetMessagesAsync(100);

            var message = (await messages.Flatten()).GetEnumerator();

            while (message.MoveNext())
            {
                if (message.Current.Author.Id == user.Id)
                {
                    deleteList.Add(message.Current);
                }
            }


            if (deleteList.Count > 0)
            {
                await Context.Channel.DeleteMessagesAsync(deleteList);
                await Context.Message.DeleteAsync();
            }
        }

        [Command("purge")]
        public async Task Purge(IGuildUser user, int count)
        {
            var deleteList = new List<IMessage>();

            if (!IsAdmin)
            {
                return;
            }

            if(count > 100)
            {
                await Context.Channel.SendMessageAsync("You can only purge up to 100 messages.");

                return;
            }
            await Context.Message.DeleteAsync();

            var messages = Context.Channel.GetMessagesAsync(count);

            var message = (await messages.Flatten()).GetEnumerator();

            while (message.MoveNext())
            {
                if (message.Current.Author.Id == user.Id)
                {
                    deleteList.Add(message.Current);
                }
            }


            if (deleteList.Count > 0)
            {
                await Context.Channel.DeleteMessagesAsync(deleteList);
            }
        }

        [Command("purge")]
        public async Task Purge(int count)
        {
            var deleteList = new List<IMessage>();

            if (!IsAdmin)
            {
                return;
            }

            if (count > 100)
            {
                await Context.Channel.SendMessageAsync("You can only purge up to 100 messages.");

                return;
            }

            await Context.Message.DeleteAsync();

            var messages = Context.Channel.GetMessagesAsync(count);

            var message = (await messages.Flatten()).GetEnumerator();

            while (message.MoveNext())
            {
                deleteList.Add(message.Current);
            }


            if (deleteList.Count > 0)
            {
                await Context.Channel.DeleteMessagesAsync(deleteList);
            }
        }

        [Command("purgeall")]
        public async Task PurgeAll()
        {
            var deleteList = new List<IMessage>();

            if (!IsAdmin)
            {
                return;
            }

            var messages = Context.Channel.GetMessagesAsync();

            var message = (await messages.Flatten()).GetEnumerator();

            while (message.MoveNext())
            {
                deleteList.Add(message.Current);
            }

            if (deleteList.Count > 0)
            {
                await Context.Channel.DeleteMessagesAsync(deleteList);
            }
        }

        [Command("requesthelp")]
        public async Task RequestHelp()
        {
            var supportChannel = (IMessageChannel)await Context.Client.GetChannelAsync(350288511414566932);

            var guild = ((IGuildUser)Context.Message.Author).Guild;
            var user = ((IGuildUser)Context.Message.Author);

            if (!user.GuildPermissions.ManageGuild)
            {
                return;
            }

            var file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + guild.Id + ".json";
            var server = new DiscordServer();

            if (File.Exists(file))
                server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(file));

            if (server != null)
            {
                var announce = await guild.GetChannelAsync(server.AnnouncementsChannel);
                var announceChannel = announce != null ? announce.Name : "Not Set";

                var golive = await guild.GetChannelAsync(server.GoLiveChannel);
                var goliveChannel = golive != null ? golive.Name : "Not Set";

                var ownerGolive = await guild.GetChannelAsync(server.OwnerLiveChannel);
                var ownerGoliveChannel = ownerGolive != null ? ownerGolive.Name : "Not Set";

                var greetings = await guild.GetChannelAsync(server.GreetingsChannel);
                var greetingsChannel = greetings != null ? greetings.Name : "Not Set";

                var vod = await guild.GetChannelAsync(server.PublishedChannel);
                var vodChannel = vod != null ? vod.Name : "Not Set";

                var ownerVod = await guild.GetChannelAsync(server.OwnerPublishedChannel);
                var ownerVodChannel = ownerVod != null ? ownerVod.Name : "Not Set";

                var ownerTwitchFeed = await guild.GetChannelAsync(server.OwnerTwitchFeedChannel);
                var ownerTwitchFeedChannel = ownerTwitchFeed != null ? ownerTwitchFeed.Name : "Not Set";

                var role = await _discordService.GetRoleByGuildAndId(server.Id, server.MentionRole);
                var roleName = "";

                if (role == null && server.MentionRole != 1)
                {
                    server.MentionRole = 0;
                }

                if (server.MentionRole == 0)
                {
                    roleName = "Everyone";
                }
                else if (server.MentionRole == 1)
                {
                    roleName = "Here";
                }
                else
                {
                    roleName = role.Name.Replace("@", "");
                }

                await supportChannel.SendMessageAsync("Support Requested For " + server.Name + " (" + server.Id + ") by " + Context.Message.Author.Username + " (" + Context.Message.Author.Id + ")\r\n");

                string info = "```Markdown\r\n" +
                              "# Configuration Settings\r\n" +
                              "- Owner Go Live Channel: " + ownerGoliveChannel + "\r\n" +
                              "- Owner Published Channel: " + ownerVodChannel + "\r\n" +
                              "- Owner Twitch Channel Feed Channel: " + ownerTwitchFeedChannel + "\r\n" +
                              "- Go Live Channel: " + goliveChannel + "\r\n" +
                              "- Published Channel: " + vodChannel + "\r\n" +
                              "- Greetings Channel: " + greetingsChannel + "\r\n" +
                              "- Allow @ Role: " + server.AllowEveryone + "\r\n" +
                              "- Allow Thumbnails: " + server.AllowThumbnails + "\r\n" +
                              "- Allow Greetings: " + server.Greetings + "\r\n" +
                              "- Allow Goodbyes: " + server.Goodbyes + "\r\n" +
                              "- Allow Live Content: " + server.AllowLive + "\r\n" +
                              "- Allow Published Content: " + server.AllowPublished + "\r\n" +
                              "- Allow Owner Twitch Channel Feed: " + server.AllowOwnerChannelFeed + "\r\n" +
                              "- Use Text Announcements: " + server.UseTextAnnouncements + "\r\n" +
                              "- Use YTG URLS For VOD Content: " + server.UseYouTubeGamingPublished + "\r\n" +
                              "- Live Message: " + (string.IsNullOrEmpty(server.LiveMessage) ? "Default" : server.LiveMessage) + "\r\n" +
                              "- Published Message: " + (string.IsNullOrEmpty(server.PublishedMessage) ? "Default" : server.PublishedMessage) + "\r\n" +
                              "- Greeting Message: " + (string.IsNullOrEmpty(server.GreetingMessage) ? "Default" : server.GreetingMessage) + "\r\n" +
                              "- Goodbye Message: " + (string.IsNullOrEmpty(server.GoodbyeMessage) ? "Default" : server.GoodbyeMessage) + "\r\n" +
                              "- Stream Offline Message: " + (string.IsNullOrEmpty(server.StreamOfflineMessage) ? "Default" : server.StreamOfflineMessage) + "\r\n" +
                              "- Mention Role: " + roleName + "\r\n" +
                              "- Time Zone Offset: " + server.TimeZoneOffset + "\r\n" +
                              "```\r\n";

                await supportChannel.SendMessageAsync(info);

                await Context.Channel.SendMessageAsync("Your request for support has been sent. Go to #support on the CouchBot Discord Server (<http://discord.couchbot.io>) for further assistance.");
            }

            //[Command("purgeservers")]
            //public async Task PurgeServers()
            //{
            //    if (Context.User.Id != Constants.OwnerId)
            //    {
            //        return;
            //    }

            //    List<ulong> toKeep = new List<ulong>();
            //    List<ulong> toDelete = new List<ulong>();
            //    List<IGuild> guilds = new List<IGuild>();

            //    var files = _fileService.GetConfiguredServerFileNames();

            //    foreach (var shard in _discord.Shards)
            //    {
            //        foreach (var guild in shard.Guilds)
            //        {
            //            if (!guilds.Contains(guild))
            //            {
            //                guilds.Add(guild);
            //            }
            //        }
            //    }

            //    foreach (var guild in guilds)
            //    {
            //        if(files.Contains(guild.Id.ToString()))
            //        {
            //            toKeep.Add(guild.Id);
            //        }
            //    }

            //    foreach (var file in files)
            //    {
            //        Console.WriteLine("File: " + file);
            //        if (!toKeep.Contains(ulong.Parse(file)))
            //        {
            //            toDelete.Add(ulong.Parse(file));
            //        }
            //    }

            //    foreach (var server in toDelete)
            //    {
            //        File.Move(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + @"\" + server + ".json", @"C:\temp\" + server + ".json");
            //    }
            //}

            //[Command("muppet")]
            //public async Task Muppet()
            //{
            //    if (Context.User.Id != Constants.OwnerId)
            //    {
            //        return;
            //    }

            //    List<ulong> configExists = new List<ulong>();
            //    List<ulong> configDoesntExist = new List<ulong>();
            //    List<IGuild> guilds = new List<IGuild>();

            //    var files = _fileService.GetConfiguredServerFileNames();

            //    foreach (var shard in _discord.Shards)
            //    {
            //        foreach(var guild in shard.Guilds)
            //        {
            //            if(!guilds.Contains(guild))
            //            {
            //                guilds.Add(guild);
            //            }
            //        }
            //    }

            //    await Context.Channel.SendMessageAsync("Checking " + guilds.Count + " Guilds.");

            //    foreach (var guild in guilds)
            //    {
            //        if (!files.Contains(guild.Id.ToString()))
            //        {
            //            configDoesntExist.Add(guild.Id);
            //        }
            //    }

            //    var message = "```";

            //    foreach(var guild in configDoesntExist)
            //    {
            //        var g = await Context.Client.GetGuildAsync(guild);
            //        var o = await g.GetOwnerAsync();

            //        message += "Name: " + g.Name + " (" + g.Id + ") - Owner: " + o.Username + " (" + o.Id + ")\r\n";
            //    }

            //    message += "```";

            //    await Context.Channel.SendMessageAsync(message);
            //}

            //[Command("leaveservers")]
            //public async Task LeaveServers()
            //{
            //    if (Context.User.Id != Constants.OwnerId)
            //    {
            //        return;
            //    }

            //    var serverFiles = Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory);

            //    foreach (var s in serverFiles)
            //    {
            //        var server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(s));

            //        if (server.GoLiveChannel == 0 && server.OwnerLiveChannel == 0
            //        && server.PublishedChannel == 0 && server.OwnerPublishedChannel == 0
            //        && server.GreetingsChannel == 0 && server.OwnerTwitchFeedChannel == 0 && server.TwitchFeedChannel == 0)
            //        {
            //            var guild = await Context.Client.GetGuildAsync(server.Id);
            //            await guild.LeaveAsync();
            //        }
            //    }
            //}
        }

        [Command("membercount")]
        [Alias("#")]
        public async Task MemberCount()
        {
            var guild = (SocketGuild) await Context.Client.GetGuildAsync(Context.Guild.Id);
            var currentUsers = guild.Users;
            var userCount = currentUsers.Count;
            var onlineCount = currentUsers.Where(x => x.Status == UserStatus.Online).Count();
            var botCount = currentUsers.Where(x => x.IsBot).Count();
            var trainerCount = currentUsers.Where(x => !x.IsBot).Count();

            EmbedBuilder eb = new EmbedBuilder();
            eb.AddField("Members", userCount, true);
            eb.AddField("Online", onlineCount, true);
            eb.AddField("Bots", botCount, true);

            await Context.Channel.SendMessageAsync("", false, eb.Build());
        }
    }
}
