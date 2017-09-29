using MTD.CouchBot.Dals;
using MTD.CouchBot.Domain.Models.Bot;
using System.Threading.Tasks;

namespace MTD.CouchBot.Managers.Implementations
{
    public class LoggingManager : ILoggingManager
    {
        private readonly ILoggingDal _loggingDal;

        public LoggingManager(ILoggingDal loggingDal)
        {
            _loggingDal = loggingDal;
        }

        public async Task LogPerformance(PerformanceMetrics metrics)
        {
            await _loggingDal.LogPerformance(metrics);
        }
    }
}
