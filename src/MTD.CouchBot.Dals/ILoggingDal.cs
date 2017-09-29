using MTD.CouchBot.Domain.Models.Bot;
using System.Threading.Tasks;

namespace MTD.CouchBot.Dals
{
    public interface ILoggingDal
    {
        Task LogPerformance(PerformanceMetrics metrics);
    }
}
