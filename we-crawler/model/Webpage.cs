using System;
using System.Text.RegularExpressions;

namespace we_crawler.model
{
    public class Webpage
    {
        public readonly int id;
        public string Url;
        public string Html;
        
        private static int idIterator = 0;

        public Webpage(string url, string html)
        {
            id = idIterator++;
            Url = url;
            Html = html;
        }
    }
}