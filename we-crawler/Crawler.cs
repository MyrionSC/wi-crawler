using System;
using System.Collections.Generic;
using System.Linq;
using we_crawler.model;

namespace we_crawler
{
    public class Crawler
    {
        public void Crawl(string seed)
        {
            Queue<string> frontier = new Queue<string>();
            HashSet<Webpage> backqueue = new HashSet<Webpage>();
            
            Fetcher fetcher = new Fetcher();
            WebParser webParser = new WebParser();
            frontier.Enqueue(seed);

            while (backqueue.Count < 100 && frontier.Count != 0)
            {
                // remove url from frontier
                string url = frontier.Dequeue();

                // fetch webpage
                Webpage wp = fetcher.Fetch(url);
                
                // check near duplicity with other pages in backqueue
                if (wp != null)
                {
                    bool nearDuplicate = false;
                    foreach (Webpage bwp in backqueue)
                    {
                        if (Jaccard.stump(wp.Html, bwp.Html, 4))
                        {
                            nearDuplicate = true;
                            break;
                        }
                    }

                    if (!nearDuplicate)
                    {
                        backqueue.Add(wp);
                        List<string> links = webParser.parse(wp);
                        links.ForEach(l =>
                        {
                            bool inFront = frontier.Contains(l);
                            bool InBack = backqueue.Any(w => w.Url == l);
                            bool wiki = l.Contains("wiki");
                            if (!inFront && !InBack && !wiki)
                            {
                                frontier.Enqueue(l);
                            }
                        });
                    }
                }

                Console.WriteLine("frontqueue count: " + frontier.Count);
                Console.WriteLine("backqueue count: " + backqueue.Count);
            }

            Console.WriteLine();
            Console.WriteLine("done crawling: webpages from the following hosts were found");
            Console.WriteLine();
            
            // print number of pages from each host
            List<Utils.MutablePair<string, int>> hostsNo = new List<Utils.MutablePair<string, int>>();
            foreach (var webpage in backqueue)
            {
                if (hostsNo.Any(h => h.First == webpage.Host))
                {
                    var host = hostsNo.Single(h => h.First == webpage.Host);
                    host.Second++;
                }
                else
                {
                    hostsNo.Add(new Utils.MutablePair<string, int>(webpage.Host, 1));
                }
            }
            hostsNo.ForEach(h =>
            {
                Console.WriteLine(h.First + ": " + h.Second);
            });
        }
    }
}