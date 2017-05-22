﻿using Discord;
using MTD.CouchBot.Domain;
using MTD.CouchBot.Domain.Models;
using MTD.CouchBot.Domain.Utilities;
using MTD.CouchBot.Json;
using MTD.CouchBot.Managers;
using MTD.CouchBot.Managers.Implementations;
using MTD.CouchBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MTD.CouchBot.Bot
{
    public static class BeamHelper
    {
        public static async Task AnnounceLiveChannel(string beamId)
        {
            IBeamManager beamManager = new BeamManager();
            var servers = BotFiles.GetConfiguredServers();

            var beamServers = new List<DiscordServer>();
            var ownerBeamServers = new List<DiscordServer>();
            var userSharedServers = new List<DiscordServer>();

            foreach(var server in servers)
            {
                if(server.ServerBeamChannels != null && server.ServerBeamChannelIds != null)
                {
                    if(server.ServerBeamChannels.Count > 0 && server.ServerBeamChannelIds.Count > 0)
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

                if(!string.IsNullOrEmpty(server.OwnerBeamChannelId) && server.OwnerBeamChannelId.Equals(beamId))
                {
                    if(server.OwnerLiveChannel != 0)
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
                        var stream = await beamManager.GetBeamChannelByName(beamId);
                        string gameName = stream.type == null ? "a game" : stream.type.name;
                        string url = "http://beam.pro/" + stream.token;
                        string avatarUrl = stream.user.avatarUrl != null ? stream.user.avatarUrl : "https://beam.pro/_latest/assets/images/main/avatars/default.jpg";
                        string thumbnailUrl = "https://thumbs.beam.pro/channel/" + stream.id + ".small.jpg";
                        string channelId = stream.id.Value.ToString();

                        messages.Add(await MessagingHelper.BuildMessage(stream.token, gameName, stream.name, url, avatarUrl, thumbnailUrl,
                            Constants.Beam, channelId, server, server.GoLiveChannel));
                    }
                }
            }

            foreach(var server in ownerBeamServers)
            {
                if (server.OwnerLiveChannel != 0 && server.Id != 0)
                {
                    if (messages.FirstOrDefault(x => x.GuildId == server.Id && x.UserId == beamId) == null)
                    {
                        var stream = await beamManager.GetBeamChannelByName(beamId);
                        string gameName = stream.type == null ? "a game" : stream.type.name;
                        string url = "http://beam.pro/" + stream.token;
                        string avatarUrl = stream.user.avatarUrl != null ? stream.user.avatarUrl : "https://beam.pro/_latest/assets/images/main/avatars/default.jpg";
                        string thumbnailUrl = "https://thumbs.beam.pro/channel/" + stream.id + ".small.jpg";
                        string channelId = stream.id.Value.ToString();

                        messages.Add(await MessagingHelper.BuildMessage(stream.token, gameName, stream.name, url, avatarUrl, thumbnailUrl,
                            Constants.Beam, channelId, server, server.OwnerLiveChannel));
                    }
                }
            }

            if (messages.Count > 0)
            {

                var channel = new LiveChannel()
                {
                    Name = beamId,
                    Servers = new List<ulong>(),
                    ChannelMessages = await MessagingHelper.SendMessages(Constants.Beam, messages)
                };

                File.WriteAllText(
                    Constants.ConfigRootDirectory +
                    Constants.LiveDirectory +
                    Constants.BeamDirectory +
                    beamId + ".json",
                    JsonConvert.SerializeObject(channel));
            }
        }      

        public static async Task StreamOffline(string beamId)
        {
            IBeamManager beamManager = new BeamManager();
            var stream = await beamManager.GetBeamChannelByName(beamId);
            var live = BotFiles.GetCurrentlyLiveBeamChannels().FirstOrDefault(x => x.Name == beamId);
            
            if (live == null)
                return;

            foreach(var message in live.ChannelMessages)
            {
                var serverFile = BotFiles.GetConfiguredServers().FirstOrDefault(x => x.Id == message.GuildId);

                if (serverFile == null)
                    continue;

                if(serverFile.DeleteWhenOffline)
                {
                    await DiscordHelper.DeleteMessage(message.GuildId, message.ChannelId, message.MessageId);
                }
                else
                {
                    await DiscordHelper.SetOfflineStream(message.GuildId, serverFile.StreamOfflineMessage, message.ChannelId, message.MessageId);
                }

                BotFiles.DeleteLiveBeamChannel(beamId);
            }
        }
    }
}
