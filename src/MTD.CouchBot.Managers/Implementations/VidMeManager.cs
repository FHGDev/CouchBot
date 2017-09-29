using MTD.CouchBot.Dals;
using MTD.CouchBot.Domain.Models.VidMe;
using System.Threading.Tasks;

namespace MTD.CouchBot.Managers.Implementations
{
    public class VidMeManager : IVidMeManager
    {
        private readonly IVidMeDal _vidMeDal;

        public VidMeManager(IVidMeDal vidMeDal)
        {
            _vidMeDal = vidMeDal;
        }

        public async Task<VidMeUserVideos> GetChannelVideosById(int id)
        {
            return await _vidMeDal.GetUserVideosById(id);
        }

        public async Task<int> GetIdByName(string name)
        {
            return await _vidMeDal.GetIdByName(name);
        }

        public async Task<VidMeUser> GetUserById(int id)
        {
            return await _vidMeDal.GetUserById(id);
        }
    }
}
