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
            string basedir = AppDomain.CurrentDomain.BaseDirectory;
            if (basedir.EndsWith("bin/Debug/"))
            {
                basedir = basedir.Substring(0, basedir.Length - 10);
            }
            return basedir;
        }

        public static string GetHostFromUrl(string url)
        {
            var regex = new Regex("^(http[s]?|ftp):\\/?\\/?([^:\\/\\s]+)");
            var match = regex.Match(url);

            return match.Groups.Count == 3 ? match.Groups[2].Value : null;
        }
        
        public static string Base64Encode(string plainText) {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData) {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
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