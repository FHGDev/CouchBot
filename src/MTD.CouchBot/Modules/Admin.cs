using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using MTD.CouchBot.Domain.Models.Bot;
using MTD.CouchBot.Services;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MTD.CouchBot.Modules
{
    [Group("admin")]
    [Alias("a")]
    public class Admin : ModuleBase
    {
        private readonly BotSettings _botSettings;
        private readonly FileService _fileService;

        public Admin(IOptions<BotSettings> botSettings, FileService fileService)
        {
            _botSettings = botSettings.Value;
            _fileService = fileService;
        }

        [Command("add")]
        public async Task Add(IGuildUser user)
        {
            var guild = ((IGuildUser)Context.Message.Author).Guild;

            var authorUser = ((IGuildUser)Context.Message.Author);

            if (!authorUser.GuildPermissions.ManageGuild)
            {
                return;
            }

            var file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + guild.Id + ".json";
            var server = new DiscordServer();

            if (File.Exists(file))
            {
                server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(file));
            }

            if (server.ApprovedAdmins == null)
            {
                server.ApprovedAdmins = new List<ulong>();
            }

            if(server.ApprovedAdmins.Contains(user.Id))
            {
                await Context.Channel.SendMessageAsync(user.Username + " is already on the approved admins list for this server.");

                return;
            }

            server.ApprovedAdmins.Add(user.Id);
            await _fileService.SaveDiscordServer(server, Context.Guild);

            await Context.Channel.SendMessageAsync(user.Username + " has been added to the approved admin list for this server.");
        }

        [Command("remove")]
        public async Task Remove(IGuildUser user)
        {
            var guild = ((IGuildUser)Context.Message.Author).Guild;

            var authorUser = ((IGuildUser)Context.Message.Author);

            if (!authorUser.GuildPermissions.ManageGuild)
            {
                return;
            }

            var file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + guild.Id + ".json";
            var server = new DiscordServer();

            if (File.Exists(file))
            {
                server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(file));
            }

            if (server.ApprovedAdmins == null || !server.ApprovedAdmins.Contains(user.Id))
            {
                await Context.Channel.SendMessageAsync(user.Username + " is not on the approved admins list for this server.");

                return;
            }

            server.ApprovedAdmins.Remove(user.Id);
            await _fileService.SaveDiscordServer(server, Context.Guild);

            await Context.Channel.SendMessageAsync(user.Username + " has been removed from the approved admin list for this server.");
        }

        [Command("list")]
        public async Task List()
        {
            var guild = ((IGuildUser)Context.Message.Author).Guild;

            var authorUser = ((IGuildUser)Context.Message.Author);

            if (!authorUser.GuildPermissions.ManageGuild)
            {
                return;
            }

            var file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.GuildDirectory + guild.Id + ".json";
            var server = new DiscordServer();

            if (File.Exists(file))
            {
                server = JsonConvert.DeserializeObject<DiscordServer>(File.ReadAllText(file));
            }

            if (server.ApprovedAdmins == null)
            {
                server.ApprovedAdmins = new List<ulong>();
            }

            var admins = "";

            foreach (var aa in server.ApprovedAdmins)
            {
                var user = await Context.Guild.GetUserAsync(aa);
                admins += user.Username + ", ";
            }

            admins = admins.Trim().TrimEnd(',');

            string info = "```Markdown\r\n" +
              "# Server Approved Admins\r\n" +
              admins + "\r\n" +
              "```\r\n";

            await Context.Channel.SendMessageAsync(info);
        }
    }
}
