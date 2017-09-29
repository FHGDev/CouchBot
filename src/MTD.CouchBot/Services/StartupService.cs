using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using MTD.CouchBot.Domain.Models.Bot;
using MTD.CouchBot.Domain.Utilities;
using MTD.CouchBot.Managers;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace MTD.CouchBot.Services
{
    public class StartupService
    {
        private readonly DiscordShardedClient _discord;
        private readonly CommandService _commands;
        private readonly BotSettings _botSettings;
        private readonly IStatisticsManager _statisticsManager;

        public StartupService(
            DiscordShardedClient discord,
            CommandService commands,
            IOptions<BotSettings> botSettings,
            IStatisticsManager statististicsManager)
        {
            _botSettings = botSettings.Value;
            _discord = discord;
            _commands = commands;
            _statisticsManager = statististicsManager;
        }

        public async Task StartAsync()
        {
            Logging.LogInfo("Logging In");

            string discordToken = _botSettings.KeySettings.DiscordToken;

            if (string.IsNullOrWhiteSpace(discordToken))
                throw new Exception("Please enter your bot's token into the `BotSettings.json` file found in the applications root directory.");

            await _statisticsManager.LogRestartTime();
            await _discord.LoginAsync(TokenType.Bot, discordToken);     
            await _discord.StartAsync();                                

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

            Logging.LogInfo("Completed - Logging In");
        }

    }
}
