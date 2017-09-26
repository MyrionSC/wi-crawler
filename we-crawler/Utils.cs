using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace we_crawler
{
    public class Utils
    {
        public static string GetBaseDir()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetHost(string url)
        {
            try
            {
                return new Uri(url).Host;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public static bool NotMediaFile(string url)
        {
            url = url.ToLower();
            return !url.EndsWith(".png") & !url.EndsWith(".jpg") & !url.EndsWith(".ogv") & !url.EndsWith(".ogg") &
                   !url.EndsWith("webm") & !url.EndsWith(".svg") & !url.EndsWith(".pdf");
        }
        
        public static string EncodeUrl(string url)
        {
            return url.Replace("/", "{");
            
//            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(url);
//            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string DecodeUrl(string encodedUrl)
        {
            return encodedUrl.Replace("{", "/");
            
            var base64EncodedBytes = System.Convert.FromBase64String(encodedUrl);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        
        public class MutablePair<TFirst, TSecond>
        {
            public TFirst First { get; set; }
            public TSecond Second { get; set; }

            public MutablePair()
            {
            }

            public MutablePair(TFirst first, TSecond second)
            {
                First = first;
                Second = second;
            }
        }
    }
}