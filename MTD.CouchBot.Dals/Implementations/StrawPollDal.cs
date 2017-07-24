﻿using MTD.CouchBot.Domain.Models.StrawPoll;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MTD.CouchBot.Dals.Implementations
{
    public class StrawPollDal : IStrawPollDal
    {
        public async Task<StrawPoll> CreateStrawPoll(StrawPollRequest poll)
        {
            var fullQuery = "https://www.strawpoll.me/api/v2/polls";

            WebRequest request = WebRequest.Create(fullQuery);
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";

            string postString = JsonConvert.SerializeObject(poll);
            byte[] data = Encoding.UTF8.GetBytes(postString);
            Stream newStream = await request.GetRequestStreamAsync();
            newStream.Write(data, 0, data.Length);
            newStream.Dispose();

            WebResponse response = await request.GetResponseAsync();
            StreamReader requestReader = new StreamReader(response.GetResponseStream());
            string webResponse = requestReader.ReadToEnd();
            response.Dispose();

            return JsonConvert.DeserializeObject<StrawPoll>(webResponse);
        }
    }
}
