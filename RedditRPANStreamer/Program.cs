using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using RedditRPANStreamer.Entities;
using RedditRPANStreamer.WebEntities.Videos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedditRPANStreamer
{
    class Program
    {
        /// <summary>
        /// Global HttpClient
        /// </summary>
        public static HttpClient client;

        /// <summary>
        /// Contains all discovered streams.
        /// </summary>
        public static Dictionary<string, Entities.RedditVideoStream> streams = new Dictionary<string, Entities.RedditVideoStream>();

        /// <summary>
        /// Output path
        /// </summary>
        public const string SAVE_PATH = "C:/Users/Roman/Desktop/rpan_out/";

        /// <summary>
        /// The version of this. Will change rapidly.
        /// </summary>
        public const int VERSION = 1;

        /// <summary>
        /// The last full refresh
        /// </summary>
        public static DateTime? last_refresh;

        static void Main(string[] args)
        {
            client = new HttpClient();
            Task.WaitAll(StartWebServer(43301), MainAsync());
        }

        public static async Task StartWebServer(int port)
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    IPAddress addr = IPAddress.Any;
                    options.Listen(addr, port, listenOptions =>
                    {
                        listenOptions.UseHttps("certificate.pfx", "password");
                    });
                })
                .UseStartup<Program>()
                .Build();

            await host.RunAsync();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.Run(HttpHandler.CreateStatus);
        }

        public static float BytesToMb(long size)
        {
            return size / 1024 / 1024;
        }

        public static async Task MainAsync()
        {
            try
            {
                //Get token for later
                Console.WriteLine("Using token " + await WebTools.ObtainToken());

                //This just loops discovery
                while (true)
                {
                    var streams = await FetchNewStreams();
                    foreach (var s in streams)
                        s.BeginRun();
                    await Task.Delay(5000);
                }
            } catch (Exception ex)
            {
                Console.WriteLine("FAILED! Trying again...");
                WebTools.DevalidateToken();
                await Task.Delay(5000);
                await MainAsync();
            }
        }

        /// <summary>
        /// Gets only new streams that we do not currently have.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Entities.RedditVideoStream>> FetchNewStreams()
        {
            //Fetch new data
            StrapiVideos[] active = (await WebTools.GetJsonAsync<StrapiVideosSeedContainer>("https://strapi.reddit.com/videos/seed/")).data;

            //Now, find which ones of these are actually new
            List<Entities.RedditVideoStream> newStreams = new List<Entities.RedditVideoStream>();
            foreach(var s in active)
            {
                //Get ID and check if we already have this
                string id = s.post.id;
                if (streams.ContainsKey(id))
                {
                    //Write update
                    File.WriteAllText(streams[id].save_location + "history/" + DateTime.UtcNow.Ticks.ToString() + ".json", JsonConvert.SerializeObject(s));

                    //Skip
                    continue;
                }

                //Create a new stream object. It's pretty useless until we do other things with it in a different function
                Entities.RedditVideoStream stream = new Entities.RedditVideoStream(s, SAVE_PATH);

                //Add it to both our list and the global table
                newStreams.Add(stream);
                streams.Add(id, stream);
            }
            last_refresh = DateTime.UtcNow;

            return newStreams;
        }
    }
}
