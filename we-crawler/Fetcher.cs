using System;
using we_crawler.model;

namespace we_crawler
{
    public class Fetcher
    {
        public static Webpage FetchWebpage(string url)
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
        public static string FetchSrc(string url)
        {
            try
            {
                return new System.Net.WebClient().DownloadString(url);
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