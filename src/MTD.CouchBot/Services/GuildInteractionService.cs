using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using MTD.CouchBot.Domain.Models.Bot;
using MTD.CouchBot.Domain.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MTD.CouchBot.Services
{
    public class GuildInteractionService
    {
        private readonly DiscordShardedClient _discord;
        private readonly BotSettings _botSettings;
        private readonly FileService _fileService;

        public GuildInteractionService(DiscordShardedClient discord, IOptions<BotSettings> botSettings, FileService fileService)
        {
            _discord = discord;
            _botSettings = botSettings.Value;
            _fileService = fileService;
        }

        public void Init()
        {
            // Not needed. Initially ran to fix new data structure. Keeping just in case needed in future. FixGuilds();

            _discord.JoinedGuild += Client_JoinedGuild;
            _discord.LeftGuild += Client_LeftGuild;
            _discord.UserJoined += Client_UserJoined;
            _discord.UserLeft += Client_UserLeft;
        }

        public void FixGuilds()
        {
            var files = _fileService.GetConfiguredServerPaths();
            var badConfigurations = new List<DiscordServer>();

            int count = 1;
            foreach (var file in files)
            {
                Console.WriteLine("Processing " + count + " of " + files.Count);

                var path = Path.GetFileNameWithoutExtension(file);
                try
                {
                    var server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(file));

                    server.AllowMentionMixerLive = server.AllowEveryone;
                    server.AllowMentionMobcrushLive = server.AllowEveryone;
                    server.AllowMentionPicartoLive = server.AllowEveryone;
                    server.AllowMentionSmashcastLive = server.AllowEveryone;
                    server.AllowMentionTwitchLive = server.AllowEveryone;
                    server.AllowMentionYouTubeLive = server.AllowEveryone;
                    server.AllowMentionOwnerLive = server.AllowEveryone;
                    server.AllowMentionOwnerMixerLive = server.AllowEveryone;
                    server.AllowMentionOwnerMobcrushLive = server.AllowEveryone;
                    server.AllowMentionOwnerPicartoLive = server.AllowEveryone;
                    server.AllowMentionOwnerSmashcastLive = server.AllowEveryone;
                    server.AllowMentionOwnerTwitchLive = server.AllowEveryone;
                    server.AllowMentionOwnerYouTubeLive = server.AllowEveryone;
                    server.AllowMentionYouTubePublished = server.AllowEveryone;
                    server.AllowMentionVidmePublished = server.AllowEveryone;
                    server.AllowMentionOwnerYouTubePublished = server.AllowEveryone;
                    server.AllowMentionOwnerVidmePublished = server.AllowEveryone;

                    _fileService.SaveDiscordServer(server);
                }
                catch (Exception ex)
                {
                    Logging.LogError("Error in CheckGuildConfigurations: " + ex.Message);
                }
                count++;
            }
        }

        public async Task Client_JoinedGuild(IGuild arg)
        {
            await CreateGuild(arg);

            await SendJoinedEmbed(arg.Name, arg.Id, (await arg.GetOwnerAsync()).Username, (await arg.GetUsersAsync()).Count, arg.CreatedAt.DateTime, arg.IconUrl, _discord.Guilds.Count);
        }

        public async Task Client_LeftGuild(IGuild arg)
        {
            File.Delete(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + arg.Id + ".json");

            await SendLeftEmbed(arg.Name, arg.Id, (await arg.GetOwnerAsync()).Username, (await arg.GetUsersAsync()).Count, arg.CreatedAt.DateTime, arg.IconUrl, _discord.Guilds.Count);
        }

        private async Task Client_UserLeft(IGuildUser arg)
        {
            await UpdateGuildUsers(arg.Guild);

            var guild = new DiscordServer();
            var guildFile = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + arg.Guild.Id + ".json";

            if (File.Exists(guildFile))
            {
                var json = File.ReadAllText(guildFile);
                guild = JsonConvert.DeserializeObject<DiscordServer>(json);
            }

            if (guild != null)
            {
                if (guild.GreetingsChannel != 0 && guild.Goodbyes)
                {
                    var channel = (IMessageChannel)await arg.Guild.GetChannelAsync(guild.GreetingsChannel);

                    if (string.IsNullOrEmpty(guild.GoodbyeMessage))
                    {
                        guild.GoodbyeMessage = "Good bye, " + arg.Username + ", thanks for hanging out!";
                    }

                    guild.GoodbyeMessage = guild.GoodbyeMessage.Replace("%USER%", arg.Username).Replace("%NEWLINE%", "\r\n");

                    await channel.SendMessageAsync(guild.GoodbyeMessage);
                }
            }
        }

        private async Task Client_UserJoined(IGuildUser arg)
        {
            await UpdateGuildUsers(arg.Guild);

            var guild = new DiscordServer();
            var guildFile = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + arg.Guild.Id + ".json";

            if (File.Exists(guildFile))
            {
                var json = File.ReadAllText(guildFile);
                guild = JsonConvert.DeserializeObject<DiscordServer>(json);
            }

            if (guild != null)
            {
                if (guild.GreetingsChannel != 0 && guild.Greetings)
                {
                    var channel = (IMessageChannel)await arg.Guild.GetChannelAsync(guild.GreetingsChannel);

                    if (string.IsNullOrEmpty(guild.GreetingMessage))
                    {
                        guild.GreetingMessage = "Welcome to the server, " + arg.Mention;
                    }

                    guild.GreetingMessage = guild.GreetingMessage.Replace("%USER%", arg.Mention).Replace("%NEWLINE%", "\r\n");

                    await channel.SendMessageAsync(guild.GreetingMessage);
                }
            }
        }

        public async Task CreateGuild(IGuild arg)
        {
            var guild = new DiscordServer();
            var guildFile = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + arg.Id + ".json";

            if (File.Exists(guildFile))
            {
                var json = File.ReadAllText(guildFile);
                guild = JsonConvert.DeserializeObject<DiscordServer>(json);
            }

            if (guild.Users == null)
                guild.Users = new List<string>();

            foreach (var user in await arg.GetUsersAsync())
            {
                guild.Users.Add(user.Id.ToString());
            }

            var owner = await arg.GetUserAsync(arg.OwnerId);
            guild.Id = arg.Id;
            guild.OwnerId = arg.OwnerId;
            guild.OwnerName = owner.Username;
            guild.Name = arg.Name;
            guild.AllowEveryone = true;

            var guildJson = JsonConvert.SerializeObject(guild);
            File.WriteAllText(guildFile, guildJson);
        }

        public async Task UpdateGuildUsers(IGuild arg)
        {
            var guild = new DiscordServer();
            var guildFile = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + arg.Id + ".json";

            if (File.Exists(guildFile))
            {
                var json = File.ReadAllText(guildFile);
                guild = JsonConvert.DeserializeObject<DiscordServer>(json);
            }

            guild.Users = new List<string>();

            foreach (var user in await arg.GetUsersAsync())
            {
                guild.Users.Add(user.Id.ToString());
            }

            var guildJson = JsonConvert.SerializeObject(guild);
            File.WriteAllText(guildFile, guildJson);
        }

        public void CheckGuildConfigurations()
        {
            var files = _fileService.GetConfiguredServerPaths();
            var badConfigurations = new List<DiscordServer>();

            foreach (var file in files)
            {
                var path = Path.GetFileNameWithoutExtension(file);
                try
                {
                    var server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(file));

                    if (server.Id != ulong.Parse(path))
                    {
                        Logging.LogInfo("Bad Configuration Found: " + path);

                        var guild = _discord.GetGuild(ulong.Parse(path));

                        if (guild == null)
                        {
                            continue;
                        }

                        var guildOwner = _discord.GetUser(guild.OwnerId);

                        server.Id = guild.Id;
                        server.Name = guild.Name;
                        server.OwnerId = guild.OwnerId;
                        server.OwnerName = guildOwner == null ? "" : guildOwner.Username;

                        _fileService.SaveDiscordServer(server);

                        Logging.LogInfo("Server Configuration Fixed: " + path);
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogError("Error in CheckGuildConfigurations: " + ex.Message);
                }
            }
        }

        public async Task SendJoinedEmbed(string serverName, ulong serverId, string ownerName, int userCount, DateTime createdDate, string iconUrl, int serverCount)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.Title = "Log: Joined a New Server";
            builder.Description = "Successfully joined a new server. Total Servers: " + serverCount;
            builder.ThumbnailUrl = iconUrl;
            
            builder.AddField("Name", serverName, true);
            builder.AddField("ID", serverId, true);
            builder.AddField("Owner", ownerName, true);
            builder.AddField("Members", userCount, true);
            builder.AddField("Created", createdDate, true);

            EmbedFooterBuilder fBuilder = new EmbedFooterBuilder();
            fBuilder.IconUrl = _discord.CurrentUser.GetAvatarUrl();
            fBuilder.Text = "Server Joined | " + DateTime.UtcNow;

            builder.Footer = fBuilder;

            var guild = _discord.GetGuild(263688866978988032);
            var channel = (IMessageChannel) guild.GetChannel(359004669110124544);
            await channel.SendMessageAsync("", false, builder.Build());
        }

        public async Task SendLeftEmbed(string serverName, ulong serverId, string ownerName, int userCount, DateTime createdDate, string iconUrl, int serverCount)
        {
            EmbedBuilder builder = new EmbedBuilder();
            builder.Title = "Log: Left a Server";
            builder.Description = "Successfully left a server. Total Servers: " + serverCount;
            builder.ThumbnailUrl = iconUrl;

            builder.AddField("Name", serverName, true);
            builder.AddField("ID", serverId, true);
            builder.AddField("Owner", ownerName, true);
            builder.AddField("Members", userCount, true);
            builder.AddField("Created", createdDate, true);

            EmbedFooterBuilder fBuilder = new EmbedFooterBuilder();
            fBuilder.IconUrl = _discord.CurrentUser.GetAvatarUrl();
            fBuilder.Text = "Server Left | " + DateTime.UtcNow;

            builder.Footer = fBuilder;

            var guild = _discord.GetGuild(263688866978988032);
            var channel = (IMessageChannel)guild.GetChannel(359004669110124544);
            await channel.SendMessageAsync("", false, builder.Build());
        }
    }
}
