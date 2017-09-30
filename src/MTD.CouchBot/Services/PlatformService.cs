using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using MTD.CouchBot.Domain;
using MTD.CouchBot.Domain.Models.Bot;
using MTD.CouchBot.Domain.Models.Mobcrush;
using MTD.CouchBot.Domain.Models.Picarto;
using MTD.CouchBot.Domain.Models.Shared;
using MTD.CouchBot.Domain.Models.Smashcast;
using MTD.CouchBot.Domain.Models.Twitch;
using MTD.CouchBot.Domain.Models.VidMe;
using MTD.CouchBot.Domain.Models.YouTube;
using MTD.CouchBot.Domain.Utilities;
using MTD.CouchBot.Managers;
using MTD.CouchBot.Models.Bot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTD.CouchBot.Services
{
    public class PlatformService
    {
        private readonly IYouTubeManager _youtubeManager;
        private readonly ITwitchManager _twitchManager;
        private readonly ISmashcastManager _smashcastManager;
        private readonly IMixerManager _mixerManager;
        private readonly IPicartoManager _picartoManager;
        private readonly IVidMeManager _vidMeManager;
        private readonly ILoggingManager _loggingManager;
        private readonly DiscordShardedClient _discord;
        private readonly MessagingService _messagingService;
        private readonly DiscordService _discordService;
        private readonly BotSettings _botSettings;
        private readonly FileService _fileService;
        private readonly IMobcrushManager _mobcrushManager;

        public PlatformService(IYouTubeManager youtubeManager, ITwitchManager twitchManager, ISmashcastManager smashcastManager, IMixerManager mixerManager,
            IPicartoManager picartoManager, IVidMeManager vidMeManager, ILoggingManager loggingManager, DiscordShardedClient discord,
            MessagingService messagingService, DiscordService discordService, FileService fileService, IOptions<BotSettings> botSettings,
            IMobcrushManager mobcrushManager)
        {
            _youtubeManager = youtubeManager;
            _twitchManager = twitchManager;
            _smashcastManager = smashcastManager;
            _mixerManager = mixerManager;
            _picartoManager = picartoManager;
            _vidMeManager = vidMeManager;
            _discord = discord;
            _messagingService = messagingService;
            _discordService = discordService;
            _loggingManager = loggingManager;
            _fileService = fileService;
            _botSettings = botSettings.Value;
            _mobcrushManager = mobcrushManager;
        }

        #region : Mixer : 

        #endregion

        #region : Mobcrush : 

        public async Task CheckMobcrushLive()
        {
            var servers = _fileService.GetConfiguredServers();
            var liveChannels = _fileService.GetCurrentlyLiveMobcrushChannels();

            // Loop through servers to broadcast.
            foreach (var server in servers)
            {
                if (!server.AllowLive)
                {
                    continue;
                }

                if (server.Id == 0 || server.GoLiveChannel == 0)
                { continue; }

                if (server.ServerMobcrushIds != null)
                {
                    foreach (var mobcrushId in server.ServerMobcrushIds)
                    {
                        var channel = liveChannels.FirstOrDefault(x => x.Name == mobcrushId);

                        Domain.Models.Mobcrush.ChannelBroadcastResponse stream = null;

                        try
                        {
                            stream = await _mobcrushManager.GetMobcrushBroadcastByChannelId(mobcrushId);
                        }
                        catch (Exception wex)
                        {
                            // Log our error and move to the next user.

                            Logging.LogError("Mobcrush Error: " + wex.Message + " for user: " + mobcrushId + " in Discord Server Id: " + server.Id);
                            continue;
                        }

                        // if our stream isnt null, and we have a return from mixer.
                        if (stream != null)
                        {
                            if (stream.IsLive)
                            {
                                var chat = await _discordService.GetMessageChannel(server.Id, server.GoLiveChannel);

                                if (chat == null)
                                {
                                    continue;
                                }

                                bool checkChannelBroadcastStatus = channel == null || !channel.Servers.Contains(server.Id);
                                bool checkGoLive = !string.IsNullOrEmpty(server.GoLiveChannel.ToString()) && server.GoLiveChannel != 0;

                                if (checkChannelBroadcastStatus)
                                {
                                    if (checkGoLive)
                                    {
                                        if (chat != null)
                                        {
                                            if (channel == null)
                                            {
                                                channel = new LiveChannel
                                                {
                                                    Name = mobcrushId,
                                                    Servers = new List<ulong>()
                                                };

                                                channel.Servers.Add(server.Id);

                                                liveChannels.Add(channel);
                                            }
                                            else
                                            {
                                                channel.Servers.Add(server.Id);
                                            }

                                            var user = await _mobcrushManager.GetMobcrushStreamById(stream.User.Id);

                                            string gameName = stream.Game == null ? "a game" : stream.Game.Name;
                                            string url = "https://mobcrush.com/" + user.Username;
                                            string avatarUrl = user.ProfileLogo == null ? "http://cdn.mobcrush.com/static/images/default-profile-pic.png" : user.ProfileLogo;
                                            string thumbnailUrl = "http://cdn.mobcrush.com/u/video/" + stream.Id + "/snapshot.jpg";

                                            var message = await _messagingService.BuildMessage(
                                                user.Username, gameName, stream.Title, url, avatarUrl, thumbnailUrl, Constants.Mobcrush,
                                                mobcrushId, server, server.GoLiveChannel, null);

                                            var finalCheck = _fileService.GetCurrentlyLiveMobcrushChannels().FirstOrDefault(x => x.Name == mobcrushId);

                                            if (finalCheck == null || !finalCheck.Servers.Contains(server.Id))
                                            {
                                                if (channel.ChannelMessages == null)
                                                {
                                                    channel.ChannelMessages = new List<ChannelMessage>();
                                                }

                                                channel.ChannelMessages.AddRange(await _messagingService.SendMessages(Constants.Mobcrush, new List<BroadcastMessage> { message }));

                                                File.WriteAllText(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.MobcrushDirectory + mobcrushId + ".json", JsonConvert.SerializeObject(channel));

                                                Logging.LogMobcrush(user.Username + " has gone online.");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public async Task CheckOwnerMobcrushLive()
        {
            var servers = _fileService.GetConfiguredServers();
            var liveChannels = _fileService.GetCurrentlyLiveMobcrushChannels();

            // Loop through servers to broadcast.
            foreach (var server in servers)
            {
                if (!server.AllowLive)
                {
                    continue;
                }

                if (server.Id == 0 || server.OwnerLiveChannel == 0)
                { continue; }

                if (server.OwnerMobcrushId != null)
                {
                    var channel = liveChannels.FirstOrDefault(x => x.Name == server.OwnerMobcrushId);

                    ChannelBroadcastResponse stream = null;

                    try
                    {
                        stream = await _mobcrushManager.GetMobcrushBroadcastByChannelId(server.OwnerMobcrushId);
                    }
                    catch (Exception wex)
                    {
                        // Log our error and move to the next user.

                        Logging.LogError("Mobcrush Error: " + wex.Message + " for user: " + server.OwnerMobcrushId + " in Discord Server Id: " + server.Id);
                        continue;
                    }

                    // if our stream isnt null, and we have a return from mixer.
                    if (stream != null)
                    {
                        if (stream.IsLive)
                        {
                            var chat = await _discordService.GetMessageChannel(server.Id, server.OwnerLiveChannel);

                            if (chat == null)
                            {
                                continue;
                            }

                            bool checkChannelBroadcastStatus = channel == null || !channel.Servers.Contains(server.Id);
                            bool checkGoLive = !string.IsNullOrEmpty(server.OwnerLiveChannel.ToString()) && server.OwnerLiveChannel != 0;

                            if (checkChannelBroadcastStatus)
                            {
                                if (checkGoLive)
                                {
                                    if (chat != null)
                                    {
                                        if (channel == null)
                                        {
                                            channel = new LiveChannel
                                            {
                                                Name = server.OwnerMobcrushId,
                                                Servers = new List<ulong>()
                                            };

                                            channel.Servers.Add(server.Id);

                                            liveChannels.Add(channel);
                                        }
                                        else
                                        {
                                            channel.Servers.Add(server.Id);
                                        }

                                        var user = await _mobcrushManager.GetMobcrushStreamById(stream.User.Id);

                                        string gameName = stream.Game == null ? "a game" : stream.Game.Name;
                                        string url = "https://mobcrush.com/" + user.Username;
                                        string avatarUrl = user.ProfileLogo == null ? "http://cdn.mobcrush.com/static/images/default-profile-pic.png" : user.ProfileLogo;
                                        string thumbnailUrl = "http://cdn.mobcrush.com/u/video/" + stream.Id + "/snapshot.jpg";

                                        var message = await _messagingService.BuildMessage(
                                                user.Username, gameName, stream.Title, url, avatarUrl, thumbnailUrl, Constants.Mobcrush,
                                                server.OwnerMobcrushId, server, server.OwnerLiveChannel, null);

                                        var finalCheck = _fileService.GetCurrentlyLiveMobcrushChannels().FirstOrDefault(x => x.Name == server.OwnerMobcrushId);

                                        if (finalCheck == null || !finalCheck.Servers.Contains(server.Id))
                                        {
                                            if (channel.ChannelMessages == null)
                                            {
                                                channel.ChannelMessages = new List<ChannelMessage>();
                                            }

                                            channel.ChannelMessages.AddRange(await _messagingService.SendMessages(Constants.Mobcrush, new List<BroadcastMessage> { message }));

                                            File.WriteAllText(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.MobcrushDirectory + server.OwnerMobcrushId + ".json", JsonConvert.SerializeObject(channel));

                                            Logging.LogMobcrush(user.Username + " has gone online.");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region : Picarto : 

        public async Task CheckPicartoLive()
        {
            var servers = _fileService.GetConfiguredServers();
            var liveChannels = _fileService.GetCurrentlyLivePicartoChannels();

            // Loop through servers to broadcast.
            foreach (var server in servers)
            {
                if (!server.AllowLive)
                {
                    continue;
                }

                if (server.Id == 0 || server.GoLiveChannel == 0)
                { continue; }

                if (server.PicartoChannels != null)
                {
                    foreach (var picartoChannel in server.PicartoChannels)
                    {
                        var channel = liveChannels.FirstOrDefault(x => x.Name.ToLower() == picartoChannel.ToLower());

                        PicartoChannel stream = null;

                        try
                        {
                            stream = await _picartoManager.GetChannelByName(picartoChannel);
                        }
                        catch (Exception wex)
                        {
                            // Log our error and move to the next user.

                            Logging.LogError("Picarto Error: " + wex.Message + " for user: " + picartoChannel + " in Discord Server Id: " + server.Id);
                            continue;
                        }

                        // if our stream isnt null, and we have a return from mixer.
                        if (stream != null)
                        {
                            if (stream.Online)
                            {
                                var chat = await _discordService.GetMessageChannel(server.Id, server.GoLiveChannel);

                                if (chat == null)
                                {
                                    continue;
                                }

                                bool checkChannelBroadcastStatus = channel == null || !channel.Servers.Contains(server.Id);
                                bool checkGoLive = !string.IsNullOrEmpty(server.GoLiveChannel.ToString()) && server.GoLiveChannel != 0;

                                if (checkChannelBroadcastStatus)
                                {
                                    if (checkGoLive)
                                    {
                                        if (chat != null)
                                        {
                                            if (channel == null)
                                            {
                                                channel = new LiveChannel
                                                {
                                                    Name = picartoChannel,
                                                    Servers = new List<ulong>()
                                                };

                                                channel.Servers.Add(server.Id);

                                                liveChannels.Add(channel);
                                            }
                                            else
                                            {
                                                channel.Servers.Add(server.Id);
                                            }

                                            if (server.LiveMessage == null)
                                            {
                                                server.LiveMessage = "%CHANNEL% just went live - %TITLE% - %URL%";
                                            }

                                            string url = "https://picarto.tv/user_data/usrimg/" + stream.Name.ToLower() + "/dsdefault.jpg";

                                            EmbedBuilder embedBuilder = new EmbedBuilder();
                                            EmbedAuthorBuilder author = new EmbedAuthorBuilder();
                                            EmbedFooterBuilder footer = new EmbedFooterBuilder();

                                            author.IconUrl = "https://picarto.tv/user_data/usrimg/" + stream.Name.ToLower() + "/dsdefault.jpg";
                                            author.Name = stream.Name;
                                            author.Url = "https://picarto.tv/" + stream.Name;
                                            embedBuilder.Author = author;

                                            footer.IconUrl = "https://picarto.tv/images/Picarto_logo.png";
                                            footer.Text = "[Picarto] - " + DateTime.UtcNow.AddHours(server.TimeZoneOffset);
                                            embedBuilder.Footer = footer;

                                            embedBuilder.Title = stream.Name + " has gone live!";
                                            embedBuilder.Color = new Color(192, 192, 192);
                                            embedBuilder.ThumbnailUrl = server.AllowThumbnails ? "https://picarto.tv/user_data/usrimg/" + stream.Name.ToLower() + "/dsdefault.jpg" : "";
                                            embedBuilder.ImageUrl = "https://thumb.picarto.tv/thumbnail/" + stream.Name + ".jpg";

                                            embedBuilder.Description = server.LiveMessage.Replace("%CHANNEL%", stream.Name).Replace("%TITLE%", stream.Title).Replace("%URL%", "https://picarto.tv/" + stream.Name).Replace("%GAME%", stream.Category);

                                            embedBuilder.AddField(f =>
                                            {
                                                f.Name = "Category";
                                                f.Value = stream.Category;
                                                f.IsInline = true;
                                            });

                                            embedBuilder.AddField(f =>
                                            {
                                                f.Name = "Adult Stream?";
                                                f.Value = stream.Adult ? "Yup!" : "Nope!";
                                                f.IsInline = true;
                                            });

                                            embedBuilder.AddField(f =>
                                            {
                                                f.Name = "Total Viewers";
                                                f.Value = stream.ViewersTotal;
                                                f.IsInline = true;
                                            });

                                            embedBuilder.AddField(f =>
                                            {
                                                f.Name = "Total Followers";
                                                f.Value = stream.Followers;
                                                f.IsInline = true;
                                            });

                                            var tags = new StringBuilder();
                                            foreach (var t in stream.Tags)
                                            {
                                                tags.Append(t + ", ");
                                            }

                                            embedBuilder.AddField(f =>
                                            {
                                                f.Name = "Stream Tags";
                                                f.Value = string.IsNullOrEmpty(tags.ToString().Trim().TrimEnd(',')) ? "None" : tags.ToString().Trim().TrimEnd(',');
                                                f.IsInline = false;
                                            });

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

                                                message += "**[Picarto]** " + server.LiveMessage.Replace("%CHANNEL%", stream.Name).Replace("%TITLE%", stream.Title).Replace("%URL%", "https://picarto.tv/" + stream.Name).Replace("%GAME%", stream.Category);
                                            }

                                            var broadcastMessage = new BroadcastMessage
                                            {
                                                GuildId = server.Id,
                                                ChannelId = server.GoLiveChannel,
                                                UserId = picartoChannel,
                                                Message = message,
                                                Platform = Constants.Picarto,
                                                Embed = (!server.UseTextAnnouncements ? embedBuilder.Build() : null)
                                            };

                                            var finalCheck = _fileService.GetCurrentlyLivePicartoChannels().FirstOrDefault(x => x.Name == picartoChannel);

                                            if (finalCheck == null || !finalCheck.Servers.Contains(server.Id))
                                            {
                                                if (channel.ChannelMessages == null)
                                                {
                                                    channel.ChannelMessages = new List<ChannelMessage>();
                                                }

                                                channel.ChannelMessages.AddRange(await _messagingService.SendMessages(Constants.Picarto, new List<BroadcastMessage> { broadcastMessage }));

                                                File.WriteAllText(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.PicartoDirectory + picartoChannel + ".json", JsonConvert.SerializeObject(channel));

                                                Logging.LogPicarto(picartoChannel + " has gone online.");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public async Task CheckOwnerPicartoLive()
        {
            var servers = _fileService.GetConfiguredServers();
            var liveChannels = _fileService.GetCurrentlyLivePicartoChannels();

            // Loop through servers to broadcast.
            foreach (var server in servers)
            {
                if (!server.AllowLive)
                {
                    continue;
                }

                if (server.Id == 0 || server.OwnerLiveChannel == 0)
                { continue; }

                if (server.OwnerPicartoChannel != null)
                {
                    var channel = liveChannels.FirstOrDefault(x => x.Name.ToLower() == server.OwnerPicartoChannel.ToLower());

                    PicartoChannel stream = null;

                    try
                    {
                        stream = await _picartoManager.GetChannelByName(server.OwnerPicartoChannel);
                    }
                    catch (Exception wex)
                    {
                        // Log our error and move to the next user.

                        Logging.LogError("Picarto Error: " + wex.Message + " for user: " + server.OwnerPicartoChannel + " in Discord Server Id: " + server.Id);
                        continue;
                    }

                    // if our stream isnt null, and we have a return from mixer.
                    if (stream != null)
                    {
                        if (stream.Online)
                        {
                            var chat = await _discordService.GetMessageChannel(server.Id, server.OwnerLiveChannel);

                            if (chat == null)
                            {
                                continue;
                            }

                            bool checkChannelBroadcastStatus = channel == null || !channel.Servers.Contains(server.Id);
                            bool checkGoLive = !string.IsNullOrEmpty(server.OwnerLiveChannel.ToString()) && server.OwnerLiveChannel != 0;

                            if (checkChannelBroadcastStatus)
                            {
                                if (checkGoLive)
                                {
                                    if (chat != null)
                                    {
                                        if (channel == null)
                                        {
                                            channel = new LiveChannel
                                            {
                                                Name = server.OwnerPicartoChannel,
                                                Servers = new List<ulong>()
                                            };

                                            channel.Servers.Add(server.Id);

                                            liveChannels.Add(channel);
                                        }
                                        else
                                        {
                                            channel.Servers.Add(server.Id);
                                        }

                                        if (server.LiveMessage == null)
                                        {
                                            server.LiveMessage = "%CHANNEL% just went live - %TITLE% - %URL%";
                                        }

                                        string url = "https://picarto.tv/user_data/usrimg/" + stream.Name.ToLower() + "/dsdefault.jpg";

                                        EmbedBuilder embedBuilder = new EmbedBuilder();
                                        EmbedAuthorBuilder author = new EmbedAuthorBuilder();
                                        EmbedFooterBuilder footer = new EmbedFooterBuilder();

                                        author.IconUrl = "https://picarto.tv/user_data/usrimg/" + stream.Name.ToLower() + "/dsdefault.jpg";
                                        author.Name = stream.Name;
                                        author.Url = "https://picarto.tv/" + stream.Name;
                                        embedBuilder.Author = author;

                                        footer.IconUrl = "https://picarto.tv/images/Picarto_logo.png";
                                        footer.Text = "[Picarto] - " + DateTime.UtcNow.AddHours(server.TimeZoneOffset);
                                        embedBuilder.Footer = footer;

                                        embedBuilder.Title = stream.Name + " has gone live!";
                                        embedBuilder.Color = new Color(192, 192, 192);
                                        embedBuilder.ThumbnailUrl = server.AllowThumbnails ? "https://picarto.tv/user_data/usrimg/" + stream.Name.ToLower() + "/dsdefault.jpg" : "";
                                        embedBuilder.ImageUrl = "https://thumb.picarto.tv/thumbnail/" + stream.Name + ".jpg";

                                        embedBuilder.Description = server.LiveMessage.Replace("%CHANNEL%", stream.Name).Replace("%TITLE%", stream.Title).Replace("%URL%", "https://picarto.tv/" + stream.Name).Replace("%GAME%", stream.Category);

                                        embedBuilder.AddField(f =>
                                        {
                                            f.Name = "Category";
                                            f.Value = stream.Category;
                                            f.IsInline = true;
                                        });

                                        embedBuilder.AddField(f =>
                                        {
                                            f.Name = "Adult Stream?";
                                            f.Value = stream.Adult ? "Yup!" : "Nope!";
                                            f.IsInline = true;
                                        });

                                        embedBuilder.AddField(f =>
                                        {
                                            f.Name = "Total Viewers";
                                            f.Value = stream.ViewersTotal;
                                            f.IsInline = true;
                                        });

                                        embedBuilder.AddField(f =>
                                        {
                                            f.Name = "Total Followers";
                                            f.Value = stream.Followers;
                                            f.IsInline = true;
                                        });

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
                                            roleName = role.Mention;
                                        }

                                        var message = (server.AllowEveryone ? roleName + " " : "");

                                        if (server.UseTextAnnouncements)
                                        {
                                            if (!server.AllowThumbnails)
                                            {
                                                url = "<" + url + ">";
                                            }

                                            message += "**[Picarto]** " + server.LiveMessage.Replace("%CHANNEL%", stream.Name).Replace("%TITLE%", stream.Title).Replace("%URL%", "https://picarto.tv/" + stream.Name).Replace("%GAME%", stream.Category);
                                        }

                                        var broadcastMessage = new BroadcastMessage
                                        {
                                            GuildId = server.Id,
                                            ChannelId = server.GoLiveChannel,
                                            UserId = server.OwnerPicartoChannel,
                                            Message = message,
                                            Platform = Constants.Picarto,
                                            Embed = (!server.UseTextAnnouncements ? embedBuilder.Build() : null)
                                        };

                                        var finalCheck = _fileService.GetCurrentlyLivePicartoChannels().FirstOrDefault(x => x.Name == server.OwnerPicartoChannel);

                                        if (finalCheck == null || !finalCheck.Servers.Contains(server.Id))
                                        {
                                            if (channel.ChannelMessages == null)
                                            {
                                                channel.ChannelMessages = new List<ChannelMessage>();
                                            }

                                            channel.ChannelMessages.AddRange(await _messagingService.SendMessages(Constants.Picarto, new List<BroadcastMessage> { broadcastMessage }));

                                            File.WriteAllText(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.PicartoDirectory + server.OwnerPicartoChannel + ".json", JsonConvert.SerializeObject(channel));

                                            Logging.LogPicarto(server.OwnerPicartoChannel + " has gone online.");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region : Smashcast : 

        public async Task CheckHitboxLive()
        {
            var servers = _fileService.GetConfiguredServers();
            var liveChannels = _fileService.GetCurrentlyLiveHitboxChannels();

            // Loop through servers to broadcast.
            foreach (var server in servers)
            {
                if (!server.AllowLive)
                {
                    continue;
                }

                if (server.Id == 0 || server.GoLiveChannel == 0)
                { continue; }

                if (server.ServerHitboxChannels != null)
                {
                    foreach (var hitboxChannel in server.ServerHitboxChannels)
                    {
                        var channel = liveChannels.FirstOrDefault(x => x.Name.ToLower() == hitboxChannel.ToLower());

                        SmashcastChannel stream = null;

                        try
                        {
                            stream = await _smashcastManager.GetChannelByName(hitboxChannel);
                        }
                        catch (Exception wex)
                        {
                            // Log our error and move to the next user.

                            Logging.LogError("Smashcast Error: " + wex.Message + " for user: " + hitboxChannel + " in Discord Server Id: " + server.Id);
                            continue;
                        }

                        // if our stream isnt null, and we have a return from mixer.
                        if (stream != null && stream.livestream != null && stream.livestream.Count > 0)
                        {
                            if (stream.livestream[0].media_is_live == "1")
                            {
                                var chat = await _discordService.GetMessageChannel(server.Id, server.GoLiveChannel);

                                if (chat == null)
                                {
                                    continue;
                                }

                                bool checkChannelBroadcastStatus = channel == null || !channel.Servers.Contains(server.Id);
                                bool checkGoLive = !string.IsNullOrEmpty(server.GoLiveChannel.ToString()) && server.GoLiveChannel != 0;

                                if (checkChannelBroadcastStatus)
                                {
                                    if (checkGoLive)
                                    {
                                        if (chat != null)
                                        {
                                            if (channel == null)
                                            {
                                                channel = new LiveChannel
                                                {
                                                    Name = hitboxChannel,
                                                    Servers = new List<ulong>()
                                                };

                                                channel.Servers.Add(server.Id);

                                                liveChannels.Add(channel);
                                            }
                                            else
                                            {
                                                channel.Servers.Add(server.Id);
                                            }

                                            string gameName = stream.livestream[0].category_name == null ? "a game" : stream.livestream[0].category_name;
                                            string url = "http://smashcast.tv/" + hitboxChannel;

                                            var message = await _messagingService.BuildMessage(
                                                hitboxChannel, gameName, stream.livestream[0].media_status, url, "http://edge.sf.hitbox.tv" +
                                                stream.livestream[0].channel.user_logo, "http://edge.sf.hitbox.tv" +
                                                stream.livestream[0].media_thumbnail_large, Constants.Smashcast, hitboxChannel, server, server.GoLiveChannel, null);

                                            var finalCheck = _fileService.GetCurrentlyLiveHitboxChannels().FirstOrDefault(x => x.Name == hitboxChannel);

                                            if (finalCheck == null || !finalCheck.Servers.Contains(server.Id))
                                            {
                                                if (channel.ChannelMessages == null)
                                                {
                                                    channel.ChannelMessages = new List<ChannelMessage>();
                                                }

                                                channel.ChannelMessages.AddRange(await _messagingService.SendMessages(Constants.Smashcast, new List<BroadcastMessage> { message }));

                                                File.WriteAllText(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.SmashcastDirectory + hitboxChannel + ".json", JsonConvert.SerializeObject(channel));

                                                Logging.LogSmashcast(hitboxChannel + " has gone online.");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public async Task CheckOwnerHitboxLive()
        {
            var servers = _fileService.GetConfiguredServers();
            var liveChannels = _fileService.GetCurrentlyLiveHitboxChannels();

            // Loop through servers to broadcast.
            foreach (var server in servers)
            {
                if (!server.AllowLive)
                {
                    continue;
                }

                if (server.Id == 0 || server.OwnerLiveChannel == 0)
                { continue; }

                if (server.OwnerHitboxChannel != null)
                {
                    var channel = liveChannels.FirstOrDefault(x => x.Name.ToLower() == server.OwnerHitboxChannel.ToLower());

                    SmashcastChannel stream = null;

                    try
                    {
                        stream = await _smashcastManager.GetChannelByName(server.OwnerHitboxChannel);
                    }
                    catch (Exception wex)
                    {
                        // Log our error and move to the next user.

                        Logging.LogError("Smashcast Error: " + wex.Message + " for user: " + server.OwnerHitboxChannel + " in Discord Server Id: " + server.Id);
                        continue;
                    }

                    // if our stream isnt null, and we have a return from mixer.
                    if (stream != null && stream.livestream != null && stream.livestream.Count > 0)
                    {
                        if (stream.livestream[0].media_is_live == "1")
                        {
                            var chat = await _discordService.GetMessageChannel(server.Id, server.OwnerLiveChannel);

                            if (chat == null)
                            {
                                continue;
                            }

                            bool checkChannelBroadcastStatus = channel == null || !channel.Servers.Contains(server.Id);
                            bool checkGoLive = !string.IsNullOrEmpty(server.OwnerLiveChannel.ToString()) && server.OwnerLiveChannel != 0;

                            if (checkChannelBroadcastStatus)
                            {
                                if (checkGoLive)
                                {
                                    if (chat != null)
                                    {
                                        if (channel == null)
                                        {
                                            channel = new LiveChannel
                                            {
                                                Name = server.OwnerHitboxChannel,
                                                Servers = new List<ulong>()
                                            };

                                            channel.Servers.Add(server.Id);

                                            liveChannels.Add(channel);
                                        }
                                        else
                                        {
                                            channel.Servers.Add(server.Id);
                                        }

                                        string gameName = stream.livestream[0].category_name == null ? "a game" : stream.livestream[0].category_name;
                                        string url = "http://smashcast.tv/" + server.OwnerHitboxChannel;

                                        var message = await _messagingService.BuildMessage(
                                            server.OwnerHitboxChannel, gameName, stream.livestream[0].media_status, url, "http://edge.sf.hitbox.tv" +
                                            stream.livestream[0].channel.user_logo, "http://edge.sf.hitbox.tv" +
                                            stream.livestream[0].media_thumbnail_large, Constants.Smashcast, server.OwnerHitboxChannel, server, server.OwnerLiveChannel, null);

                                        var finalCheck = _fileService.GetCurrentlyLiveHitboxChannels().FirstOrDefault(x => x.Name == server.OwnerHitboxChannel);

                                        if (finalCheck == null || !finalCheck.Servers.Contains(server.Id))
                                        {
                                            if (channel.ChannelMessages == null)
                                            {
                                                channel.ChannelMessages = new List<ChannelMessage>();
                                            }

                                            channel.ChannelMessages.AddRange(await _messagingService.SendMessages(Constants.Smashcast, new List<BroadcastMessage> { message }));

                                            File.WriteAllText(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.SmashcastDirectory + server.OwnerHitboxChannel + ".json", JsonConvert.SerializeObject(channel));

                                            Logging.LogSmashcast(server.OwnerHitboxChannel + " has gone online.");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region : Twitch :

        public async Task CheckTwitchLive()
        {
            var servers = _fileService.GetConfiguredServersWithLiveChannel();
            var liveChannels = _fileService.GetCurrentlyLiveTwitchChannels();
            var twitchChannelList = new List<TwitchChannelServerModel>();

            foreach (var server in servers)
            {
                if (!server.AllowLive)
                {
                    continue;
                }

                if (server.ServerTwitchChannelIds != null)
                {
                    foreach (var c in server.ServerTwitchChannelIds)
                    {
                        if (c == null)
                        {
                            continue;
                        }

                        var channelServerModel = twitchChannelList.Count == 0 ? null : twitchChannelList.FirstOrDefault(x => x.TwitchChannelId.Equals(c, StringComparison.CurrentCultureIgnoreCase));

                        if (channelServerModel == null)
                        {
                            twitchChannelList.Add(
                                new TwitchChannelServerModel
                                {
                                    TwitchChannelId = c,
                                    Servers = new List<ServerOwnerModel>
                                    {
                                        new ServerOwnerModel { ServerId = server.Id, IsOwner = false }
                                    }
                                });
                        }
                        else
                        {
                            channelServerModel.Servers.Add(new ServerOwnerModel
                            {
                                ServerId = server.Id,
                                IsOwner = false
                            });
                        }
                    }
                }

                if (server.OwnerTwitchChannelId != null)
                {
                    var channelServerModel =
                        twitchChannelList.FirstOrDefault(x =>
                            x.TwitchChannelId.Equals(server.OwnerTwitchChannelId,
                            StringComparison.CurrentCultureIgnoreCase));

                    if (channelServerModel == null)
                    {
                        twitchChannelList.Add(
                            new TwitchChannelServerModel
                            {
                                TwitchChannelId = server.OwnerTwitchChannelId,
                                Servers = new List<ServerOwnerModel>
                                {
                                        new ServerOwnerModel { ServerId = server.Id, IsOwner = true }
                                }
                            });
                    }
                    else
                    {
                        channelServerModel.Servers.Add(new ServerOwnerModel
                        {
                            ServerId = server.Id,
                            IsOwner = true
                        });
                    }
                }
            }

            var splitLists = GetTwitchIdLists(twitchChannelList);

            foreach (var list in splitLists)
            {
                TwitchStreamsV5 streams = null;

                try
                {
                    // Query Twitch for our stream.
                    streams = await _twitchManager.GetStreamsByIdList(list);
                }
                catch (Exception wex)
                {
                    // Log our error and move to the next user.

                    Logging.LogError("Twitch Server Error: " + wex.Message);
                    continue;
                }

                if (streams == null || streams.streams == null || streams.streams.Count < 1)
                {
                    continue;
                }

                foreach (var stream in streams.streams)
                {
                    // Get currently live channel from Live/Twitch, if it exists.
                    var channel = liveChannels.FirstOrDefault(x => x.Name == stream.channel._id.ToString());

                    if (stream != null)
                    {
                        var twitchChannel = twitchChannelList.FirstOrDefault(x => x.TwitchChannelId.Equals(stream.channel._id.ToString()));

                        if (twitchChannel == null)
                        {
                            continue;
                        }

                        foreach (var s in twitchChannel.Servers)
                        {
                            var server = _fileService.GetConfiguredServerById(s.ServerId);

                            if (server == null)
                            {
                                continue;
                            }

                            bool checkChannelBroadcastStatus = channel == null || !channel.Servers.Contains(server.Id);
                            bool checkGoLive =
                                s.IsOwner ? !string.IsNullOrEmpty(server.OwnerLiveChannel.ToString()) && server.OwnerLiveChannel != 0
                                          : !string.IsNullOrEmpty(server.GoLiveChannel.ToString()) && server.GoLiveChannel != 0;

                            if (checkChannelBroadcastStatus)
                            {
                                if (checkGoLive)
                                {
                                    if (channel == null)
                                    {
                                        channel = new LiveChannel
                                        {
                                            Name = stream.channel._id.ToString(),
                                            Servers = new List<ulong>()
                                        };

                                        channel.Servers.Add(server.Id);

                                        liveChannels.Add(channel);
                                    }
                                    else
                                    {
                                        channel.Servers.Add(server.Id);
                                    }

                                    // Build our message
                                    string url = stream.channel.url;
                                    string channelName = StringUtilities.ScrubChatMessage(stream.channel.display_name);
                                    string avatarUrl = stream.channel.logo != null ? stream.channel.logo : "https://static-cdn.jtvnw.net/jtv_user_pictures/xarth/404_user_70x70.png";
                                    string thumbnailUrl = stream.preview.large;

                                    var message = await _messagingService.BuildMessage(channelName, stream.game, stream.channel.status, url, avatarUrl,
                                        thumbnailUrl, Constants.Twitch, stream.channel._id.ToString(), server, s.IsOwner ? server.OwnerLiveChannel : server.GoLiveChannel, null);

                                    var finalCheck = _fileService.GetCurrentlyLiveTwitchChannels().FirstOrDefault(x => x.Name == stream.channel._id.ToString());

                                    if (finalCheck == null || !finalCheck.Servers.Contains(server.Id))
                                    {
                                        if (channel.ChannelMessages == null)
                                        {
                                            channel.ChannelMessages = new List<ChannelMessage>();
                                        }

                                        channel.ChannelMessages.AddRange(await _messagingService.SendMessages(Constants.Twitch, new List<BroadcastMessage> { message }));

                                        File.WriteAllText(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.TwitchDirectory + stream.channel._id.ToString() + ".json",
                                            JsonConvert.SerializeObject(channel));

                                        Logging.LogTwitch(channelName + " has gone online.");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public async Task CheckTwitchChannelFeeds()
        {
            // Nothing.
        }

        public async Task CheckTwitchOwnerChannelFeeds()
        {
            var servers = _fileService.GetConfiguredServers();

            // Loop through servers to broadcast.
            foreach (var server in servers)
            {
                if (!server.AllowOwnerChannelFeed)
                {
                    continue;
                }

                if (server.Id != 0 && server.OwnerTwitchFeedChannel != 0 &&
                    !string.IsNullOrEmpty(server.OwnerTwitchChannel) && !string.IsNullOrEmpty(server.OwnerTwitchChannelId))
                {
                    TwitchChannelFeed feed = null;

                    try
                    {
                        feed = await _twitchManager.GetChannelFeedPosts(server.OwnerTwitchChannelId);
                    }
                    catch (Exception wex)
                    {
                        Logging.LogError("Twitch Server Error: " + wex.Message + " in Discord Server Id: " + server.Id);
                        continue;
                    }

                    if (feed == null || feed.posts == null || feed.posts.Count == 0)
                    {
                        continue;
                    }

                    var chat = await _discordService.GetMessageChannel(server.Id, server.OwnerTwitchFeedChannel);

                    if (chat == null)
                    {
                        continue;
                    }

                    foreach (var post in feed.posts)
                    {
                        DateTime now = DateTime.UtcNow;
                        DateTime created = DateTime.Parse(post.created_at).ToUniversalTime();
                        TimeSpan diff = now - created;

                        if (diff.TotalMinutes <= 2)
                        {
                            string message = "**[New Channel Feed Update - " + created.AddHours(server.TimeZoneOffset).ToString("MM/dd/yyyy hh:mm tt") + "]**\r\n" +
                                post.body + "\r\n\r\n" +
                                "<https://twitch.tv/" + server.OwnerTwitchChannel + "/p/" + post.id + ">";

                            try
                            {
                                await chat.SendMessageAsync(message);
                            }
                            catch (Exception wex)
                            {
                                Logging.LogError("Twitch Channel Feed Error: " + wex.Message + " for user: " + server.OwnerTwitchChannel + " in server: " + server.Id);
                            }

                            Logging.LogTwitch(server.OwnerTwitchChannel + " posted a new channel feed message.");
                        }
                    }
                }
            }
        }

        public async Task CheckTwitchTeams()
        {
            var servers = _fileService.GetConfiguredServers();
            var liveChannels = _fileService.GetCurrentlyLiveTwitchChannels();

            // Loop through servers to broadcast.
            foreach (var server in servers)
            {
                if (!server.AllowLive)
                {
                    continue;
                }

                if (server.Id != 0 && server.GoLiveChannel != 0 &&
                    server.TwitchTeams != null && server.TwitchTeams.Count > 0)
                {

                    if (server.TwitchTeams == null)
                    {
                        continue;
                    }

                    foreach (var team in server.TwitchTeams)
                    {
                        var userList = await _twitchManager.GetDelimitedListOfTwitchMemberIds(team);
                        var teamResponse = await _twitchManager.GetTwitchTeamByName(team);

                        TwitchStreamsV5 streams = null;

                        try
                        {
                            // Query Twitch for our stream.
                            streams = await _twitchManager.GetStreamsByIdList(userList);
                        }
                        catch (Exception wex)
                        {
                            // Log our error and move to the next user.

                            Logging.LogError("Twitch Team Server Error: " + wex.Message + " in Discord Server Id: " + server.Id);
                            continue;
                        }

                        if (streams == null || streams.streams == null || streams.streams.Count < 1)
                        {
                            continue;
                        }

                        foreach (var stream in streams.streams)
                        {
                            // Get currently live channel from Live/Twitch, if it exists.
                            var channel = liveChannels.FirstOrDefault(x => x.Name == stream.channel._id.ToString());

                            if (stream != null)
                            {
                                var chat = await _discordService.GetMessageChannel(server.Id, server.GoLiveChannel);

                                if (chat == null)
                                {
                                    continue;
                                }

                                bool checkChannelBroadcastStatus = channel == null || !channel.Servers.Contains(server.Id);
                                bool checkGoLive = !string.IsNullOrEmpty(server.GoLiveChannel.ToString()) && server.GoLiveChannel != 0;

                                if (checkChannelBroadcastStatus)
                                {
                                    if (checkGoLive)
                                    {
                                        if (channel == null)
                                        {
                                            channel = new LiveChannel
                                            {
                                                Name = stream.channel._id.ToString(),
                                                Servers = new List<ulong>()
                                            };

                                            channel.Servers.Add(server.Id);

                                            liveChannels.Add(channel);
                                        }
                                        else
                                        {
                                            channel.Servers.Add(server.Id);
                                        }

                                        // Build our message
                                        string url = stream.channel.url;
                                        string channelName = StringUtilities.ScrubChatMessage(stream.channel.display_name);
                                        string avatarUrl = stream.channel.logo != null ? stream.channel.logo : "https://static-cdn.jtvnw.net/jtv_user_pictures/xarth/404_user_70x70.png";
                                        string thumbnailUrl = stream.preview.large;

                                        var message = await _messagingService.BuildMessage(channelName, stream.game, stream.channel.status, url, avatarUrl,
                                            thumbnailUrl, Constants.Twitch, stream.channel._id.ToString(), server, server.GoLiveChannel, teamResponse.DisplayName);

                                        var finalCheck = _fileService.GetCurrentlyLiveTwitchChannels().FirstOrDefault(x => x.Name == stream.channel._id.ToString());

                                        if (finalCheck == null || !finalCheck.Servers.Contains(server.Id))
                                        {
                                            if (channel.ChannelMessages == null)
                                            {
                                                channel.ChannelMessages = new List<ChannelMessage>();
                                            }

                                            channel.ChannelMessages.AddRange(await _messagingService.SendMessages(Constants.Twitch, new List<BroadcastMessage> { message }));

                                            File.WriteAllText(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.TwitchDirectory + stream.channel._id.ToString() + ".json",
                                                JsonConvert.SerializeObject(channel));

                                            Logging.LogTwitch(teamResponse.Name + " team member " + channelName + " has gone online.");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public async Task CheckTwitchGames()
        {
            var servers = _fileService.GetConfiguredServersWithLiveChannel();
            var liveChannels = _fileService.GetCurrentlyLiveTwitchChannels();
            var gameList = new List<TwitchGameServerModel>();

            foreach (var s in servers)
            {
                if (s.ServerGameList == null)
                {
                    continue;
                }

                foreach (var g in s.ServerGameList)
                {
                    var gameServerModel = gameList.FirstOrDefault(x => x.Name.Equals(g, StringComparison.CurrentCultureIgnoreCase));

                    if (gameServerModel == null)
                    {
                        gameList.Add(new TwitchGameServerModel { Name = g, Servers = new List<ulong> { s.Id } });
                    }
                    else
                    {
                        gameServerModel.Servers.Add(s.Id);
                    }
                }
            }

            foreach (var game in gameList)
            {
                List<TwitchStreamsV5.Stream> gameResponse = null;

                try
                {
                    // Query Twitch for our stream.
                    gameResponse = await _twitchManager.GetStreamsByGameName(game.Name);
                }
                catch (Exception wex)
                {
                    // Log our error and move to the next user.

                    Logging.LogError("Twitch Game Error: " + wex.Message);
                    continue;
                }

                if (gameResponse == null || gameResponse.Count == 0)
                {
                    continue;
                }

                int count = 0;

                foreach (var stream in gameResponse)
                {
                    if (count >= 5)
                    {
                        continue;
                    }

                    DateTime now = DateTime.UtcNow;
                    DateTime created = DateTime.Parse(stream.created_at).ToUniversalTime();
                    TimeSpan diff = now - created;
                    var interval = ((_botSettings.IntervalSettings.Twitch / 1000) / 60);

                    if (diff.TotalMinutes > interval)
                    {
                        continue;
                    }

                    foreach (var s in game.Servers)
                    {
                        var server = _fileService.GetConfiguredServerById(s);

                        if (server == null)
                        {
                            continue;
                        }

                        var channel = liveChannels.FirstOrDefault(x => x.Name == stream.channel._id.ToString());

                        var chat = await _discordService.GetMessageChannel(server.Id, server.GoLiveChannel);

                        if (chat == null)
                        {
                            continue;
                        }

                        bool checkChannelBroadcastStatus = channel == null || !channel.Servers.Contains(server.Id);

                        if (checkChannelBroadcastStatus)
                        {
                            if (channel == null)
                            {
                                channel = new LiveChannel
                                {
                                    Name = stream.channel._id.ToString(),
                                    Servers = new List<ulong>()
                                };

                                channel.Servers.Add(server.Id);

                                liveChannels.Add(channel);
                            }
                            else
                            {
                                channel.Servers.Add(server.Id);
                            }

                            // Build our message
                            string url = stream.channel.url;
                            string channelName = StringUtilities.ScrubChatMessage(stream.channel.display_name);
                            string avatarUrl = stream.channel.logo != null ? stream.channel.logo : "https://static-cdn.jtvnw.net/jtv_user_pictures/xarth/404_user_70x70.png";
                            string thumbnailUrl = stream.preview.large;

                            var message = await _messagingService.BuildMessage(channelName, stream.game, stream.channel.status, url, avatarUrl,
                                thumbnailUrl, Constants.Twitch, stream.channel._id.ToString(), server, server.GoLiveChannel, null);

                            var finalCheck = _fileService.GetCurrentlyLiveTwitchChannels().FirstOrDefault(x => x.Name == stream.channel._id.ToString());

                            if (finalCheck == null || !finalCheck.Servers.Contains(server.Id))
                            {
                                if (channel.ChannelMessages == null)
                                {
                                    channel.ChannelMessages = new List<ChannelMessage>();
                                }

                                channel.ChannelMessages.AddRange(await _messagingService.SendMessages(Constants.Twitch, new List<BroadcastMessage> { message }));

                                File.WriteAllText(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.TwitchDirectory + stream.channel._id.ToString() + ".json",
                                    JsonConvert.SerializeObject(channel));

                                Logging.LogTwitch(channelName + " has gone live playing " + game.Name);
                            }
                        }
                    }

                    count++;
                }
            }
        }

        public async Task CheckTwitchServer()
        {
            var servers = _fileService.GetServersWithLiveChannelAndAllowDiscover();
            var liveChannels = _fileService.GetCurrentlyLiveTwitchChannels();

            foreach (var server in servers)
            {
                if (string.IsNullOrEmpty(server.DiscoverTwitch))
                {
                    continue;
                }

                var guild = (IGuild)_discord.GetGuild(server.Id);

                if (guild == null)
                {
                    continue;
                }

                var users = (await guild.GetUsersAsync()).Where(u => u.Game.HasValue && !string.IsNullOrEmpty(u.Game.Value.StreamUrl)).ToList();

                if (users == null || users.Count == 0)
                {
                    continue;
                }

                var twitchNameList = new List<string>();

                foreach (var u in users)
                {
                    if (server.DiscoverTwitch.Equals("all") || (server.DiscoverTwitch.Equals("role") && u.RoleIds.Contains(server.DiscoverTwitchRole)))
                    {
                        if (u.Game.Value.StreamType == StreamType.Twitch && u.Game.Value.StreamUrl.Contains("twitch.tv"))
                        {
                            twitchNameList.Add(u.Game.Value.StreamUrl.Replace("https://www.twitch.tv/", "").Replace("http://www.twitch.tv/", "").Replace("'", "").Replace("%20", ""));
                        }
                    }
                }

                if (twitchNameList.Count == 0)
                {
                    continue;
                }

                List<string> twitchIds = new List<string>();

                twitchIds = (await _twitchManager.GetTwitchIdsByLoginList(string.Join(",", twitchNameList)));

                if (twitchIds == null)
                {
                    continue;
                }

                foreach (var twitchId in twitchIds)
                {

                    var currentlyLive = liveChannels.FirstOrDefault(lc => lc.Name.Equals(twitchId));

                    if (currentlyLive != null)
                    {
                        if (currentlyLive.Servers.Contains(server.Id))
                        {
                            continue;
                        }
                    }

                    TwitchStreamV5 liveStream = null;

                    try
                    {
                        // Query Twitch for our stream.
                        liveStream = await _twitchManager.GetStreamById(twitchId);
                    }
                    catch (Exception wex)
                    {
                        // Log our error and move to the next user.
                        Logging.LogError("Twitch Server Error: " + wex.Message);
                        continue;
                    }

                    if (liveStream == null || liveStream.stream == null)
                    {
                        continue;
                    }

                    var stream = liveStream.stream;

                    if (currentlyLive == null)
                    {
                        currentlyLive = new LiveChannel
                        {
                            Name = stream.channel._id.ToString(),
                            Servers = new List<ulong>()
                        };

                        currentlyLive.Servers.Add(server.Id);

                        liveChannels.Add(currentlyLive);
                    }
                    else
                    {
                        currentlyLive.Servers.Add(server.Id);
                    }

                    // Build our message
                    string url = stream.channel.url;
                    string channelName = StringUtilities.ScrubChatMessage(stream.channel.display_name);
                    string avatarUrl = stream.channel.logo != null ? stream.channel.logo : "https://static-cdn.jtvnw.net/jtv_user_pictures/xarth/404_user_70x70.png";
                    string thumbnailUrl = stream.preview.large;

                    var message = await _messagingService.BuildMessage(channelName, stream.game, stream.channel.status, url, avatarUrl,
                        thumbnailUrl, Constants.Twitch, stream.channel._id.ToString(), server, server.GoLiveChannel, null);

                    var finalCheck = _fileService.GetCurrentlyLiveTwitchChannels().FirstOrDefault(x => x.Name == stream.channel._id.ToString());

                    if (finalCheck == null || !finalCheck.Servers.Contains(server.Id))
                    {
                        if (currentlyLive.ChannelMessages == null)
                        {
                            currentlyLive.ChannelMessages = new List<ChannelMessage>();
                        }

                        currentlyLive.ChannelMessages.AddRange(await _messagingService.SendMessages(Constants.Twitch, new List<BroadcastMessage> { message }));

                        File.WriteAllText(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.TwitchDirectory + stream.channel._id.ToString() + ".json",
                            JsonConvert.SerializeObject(currentlyLive));

                        Logging.LogTwitch(channelName + " has gone online.");
                    }
                }
            }
        }

        public List<string> GetTwitchIdLists(List<TwitchChannelServerModel> twitchChannelList)
        {
            var allTwitchIdsBuilder = new StringBuilder();
            var lists = new List<string>();

            foreach (var c in twitchChannelList)
            {
                allTwitchIdsBuilder.Append(c.TwitchChannelId + ",");
            }

            var allTwitchIds = allTwitchIdsBuilder.ToString().TrimEnd(',');
            var splitList = allTwitchIds.Split(',');

            var list = new StringBuilder();

            for (int i = 0; i < splitList.Length; i++)
            {
                list.Append(splitList[i] + ",");

                if (i % 100 == 0 && i != 0)
                {
                    var sublist = list.ToString().TrimEnd(',');

                    lists.Add(sublist);

                    list.Clear();
                }
            }

            if (!string.IsNullOrEmpty(list.ToString()))
            {
                var sublist = list.ToString().TrimEnd(',');

                lists.Add(sublist);
            }

            return lists;
        }

        #endregion

        #region : YouTube : 

        public async Task CheckYouTubeLive()
        {
            var servers = _fileService.GetConfiguredServersWithLiveChannel();
            var liveChannels = _fileService.GetCurrentlyLiveTwitchChannels();
            var youtubeChannelList = new List<YouTubeChannelServerModel>();

            foreach (var server in servers)
            {
                if (!server.AllowLive)
                {
                    continue;
                }

                if (server.ServerYouTubeChannelIds != null)
                {
                    foreach (var c in server.ServerYouTubeChannelIds)
                    {
                        if (c == null)
                        {
                            continue;
                        }

                        var channelServerModel = youtubeChannelList.Count == 0 ? null : youtubeChannelList.FirstOrDefault(x => x.YouTubeChannelId.Equals(c, StringComparison.CurrentCultureIgnoreCase));

                        if (channelServerModel == null)
                        {
                            youtubeChannelList.Add(
                                new YouTubeChannelServerModel
                                {
                                    YouTubeChannelId = c,
                                    Servers = new List<ServerOwnerModel>
                                    {
                                        new ServerOwnerModel { ServerId = server.Id, IsOwner = false }
                                    }
                                });
                        }
                        else
                        {
                            channelServerModel.Servers.Add(new ServerOwnerModel
                            {
                                ServerId = server.Id,
                                IsOwner = false
                            });
                        }
                    }
                }

                if (server.OwnerYouTubeChannelId != null)
                {
                    var channelServerModel =
                        youtubeChannelList.FirstOrDefault(x =>
                            x.YouTubeChannelId.Equals(server.OwnerYouTubeChannelId,
                            StringComparison.CurrentCultureIgnoreCase));

                    if (channelServerModel == null)
                    {
                        youtubeChannelList.Add(
                            new YouTubeChannelServerModel
                            {
                                YouTubeChannelId = server.OwnerYouTubeChannelId,
                                Servers = new List<ServerOwnerModel>
                                {
                                        new ServerOwnerModel { ServerId = server.Id, IsOwner = true }
                                }
                            });
                    }
                    else
                    {
                        channelServerModel.Servers.Add(new ServerOwnerModel
                        {
                            ServerId = server.Id,
                            IsOwner = true
                        });
                    }
                }
            }

            var splitLists = GetYouTubeIdLists(youtubeChannelList);

            foreach (var list in splitLists)
            {
                LiveChatStatusResponse streamResult = null;

                try
                {
                    // Query Youtube for our stream.
                    streamResult = await _youtubeManager.GetLiveChannels(list);
                }
                catch (Exception wex)
                {
                    // Log our error and move to the next user.
                    Logging.LogError("YouTube Error: " + wex.Message);
                    continue;
                }

                if (streamResult != null && streamResult.Items.Count > 0)
                {
                    foreach (var item in streamResult.Items)
                    {
                        var youtubeChannel = youtubeChannelList.FirstOrDefault(x => x.YouTubeChannelId.Equals(item.Snippet.ChannelId));

                        if (youtubeChannel == null)
                        {
                            continue;
                        }

                        YouTubeSearchListChannel streamResponse = null;

                        try
                        {
                            streamResponse = await _youtubeManager.GetVideoById(item.Snippet.CurrentVideoId);
                        }
                        catch (Exception ex)
                        {
                            // Log our error and move to the next user.
                            Logging.LogError("YouTube Error: " + ex.Message);
                            continue;
                        }

                        if (streamResponse == null || streamResponse.items == null || streamResponse.items.Count == 0)
                        {
                            continue;
                        }

                        var stream = streamResponse.items[0];

                        if (stream.snippet.liveBroadcastContent != "live")
                        {
                            continue;
                        }

                        foreach (var s in youtubeChannel.Servers)
                        {
                            var server = _fileService.GetConfiguredServerById(s.ServerId);
                            var channel = liveChannels.FirstOrDefault(x => x.Name.ToLower() == item.Snippet.ChannelId.ToLower());

                            bool checkChannelBroadcastStatus = channel == null || !channel.Servers.Contains(server.Id);
                            bool checkGoLive =
                                s.IsOwner ? !string.IsNullOrEmpty(server.OwnerLiveChannel.ToString()) && server.OwnerLiveChannel != 0
                                          : !string.IsNullOrEmpty(server.GoLiveChannel.ToString()) && server.GoLiveChannel != 0;

                            if (!checkChannelBroadcastStatus)
                            {
                                continue;
                            }

                            if (!checkGoLive)
                            {
                                continue;
                            }

                            if (channel == null)
                            {
                                channel = new LiveChannel
                                {
                                    Name = item.Snippet.ChannelId,
                                    Servers = new List<ulong>()
                                };

                                channel.Servers.Add(server.Id);

                                liveChannels.Add(channel);
                            }
                            else
                            {
                                channel.Servers.Add(server.Id);
                            }

                            // Build our message
                            YouTubeChannelSnippet channelData = null;

                            try
                            {
                                channelData = await _youtubeManager.GetYouTubeChannelSnippetById(item.Snippet.ChannelId);
                            }
                            catch (Exception wex)
                            {
                                // Log our error and move to the next user.

                                Logging.LogError("YouTube Error: " + wex.Message + " for user: " + item.Snippet.ChannelId);
                                continue;
                            }

                            if (channelData == null)
                            {
                                continue;
                            }

                            string url = "http://" + (server.UseYouTubeGamingPublished ? "gaming" : "www") + ".youtube.com/watch?v=" + stream.id;
                            string channelTitle = stream.snippet.channelTitle;
                            string avatarUrl = channelData.items.Count > 0 ? channelData.items[0].snippet.thumbnails.high.url : "";
                            string thumbnailUrl = stream.snippet.thumbnails.high.url;

                            var message = await _messagingService.BuildMessage(channelTitle, "a game", stream.snippet.title,
                                url, avatarUrl, thumbnailUrl, Constants.YouTubeGaming, item.Snippet.ChannelId, server,
                                s.IsOwner ? server.OwnerLiveChannel : server.GoLiveChannel, null);

                            var finalCheck = _fileService.GetCurrentlyLiveYouTubeChannels().FirstOrDefault(x => x.Name == item.Snippet.ChannelId);

                            if (finalCheck == null || !finalCheck.Servers.Contains(server.Id))
                            {
                                if (channel.ChannelMessages == null)
                                {
                                    channel.ChannelMessages = new List<ChannelMessage>();
                                }

                                channel.ChannelMessages.AddRange(await _messagingService.SendMessages(Constants.YouTubeGaming, new List<BroadcastMessage> { message }));
                                Logging.LogYouTubeGaming(channelTitle + " has gone online.");
                                File.WriteAllText(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.YouTubeDirectory + item.Snippet.ChannelId + ".json", JsonConvert.SerializeObject(channel));
                            }
                        }
                    }
                }
            }
        }

        public async Task CheckPublishedYouTube()
        {
            var servers = _fileService.GetConfiguredServers();
            var now = DateTime.UtcNow;
            var then = now.AddMilliseconds(-(_botSettings.IntervalSettings.YouTubePublished));

            foreach (var server in servers)
            {
                if (!server.AllowPublished)
                {
                    continue;
                }

                // If server isnt set or published channel isnt set, skip it.
                if (server.Id == 0 || server.PublishedChannel == 0)
                {
                    continue;
                }

                // If they dont allow published, skip it.
                if (!server.AllowPublished)
                {
                    continue;
                }

                var chat = await _discordService.GetMessageChannel(server.Id, server.PublishedChannel);

                if (chat == null)
                {
                    continue;
                }

                if (server.ServerYouTubeChannelIds == null || server.ServerYouTubeChannelIds.Count < 0)
                {
                    continue;
                }

                foreach (var user in server.ServerYouTubeChannelIds)
                {
                    if (string.IsNullOrEmpty(user))
                    {
                        continue;
                    }

                    YouTubePlaylist playlist = null;

                    try
                    {
                        var details = await _youtubeManager.GetContentDetailsByChannelId(user);

                        if (details == null || details.items == null || details.items.Count < 1 || string.IsNullOrEmpty(details.items[0].contentDetails.relatedPlaylists.uploads))
                        {
                            continue;
                        }

                        playlist = await _youtubeManager.GetPlaylistItemsByPlaylistId(details.items[0].contentDetails.relatedPlaylists.uploads);

                        if (playlist == null || playlist.items == null || playlist.items.Count < 1)
                        {
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError("YouTube Published Error: " + ex.Message + " for user: " + user + " in Discord Server: " + server.Id);
                        continue;
                    }

                    foreach (var video in playlist.items)
                    {
                        var publishDate = DateTime.Parse(video.snippet.publishedAt, null, System.Globalization.DateTimeStyles.AdjustToUniversal);

                        if (!(publishDate > then && publishDate < now))
                        {
                            continue;
                        }

                        string url = "http://" + (server.UseYouTubeGamingPublished ? "gaming" : "www") + ".youtube.com/watch?v=" + video.snippet.resourceId.videoId;

                        EmbedBuilder embed = new EmbedBuilder();
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder();
                        EmbedFooterBuilder footer = new EmbedFooterBuilder();

                        YouTubeChannelSnippet channelData = null;

                        try
                        {
                            channelData = await _youtubeManager.GetYouTubeChannelSnippetById(video.snippet.channelId);
                        }
                        catch (Exception wex)
                        {
                            // Log our error and move to the next user.

                            Logging.LogError("YouTube Error: " + wex.Message + " for user: " + video.snippet.channelId);
                            continue;
                        }

                        if (channelData == null)
                        {
                            continue;
                        }

                        if (server.PublishedMessage == null)
                        {
                            server.PublishedMessage = "%CHANNEL% just published a new video - %TITLE% - %URL%";
                        }

                        Color red = new Color(179, 18, 23);
                        author.IconUrl = _discord.CurrentUser.GetAvatarUrl() + "?_=" + Guid.NewGuid().ToString().Replace("-", "");
                        author.Name = _discord.CurrentUser.Username;
                        author.Url = url;
                        footer.Text = "[" + Constants.YouTube + "] - " + DateTime.UtcNow.AddHours(server.TimeZoneOffset);
                        footer.IconUrl = "http://couchbot.io/img/ytg.jpg";
                        embed.Author = author;
                        embed.Color = red;
                        embed.Description = server.PublishedMessage.Replace("%CHANNEL%", video.snippet.channelTitle).Replace("%GAME%", "a game").Replace("%TITLE%", video.snippet.title).Replace("%URL%", url);
                        embed.Title = video.snippet.channelTitle + " published a new video!";
                        embed.ThumbnailUrl = channelData.items.Count > 0 ? channelData.items[0].snippet.thumbnails.high.url + "?_=" + Guid.NewGuid().ToString().Replace("-", "") : "";
                        embed.ImageUrl = server.AllowThumbnails ? video.snippet.thumbnails.high.url + "?_=" + Guid.NewGuid().ToString().Replace("-", "") : "";
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

                            message += "**[" + Constants.YouTube + "]** " + server.PublishedMessage.Replace("%CHANNEL%", video.snippet.channelTitle).Replace("%TITLE%", video.snippet.title).Replace("%URL%", url);
                        }

                        Logging.LogYouTube(video.snippet.channelTitle + " has published a new video.");

                        await _messagingService.SendMessages(Constants.YouTube, new List<BroadcastMessage> {
                                new BroadcastMessage
                                {
                                    GuildId = server.Id,
                                    ChannelId = server.PublishedChannel,
                                    UserId = user,
                                    Message = message,
                                    Platform = Constants.YouTube,
                                    Embed = (!server.UseTextAnnouncements ? embed.Build() : null)
                                }
                            }
                        );
                    }
                }
            }
        }

        public async Task CheckOwnerPublishedYouTube()
        {
            var servers = _fileService.GetConfiguredServers();
            var now = DateTime.UtcNow;
            var then = now.AddMilliseconds(-(_botSettings.IntervalSettings.YouTubePublished));

            foreach (var server in servers)
            {
                if (!server.AllowPublished)
                {
                    continue;
                }

                // If server isnt set or published channel isnt set, skip it.
                if (server.Id == 0 || server.OwnerPublishedChannel == 0)
                {
                    continue;
                }

                var chat = await _discordService.GetMessageChannel(server.Id, server.OwnerPublishedChannel);

                if (chat == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(server.OwnerYouTubeChannelId))
                {
                    continue;
                }

                YouTubePlaylist playlist = null;

                try
                {
                    var details = await _youtubeManager.GetContentDetailsByChannelId(server.OwnerYouTubeChannelId);

                    if (details == null || details.items == null || details.items.Count < 1 || string.IsNullOrEmpty(details.items[0].contentDetails.relatedPlaylists.uploads))
                    {
                        continue;
                    }

                    playlist = await _youtubeManager.GetPlaylistItemsByPlaylistId(details.items[0].contentDetails.relatedPlaylists.uploads);

                    if (playlist == null || playlist.items == null || playlist.items.Count < 1)
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogError("YouTube Published Error: " + ex.Message + " for user: " + server.OwnerYouTubeChannelId + " in Discord Server: " + server.Id);
                    continue;
                }

                foreach (var video in playlist.items)
                {
                    var publishDate = DateTime.Parse(video.snippet.publishedAt, null, System.Globalization.DateTimeStyles.AdjustToUniversal);

                    if (!(publishDate > then && publishDate < now))
                    {
                        continue;
                    }

                    string url = "http://" + (server.UseYouTubeGamingPublished ? "gaming" : "www") + ".youtube.com/watch?v=" + video.snippet.resourceId.videoId;

                    EmbedBuilder embed = new EmbedBuilder();
                    EmbedAuthorBuilder author = new EmbedAuthorBuilder();
                    EmbedFooterBuilder footer = new EmbedFooterBuilder();

                    YouTubeChannelSnippet channelData = null;

                    try
                    {
                        channelData = await _youtubeManager.GetYouTubeChannelSnippetById(video.snippet.channelId);
                    }
                    catch (Exception wex)
                    {
                        // Log our error and move to the next user.

                        Logging.LogError("YouTube Error: " + wex.Message + " for user: " + video.snippet.channelId);
                        continue;
                    }

                    if (channelData == null)
                    {
                        continue;
                    }

                    if (server.PublishedMessage == null)
                    {
                        server.PublishedMessage = "%CHANNEL% just published a new video - %TITLE% - %URL%";
                    }

                    Color red = new Color(179, 18, 23);
                    author.IconUrl = _discord.CurrentUser.GetAvatarUrl() + "?_=" + Guid.NewGuid().ToString().Replace("-", "");
                    author.Name = _discord.CurrentUser.Username;
                    author.Url = url;
                    footer.Text = "[" + Constants.YouTube + "] - " + DateTime.UtcNow.AddHours(server.TimeZoneOffset);
                    footer.IconUrl = "http://couchbot.io/img/ytg.jpg";
                    embed.Author = author;
                    embed.Color = red;
                    embed.Description = server.PublishedMessage.Replace("%CHANNEL%", video.snippet.channelTitle).Replace("%GAME%", "a game").Replace("%TITLE%", video.snippet.title).Replace("%URL%", url);
                    embed.Title = video.snippet.channelTitle + " published a new video!";
                    embed.ThumbnailUrl = channelData.items.Count > 0 ? channelData.items[0].snippet.thumbnails.high.url + "?_=" + Guid.NewGuid().ToString().Replace("-", "") : "";
                    embed.ImageUrl = server.AllowThumbnails ? video.snippet.thumbnails.high.url + "?_=" + Guid.NewGuid().ToString().Replace("-", "") : "";
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

                        message += "**[" + Constants.YouTube + "]** " + server.PublishedMessage.Replace("%CHANNEL%", video.snippet.channelTitle).Replace("%TITLE%", video.snippet.title).Replace("%URL%", url);
                    }

                    Logging.LogYouTube(video.snippet.channelTitle + " has published a new video.");

                    await _messagingService.SendMessages(Constants.YouTube, new List<BroadcastMessage> {
                                new BroadcastMessage
                                {
                                    GuildId = server.Id,
                                    ChannelId = server.OwnerPublishedChannel,
                                    UserId = server.OwnerYouTubeChannelId,
                                    Message = message,
                                    Platform = Constants.YouTube,
                                    Embed = (!server.UseTextAnnouncements ? embed.Build() : null)
                                }
                            }
                    );
                }
            }
        }

        public List<string> GetYouTubeIdLists(List<YouTubeChannelServerModel> youTubeChannelList)
        {
            var allYouTubeIdsBuilder = new StringBuilder();
            var lists = new List<string>();

            foreach (var c in youTubeChannelList)
            {
                allYouTubeIdsBuilder.Append(c.YouTubeChannelId + ",");
            }

            var allYouTubeIds = allYouTubeIdsBuilder.ToString().TrimEnd(',');
            var splitList = allYouTubeIds.Split(',');

            var list = new StringBuilder();

            for (int i = 0; i < splitList.Length; i++)
            {
                list.Append(splitList[i] + ",");

                if (i % 100 == 0 && i != 0)
                {
                    var sublist = list.ToString().TrimEnd(',');

                    lists.Add(sublist);

                    list.Clear();
                }
            }

            if (!string.IsNullOrEmpty(list.ToString()))
            {
                var sublist = list.ToString().TrimEnd(',');

                lists.Add(sublist);
            }

            return lists;
        }

        #endregion

        #region : Vidme : 

        public async Task CheckVidMe()
        {
            var servers = _fileService.GetConfiguredServers();
            var now = DateTime.UtcNow;
            var then = now.AddMilliseconds(-(_botSettings.IntervalSettings.VidMe));

            foreach (var server in servers)
            {
                // If server isnt set or published channel isnt set, skip it.
                if (server.Id == 0 || server.PublishedChannel == 0)
                {
                    continue;
                }

                // If they dont allow published, skip it.
                if (!server.AllowPublished)
                {
                    continue;
                }

                var chat = await _discordService.GetMessageChannel(server.Id, server.PublishedChannel);

                if (chat == null)
                {
                    continue;
                }

                if (server.ServerVidMeChannelIds == null || server.ServerVidMeChannelIds.Count < 0)
                {
                    continue;
                }

                foreach (var channelId in server.ServerVidMeChannelIds)
                {
                    if (channelId == 0)
                    {
                        continue;
                    }

                    VidMeUserVideos videoResponse = null;

                    try
                    {
                        videoResponse = await _vidMeManager.GetChannelVideosById(channelId);

                        if (videoResponse == null || videoResponse.videos == null || videoResponse.videos.Count < 1)
                        {
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.LogError("VidMe Published Error: " + ex.Message + " for user: " + channelId + " in Discord Server: " + server.Id);
                        continue;
                    }

                    foreach (var video in videoResponse.videos)
                    {
                        if (string.IsNullOrEmpty(video.date_published))
                        {
                            continue;
                        }

                        DateTime tryParse;

                        if (DateTime.TryParse(video.date_published, out tryParse))
                        {
                            // Good
                        }
                        else
                        {
                            Logging.LogError("********* BAD DATETIME VIDME: " + video.date_published);
                            continue;
                        }

                        var publishDate = DateTime.Parse(video.date_published, null, System.Globalization.DateTimeStyles.AdjustToUniversal);

                        if (!(publishDate > then && publishDate < now))
                        {
                            continue;
                        }

                        string url = video.full_url;

                        EmbedBuilder embed = new EmbedBuilder();
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder();
                        EmbedFooterBuilder footer = new EmbedFooterBuilder();

                        if (server.PublishedMessage == null)
                        {
                            server.PublishedMessage = "%CHANNEL% just published a new video - %TITLE% - %URL%";
                        }

                        Color red = new Color(179, 18, 23);
                        author.IconUrl = _discord.CurrentUser.GetAvatarUrl() + "?_=" + Guid.NewGuid().ToString().Replace("-", "");
                        author.Name = _discord.CurrentUser.Username;
                        author.Url = url;
                        footer.Text = "[" + Constants.VidMe + "] - " + DateTime.UtcNow.AddHours(server.TimeZoneOffset);
                        footer.IconUrl = "http://couchbot.io/img/vidme.png";
                        embed.Author = author;
                        embed.Color = red;
                        embed.Description = server.PublishedMessage.Replace("%CHANNEL%", video.user.username).Replace("%GAME%", "a video").Replace("%TITLE%", video.title).Replace("%URL%", url);
                        embed.Title = video.user.username + " published a new video!";
                        embed.ThumbnailUrl = video.user.avatar_url;
                        embed.ImageUrl = video.thumbnail_url;
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

                            message += "**[" + Constants.VidMe + "]** " +
                                server.PublishedMessage.Replace("%CHANNEL%", video.user.username).Replace("%GAME%", "a video").Replace("%TITLE%", video.title).Replace("%URL%", url);
                        }

                        Logging.LogVidMe(video.user.username + " has published a new video.");

                        await _messagingService.SendMessages(Constants.VidMe, new List<BroadcastMessage> {
                                new BroadcastMessage
                                {
                                    GuildId = server.Id,
                                    ChannelId = server.PublishedChannel,
                                    UserId = video.user.username,
                                    Message = message,
                                    Platform = Constants.VidMe,
                                    Embed = (!server.UseTextAnnouncements ? embed.Build() : null)
                                }
                            }
                        );
                    }
                }
            }
        }

        public async Task CheckOwnerVidMe()
        {
            var servers = _fileService.GetConfiguredServers();
            var now = DateTime.UtcNow;
            var then = now.AddMilliseconds(-(_botSettings.IntervalSettings.VidMe));

            foreach (var server in servers)
            {
                if (!server.AllowPublished)
                {
                    continue;
                }

                // If server isnt set or published channel isnt set, skip it.
                if (server.Id == 0 || server.OwnerPublishedChannel == 0)
                {
                    continue;
                }

                var chat = await _discordService.GetMessageChannel(server.Id, server.OwnerPublishedChannel);

                if (chat == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(server.OwnerVidMeChannel) || server.OwnerVidMeChannelId == 0)
                {
                    continue;
                }

                var channelId = server.OwnerVidMeChannelId;
                VidMeUserVideos videoResponse = null;

                try
                {
                    videoResponse = await _vidMeManager.GetChannelVideosById(channelId);

                    if (videoResponse == null || videoResponse.videos == null || videoResponse.videos.Count < 1)
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogError("VidMe Published Error: " + ex.Message + " for user: " + channelId + " in Discord Server: " + server.Id);
                    continue;
                }

                foreach (var video in videoResponse.videos)
                {
                    if (string.IsNullOrEmpty(video.date_published))
                    {
                        continue;
                    }

                    DateTime tryParse;

                    if (DateTime.TryParse(video.date_published, out tryParse))
                    {
                        // Good
                    }
                    else
                    {
                        Logging.LogError("********* BAD DATETIME VIDME: " + video.date_published);
                        continue;
                    }

                    var publishDate = DateTime.Parse(video.date_published, null, System.Globalization.DateTimeStyles.AdjustToUniversal);

                    if (!(publishDate > then && publishDate < now))
                    {
                        continue;
                    }

                    string url = video.full_url;

                    EmbedBuilder embed = new EmbedBuilder();
                    EmbedAuthorBuilder author = new EmbedAuthorBuilder();
                    EmbedFooterBuilder footer = new EmbedFooterBuilder();

                    if (server.PublishedMessage == null)
                    {
                        server.PublishedMessage = "%CHANNEL% just published a new video - %TITLE% - %URL%";
                    }

                    Color red = new Color(179, 18, 23);
                    author.IconUrl = _discord.CurrentUser.GetAvatarUrl() + "?_=" + Guid.NewGuid().ToString().Replace("-", "");
                    author.Name = _discord.CurrentUser.Username;
                    author.Url = url;
                    footer.Text = "[" + Constants.VidMe + "] - " + DateTime.UtcNow.AddHours(server.TimeZoneOffset);
                    footer.IconUrl = "http://couchbot.io/img/vidme.png";
                    embed.Author = author;
                    embed.Color = red;
                    embed.Description = server.PublishedMessage.Replace("%CHANNEL%", video.user.username).Replace("%GAME%", "a video").Replace("%TITLE%", video.title).Replace("%URL%", url);
                    embed.Title = video.user.username + " published a new video!";
                    embed.ThumbnailUrl = video.user.avatar_url;
                    embed.ImageUrl = video.thumbnail_url;
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

                        message += "**[" + Constants.VidMe + "]** " +
                            server.PublishedMessage.Replace("%CHANNEL%", video.user.username).Replace("%GAME%", "a video").Replace("%TITLE%", video.title).Replace("%URL%", url);
                    }

                    Logging.LogVidMe(video.user.username + " has published a new video.");

                    await _messagingService.SendMessages(Constants.VidMe, new List<BroadcastMessage> {
                                new BroadcastMessage
                                {
                                    GuildId = server.Id,
                                    ChannelId = server.OwnerPublishedChannel,
                                    UserId = video.user.username,
                                    Message = message,
                                    Platform = Constants.VidMe,
                                    Embed = (!server.UseTextAnnouncements ? embed.Build() : null)
                                }
                            }
                        );
                }
            }
        }

        #endregion

        public async Task CleanUpLiveStreams(string platform)
        {
            if (platform == Constants.Twitch)
            {
                var liveStreams = new List<LiveChannel>();

                foreach (var live in Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.TwitchDirectory))
                {
                    var channel = JsonConvert.DeserializeObject<LiveChannel>(File.ReadAllText(live));
                    if (liveStreams.FirstOrDefault(x => x.Name == channel.Name) == null)
                    {
                        liveStreams.Add(channel);
                    }
                }

                foreach (var stream in liveStreams)
                {
                    try
                    {
                        var liveStream = await _twitchManager.GetStreamById(stream.Name);

                        if (liveStream == null || liveStream.stream == null)
                        {
                            await CleanupMessages(stream.ChannelMessages).ConfigureAwait(false);

                            File.Delete(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.TwitchDirectory + stream.Name + ".json");
                        }
                    }
                    catch (Exception wex)
                    {

                        Logging.LogError("Clean Up Twitch Error: " + wex.Message + " for user: " + stream.Name);
                    }
                }
            }

            if (platform == Constants.YouTubeGaming)
            {
                var liveStreams = new List<LiveChannel>();

                foreach (var live in Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.YouTubeDirectory))
                {
                    var channel = JsonConvert.DeserializeObject<LiveChannel>(File.ReadAllText(live));
                    if (liveStreams.FirstOrDefault(x => x.Name == channel.Name) == null)
                    {
                        liveStreams.Add(channel);
                    }
                }

                foreach (var stream in liveStreams)
                {
                    try
                    {
                        var youtubeStream = await _youtubeManager.GetLiveVideoByChannelId(stream.Name);

                        if (youtubeStream == null || youtubeStream.items.Count < 1)
                        {
                            var file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.YouTubeDirectory + stream.Name + ".json";

                            await CleanupMessages(stream.ChannelMessages).ConfigureAwait(false);

                            File.Delete(file);
                        }
                    }
                    catch (Exception wex)
                    {

                        Logging.LogError("Clean Up YouTube Error: " + wex.Message + " for user: " + stream.Name);
                    }
                }
            }

            if (platform == Constants.Smashcast)
            {
                var liveStreams = new List<LiveChannel>();

                foreach (var live in Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.SmashcastDirectory))
                {
                    var channel = JsonConvert.DeserializeObject<LiveChannel>(File.ReadAllText(live));
                    if (liveStreams.FirstOrDefault(x => x.Name == channel.Name) == null)
                    {
                        liveStreams.Add(channel);
                    }
                }

                foreach (var stream in liveStreams)
                {
                    try
                    {
                        var liveStream = await _smashcastManager.GetChannelByName(stream.Name);

                        if (liveStream == null || liveStream.livestream == null || liveStream.livestream.Count < 1 || liveStream.livestream[0].media_is_live == "0")
                        {
                            await CleanupMessages(stream.ChannelMessages).ConfigureAwait(false);

                            File.Delete(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.SmashcastDirectory + stream.Name + ".json");
                        }
                    }
                    catch (Exception wex)
                    {

                        Logging.LogError("Clean Up Smashcast Error: " + wex.Message + " for user: " + stream.Name);
                    }
                }
            }

            if (platform == Constants.Picarto)
            {
                var liveStreams = new List<LiveChannel>();

                foreach (var live in Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.PicartoDirectory))
                {
                    var channel = JsonConvert.DeserializeObject<LiveChannel>(File.ReadAllText(live));
                    if (liveStreams.FirstOrDefault(x => x.Name == channel.Name) == null)
                    {
                        liveStreams.Add(channel);
                    }
                }

                foreach (var stream in liveStreams)
                {
                    try
                    {
                        var liveStream = await _picartoManager.GetChannelByName(stream.Name);

                        if (liveStream == null || !liveStream.Online)
                        {
                            await CleanupMessages(stream.ChannelMessages).ConfigureAwait(false);

                            File.Delete(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.PicartoDirectory + stream.Name + ".json");
                        }
                    }
                    catch (Exception wex)
                    {

                        Logging.LogError("Clean Up Picarto Error: " + wex.Message + " for user: " + stream.Name);
                    }
                }
            }

            if (platform == Constants.Mobcrush)
            {
                var liveStreams = new List<LiveChannel>();

                foreach (var live in Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.MobcrushDirectory))
                {
                    var channel = JsonConvert.DeserializeObject<LiveChannel>(File.ReadAllText(live));
                    if (liveStreams.FirstOrDefault(x => x.Name == channel.Name) == null)
                    {
                        liveStreams.Add(channel);
                    }
                }

                foreach (var stream in liveStreams)
                {
                    try
                    {
                        var liveStream = await _mobcrushManager.GetMobcrushBroadcastByChannelId(stream.Name);

                        if (liveStream == null || !liveStream.IsLive)
                        {
                            await CleanupMessages(stream.ChannelMessages).ConfigureAwait(false);

                            File.Delete(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.MobcrushDirectory + stream.Name + ".json");
                        }
                    }
                    catch (Exception wex)
                    {

                        Logging.LogError("Clean Up Mobcrush Error: " + wex.Message + " for user: " + stream.Name);
                    }
                }
            }
        }
        private async Task CleanupMessages(List<ChannelMessage> channelMessages)
        {
            if (channelMessages != null && channelMessages.Count > 0)
            {
                foreach (var message in channelMessages)
                {
                    var serverFile = _fileService.GetConfiguredServers().FirstOrDefault(x => x.Id == message.GuildId);

                    if (serverFile == null)
                    {
                        continue;
                    }

                    if (serverFile.DeleteWhenOffline)
                    {
                        await _discordService.DeleteMessage(message.GuildId, message.ChannelId, message.MessageId);
                    }
                    else
                    {
                        await _discordService.SetOfflineStream(message.GuildId, serverFile.StreamOfflineMessage, message.ChannelId, message.MessageId);
                    }
                }
            }
        }
    }
}
