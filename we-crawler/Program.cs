
using System;

namespace we_crawler
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string seed = "http://www.teamliquid.net/";
            DateTime start = DateTime.Now;
            
            var crawler = new Crawler();
            crawler.StartCrawl(seed);

            Console.WriteLine();
            Console.WriteLine("executiontime:");
            Console.WriteLine(DateTime.Now - start);
        }
    }
}
