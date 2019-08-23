using System;
using System.Collections.Generic;
using System.Text;

namespace RedditRPANStreamer.WebEntities.Status
{
    public class Status
    {
        public int total_streams;
        public int finished_streams;
        public long total_size;
        public List<StatusStream> streams;
        public DateTime? last_refresh;
    }

    public class StatusStream
    {
        public long size;
        public string id;
        public int total_chunks;
        public int completed_chunks;
        public int failed_chunks;
        public bool completed;
        public bool fatal_errored;
        public string title;
        public string author;
        public string thumbnail;
        public string latest_thumbnail_hash;
        public int thumbnail_count;
        public DateTime? last_thumbnail;
        public DateTime? last_chunk;
        public int thumbnail_fails;
    }
}
