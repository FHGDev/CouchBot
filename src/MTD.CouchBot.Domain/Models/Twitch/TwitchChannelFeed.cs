﻿using System.Collections.Generic;

namespace MTD.CouchBot.Domain.Models.Twitch
{
    public class TwitchChannelFeed
    {
        public class Post
        {
            public string id { get; set; }
            public string body { get; set; }
            public string created_at { get; set; }
        }

        public bool _disabled { get; set; }
        public string _cursor { get; set; }
        public string _topic { get; set; }
        public List<Post> posts { get; set; }
    }
}
