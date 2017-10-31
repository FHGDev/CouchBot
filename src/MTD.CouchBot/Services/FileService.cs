using Discord;
using Microsoft.Extensions.Options;
using MTD.CouchBot.Domain.Models.Bot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MTD.CouchBot.Services
{
    public class FileService
    {
        private readonly BotSettings _botSettings;

        public FileService(IOptions<BotSettings> botSettings)
        {
            _botSettings = botSettings.Value;
        }

        public void SaveDiscordServer(DiscordServer server)
        {
            var file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + server.Id + ".json";
            File.WriteAllText(file, JsonConvert.SerializeObject(server));
        }

        public async Task SaveDiscordServer(DiscordServer server, IGuild guild)
        {
            var guildOwner = await guild.GetOwnerAsync();
            server.Id = guild.Id;
            server.Name = guild.Name;
            server.OwnerId = guild.OwnerId;
            server.OwnerName = guildOwner == null ? "" : guildOwner.Username;

            var file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + server.Id + ".json";
            File.WriteAllText(file, JsonConvert.SerializeObject(server));
        }

        public List<string> GetConfiguredServerFileNames()
        {
            var servers = new List<string>();

            // Get Servers
            foreach (var server in Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory))
            {
                servers.Add(Path.GetFileName(server.Replace(".json", "")));
            }

            return servers;
        }

        public List<DiscordServer> GetConfiguredServers()
        {
            var servers = new List<DiscordServer>();

            // Get Servers
            foreach (var server in Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory))
            {
                try
                {
                    servers.Add(JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(server)));
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return servers;
        }

        public List<string> GetConfiguredServerPaths()
        {
            var servers = new List<string>();

            // Get Servers
            foreach (var server in Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory))
            {
                servers.Add(server);
            }

            return servers;
        }

        public List<DiscordServer> GetConfiguredServersWithLiveChannel()
        {
            var servers = new List<DiscordServer>();

            // Get Servers
            foreach (var s in Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory))
            {
                try
                {
                    var server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(s));

                    if (!server.AllowLive)
                    {
                        continue;
                    }

                    if ((server.Id == 0 || server.GoLiveChannel == 0) && (server.Id == 0 || server.OwnerLiveChannel == 0))
                    {
                        continue;
                    }

                    servers.Add(server);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return servers;
        }

        public List<DiscordServer> GetConfiguredServersWithOwnerLiveChannel()
        {
            var servers = new List<DiscordServer>();

            // Get Servers
            foreach (var s in Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory))
            {
                try
                {
                    var server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(s));

                    if (!server.AllowLive)
                    {
                        continue;
                    }

                    if (server.Id == 0 || server.OwnerLiveChannel == 0)
                    {
                        continue;
                    }

                    servers.Add(server);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return servers;
        }

        public List<DiscordServer> GetServersWithLiveChannelAndAllowDiscover()
        {
            var servers = new List<DiscordServer>();

            // Get Servers
            foreach (var s in Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory))
            {
                try
                {
                    var server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(s));

                    if (string.IsNullOrEmpty(server.DiscoverTwitch))
                    {
                        continue;
                    }

                    if (!server.AllowLive && !(server.DiscoverTwitch.Equals("all") || server.DiscoverTwitch.Equals("role")))
                    {
                        continue;
                    }

                    if (server.Id == 0 || server.GoLiveChannel == 0)
                    {
                        continue;
                    }

                    servers.Add(server);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return servers;
        }

        public DiscordServer GetConfiguredServerById(ulong id)
        {
            try
            {
                return JsonConvert.DeserializeObject<DiscordServer>(
                    File.ReadAllText(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + id + ".json"));
            }
            catch (Exception)
            {
                // Error on finding file.
                return null;
            }
        }

        public List<User> GetConfiguredUsers()
        {
            var users = new List<User>();

            // Get Users
            foreach (var user in Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.UserDirectory))
            {
                try
                {
                    users.Add(JsonConvert.DeserializeObject<User>(File.ReadAllText(user)));
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return users;
        }

        public List<LiveChannel> GetCurrentlyLiveBeamChannels()
        {
            var liveChannels = new List<LiveChannel>();

            // Get Live Channels
            foreach (var live in Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.MixerDirectory))
            {
                try
                {
                    liveChannels.Add(JsonConvert.DeserializeObject<LiveChannel>(File.ReadAllText(live)));
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return liveChannels;
        }

        public List<LiveChannel> GetCurrentlyLiveYouTubeChannels()
        {
            var liveChannels = new List<LiveChannel>();

            // Get Live Channels
            foreach (var live in Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.YouTubeDirectory))
            {
                try
                {
                    liveChannels.Add(JsonConvert.DeserializeObject<LiveChannel>(File.ReadAllText(live)));
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return liveChannels;
        }

        public List<LiveChannel> GetCurrentlyLiveTwitchChannels()
        {
            var liveChannels = new List<LiveChannel>();

            // Get Live Channels
            foreach (var live in Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.TwitchDirectory))
            {
                try
                {
                    liveChannels.Add(JsonConvert.DeserializeObject<LiveChannel>(File.ReadAllText(live)));
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return liveChannels;
        }

        public List<LiveChannel> GetCurrentlyLiveHitboxChannels()
        {
            var liveChannels = new List<LiveChannel>();

            // Get Live Channels
            foreach (var live in Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.SmashcastDirectory))
            {
                try
                {
                    liveChannels.Add(JsonConvert.DeserializeObject<LiveChannel>(File.ReadAllText(live)));
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return liveChannels;
        }

        public List<LiveChannel> GetCurrentlyLivePicartoChannels()
        {
            var liveChannels = new List<LiveChannel>();

            // Get Live Channels
            foreach (var live in Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.PicartoDirectory))
            {
                try
                {
                    liveChannels.Add(JsonConvert.DeserializeObject<LiveChannel>(File.ReadAllText(live)));
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return liveChannels;
        }

        public List<LiveChannel> GetCurrentlyLiveMobcrushChannels()
        {
            var liveChannels = new List<LiveChannel>();

            // Get Live Channels
            foreach (var live in Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.MobcrushDirectory))
            {
                try
                {
                    liveChannels.Add(JsonConvert.DeserializeObject<LiveChannel>(File.ReadAllText(live)));
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return liveChannels;
        }

        public void DeleteLiveBeamChannel(string beamId)
        {
            File.Delete(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.LiveDirectory + _botSettings.DirectorySettings.MixerDirectory + beamId + ".json");
        }

        public List<DiscordServer> GetServersWithNoChannelsSet()
        {
            var servers = new List<DiscordServer>();

            // Get Servers
            foreach (var s in Directory.GetFiles(_botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory))
            {
                try
                {
                    var server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(s));

                    if(server.OwnerLiveChannel == 0 &&
                        server.OwnerPublishedChannel == 0 &&
                        server.GoLiveChannel == 0 &&
                        server.PublishedChannel == 0)
                    {
                        servers.Add(server);
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            return servers;
        }
    }
}
