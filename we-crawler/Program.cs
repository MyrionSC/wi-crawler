
using System;

namespace we_crawler
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string seed = "https://en.wikipedia.org/wiki/Main_Page";
            DateTime start = DateTime.Now;
            
            var crawler = new Crawler();
            crawler.Crawl(seed);

            Console.WriteLine();
            Console.WriteLine("executiontime:");
            Console.WriteLine(DateTime.Now - start);
        }
    }
}
