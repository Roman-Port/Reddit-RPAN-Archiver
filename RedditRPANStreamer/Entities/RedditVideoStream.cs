using Newtonsoft.Json;
using RedditRPANStreamer.Exceptions;
using RedditRPANStreamer.WebEntities.Videos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RedditRPANStreamer.Entities
{
    public class RedditVideoStream
    {
        public StrapiVideos post; //Post data
        public string id; //Post ID
        public string thumbnail; //Post thumbnail
        public string name; //Post name
        public string author_name; //Author username
        public int progress; //A lot like the downloaded one, but in real-time
        public string index_endpoint; //Doesn't really contain much but a URL to the source
        public string index_source_endpoint; //Contains all videos
        public string save_location; //Where this'll be outputted to

        public HttpClient client; //Client to be used
        public int chunk_count = 0; //Total number of chunks
        public int downloaded = 0; //Number of segments that have been downloaded
        public bool finished = false; //Has the stream finished
        public bool failed = false; //Fatal errored
        public List<int> failed_chunks; //Failed indexes
        public int thumbnail_fails = 0; //Number of fails on the thumbnail
        public string latest_thumbnail_hash = null; //The latest hash of the thumbnail
        public int thumbnail_index = 0; //Number of thumbnails we've captured
        public DateTime? last_thumbnail_time; //Time of the last thumbnail
        public DateTime? last_chunk_time; //Last time a chunk was seen

        public Stream outputVideo; //The video output
        public long? output_video_length_finished = null; //The length of the video ONLY if it has finished

        public Task worker; //The current worker for this
        public Task thumbnail_worker; //Worker for thumbnails

        public RedditVideoStream(StrapiVideos data, string root_save)
        {
            post = data;
            id = data.post.id;
            thumbnail = data.stream.thumbnail;
            name = data.post.title;
            author_name = data.post.authorInfo.name;
            index_endpoint = data.stream.hls_url;
            save_location = root_save + id + "/";
            client = new HttpClient();
            failed_chunks = new List<int>();
        }

        /// <summary>
        /// Begins this running
        /// </summary>
        public void BeginRun()
        {
            worker = RunAsync();
        }

        /// <summary>
        /// Runs the archive. Finishes when this stream finishes
        /// </summary>
        /// <returns></returns>
        public async Task RunAsync()
        {
            //First, open the output
            OpenOutput();

            //Now, gather stream info
            await BeginStream();

            //Get the thumbnail
            await RefreshThumbnail();

            //Start thumbnail loop
            thumbnail_worker = ThumbnailLoop();

            //Now, download segments
            int tries = 0; //-1 means completed OK
            while(tries < 5)
            {
                tries++;
                try
                {
                    while (true)
                    {
                        await RefreshVideoSegments();
                        await Task.Delay(1000);
                    }
                }
                catch (StreamEndedException)
                {
                    //This stream was correctly finished.
                    tries = -1;
                    break;
                }
            }
            
            //Finish and clean up
            finished = true;
            if (tries != -1)
                failed = true;
            output_video_length_finished = outputVideo.Length;
            outputVideo.Close();
        }

        /// <summary>
        /// Gathers thumbnails
        /// </summary>
        /// <returns></returns>
        private async Task ThumbnailLoop()
        {
            while(!finished)
            {
                await Task.Delay(5000);
                await RefreshThumbnail();
            }
        }

        /// <summary>
        /// Gathers stream info, but does not start streaming video
        /// </summary>
        /// <returns></returns>
        public async Task BeginStream()
        {
            //Open the index_source_endpoint stream
            index_source_endpoint = (await DownloadM3u8Media(index_endpoint))[0];
        }

        /// <summary>
        /// Opens the output stream
        /// </summary>
        public void OpenOutput()
        {
            //Create directory
            if (!Directory.Exists(save_location))
                Directory.CreateDirectory(save_location);
            if (!Directory.Exists(save_location + "history/"))
                Directory.CreateDirectory(save_location + "history/");
            if (!Directory.Exists(save_location + "thumbnails/"))
                Directory.CreateDirectory(save_location + "thumbnails/");

            //Create streams
            outputVideo = new FileStream(save_location+"stream.ts", FileMode.Create, FileAccess.Write);

            //Write data
            File.WriteAllText(save_location + "raw.json", JsonConvert.SerializeObject(post));
            File.WriteAllText(save_location + "metadata.json", JsonConvert.SerializeObject(new RedditVideoStreamMetadata
            {
                author_name = author_name,
                id = id,
                name = name,
                thumbnail = thumbnail,
                firstSeen = DateTime.UtcNow,
                archive_version = Program.VERSION
            }));
        }

        /// <summary>
        /// Refreshes the segments that are downloaded. This method is NOT thread safe.
        /// </summary>
        /// <returns></returns>
        public async Task RefreshVideoSegments()
        {
            //Request segments
            var segments = await DownloadM3u8Media(index_source_endpoint);
            chunk_count = segments.Count;

            //Now, download each new segment
            string root = index_source_endpoint.Substring(0, index_source_endpoint.LastIndexOf('/') + 1);
            for(int i = downloaded; i<segments.Count; i++)
            {
                //Start
                var d = await client.GetAsync(root + segments[i]);

                //Ensure that we have an OK status code
                if(!d.IsSuccessStatusCode)
                {
                    failed_chunks.Add(i);
                    progress++;
                    last_chunk_time = DateTime.UtcNow;
                    continue;
                }

                //Now, wait for the download to complete and copy it to the output file
                Stream data = await d.Content.ReadAsStreamAsync();
                await data.CopyToAsync(outputVideo);
                progress++;
                last_chunk_time = DateTime.UtcNow;
            }
            downloaded = segments.Count;
            progress = downloaded;
        }

        /// <summary>
        /// Refreshes the thumbnail if it has changed.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RefreshThumbnail()
        {
            try
            {
                //Fetch stream
                byte[] data;
                using (MemoryStream ms = new MemoryStream())
                {
                    var thumb = await client.GetStreamAsync(thumbnail);
                    await thumb.CopyToAsync(ms);
                    ms.Position = 0;
                    data = new byte[ms.Length];
                    await ms.ReadAsync(data, 0, data.Length);
                }

                //Now, hash it
                string hash;
                using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
                {
                    hash = Convert.ToBase64String(sha1.ComputeHash(data));
                }

                //Compare
                if (hash == latest_thumbnail_hash)
                    return false; //This is the same image as last time

                //This is a new image. Save to disk and update hash.
                latest_thumbnail_hash = hash;
                thumbnail_index++;
                last_thumbnail_time = DateTime.UtcNow;
                await File.WriteAllBytesAsync(save_location + "thumbnails/" + DateTime.UtcNow.Ticks + ".jpg", data);
                return true;
            } catch (Exception ex)
            {
                thumbnail_fails++;
                return false;
            }
        }

        private async Task<List<string>> DownloadM3u8Media(string url)
        {
            var response = await client.GetAsync(url);
            if(response.StatusCode == System.Net.HttpStatusCode.Gone)
            {
                throw new StreamEndedException();
            }
            if (!response.IsSuccessStatusCode)
                throw new BadHttpCodeException((int)response.StatusCode);
            var stream = await response.Content.ReadAsStringAsync();
            var segments = M3u8Tool.GetMedia(stream);
            return segments;
        }
    }
}
