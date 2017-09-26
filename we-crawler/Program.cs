
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
            string seed = "http://www.bbc.com/";
            DateTime start = DateTime.Now;

            var crawler = new Crawler();
            crawler.StartCrawl(seed);
            
            Console.WriteLine();
            Console.WriteLine("Crawling time:");
            Console.WriteLine(DateTime.Now - start);
            
//            StartIndexing(start);
        }

        public static void StartIndexing(DateTime start)
        {
            
            // init hosts with local data
            List<Webhost> webhosts = Initialiser.LoadWebhosts();
            
            // throw all pages into list
            List<Webpage> webpages = new List<Webpage>();
            webhosts.ForEach(wh => { webpages.AddRange(wh.BackQueue); });
            
            // index list of webpages
            var indexer = new Indexer(webpages);
            
            Console.WriteLine();
            Console.WriteLine("Init and indexing time:");
            Console.WriteLine(DateTime.Now - start);

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Input search term:");
                Console.WriteLine();
                
                string input = Console.ReadLine();
                var results = indexer.Search(input);

                Console.WriteLine();
                Console.WriteLine("results for search:");
                results.ForEach(r =>
                {
                    Console.WriteLine(r);
                });
            }
        }
    }
}
