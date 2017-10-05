using Microsoft.Extensions.Options;
using MTD.CouchBot.Domain;
using MTD.CouchBot.Domain.Models.Bot;
using MTD.CouchBot.Domain.Models.Mixer;
using MTD.CouchBot.Domain.Utilities;
using MTD.CouchBot.Managers;
using MTD.CouchBot.Models.Bot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MTD.CouchBot.Services
{
    public class MixerConstellationService
    {
        public ClientWebSocket client = new ClientWebSocket();
        private readonly IMixerManager _mixerManager;
        private readonly IStatisticsManager _statisticsManager;
        private readonly MessagingService _messagingService;
        private readonly DiscordService _discordService;
        private readonly BotSettings _botSettings;
        private readonly FileService _fileService;

        public MixerConstellationService(IMixerManager mixerManager, IStatisticsManager statisticsManager, MessagingService messagingService, 
            DiscordService discordService, IOptions<BotSettings> botSettings, FileService fileService)
        {
            _mixerManager = mixerManager;
            _statisticsManager = statisticsManager;
            _messagingService = messagingService;
            _discordService = discordService;
            _botSettings = botSettings.Value;
            _fileService = fileService;
        }

        public WebSocketState Status()
        {
            return client.State;
        }

        public async Task RunWebSockets()
        {
            if(client.State == WebSocketState.Aborted)
            {
                client.Dispose();
                client = new ClientWebSocket();
            }

            if (client.State == WebSocketState.None || 
                client.State == WebSocketState.Closed || 
                client.State == WebSocketState.Aborted)
            {
                try
                {
                    client.Options.SetRequestHeader("x-is-bot", "true");
                    await client.ConnectAsync(new Uri("wss://constellation.mixer.com"), CancellationToken.None);

                    var receiving = Receiving(client);
                }
                catch(Exception ex)
                {
                    Logging.LogError("Error in RunWebSockets " + ex.Message);
                    Logging.LogError("Error Details: Client State - " + client.State);
                }
            }
        }

        private async Task Receiving(ClientWebSocket client)
        {
            var buffer = new byte[1024 * 4];

            while (true)
            {
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var connectionStatus = client.State;
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var data = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    if (data.Replace(" ", "").ToLower().Contains("{\"online\":true}"))
                    {
                        var payload = JsonConvert.DeserializeObject<MixerPayload>(data);
                        var channelData = payload.data.channel.Split(':');
                        var channelId = channelData[1];
                        var channel = await _mixerManager.GetChannelById(channelId);

                        Logging.LogMixer(channel.token + " has gone online.");
                        await AnnounceLiveChannel(channelId);
                    }
                    else if (data.Replace(" ", "").ToLower().Contains("{\"online\":false}"))
                    {
                        var payload = JsonConvert.DeserializeObject<MixerPayload>(data);
                        var channelData = payload.data.channel.Split(':');
                        var channelId = channelData[1];
                        var channel = await _mixerManager.GetChannelById(channelId);

                        Logging.LogMixer(channel.token + " has gone offline.");
                        await StreamOffline(channelId);
                    }
                }

                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);

                    break;
                }
            }
        }

        public async Task SubscribeToLiveAnnouncements(string beamId)
        {
            //var channel = await beamManager.GetBeamChannelByName(channelName);           
            var subscribe = "{\"type\": \"method\", \"method\": \"livesubscribe\", \"params\": {\"events\": [\"channel:" + beamId + ":update\"]}, \"id\": " + beamId + "}";

            var bytes = Encoding.UTF8.GetBytes(subscribe);

            try
            {
                if (client.State == WebSocketState.Open)
                {
                    await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else
                {
                    await RunWebSockets();
                    await ResubscribeToBeamEvents();
                }
            }
            catch (Exception ex)
            {
                Logging.LogMixer("Exception in SubscribeToLiveAnnouncements: " + ex.Message);
            }
        }

        public async Task UnsubscribeFromLiveAnnouncements(string beamId)
        {
            var unsubscribe = "{\"type\": \"method\", \"method\": \"liveunsubscribe\", \"params\": {\"events\": [\"channel:" + beamId + ":update\"]}, \"id\": " + GetRandomInt() + "}";

            var bytes = Encoding.UTF8.GetBytes(unsubscribe);

            if (client.State == WebSocketState.Open)
            {
                await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            else
            {
                await RunWebSockets();
                await ResubscribeToBeamEvents();
            }
        }

        private int GetRandomInt()
        {
            Random r = new Random();
            return r.Next(1, 100000);
        }

        public async Task Ping()
        {
            var ping = "{\"id\":1,\"type\":\"method\",\"method\":\"ping\",\"params\":null}";

            var bytes = Encoding.UTF8.GetBytes(ping);

            try
            {
                if (client.State == WebSocketState.Open)
                {
                    await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else
                {
                    await RunWebSockets();
                    await ResubscribeToBeamEvents();
                }
            }
            catch (Exception ex)
            {
                Logging.LogMixer("Exception in Ping: " + ex.Message);
            }
        }

        public async Task AnnounceLiveChannel(string beamId)
        {
            var servers = _fileService.GetConfiguredServers();

            var beamServers = new List<DiscordServer>();
            var ownerBeamServers = new List<DiscordServer>();
            var userSharedServers = new List<DiscordServer>();

            foreach (var server in servers)
            {
                if (server.ServerBeamChannels != null && server.ServerBeamChannelIds != null)
                {
                    if (server.ServerBeamChannels.Count > 0 && server.ServerBeamChannelIds.Count > 0)
                    {
                        if (server.ServerBeamChannelIds.Contains(beamId))
                        {
                            if (server.GoLiveChannel != 0)
                            {
                                beamServers.Add(server);
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(server.OwnerBeamChannelId) && server.OwnerBeamChannelId.Equals(beamId))
                {
                    if (server.OwnerLiveChannel != 0)
                    {
                        ownerBeamServers.Add(server);
                    }
                }
            }

            List<BroadcastMessage> messages = new List<BroadcastMessage>();

            foreach (var server in beamServers)
            {
                // Check to see if we have a message already queued up. If so, jump to the next server.

                if (server.GoLiveChannel != 0 && server.Id != 0)
                {
                    if (messages.FirstOrDefault(x => x.GuildId == server.Id && x.UserId == beamId) == null)
                    {
                        var stream = await _mixerManager.GetChannelByName(beamId);
                        string gameName = stream.type == null ? "a game" : stream.type.name;
                        string url = "http://mixer.com/" + stream.token;
                        string avatarUrl = stream.user.avatarUrl != null ? stream.user.avatarUrl : "https://mixer.com/_latest/assets/images/main/avatars/default.jpg";
                        string thumbnailUrl = "https://thumbs.mixer.com/channel/" + stream.id + ".small.jpg";
                        string channelId = stream.id.Value.ToString();

                        messages.Add(await _messagingService.BuildMessage(stream.token, gameName, stream.name, url, avatarUrl, thumbnailUrl,
                            Constants.Mixer, channelId, server, server.GoLiveChannel, null));
                    }
                }
            }

            foreach (var server in ownerBeamServers)
            {
                if (server.OwnerLiveChannel != 0 && server.Id != 0)
                {
                    if (messages.FirstOrDefault(x => x.GuildId == server.Id && x.UserId == beamId) == null)
                    {
                        var stream = await _mixerManager.GetChannelByName(beamId);
                        string gameName = stream.type == null ? "a game" : stream.type.name;
                        string url = "http://mixer.com/" + stream.token;
                        string avatarUrl = stream.user.avatarUrl != null ? stream.user.avatarUrl : "https://mixer.com/_latest/assets/images/main/avatars/default.jpg";
                        string thumbnailUrl = "https://thumbs.mixer.com/channel/" + stream.id + ".small.jpg";
                        string channelId = stream.id.Value.ToString();

                        messages.Add(await _messagingService.BuildMessage(stream.token, gameName, stream.name, url, avatarUrl, thumbnailUrl,
                            Constants.Mixer, channelId, server, server.OwnerLiveChannel, null));
                    }
                }
            }

            if (messages.Count > 0)
            {

                var channel = new LiveChannel()
                {
                    Name = beamId,
                    Servers = new List<ulong>(),
                    ChannelMessages = await _messagingService.SendMessages(Constants.Mixer, messages)
                };

                File.WriteAllText(
                    _botSettings.DirectorySettings.ConfigRootDirectory +
                    _botSettings.DirectorySettings.LiveDirectory +
                    _botSettings.DirectorySettings.MixerDirectory +
                    beamId + ".json",
                    JsonConvert.SerializeObject(channel));
            }
        }

        public async Task StreamOffline(string beamId)
        {
            var stream = await _mixerManager.GetChannelByName(beamId);
            var live = _fileService.GetCurrentlyLiveBeamChannels().FirstOrDefault(x => x.Name == beamId);

            if (live == null)
            {
                return;
            }

            foreach (var message in live.ChannelMessages)
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
                    if (string.IsNullOrEmpty(serverFile.StreamOfflineMessage))
                    {
                        serverFile.StreamOfflineMessage = "This stream is now offline.";
                    }

                    await _discordService.SetOfflineStream(message.GuildId, serverFile.StreamOfflineMessage, message.ChannelId, message.MessageId);
                }

                _fileService.DeleteLiveBeamChannel(beamId);
            }
        }

        public async Task ResubscribeToBeamEvents()
        {
            var count = 0;
            var alreadyProcessed = new List<string>();

            Logging.LogMixer("Getting Server Files.");

            var servers = _fileService.GetConfiguredServers().Where(x => x.ServerBeamChannelIds != null && x.ServerBeamChannelIds.Count > 0);

            if (client.State != WebSocketState.Open)
            {
                await Task.Run(async () =>
                {
                    Logging.LogMixer("Connecting to Mixer Constellation.");

                    await RunWebSockets();

                    Logging.LogMixer("Connected to Mixer Constellation.");
                });
            }

            Logging.LogMixer("Initiating Subscription Loop.");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (servers != null && servers.Count() > 0)
            {
                foreach (var s in servers)
                {
                    foreach (var b in s.ServerBeamChannelIds)
                    {
                        if (!alreadyProcessed.Contains(b))
                        {
                            await SubscribeToLiveAnnouncements(b);
                            count++;
                            alreadyProcessed.Add(b);
                        }
                    }

                    if (!string.IsNullOrEmpty(s.OwnerBeamChannelId))
                    {
                        if (!alreadyProcessed.Contains(s.OwnerBeamChannelId))
                        {
                            await SubscribeToLiveAnnouncements(s.OwnerBeamChannelId);
                            count++;
                            alreadyProcessed.Add(s.OwnerBeamChannelId);
                        }
                    }
                }
            }

            sw.Stop();
            Logging.LogMixer("Subscription Loop Complete. Processed " + count + " channels in " + sw.ElapsedMilliseconds + " milliseconds.");
        }
    }
}
