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
//                            bool notWiki = !l.Contains("wiki");
                            if (!inFront && !InBack)
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
            
            // print unique hosts
            HashSet<string> hosts = new HashSet<string>();
            foreach (var webpage in backqueue)
            {
                hosts.Add(webpage.Host);
            }
            
            foreach (var s in hosts)
            {
                Console.WriteLine(s);
            }
        }
    }
}