﻿
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
            string seed = "http://en.wikipedia.org/";
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
                var results = indexer.Search(input, 10);

                Console.WriteLine();
                Console.WriteLine("results for search: " + input);

                if (results.Count == 0)
                {
                    Console.WriteLine("no results found");
                }
                else
                {
                    for (int i = 0; i < results.Count; i++)
                    {
                        var r = results[i];
                        Console.WriteLine(1 + i + " - " + r.Value + ": Rank: " + r.Key);
                    }
                }
            }
        }
    }
}
