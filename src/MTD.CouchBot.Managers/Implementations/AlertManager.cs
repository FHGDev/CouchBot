using MTD.CouchBot.Dals;
using System.Threading.Tasks;

namespace MTD.CouchBot.Managers.Implementations
{
    public class AlertManager : IAlertManager
    {
        private readonly IAlertDal _alertDal;

        public AlertManager(IAlertDal alertDal)
        {
            _alertDal = alertDal;
        }

        public async Task LogAlert(string platform, ulong guildId)
        {
           await _alertDal.LogAlert(platform, guildId);
        }
    }
}
