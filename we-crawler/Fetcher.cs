using System;
using we_crawler.model;

namespace we_crawler
{
    public class Fetcher
    {
        public Webpage Fetch(string url)
        {
            try
            {
                var html = new System.Net.WebClient().DownloadString(url);
                return new Webpage(url, html);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(url);
                return null;
            }
            
        }
    }
}