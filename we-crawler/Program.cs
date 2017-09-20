
using System;
using System.Collections.Generic;
using System.Linq;
using we_crawler.model;

namespace we_crawler
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string seed = "https://en.wikipedia.org/wiki/Main_Page";
            DateTime start = DateTime.Now;
            
//            var crawler = new Crawler();
//            crawler.StartCrawl(seed);

            // init hosts with local data
            List<Webhost> webhosts = Initialiser.LoadWebhosts();
            
            // throw all pages into list
            List<Webpage> webpages = new List<Webpage>();
            webhosts.ForEach(wh =>
            {
                webpages.AddRange(wh.BackQueue);
            });
            
            // index list of webpages
            

            Console.WriteLine();
            Console.WriteLine("executiontime:");
            Console.WriteLine(DateTime.Now - start);
        }
    }
}
