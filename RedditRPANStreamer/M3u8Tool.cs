using System;
using System.Collections.Generic;
using System.Text;

namespace RedditRPANStreamer
{
    public static class M3u8Tool
    {
        public static List<string> GetMedia(string input)
        {
            //This ia very lazy tool. We just get the media strings
            //For some reason, some of these use spaces instead of newlines. That probably isn't to spec, but oh well...
            //We also just get any lines without a # in front of them.
            string[] lines = input.Replace(" ", "\n").Replace("\r", "").Split('\n');
            List<string> output = new List<string>();
            foreach(var s in lines)
            {
                if (s.StartsWith('#'))
                    continue;
                output.Add(s);
            }
            return output;
        }
    }
}
