using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using MTD.CouchBot.Domain.Models.Bot;
using MTD.CouchBot.Domain.Utilities;
using MTD.CouchBot.Managers;
using MTD.CouchBot.Services;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MTD.CouchBot.Modules
{
    [Group("streamer")]
    public class Streamer : ModuleBase
    {
        private readonly IYouTubeManager _youtubeManager;
        private readonly IMixerManager _mixerManager;
        private readonly ITwitchManager _twitchManager;
        private readonly IVidMeManager _vidMeManager;
        private readonly IMobcrushManager _mobCrushManager;
        private readonly BotSettings _botSettings;
        private readonly FileService _fileService;

        public Streamer(IYouTubeManager youTubeManager, IMixerManager mixerManager, ITwitchManager twitchManager,
            IVidMeManager vidMeManager, IOptions<BotSettings> botSettings, FileService fileService, IMobcrushManager mobCrushManager)
        {
            _youtubeManager = youTubeManager;
            _vidMeManager = vidMeManager;
            _twitchManager = twitchManager;
            _mixerManager = mixerManager;
            _botSettings = botSettings.Value;
            _fileService = fileService;
            _mobCrushManager = mobCrushManager;
        }

        [Command("list"), Summary("List server streamers")]
        public async Task List()
        {
            var guild = ((IGuildUser)Context.Message.Author).Guild;
            var guildObject = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + guild.Id + ".json"));
            List<string> twitch = new List<string>();
            List<string> youtube = new List<string>();
            List<string> beam = new List<string>();
            List<string> hitbox = new List<string>();
            List<string> picarto = new List<string>();
            List<string> vidMe = new List<string>();
            List<string> mobCrush = new List<string>();

            if (guildObject.PicartoChannels != null && guildObject.PicartoChannels.Count > 0)
            {
                foreach (var streamer in guildObject.PicartoChannels)
                {
                    picarto.Add(streamer);
                }
            }

            if (guildObject.ServerTwitchChannelIds != null && guildObject.ServerTwitchChannelIds.Count > 0)
            {
                foreach (var streamer in guildObject.ServerTwitchChannelIds)
                {
                    var twitchChannel = await _twitchManager.GetTwitchChannelById(streamer);

                    if (twitchChannel != null)
                    {
                        var name = twitchChannel == null ? streamer + " (Streamer Not Found)" : twitchChannel.DisplayName;

                        twitch.Add(name);
                    }
                }
            }

            if (guildObject.ServerYouTubeChannelIds != null && guildObject.ServerYouTubeChannelIds.Count > 0)
            {
                foreach (var streamer in guildObject.ServerYouTubeChannelIds)
                {
                    var channel = await _youtubeManager.GetYouTubeChannelSnippetById(streamer);

                    youtube.Add((channel.items.Count > 0 ? channel.items[0].snippet.title + " (" + streamer + ")" : streamer));
                }
            }

            if (guildObject.ServerBeamChannels != null && guildObject.ServerBeamChannels.Count > 0)
            {
                foreach (var streamer in guildObject.ServerBeamChannels)
                {
                    beam.Add(streamer);
                }
            }

            if (guildObject.ServerHitboxChannels != null && guildObject.ServerHitboxChannels.Count > 0)
            {
                foreach (var streamer in guildObject.ServerHitboxChannels)
                {
                    hitbox.Add(streamer);
                }
            }

            if (guildObject.ServerVidMeChannels != null && guildObject.ServerVidMeChannels.Count > 0)
            {
                foreach (var streamer in guildObject.ServerVidMeChannels)
                {
                    vidMe.Add(streamer);
                }
            }

            if(guildObject.ServerMobcrushIds != null && guildObject.ServerMobcrushIds.Count >0)
            {
                foreach(var streamer in guildObject.ServerMobcrushIds)
                {
                    var channel = await _mobCrushManager.GetMobcrushChannelById(streamer);

                    if (channel != null)
                    {
                        mobCrush.Add(channel.name);
                    }
                }
            }

            var ownerYouTube = "Not Set";

            if(!string.IsNullOrEmpty(guildObject.OwnerYouTubeChannelId))
            {
                var channel = await _youtubeManager.GetYouTubeChannelSnippetById(guildObject.OwnerYouTubeChannelId);

                if(channel != null && channel.items.Count > 0)
                {
                    ownerYouTube = channel.items[0].snippet.title + " (" + guildObject.OwnerYouTubeChannelId + ")";
                }
            }

            var ownerVidMe = "Not Set";

            if (!string.IsNullOrEmpty(guildObject.OwnerVidMeChannel))
            {
                ownerVidMe = guildObject.OwnerVidMeChannel;
            }

            var ownerMobcrush = "Not Set";

            if(!string.IsNullOrEmpty(guildObject.OwnerMobcrushId))
            {
                var channel = await _mobCrushManager.GetMobcrushChannelById(guildObject.OwnerMobcrushId);

                if(channel != null)
                {
                    ownerMobcrush = channel.name;
                }
            }

            var beamString = string.Join(", ", beam);
            var picartoString = string.Join(", ", picarto);
            var hitboxString = string.Join(", ", hitbox);
            var twitchString = string.Join(", ", twitch);
            var vidMeString = string.Join(", ", vidMe);
            var youtubeString = string.Join(", ", youtube);
            var mobcrushString = string.Join(", ", mobCrush);

            var ownerBeam = (string.IsNullOrEmpty(guildObject.OwnerBeamChannel) ? "Not Set" : guildObject.OwnerBeamChannel);
            var ownerPicartor = (string.IsNullOrEmpty(guildObject.OwnerPicartoChannel) ? "Not Set" : guildObject.OwnerPicartoChannel);
            var ownerSmashcast = (string.IsNullOrEmpty(guildObject.OwnerHitboxChannel) ? "Not Set" : guildObject.OwnerHitboxChannel);
            var ownerTwitch = (string.IsNullOrEmpty(guildObject.OwnerTwitchChannel) ? "Not Set" : guildObject.OwnerTwitchChannel);

            string info = string.Format("```Markdown\r\n" +
              "# Server Configured Channels\r\n" +
              "- Mixer: {0}\r\n" +
              "- Mobcrush: {12}\r\n" +
              "- Picarto: {1}\r\n" +
              "- Smashcast: {2}\r\n" +
              "- Twitch: {3}\r\n" +
              "- VidMe: {4}\r\n" +
              "- YouTube: {5}\r\n" +
              "- Owner Mixer: {6}\r\n" +
              "- Owner Mobcrush: {13}\r\n" +
              "- Owner Picarto: {7}\r\n" +
              "- Owner Smashcast: {8}\r\n" +
              "- Owner Twitch: {9}\r\n" +
              "- Owner VidMe: {10}\r\n" +
              "- Owner YouTube: {11}\r\n" +
              "```\r\n", beamString, picartoString, hitboxString, twitchString, vidMeString, youtubeString,
              ownerBeam, ownerPicartor, ownerSmashcast, ownerTwitch, ownerVidMe, ownerYouTube, mobcrushString, ownerMobcrush);

            if (info.Length > 2000)
            {
                await Context.Channel.SendMessageAsync(StringUtilities.ScrubChatMessage(info.Substring(0, 1900) + "\r\n```"));
                await Context.Channel.SendMessageAsync(StringUtilities.ScrubChatMessage("```Markdown\r\n" + info.Substring(1901)));
            }
            else
            {
                await Context.Channel.SendMessageAsync(StringUtilities.ScrubChatMessage(info));
            }
        }

        [Command("live"), Summary("Display who is currently live in a server.")]
        public async Task Live()
        {
            var beam = _fileService.GetCurrentlyLiveBeamChannels();
            var hitbox = _fileService.GetCurrentlyLiveHitboxChannels();
            var twitch = _fileService.GetCurrentlyLiveTwitchChannels();
            var youtube = _fileService.GetCurrentlyLiveYouTubeChannels();
            var picarto = _fileService.GetCurrentlyLivePicartoChannels();


            var guildId = Context.Guild.Id;

            var beamLive = "";
            var hitboxLive = "";
            var twitchLive = "";
            var youtubeLive = "";
            var picartoLive = "";

            foreach(var b in beam)
            {
                foreach(var cm in b.ChannelMessages)
                {
                    if(cm.GuildId == guildId)
                    {
                        var channel = await _mixerManager.GetChannelById(b.Name);

                        if(channel != null && channel.online)
                        beamLive += channel.token + ", ";

                        break;
                    }
                }
            }

            foreach (var p in picarto)
            {
                foreach (var cm in p.ChannelMessages)
                {
                    if (cm.GuildId == guildId)
                    {
                        var channel = await _mixerManager.GetChannelById(p.Name);

                        if (channel != null && channel.online)
                            picartoLive += channel.token + ", ";

                        break;
                    }
                }
            }

            foreach (var h in hitbox)
            {
                foreach (var cm in h.ChannelMessages)
                {
                    if (cm.GuildId == guildId)
                    {
                        hitboxLive += h.Name + ", ";

                        break;
                    }
                }
            }

            foreach (var t in twitch)
            {
                foreach (var cm in t.ChannelMessages)
                {
                    if (cm.GuildId == guildId)
                    {
                        var channel = await _twitchManager.GetStreamById(t.Name);

                        if (channel != null && channel.stream != null)
                        {
                            twitchLive += channel.stream.channel.name + ", ";
                        }

                        break;
                    }
                }
            }

            foreach (var yt in youtube)
            {
                foreach (var cm in yt.ChannelMessages)
                {
                    if (cm.GuildId == guildId)
                    {
                        var channel = await _youtubeManager.GetLiveVideoByChannelId(yt.Name);

                        if (channel != null && channel.items != null && channel.items.Count > 0)
                        {
                            youtubeLive += channel.items[0].snippet.channelTitle + ", ";
                        }

                        break;
                    }
                }
            }

            beamLive = beamLive.Trim().TrimEnd(',');
            hitboxLive = hitboxLive.Trim().TrimEnd(',');
            twitchLive = twitchLive.Trim().TrimEnd(',');
            youtubeLive = youtubeLive.Trim().TrimEnd(',');
            picartoLive = picartoLive.Trim().TrimEnd(',');

            string info = "```Markdown\r\n" +
              "# Currently Live\r\n" +
              "- Mixer: " + beamLive + "\r\n" +
              "- Picarto: " + picartoLive + "\r\n" +
              "- Smashcast: " + hitboxLive + "\r\n" +
              "- Twitch: " + twitchLive + "\r\n" +
              "- YouTube Gaming: " + youtubeLive + "\r\n" +
              "```\r\n";

            await Context.Channel.SendMessageAsync(StringUtilities.ScrubChatMessage(info));
        }
    }
}
