using Microsoft.Extensions.Options;
using MTD.CouchBot.Domain.Models.Bot;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MTD.CouchBot.Dals.Implementations
{
    public class ApiAiDal : IApiAiDal
    {
        private readonly BotSettings _botSettings;

        public ApiAiDal(IOptions<BotSettings> botSettings)
        {
            _botSettings = botSettings.Value;
        }

        public async Task<ApiAiResponse> AskAQuestion(string question)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create("https://api.api.ai/v1/query?query=" + question + "?&lang=en&sessionId=CouchBot");
            webRequest.Headers["Authorization"] = "Bearer " + _botSettings.KeySettings.ApiAiKey;
            webRequest.ContentType = "application/json; charset=utf-8";
            string str;
            using (StreamReader streamReader = new StreamReader((await webRequest.GetResponseAsync()).GetResponseStream()))
                str = streamReader.ReadToEnd();

            return JsonConvert.DeserializeObject<ApiAiResponse>(str);
        }
    }
}
