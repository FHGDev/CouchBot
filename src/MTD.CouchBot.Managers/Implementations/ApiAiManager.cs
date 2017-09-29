using MTD.CouchBot.Dals;
using MTD.CouchBot.Domain.Models.Bot;
using System.Threading.Tasks;

namespace MTD.CouchBot.Managers.Implementations
{
    public class ApiAiManager : IApiAiManager
    {
        private readonly IApiAiDal _apiAiDal;

        public ApiAiManager(IApiAiDal apiAiDal)
        {
            _apiAiDal = apiAiDal;
        }

        public async Task<ApiAiResponse> AskAQuestion(string question)
        {
            return await _apiAiDal.AskAQuestion(question);
        }
    }
}
