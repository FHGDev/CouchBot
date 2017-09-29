using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using MTD.CouchBot.Domain.Models.Bot;
using System;
using System.Threading.Tasks;

namespace MTD.CouchBot.Services
{
    public class CommandHandler
    {
        private readonly DiscordShardedClient _discord;
        private readonly CommandService _commands;
        private readonly BotSettings _botSettings;
        private readonly IServiceProvider _provider;

        public CommandHandler(
            DiscordShardedClient discord,
            CommandService commands,
            IOptions<BotSettings> botSettings,
            IServiceProvider provider)
        {
            _discord = discord;
            _commands = commands;
            _botSettings = botSettings.Value;
            _provider = provider;

            _discord.MessageReceived += OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null) return;
            if (msg.Author == _discord.CurrentUser) return;

            var context = new ShardedCommandContext(_discord, msg);

            int argPos = 0;
            if (msg.HasStringPrefix(_botSettings.BotConfig.Prefix, ref argPos) || msg.HasMentionPrefix(_discord.CurrentUser, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _provider);
            }
        }
    }
}
