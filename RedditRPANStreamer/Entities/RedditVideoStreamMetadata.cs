using System;
using System.Collections.Generic;
using System.Text;

namespace RedditRPANStreamer.Entities
{
    public class RedditVideoStreamMetadata
    {
        public string id; //Post ID
        public string thumbnail; //Post thumbnail
        public string name; //Post name
        public string author_name; //Author username
        public DateTime firstSeen;
        public int archive_version;
    }
}
