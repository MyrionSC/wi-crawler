using System;
using System.Text.RegularExpressions;

namespace we_crawler.model
{
    public class Webpage
    {
        public string Url;
        public string Html;
        public string Protocol;
        public string Host;

        public Webpage(string url, string html)
        {
            Url = url;
            Html = html;
            
            var regex = new Regex("^(http[s]?|ftp):\\/?\\/?([^:\\/\\s]+)");
            var match = regex.Match(url);

            if (match.Groups.Count == 3)
            {
                Protocol = match.Groups[1].Value;
                Host = match.Groups[2].Value;
            }
        }

        public string getProtocalAndHost()
        {
            return Protocol + "://" + Host;
        }
    }
}