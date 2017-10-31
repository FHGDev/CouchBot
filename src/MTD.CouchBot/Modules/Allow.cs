using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using MTD.CouchBot.Domain.Models.Bot;
using MTD.CouchBot.Services;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace MTD.CouchBot.Modules
{
    [Group("allow"), Summary("Subset of Commands to configure server settings.")]
    public class Allow : ModuleBase
    {
        private readonly BotSettings _botSettings;
        private readonly FileService _fileService;

        public Allow(IOptions<BotSettings> botSettings, FileService fileService)
        {
            _botSettings = botSettings.Value;
            _fileService = fileService;
        }

        [Command("mention"), Summary("Sets use of a mention tag.")]
        public async Task Mention(string trueFalse)
        {
            var guild = ((IGuildUser)Context.Message.Author).Guild;

            var user = ((IGuildUser)Context.Message.Author);

            if (!user.GuildPermissions.ManageGuild)
            {
                return;
            }

            trueFalse = trueFalse.ToLower();
            if (!trueFalse.Equals("true") && !trueFalse.Equals("false"))
            {
                await Context.Channel.SendMessageAsync("Pass true or false when configuring AllowEveryone. (ie: !cb config AllowEveryone true)");
                return;
            }

            var file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + guild.Id + ".json";
            var server = new DiscordServer();

            if (File.Exists(file))
            {
                server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(file));
            }

            server.AllowEveryone = bool.Parse(trueFalse);

            server.AllowMentionMixerLive = bool.Parse(trueFalse);
            server.AllowMentionMobcrushLive = bool.Parse(trueFalse);
            server.AllowMentionPicartoLive = bool.Parse(trueFalse);
            server.AllowMentionSmashcastLive = bool.Parse(trueFalse);
            server.AllowMentionTwitchLive = bool.Parse(trueFalse);
            server.AllowMentionYouTubeLive = bool.Parse(trueFalse);
            server.AllowMentionOwnerLive = bool.Parse(trueFalse);
            server.AllowMentionOwnerMixerLive = bool.Parse(trueFalse);
            server.AllowMentionOwnerMobcrushLive = bool.Parse(trueFalse);
            server.AllowMentionOwnerPicartoLive = bool.Parse(trueFalse);
            server.AllowMentionOwnerSmashcastLive = bool.Parse(trueFalse);
            server.AllowMentionOwnerTwitchLive = bool.Parse(trueFalse);
            server.AllowMentionOwnerYouTubeLive = bool.Parse(trueFalse);
            server.AllowMentionYouTubePublished = bool.Parse(trueFalse);
            server.AllowMentionVidmePublished = bool.Parse(trueFalse);
            server.AllowMentionOwnerYouTubePublished = bool.Parse(trueFalse);
            server.AllowMentionOwnerVidmePublished = bool.Parse(trueFalse);

            await _fileService.SaveDiscordServer(server, Context.Guild);
            await Context.Channel.SendMessageAsync("Allow everyone has been set to: " + trueFalse);
        }

        [Command("mention"), Summary("Sets use of a mention tag.")]
        public async Task Mention(string platform, string type, string trueFalse)
        {
            var guild = ((IGuildUser)Context.Message.Author).Guild;
            var user = ((IGuildUser)Context.Message.Author);

            if (!user.GuildPermissions.ManageGuild)
            {
                return;
            }

            var platformLower = platform.ToLower();
            var typeLower = type.ToLower();
            var trueFalseLower = trueFalse.ToLower();

            if (!platformLower.Equals("mixer") && !platformLower.Equals("mobcrush") && !platformLower.Equals("picarto")
                && !platformLower.Equals("twitch") && !platformLower.Equals("youtube") && !platformLower.Equals("vidme")
                 && !platformLower.Equals("smashcast"))
            {
                await Context.Channel.SendMessageAsync("Invalid platform. Provide one of the following: mixer, mobcrush, picarto, smashcast, twitch, youtube, or vidme.");

                return;
            }

            if (!typeLower.Equals("live") && !typeLower.Equals("published") && !typeLower.Equals("ownerlive") && !typeLower.Equals("ownerpublished"))
            {
                await Context.Channel.SendMessageAsync("Invalid type. Provide one of the following: live, published, ownerlive, or ownerpublished.");

                return;
            }

            trueFalse = trueFalse.ToLower();
            if (!trueFalse.Equals("true") && !trueFalse.Equals("false"))
            {
                await Context.Channel.SendMessageAsync("Pass true or false when configuring AllowEveryone. (ie: !cb config AllowEveryone true)");
                return;
            }

            var file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + guild.Id + ".json";
            var server = new DiscordServer();

            if (File.Exists(file))
            {
                server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(file));
            }

            switch(platformLower)
            {
                case "mixer":
                    if(typeLower.Equals("ownerlive"))
                    {
                        server.AllowMentionOwnerMixerLive = bool.Parse(trueFalse);
                    }
                    else if (typeLower.Equals("live"))
                    {
                        server.AllowMentionMixerLive = bool.Parse(trueFalse);
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync("The only valid type for this platform is live or ownerlive. Try again.");
                        return;
                    }
                    break;
                case "mobcrush":
                    if (typeLower.Equals("ownerlive"))
                    {
                        server.AllowMentionOwnerMobcrushLive = bool.Parse(trueFalse);
                    }
                    else if (typeLower.Equals("live"))
                    {
                        server.AllowMentionMobcrushLive = bool.Parse(trueFalse);
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync("The only valid type for this platform is live or ownerlive. Try again.");
                        return;
                    }
                    break;
                case "picarto":
                    if (typeLower.Equals("ownerlive"))
                    {
                        server.AllowMentionOwnerPicartoLive = bool.Parse(trueFalse);
                    }
                    else if (typeLower.Equals("live"))
                    {
                        server.AllowMentionPicartoLive = bool.Parse(trueFalse);
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync("The only valid type for this platform is live or ownerlive. Try again.");
                        return;
                    }
                    break;
                case "smashcast":
                    if (typeLower.Equals("ownerlive"))
                    {
                        server.AllowMentionOwnerSmashcastLive = bool.Parse(trueFalse);
                    }
                    else if (typeLower.Equals("live"))
                    {
                        server.AllowMentionSmashcastLive = bool.Parse(trueFalse);
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync("The only valid type for this platform is live or ownerlive. Try again.");
                        return;
                    }
                    break;
                case "twitch":
                    if (typeLower.Equals("ownerlive"))
                    {
                        server.AllowMentionOwnerTwitchLive = bool.Parse(trueFalse);
                    }
                    else if(typeLower.Equals("live"))
                    {
                        server.AllowMentionTwitchLive = bool.Parse(trueFalse);
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync("The only valid type for this platform is live or ownerlive. Try again.");
                        return;
                    }
                    break;
                case "vidme":
                    if (typeLower.Equals("ownerpublished"))
                    {
                        server.AllowMentionOwnerVidmePublished = bool.Parse(trueFalse);
                    }
                    else if (typeLower.Equals("published"))
                    {
                        server.AllowMentionVidmePublished = bool.Parse(trueFalse);
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync("The only valid type for this platform is published or ownerpublished. Try again.");
                        return;
                    }
                    break;
                case "youtube":
                    if (typeLower.Equals("ownerlive"))
                    {
                        server.AllowMentionOwnerYouTubeLive = bool.Parse(trueFalse);
                    }
                    else if (typeLower.Equals("live"))
                    {
                        server.AllowMentionYouTubeLive = bool.Parse(trueFalse);
                    }
                    else if (typeLower.Equals("ownerpublished"))
                    {
                        server.AllowMentionOwnerYouTubePublished = bool.Parse(trueFalse);
                    }
                    else if (typeLower.Equals("published"))
                    {
                        server.AllowMentionYouTubePublished = bool.Parse(trueFalse);
                    }
                    break;
            }

            await _fileService.SaveDiscordServer(server, Context.Guild);
            await Context.Channel.SendMessageAsync("Allow mention on '" + platform + " - " + type + "' has been set to: " + trueFalse);
        }

        [Command("thumbnails"), Summary("Sets use of thumbnails.")]
        public async Task Thumbnails(string trueFalse)
        {
            var guild = ((IGuildUser)Context.Message.Author).Guild;
            var user = ((IGuildUser)Context.Message.Author);

            if (!user.GuildPermissions.ManageGuild)
            {
                return;
            }

            trueFalse = trueFalse.ToLower();
            if (!trueFalse.Equals("true") && !trueFalse.Equals("false"))
            {
                await Context.Channel.SendMessageAsync("Pass true or false when configuring AllowThumbnails. (ie: !cb config AllowThumbnails true)");
                return;
            }

            var file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + guild.Id + ".json";
            var server = new DiscordServer();

            if (File.Exists(file))
            {
                server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(file));
            }

            server.AllowThumbnails = bool.Parse(trueFalse);
            await _fileService.SaveDiscordServer(server, Context.Guild);
            await Context.Channel.SendMessageAsync("Allow thumbnails has been set to: " + trueFalse);
        }

        [Command("live"), Summary("Sets announcing of published content.")]
        public async Task Live(string trueFalse)
        {
            var guild = ((IGuildUser)Context.Message.Author).Guild;
            var user = ((IGuildUser)Context.Message.Author);

            if (!user.GuildPermissions.ManageGuild)
            {
                return;
            }

            trueFalse = trueFalse.ToLower();
            if (!trueFalse.Equals("true") && !trueFalse.Equals("false"))
            {
                await Context.Channel.SendMessageAsync("Pass true or false when configuring allow live. (ie: !cb allow live true)");
                return;
            }

            var file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + guild.Id + ".json";
            var server = new DiscordServer();

            if (File.Exists(file))
            {
                server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(file));
            }

            server.AllowLive = bool.Parse(trueFalse);
            await _fileService.SaveDiscordServer(server, Context.Guild);
            await Context.Channel.SendMessageAsync("Allow live has been set to: " + trueFalse);
        }

        [Command("published"), Summary("Sets announcing of published content.")]
        public async Task Published(string trueFalse)
        {
            var guild = ((IGuildUser)Context.Message.Author).Guild;
            var user = ((IGuildUser)Context.Message.Author);

            if (!user.GuildPermissions.ManageGuild)
            {
                return;
            }

            trueFalse = trueFalse.ToLower();
            if (!trueFalse.Equals("true") && !trueFalse.Equals("false"))
            {
                await Context.Channel.SendMessageAsync("Pass true or false when configuring allow published. (ie: !cb allow published true)");
                return;
            }

            var file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + guild.Id + ".json";
            var server = new DiscordServer();

            if (File.Exists(file))
            {
                server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(file));
            }

            server.AllowPublished = bool.Parse(trueFalse);
            await _fileService.SaveDiscordServer(server, Context.Guild);
            await Context.Channel.SendMessageAsync("Allow published has been set to: " + trueFalse);
        }

        [Command("goals"), Summary("Sets broadcasting of sub goals being met.")]
        public async Task Goals(string trueFalse)
        {
            var guild = ((IGuildUser)Context.Message.Author).Guild;
            var user = ((IGuildUser)Context.Message.Author);

            if (!user.GuildPermissions.ManageGuild)
            {
                return;
            }

            trueFalse = trueFalse.ToLower();
            if (!trueFalse.Equals("true") && !trueFalse.Equals("false"))
            {
                await Context.Channel.SendMessageAsync("Pass true or false when configuring BroadcastSubGoals. (ie: !cb config BroadcastSubGoals true)");
                return;
            }

            var file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + guild.Id + ".json";
            var server = new DiscordServer();

            if (File.Exists(file))
            {
                server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(file));
            }

            server.BroadcastSubGoals = bool.Parse(trueFalse);
            await _fileService.SaveDiscordServer(server, Context.Guild);
            await Context.Channel.SendMessageAsync("Allow sub goals has been set to: " + trueFalse);
        }

        [Command("channelfeed"), Summary("Sets announcing of channel feed.")]
        public async Task ChannelFeed(string trueFalse)
        {
            var guild = ((IGuildUser)Context.Message.Author).Guild;
            var user = ((IGuildUser)Context.Message.Author);

            if (!user.GuildPermissions.ManageGuild)
            {
                return;
            }

            trueFalse = trueFalse.ToLower();
            if (!trueFalse.Equals("true") && !trueFalse.Equals("false"))
            {
                await Context.Channel.SendMessageAsync("Pass true or false when configuring allow channel feed. (ie: !cb allow channelfeed true)");
                return;
            }

            var file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + guild.Id + ".json";
            var server = new DiscordServer();

            if (File.Exists(file))
            {
                server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(file));
            }

            server.AllowChannelFeed = bool.Parse(trueFalse);
            await _fileService.SaveDiscordServer(server, Context.Guild);
            await Context.Channel.SendMessageAsync("Allow channel feed has been set to: " + trueFalse);
        }

        [Command("ownerchannelfeed"), Summary("Sets announcing of owner channel feed.")]
        public async Task ChannelFeedOwner(string trueFalse)
        {
            var guild = ((IGuildUser)Context.Message.Author).Guild;
            var user = ((IGuildUser)Context.Message.Author);

            if (!user.GuildPermissions.ManageGuild)
            {
                return;
            }

            trueFalse = trueFalse.ToLower();
            if (!trueFalse.Equals("true") && !trueFalse.Equals("false"))
            {
                await Context.Channel.SendMessageAsync("Pass true or false when configuring allow owner channel feed. (ie: !cb allow ownerchannelfeed true)");
                return;
            }

            var file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + guild.Id + ".json";
            var server = new DiscordServer();

            if (File.Exists(file))
            {
                server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(file));
            }

            server.AllowOwnerChannelFeed = bool.Parse(trueFalse);
            await _fileService.SaveDiscordServer(server, Context.Guild);
            await Context.Channel.SendMessageAsync("Allow owner channel feed has been set to: " + trueFalse);
        }

        [Command("all")]
        public async Task All(string trueFalse)
        {
            var guild = ((IGuildUser)Context.Message.Author).Guild;
            var user = ((IGuildUser)Context.Message.Author);

            if (!user.GuildPermissions.ManageGuild)
            {
                return;
            }

            trueFalse = trueFalse.ToLower();
            if (!trueFalse.Equals("true") && !trueFalse.Equals("false"))
            {
                await Context.Channel.SendMessageAsync("Pass true or false when configuring allow all. (ie: !cb allow all true)");
                return;
            }

            var file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + guild.Id + ".json";
            var server = new DiscordServer();

            if (File.Exists(file))
            {
                server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(file));
            }

            server.AllowLive = bool.Parse(trueFalse);
            server.AllowPublished = bool.Parse(trueFalse);

            await _fileService.SaveDiscordServer(server, Context.Guild);
            await Context.Channel.SendMessageAsync("Allow all (Live and Published) has been set to: " + trueFalse);
        }
    }
}
