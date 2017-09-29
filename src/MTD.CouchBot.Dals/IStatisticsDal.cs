using MTD.CouchBot.Domain.Models.Bot;
using System.Threading.Tasks;

namespace MTD.CouchBot.Dals
{
    public interface IStatisticsDal
    {
        Task<int> GetYouTubeAlertCount();
        Task<int> GetTwitchAlertCount();
        Task<int> GetBeamAlertCount();
        Task<int> GetHitboxAlertCount();
        Task<int> GetUptimeMinutes();
        Task AddToYouTubeAlertCount();
        Task AddToTwitchAlertCount();
        Task AddToBeamAlertCount();
        Task AddToHitboxAlertCount();
        Task AddToPicartoAlertCount();
        Task AddToVidMeAlertCount();
        Task<int> GetVidMeAlertCount();
        Task AddToHaiBaiCount();
        Task AddUptimeMinutes();
        Task<BotStats> GetBotStats();
        Task LogRestartTime();
        Task<int> GetHaiBaiCount();
        Task<int> GetFlipCount();
        Task AddToFlipCount();
        Task<int> GetUnflipCount();
        Task AddToUnflipCount();
        Task AddToMobcrushAlertCount();
        Task<int> GetMobcrushAlertCount();
    }
}
