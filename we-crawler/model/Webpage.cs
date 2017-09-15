using System;
using System.Text.RegularExpressions;

namespace we_crawler.model
{
    public class Webpage
    {
        public string Url;
        public string Html;

        public Webpage(string url, string html)
        {
            Url = url;
            Html = html;
        }
    }
}