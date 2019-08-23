using RedditRPANStreamer.WebEntities.Status;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RedditRPANStreamer
{
    public static class HttpHandler
    {
        public static async Task CreateStatus(Microsoft.AspNetCore.Http.HttpContext context)
        {
            //Add CORS headers
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            //Create status
            Status status = new Status
            {
                streams = new List<StatusStream>(),
                total_streams = Program.streams.Count,
                last_refresh = Program.last_refresh
            };
            foreach(var s in Program.streams.Values)
            {
                if(s.finished)
                    status.finished_streams++;
                long length;
                if (s.output_video_length_finished.HasValue)
                    length = s.output_video_length_finished.Value; //Has finished
                else
                    length = s.outputVideo.Length; //Has not finished
                status.total_size += length;
                StatusStream os = new StatusStream
                {
                    author = s.author_name,
                    completed = s.finished,
                    fatal_errored = s.failed,
                    failed_chunks = s.failed_chunks.Count,
                    id = s.id,
                    size = length,
                    thumbnail = s.thumbnail,
                    title = s.name,
                    total_chunks = s.chunk_count,
                    completed_chunks = s.progress,
                    last_chunk = s.last_chunk_time,
                    last_thumbnail = s.last_thumbnail_time,
                    latest_thumbnail_hash = s.latest_thumbnail_hash,
                    thumbnail_count = s.thumbnail_index,
                    thumbnail_fails = s.thumbnail_fails
                };
                status.streams.Add(os);
            }

            //Now, serialize and write
            string data = JsonConvert.SerializeObject(status, Formatting.Indented);
            byte[] bdata = Encoding.UTF8.GetBytes(data);
            await context.Response.Body.WriteAsync(bdata, 0, bdata.Length);
        }
    }
}
