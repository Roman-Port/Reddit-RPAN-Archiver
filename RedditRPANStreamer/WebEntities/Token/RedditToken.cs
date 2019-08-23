using System;
using System.Collections.Generic;
using System.Text;

namespace RedditRPANStreamer.WebEntities.Token
{
    public class RedditToken
    {
        public string accessToken;
        public DateTime expires;
        public int expiresIn;
        public bool unsafeLoggedOut;
        public bool safe;
    }

    public class RedditUser
    {
        public RedditToken session;
    }

    public class RedditPage
    {
        public RedditUser user;
    }
}
