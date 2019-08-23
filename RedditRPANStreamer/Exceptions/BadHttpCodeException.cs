using System;
using System.Collections.Generic;
using System.Text;

namespace RedditRPANStreamer.Exceptions
{
    public class BadHttpCodeException : Exception
    {
        public int httpCode;

        public BadHttpCodeException(int code)
        {
            httpCode = code;
        }
    }
}
