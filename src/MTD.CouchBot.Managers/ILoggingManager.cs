using MTD.CouchBot.Domain.Models.Bot;
using System.Threading.Tasks;

namespace MTD.CouchBot.Managers
{
    public interface ILoggingManager
    {
        Task LogPerformance(PerformanceMetrics metrics);
    }
}
