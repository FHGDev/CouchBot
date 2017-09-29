using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using MTD.CouchBot.Domain;
using MTD.CouchBot.Domain.Models.Bot;
using MTD.CouchBot.Domain.Utilities;
using MTD.CouchBot.Managers;
using MTD.CouchBot.Models.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTD.CouchBot.Services
{
    public class MessagingService
    {
        private readonly DiscordShardedClient _discord;

        private readonly IAlertManager _alertManager;
        private readonly IStatisticsManager _statisticsManager;
        private readonly DiscordService _discordService;
        private readonly BotSettings _botSettings;
        private readonly FileService _fileService;

        public MessagingService(DiscordShardedClient discord, IAlertManager alertManager, IStatisticsManager statisticsManager,
            DiscordService discordService, IOptions<BotSettings> botSettings, FileService fileService)
        {
            _discord = discord;
            _alertManager = alertManager;
            _statisticsManager = statisticsManager;
            _discordService = discordService;
            _botSettings = botSettings.Value;
            _fileService = fileService;
        }

        public async Task<BroadcastMessage> BuildTestPublishedMessage(SocketUser user, ulong guildId, ulong channelId)
        {
            var servers = _fileService.GetConfiguredServers();
            var server = servers.FirstOrDefault(x => x.Id == guildId);

            if (server == null)
                return null;

            string url = "http://" + (server.UseYouTubeGamingPublished ? "gaming" : "www") + ".youtube.com/watch?v=B7wkzmZ4GBw";

            EmbedBuilder embed = new EmbedBuilder();
            EmbedAuthorBuilder author = new EmbedAuthorBuilder();
            EmbedFooterBuilder footer = new EmbedFooterBuilder();


            if (server.PublishedMessage == null)
            {
                server.PublishedMessage = "%CHANNEL% just published a new video.";
            }

            Color red = new Color(179, 18, 23);
            author.IconUrl = user.GetAvatarUrl() + "?_=" + Guid.NewGuid().ToString().Replace("-", "");
            author.Name = _discord.CurrentUser.Username;
            author.Url = url;
            footer.Text = "[" + Constants.YouTube + "] - " + DateTime.UtcNow.AddHours(server.TimeZoneOffset);
            footer.IconUrl = "http://couchbot.io/img/ytg.jpg";
            embed.Author = author;
            embed.Color = red;
            embed.Description = server.PublishedMessage.Replace("%CHANNEL%", "Test Channel").Replace("%TITLE%", "Test Title").Replace("%URL%", url);

            embed.Title = "Test Channel published a new video!";
            embed.ThumbnailUrl = "http://couchbot.io/img/bot/vader.jpg" + "?_=" + Guid.NewGuid().ToString().Replace("-", "");
            embed.ImageUrl = server.AllowThumbnails ? "http://couchbot.io/img/bot/test_thumbnail.jpg" + "?_=" + Guid.NewGuid().ToString().Replace("-", "") : "";
            embed.Footer = footer;

            var role = await _discordService.GetRoleByGuildAndId(server.Id, server.MentionRole);
            var roleName = "";

            if (role == null && server.MentionRole != 1)
            {
                server.MentionRole = 0;
            }

            if (server.MentionRole == 0)
            {
                roleName = "@everyone";
            }
            else if (server.MentionRole == 1)
            {
                roleName = "@here";
            }
            else
            {
                roleName = role.Mention;
            }

            var message = (server.AllowEveryone ? roleName + " " : "");

            if (server.UseTextAnnouncements)
            {
                if (!server.AllowThumbnails)
                {
                    url = "<" + url + ">";
                }

                message += "**[Test]** " + server.PublishedMessage.Replace("%CHANNEL%", "Test Channel").Replace("%TITLE%", "Test Title").Replace("%URL%", url);
            }

            var broadcastMessage = new BroadcastMessage()
            {
                GuildId = server.Id,
                ChannelId = channelId,
                UserId = "0",
                Message = message,
                Platform = "Test",
                Embed = (!server.UseTextAnnouncements ? embed.Build() : null)
            };

            return broadcastMessage;
        }

        public async Task<BroadcastMessage> BuildTestMessage(SocketUser user, ulong guildId, ulong channelId, string platform)
        {
            var servers = _fileService.GetConfiguredServers();
            var server = servers.FirstOrDefault(x => x.Id == guildId);

            if (server == null)
                return null;

            string gameName = "a game"; ;
            string url = "http://couchbot.io";

            EmbedBuilder embed = new EmbedBuilder();
            EmbedAuthorBuilder author = new EmbedAuthorBuilder();
            EmbedFooterBuilder footer = new EmbedFooterBuilder();

            if (server.LiveMessage == null)
            {
                server.LiveMessage = "%CHANNEL% just went live with %GAME% - %TITLE% - %URL%";
            }

            Color color = new Color(76, 144, 243);
            if (platform.Equals(Constants.Twitch))
            {
                color = new Color(100, 65, 164);
                footer.Text = "[" + Constants.Twitch + "] - " + DateTime.UtcNow.AddHours(server.TimeZoneOffset);
                footer.IconUrl = "http://couchbot.io/img/twitch.jpg";
            }
            else if (platform.Equals(Constants.YouTube) || platform.Equals(Constants.YouTubeGaming))
            {
                color = new Color(179, 18, 23);
                footer.Text = "[" + Constants.YouTubeGaming + "] - " + DateTime.UtcNow.AddHours(server.TimeZoneOffset);
                footer.IconUrl = "http://couchbot.io/img/ytg.jpg";
            }
            else if (platform.Equals(Constants.Smashcast))
            {
                color = new Color(153, 204, 0);
                footer.Text = "[" + Constants.Smashcast + "] - " + DateTime.UtcNow.AddHours(server.TimeZoneOffset);
                footer.IconUrl = "http://couchbot.io/img/smashcast.png";
            }
            else if (platform.Equals(Constants.Mixer))
            {
                color = new Color(76, 144, 243);
                footer.Text = "[" + Constants.Mixer + "] - " + DateTime.UtcNow.AddHours(server.TimeZoneOffset);
                footer.IconUrl = "http://couchbot.io/img/beam.jpg";
            }

            author.IconUrl = (user.GetAvatarUrl() != null ? user.GetAvatarUrl() : "http://couchbot.io/img/bot/discord.png") + "?_=" + Guid.NewGuid().ToString().Replace("-", "");
            author.Name = _discord.CurrentUser.Username;
            author.Url = url;
            embed.Author = author;
            embed.Color = color;
            embed.Description = server.LiveMessage.Replace("%CHANNEL%", "Test Channel").Replace("%GAME%", gameName).Replace("%TITLE%", "Test Title").Replace("%URL%", url);
            embed.Title = "Test Channel has gone live!";
            embed.ThumbnailUrl = "http://couchbot.io/img/bot/vader.jpg" + "?_=" + Guid.NewGuid().ToString().Replace("-", "");
            embed.ImageUrl = server.AllowThumbnails ? "http://couchbot.io/img/bot/test_thumbnail.jpg" + "?_=" + Guid.NewGuid().ToString().Replace("-", "") : "";
            embed.Footer = footer;

            var role = await _discordService.GetRoleByGuildAndId(server.Id, server.MentionRole);
            var roleName = "";

            if (role == null && server.MentionRole != 1)
            {
                server.MentionRole = 0;
            }

            if (server.MentionRole == 0)
            {
                roleName = "@everyone";
            }
            else if (server.MentionRole == 1)
            {
                roleName = "@here";
            }
            else
            {
                roleName = role.Mention;
            }

            var message = (server.AllowEveryone ? roleName + " " : "");

            if (server.UseTextAnnouncements)
            {
                if (!server.AllowThumbnails)
                {
                    url = "<" + url + ">";
                }

                message += "**[Test]** " + server.LiveMessage.Replace("%CHANNEL%", "Test Channel").Replace("%GAME%", gameName).Replace("%TITLE%", "Test Title").Replace("%URL%", url);
            }

            var broadcastMessage = new BroadcastMessage()
            {
                GuildId = server.Id,
                ChannelId = channelId,
                UserId = "0",
                Message = message,
                Platform = "Test",
                Embed = (!server.UseTextAnnouncements ? embed.Build() : null)
            };

            return broadcastMessage;
        }

        public async Task<BroadcastMessage> BuildMessage(string channel,
            string gameName, string title, string url, string avatarUrl, string thumbnailUrl, string platform,
            string channelId, DiscordServer server, ulong discordChannelId, string teamName)
        {
            EmbedBuilder embed = new EmbedBuilder();
            EmbedAuthorBuilder author = new EmbedAuthorBuilder();
            EmbedFooterBuilder footer = new EmbedFooterBuilder();

            if (server.LiveMessage == null)
            {
                server.LiveMessage = "%CHANNEL% just went live with %GAME% - %TITLE% - %URL%";
            }

            author.IconUrl = _discord.CurrentUser.GetAvatarUrl() + "?_=" + Guid.NewGuid().ToString().Replace("-", "");
            author.Name = _discord.CurrentUser.Username;
            author.Url = url;
            footer.Text = "[" + platform + "] - " + DateTime.UtcNow.AddHours(server.TimeZoneOffset);
            embed.Author = author;

            if (platform.Equals(Constants.Mixer))
            {
                embed.Color = Constants.Blue;
                embed.ThumbnailUrl = avatarUrl != null ?
                        avatarUrl + "?_=" + Guid.NewGuid().ToString().Replace("-", "") :
                        "https://mixer.com/_latest/assets/images/main/avatars/default.jpg";
                footer.IconUrl = "http://couchbot.io/img/beam.jpg";
            }
            else if (platform.Equals(Constants.YouTubeGaming))
            {
                embed.Color = Constants.Red;
                embed.ThumbnailUrl = avatarUrl + "?_=" + Guid.NewGuid().ToString().Replace("-", "");
                footer.IconUrl = "http://couchbot.io/img/ytg.jpg";
            }
            else if (platform.Equals(Constants.Twitch))
            {
                embed.Color = Constants.Purple;
                embed.ThumbnailUrl = avatarUrl != null ?
                        avatarUrl + "?_=" + Guid.NewGuid().ToString().Replace("-", "") :
                        "https://static-cdn.jtvnw.net/jtv_user_pictures/xarth/404_user_70x70.png";
                footer.IconUrl = "http://couchbot.io/img/twitch.jpg";
            }
            else if (platform.Equals(Constants.Smashcast))
            {
                embed.Color = Constants.Green;
                embed.ThumbnailUrl = avatarUrl + "?_=" + Guid.NewGuid().ToString().Replace("-", "");
                footer.IconUrl = "http://couchbot.io/img/smashcast.png";
            }
            else if (platform.Equals(Constants.Mobcrush))
            {
                embed.Color = Constants.Yellow;
                embed.ThumbnailUrl = avatarUrl + "?_=" + Guid.NewGuid().ToString().Replace("-", "");
                footer.IconUrl = "http://couchbot.io/img/mobcrush.jpg";
            }

            embed.Description = server.LiveMessage
                .Replace("%CHANNEL%", channel)
                .Replace("%GAME%", gameName)
                .Replace("%TITLE%", title)
                .Replace("%URL%", url);
            embed.Title = channel + (string.IsNullOrEmpty(teamName) ? "" : " from the team '" + teamName + "'") + " has gone live!";
            embed.ImageUrl = server.AllowThumbnails ? thumbnailUrl + "?_=" + Guid.NewGuid().ToString().Replace("-", "") : "";
            embed.Footer = footer;

            var role = await _discordService.GetRoleByGuildAndId(server.Id, server.MentionRole);
            var roleName = "";

            if (role == null && server.MentionRole != 1)
            {
                server.MentionRole = 0;
            }

            if (server.MentionRole == 0)
            {
                roleName = "@everyone";
            }
            else if (server.MentionRole == 1)
            {
                roleName = "@here";
            }
            else
            {
                roleName = role.Mention;
            }

            var message = (server.AllowEveryone ? roleName + " " : "");

            if (server.UseTextAnnouncements)
            {
                if (!server.AllowThumbnails)
                {
                    url = "<" + url + ">";
                }

                message += "**[" + platform + "]** " + server.LiveMessage.Replace("%CHANNEL%", channel).Replace("%GAME%", gameName).Replace("%TITLE%", title).Replace("%URL%", url);
            }

            var broadcastMessage = new BroadcastMessage()
            {
                GuildId = server.Id,
                ChannelId = discordChannelId,
                UserId = channelId,
                Message = message,
                Platform = platform,
                Embed = (!server.UseTextAnnouncements ? embed.Build() : null),
                DeleteOffline = server.DeleteWhenOffline
            };

            return broadcastMessage;
        }


        public async Task<List<ChannelMessage>> SendMessages(string platform, List<BroadcastMessage> messages)
        {
            var channelMessages = new List<ChannelMessage>();

            foreach (var message in messages)
            {
                var chat = await _discordService.GetMessageChannel(message.GuildId, message.ChannelId);

                if (chat != null)
                {
                    try
                    {
                        ChannelMessage channelMessage = new ChannelMessage();
                        channelMessage.ChannelId = message.ChannelId;
                        channelMessage.GuildId = message.GuildId;
                        channelMessage.DeleteOffline = message.DeleteOffline;

                        if (message.Embed != null)
                        {
                            RequestOptions options = new RequestOptions();
                            options.RetryMode = RetryMode.AlwaysRetry;
                            var msg = await chat.SendMessageAsync(message.Message, false, message.Embed, options);

                            if (msg != null || msg.Id != 0)
                            {
                                channelMessage.MessageId = msg.Id;
                            }
                        }
                        else
                        {
                            var msg = await chat.SendMessageAsync(message.Message);

                            if (msg != null || msg.Id != 0)
                            {
                                channelMessage.MessageId = msg.Id;
                            }
                        }

                        channelMessages.Add(channelMessage);

                        if (platform.Equals(Constants.Mixer))
                        {
                            await _statisticsManager.AddToBeamAlertCount();
                            await _alertManager.LogAlert(Constants.Mixer, message.GuildId);
                        }
                        else if (platform.Equals(Constants.Smashcast))
                        {
                            await _statisticsManager.AddToHitboxAlertCount();
                            await _alertManager.LogAlert(Constants.Smashcast, message.GuildId);
                        }
                        else if (platform.Equals(Constants.Twitch))
                        {
                            await _statisticsManager.AddToTwitchAlertCount();
                            await _alertManager.LogAlert(Constants.Twitch, message.GuildId);
                        }
                        else if (platform.Equals(Constants.YouTubeGaming))
                        {
                            await _statisticsManager.AddToYouTubeAlertCount();
                            await _alertManager.LogAlert(Constants.YouTubeGaming, message.GuildId);
                        }
                        else if (platform.Equals(Constants.Picarto))
                        {
                            await _statisticsManager.AddToPicartoAlertCount();
                            await _alertManager.LogAlert(Constants.Picarto, message.GuildId);
                        }
                        else if (platform.Equals(Constants.VidMe))
                        {
                            await _statisticsManager.AddToVidMeAlertCount();
                            await _alertManager.LogAlert(Constants.VidMe, message.GuildId);
                        }
                        else if (message.Platform.Equals(Constants.YouTube))
                        {
                            await _statisticsManager.AddToYouTubeAlertCount();
                            await _alertManager.LogAlert(Constants.YouTube, message.GuildId);
                        }
                        else if (message.Platform.Equals(Constants.Mobcrush))
                        {
                            await _statisticsManager.AddToMobcrushAlertCount();
                            await _alertManager.LogAlert(Constants.Mobcrush, message.GuildId);
                        }

                    }
                    catch (Exception ex)
                    {
                        Logging.LogError("Send Message Error: " + ex.Message + " in server " + message.GuildId);
                    }
                }
            }

            return channelMessages;
        }
    }
}
