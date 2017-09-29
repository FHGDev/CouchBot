using System.Threading.Tasks;

namespace MTD.CouchBot.Dals
{
    public interface IAlertDal
    {
        Task LogAlert(string platform, ulong guildId);
    }
}
