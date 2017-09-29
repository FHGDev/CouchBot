using System.Threading.Tasks;

namespace MTD.CouchBot.Managers
{
    public interface IAlertManager
    {
        Task LogAlert(string platform, ulong guildId);
    }
}
