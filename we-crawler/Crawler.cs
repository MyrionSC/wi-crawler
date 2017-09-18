using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using we_crawler.model;

namespace we_crawler
{
    public class Crawler
    {
        HashSet<Webhost> webhosts = new HashSet<Webhost>();
        
        private void SpawnHostCrawler (Webhost webhost)
        {
            // spawn thread for crawling frontier
            Thread frontThread = new Thread(new ThreadStart(() =>
            {
                int i = 0;
                while (true)
                {
                    i++;
                    Console.WriteLine(i);
                    Thread.Sleep(1);
                    if (i > 100)
                    {
                        Thread.CurrentThread.Abort();
                    }
                }
            }));
            frontThread.Start();
            
            // spawn thread for crawling backqueue
            Thread backThread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    int i = 100;
                    while (true)
                    {
                        i--;
                        Console.WriteLine(i);
                        Thread.Sleep(1);
                        if (i < 0)
                        {
                            Thread.CurrentThread.Abort();
                        }
                    }
                }
            }));
            backThread.Start();
            
            
        }
        
        public void StartCrawl(string seed)
        {
            Webpage seedwp = Fetcher.FetchWebpage(seed);
            Webhost seedHost = new Webhost(seedwp);
            seedHost.Frontier.Enqueue(seed);
            webhosts.Add(seedHost);
            
            // start crawling!
            SpawnHostCrawler(seedHost);
            
//            Queue<string> frontier = new Queue<string>();
//            HashSet<Webpage> backqueue = new HashSet<Webpage>();
//            
//            frontier.Enqueue(seed);
//
//            while (backqueue.Count < 100 && frontier.Count != 0)
//            {
//                // remove url from frontier
//                string url = frontier.Dequeue();
//
//                // fetch webpage
//                Webpage wp = fetcher.Fetch(url);
//                
//                // check near duplicity with other pages in backqueue
//                if (wp != null)
//                {
//                    bool nearDuplicate = Jaccard.CheckNearDuplicate(wp, backqueue, 4);
//
//                    if (!nearDuplicate)
//                    {
//                        backqueue.Add(wp);
//                        List<string> links = webParser.parse(wp);
//                        links.ForEach(l =>
//                        {
//                            bool inFront = frontier.Contains(l);
//                            bool InBack = backqueue.Any(w => w.Url == l);
//                            bool wiki = l.Contains("wiki");
//                            if (!inFront && !InBack && !wiki)
//                            {
//                                frontier.Enqueue(l);
//                            }
//                        });
//                    }
//                }
//
//                Console.WriteLine("frontqueue count: " + frontier.Count);
//                Console.WriteLine("backqueue count: " + backqueue.Count);
//            }

//            Console.WriteLine();
//            Console.WriteLine("done crawling: webpages from the following hosts were found");
//            Console.WriteLine();
//            
//            // print number of pages from each host
//            List<Utils.MutablePair<string, int>> hostsNo = new List<Utils.MutablePair<string, int>>();
//            foreach (var webpage in backqueue)
//            {
//                if (hostsNo.Any(h => h.First == webpage.Host))
//                {
//                    var host = hostsNo.Single(h => h.First == webpage.Host);
//                    host.Second++;
//                }
//                else
//                {
//                    hostsNo.Add(new Utils.MutablePair<string, int>(webpage.Host, 1));
//                }
//            }
//            hostsNo.ForEach(h =>
//            {
//                Console.WriteLine(h.First + ": " + h.Second);
//            });
        }
    }
}