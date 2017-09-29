using Discord.Commands;
using Microsoft.Extensions.Options;
using MTD.CouchBot.Domain.Models.Bot;
using MTD.CouchBot.Services;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MTD.CouchBot.Modules
{
    [Group("my")]
    public class My : ModuleBase
    {
        private readonly BotSettings _botSettings;
        private readonly FileService _fileService;

        public My(IOptions<BotSettings> botSettings, FileService fileService)
        {
            _botSettings = botSettings.Value;
            _fileService = fileService;
        }

        [Command("birthday"), Summary("Sets users birthday.")]
        public async Task Birthday(string date)
        {
            string file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.UserDirectory + Context.Message.Author.Id + ".json";

            var user = new User();

            if (File.Exists(file))
                user = JsonConvert.DeserializeObject<User>(File.ReadAllText(file));

            if (!string.Equals(date, "clear", StringComparison.CurrentCultureIgnoreCase))
            {
                user.Id = Context.Message.Author.Id;
                try
                {
                    user.Birthday = Convert.ToDateTime(date);
                    File.WriteAllText(file, JsonConvert.SerializeObject(user));
                    await Context.Channel.SendMessageAsync("Your Birthday has been set.");
                }
                catch(FormatException)
                {
                    await Context.Channel.SendMessageAsync("Correct Format: mm/dd/yyyy");
                }
            }
            else
            {
                user.Id = Context.Message.Author.Id;
                user.Birthday = null;
                File.WriteAllText(file, JsonConvert.SerializeObject(user));
                await Context.Channel.SendMessageAsync("Your Birthday has been cleared.");
            }
        }

        [Command("timezoneoffset"), Summary("Sets users time zone offset.")]
        public async Task TimeZoneOffset(float offset)
        {
            string file = _botSettings.DirectorySettings.ConfigRootDirectory + _botSettings.DirectorySettings.UserDirectory + Context.Message.Author.Id + ".json";

            var user = new User();

            if (File.Exists(file))
            {
                user = JsonConvert.DeserializeObject<User>(File.ReadAllText(file));
            }

            user.TimeZoneOffset = offset;
            File.WriteAllText(file, JsonConvert.SerializeObject(user));
            await Context.Channel.SendMessageAsync("Your Time Zone Offset has been set.");
        }
    }
}
