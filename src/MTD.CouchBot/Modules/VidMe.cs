using Discord.Commands;
using Microsoft.Extensions.Options;
using MTD.CouchBot.Domain.Models.Bot;
using MTD.CouchBot.Managers;
using MTD.CouchBot.Modules;
using MTD.CouchBot.Services;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MTD.DiscordBot.Modules
{
    [Group("vidme")]
    public class VidMe : BaseModule
    {
        private readonly IVidMeManager _vidMeManager;
        private readonly BotSettings _botSettings;
        private readonly FileService _fileService;

        public VidMe(IVidMeManager vidMeManager, IOptions<BotSettings> botSettings, FileService fileService)
            : base(botSettings)
        {
            _vidMeManager = vidMeManager;
            _botSettings = botSettings.Value;
            _fileService = fileService;
        }
        
        [Command("add")]
        public async Task Add(string name)
        {
            if (!IsApprovedAdmin)
            {
                return;
            }

            var id = await _vidMeManager.GetIdByName(name);

            if(id == 0)
            {
                await Context.Channel.SendFileAsync("No channel exists with the name " + name + ".");
            }

            var file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + Context.Guild.Id + ".json";
            var server = new DiscordServer();

            if (File.Exists(file))
            {
                server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(file));
            }

            if (server.ServerVidMeChannels == null)
                server.ServerVidMeChannels = new List<string>();

            if (server.ServerVidMeChannelIds == null)
                server.ServerVidMeChannelIds = new List<int>();

            if (!string.IsNullOrEmpty(server.OwnerVidMeChannel) && server.OwnerVidMeChannel.ToLower().Equals(name.ToLower()))
            {
                await Context.Channel.SendMessageAsync("The channel " + name + " is configured as the Owner Vid.me channel. " +
                    "Please remove it with the '!cb vidme resetowner' command and then try re-adding it.");

                return;
            }

            if (!server.ServerVidMeChannels.Contains(name.ToLower()))
            {
                server.ServerVidMeChannels.Add(name.ToLower());
                server.ServerVidMeChannelIds.Add(id);
                await _fileService.SaveDiscordServer(server, Context.Guild);
                await Context.Channel.SendMessageAsync("Added " + name + " to the server Vid.me list.");
            }
            else
            {
                await Context.Channel.SendMessageAsync(name + " is already on the server Vid.me list.");
            }
        }

        [Command("remove")]
        public async Task Remove(string name)
        {
            if (!IsApprovedAdmin)
            {
                return;
            }

            var id = await _vidMeManager.GetIdByName(name);

            var file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + Context.Guild.Id + ".json";
            var server = new DiscordServer();

            if (File.Exists(file))
                server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(file));

            if (server.ServerVidMeChannels == null)
                return;

            if (server.ServerVidMeChannels.Contains(name.ToLower()))
            {
                server.ServerVidMeChannels.Remove(name.ToLower());
                server.ServerVidMeChannelIds.Remove(id);
                await _fileService.SaveDiscordServer(server, Context.Guild);
                await Context.Channel.SendMessageAsync("Removed " + name + " from the server Vid.me list.");
            }
            else
            {
                await Context.Channel.SendMessageAsync(name + " wasn't on the server Vid.me list.");
            }
        }

        [Command("owner")]
        public async Task Owner(string name)
        {
            if (!IsAdmin)
            {
                return;
            }

            var id = await _vidMeManager.GetIdByName(name);

            if (id == 0)
            {
                await Context.Channel.SendMessageAsync("No channel exists with the name " + name + ".");
                return;
            }

            var file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + Context.Guild.Id + ".json";
            var server = new DiscordServer();

            if (File.Exists(file))
                server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(file));

            if (server.Id == 0)
                return;

            if (server.ServerVidMeChannels != null && server.ServerVidMeChannels.Contains(name.ToLower()))
            {
                await Context.Channel.SendMessageAsync("The channel " + name + " is in the list of server Vid.me Channels. " +
                    "Please remove it with '!cb vidme remove " + name + "' and then retry setting your owner channel.");

                return;
            }

            server.OwnerVidMeChannel = name;
            server.OwnerVidMeChannelId = id;
            await _fileService.SaveDiscordServer(server, Context.Guild);
            await Context.Channel.SendMessageAsync("Owner Vid.me channel has been set to " + name + ".");
        }

        [Command("resetowner")]
        public async Task ResetOwner()
        {
            if (!IsAdmin)
            {
                return;
            }

            var file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + Context.Guild.Id + ".json";
            var server = new DiscordServer();

            if (File.Exists(file))
                server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(file));

            server.OwnerVidMeChannel = null;
            server.OwnerVidMeChannelId = 0;
            await _fileService.SaveDiscordServer(server, Context.Guild);
            await Context.Channel.SendMessageAsync("Owner Vid.me channel has been reset.");
        }
    }
}
