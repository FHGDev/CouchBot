using MTD.CouchBot.Dals;
using MTD.CouchBot.Domain.Models.Bot;
using System.Threading.Tasks;

namespace MTD.CouchBot.Managers.Implementations
{
    public class StatisticsManager : IStatisticsManager
    {
        private readonly IStatisticsDal _statisticsDal;

        public StatisticsManager(IStatisticsDal statisticsDal)
        {
            _statisticsDal = statisticsDal;
        }

        public async Task AddToBeamAlertCount()
        {
            await _statisticsDal.AddToBeamAlertCount();
        }

        public async Task AddToTwitchAlertCount()
        {
            await _statisticsDal.AddToTwitchAlertCount();
        }

        public async Task AddToYouTubeAlertCount()
        {
            await _statisticsDal.AddToYouTubeAlertCount();
        }

        public async Task AddToMobcrushAlertCount()
        {
            await _statisticsDal.AddToMobcrushAlertCount();
        }

        public async Task<int> GetMobcrushAlertCount()
        {
            return await _statisticsDal.GetMobcrushAlertCount();
        }

        public async Task AddToHitboxAlertCount()
        {
            await _statisticsDal.AddToHitboxAlertCount();
        }

        public async Task AddToPicartoAlertCount()
        {
            await _statisticsDal.AddToPicartoAlertCount();
        }

        public async Task AddUptimeMinutes()
        {
            await _statisticsDal.AddUptimeMinutes();
        }

        public async Task<int> GetBeamAlertCount()
        {
            return await _statisticsDal.GetBeamAlertCount();
        }

        public async Task<int> GetTwitchAlertCount()
        {
            return await _statisticsDal.GetTwitchAlertCount();
        }

        public async Task<int> GetUptimeMinutes()
        {
            return await _statisticsDal.GetUptimeMinutes();
        }

        public async Task<int> GetYouTubeAlertCount()
        {
            return await _statisticsDal.GetYouTubeAlertCount();
        }

        public async Task<int> GetHitboxAlertCount()
        {
            return await _statisticsDal.GetHitboxAlertCount();
        }

        public async Task<BotStats> GetBotStats()
        {
            return await _statisticsDal.GetBotStats();
        }

        public async Task LogRestartTime()
        {
            await _statisticsDal.LogRestartTime();
        }

        public async Task AddToHaiBaiCount()
        {
            await _statisticsDal.AddToHaiBaiCount();
        }

        public async Task<int> GetHaiBaiCount()
        {
            return await _statisticsDal.GetHaiBaiCount();
        }

        public async Task AddToFlipCount()
        {
            await _statisticsDal.AddToFlipCount();
        }

        public async Task<int> GetFlipCount()
        {
            return await _statisticsDal.GetFlipCount();
        }

        public async Task AddToUnflipCount()
        {
            await _statisticsDal.AddToUnflipCount();
        }

        public async Task<int> GetUnflipCount()
        {
            return await _statisticsDal.GetUnflipCount();
        }

        public async Task AddToVidMeAlertCount()
        {
            await _statisticsDal.AddToVidMeAlertCount();
        }

        public async Task<int> GetVidMeAlertCount()
        {
            return await GetVidMeAlertCount();
        }
    }
}
