using Newtonsoft.Json;
using HtmlAgilityPack;
using RedditRPANStreamer.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RedditRPANStreamer.WebEntities.Token;

namespace RedditRPANStreamer
{
    public static class WebTools
    {
        private static string cachedToken;
        public static DateTime cachedTokenExpire;
        
        /// <summary>
        /// Unfortunately, we cannot obtain a token acceptable by Reddit under normal means. We have to use this janky workaround.
        /// </summary>
        /// <returns></returns>
        public static async Task<string> ObtainToken()
        {
            //Check if we even need to refresh
            if (DateTime.UtcNow.AddMinutes(10) < cachedTokenExpire && cachedToken != null)
                return cachedToken;
            
            //We need to request the Reddit homepage
            string homepage = await Program.client.GetStringAsync("https://reddit.com/");

            //Now, parse out the page
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(homepage);

            //Get the element with the data
            var ele = doc.GetElementbyId("data");
            string payload = ele.InnerHtml.Substring("window.___r = ".Length);
            var data = JsonConvert.DeserializeObject<RedditPage>(payload, new JsonSerializerSettings
            {
                CheckAdditionalContent = false
            }); //This skips the data after our payload data

            //Now, get the token and expire time
            cachedToken = data.user.session.accessToken;
            cachedTokenExpire = data.user.session.expires;

            //Return token
            return cachedToken;
        }

        public static void DevalidateToken()
        {
            cachedToken = null;
        }

        public static async Task<T> GetJsonAsync<T>(string url)
        {
            Program.client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await ObtainToken());
            var response = await Program.client.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.Gone)
                throw new StreamEndedException();
            if (!response.IsSuccessStatusCode)
                throw new BadHttpCodeException((int)response.StatusCode);
            var stream = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(stream);
        }
    }
}
