using Discord;
using Discord.Commands;

namespace MTD.CouchBot.Modules
{
    [Group("purge")]
    public class Purge : ModuleBase
    {
        [Command("bot")]
        public void Bot(int count)
        {
            var guild = ((IGuildUser)Context.Message.Author).Guild;

            var user = ((IGuildUser)Context.Message.Author);

            if (!user.GuildPermissions.ManageGuild)
            {
                return;
            }
        }
    }
}
