using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Security.Cryptography;
using MTD.CouchBot.Webhooks.Models;
using Newtonsoft.Json;

namespace MTD.CouchBot.Webhooks.Controllers
{
    [Route("api/[controller]")]
    public class TwitterController : Controller
    {

        //Consumer Key(API Key)  wU6VsmtpTUgOUSl9Vq5PdJFFx
        //Consumer Secret(API Secret)    
        private string _consumerSecret = "mgpV9LMfahEMVYxyEvbtWLOYcFP8fFchbQguWzcNC5ZwkmkgL5";
        //Access Level Read, write, and direct messages(modify app permissions)
        //Owner dawgeth
        //Owner ID	634018649

        //Access Token	634018649-6WZDY1rN7RdRHHkTXmvEBlMarJlCnEhJIknn2pPZ
        //Access Token Secret X4MWXeauZmolpi2odNwoBav3wOwhYbf8EKsXnF5UPgXug
        //Access Level Read, write, and direct messages
        //Owner dawgeth
        //Owner ID	634018649

        // GET api/twitter/5
        [HttpGet("{crc_token}")]
        public string Get(string crc_token)
        {
            return JsonConvert.SerializeObject(GetHash(crc_token));
        }

        // POST api/twitter
        [HttpPost]
        public void Post([FromBody]string value)
        {

        }

        private CrcResponse GetHash(string crc_token)
        {
            byte[] keyByte = new ASCIIEncoding().GetBytes(_consumerSecret);
            byte[] messageBytes = new ASCIIEncoding().GetBytes(crc_token);

            byte[] hashmessage = new HMACSHA256(keyByte).ComputeHash(messageBytes);

            // to lowercase hexits
            //String.Concat(Array.ConvertAll(hashmessage, x => x.ToString("x2")));
            String.Concat(hashmessage.Select(element => element.ToString("x2")).ToArray());

            // to base64
            return new CrcResponse { response_token = Convert.ToBase64String(hashmessage) };
        }
    }
}
