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
        private HashSet<Webhost> webhosts = new HashSet<Webhost>();
        private Queue<Thread> Threads = new Queue<Thread>();
        private int backCount = 0;
        private bool criticalLock = false;

        private void SpawnHostCrawler(Webhost webhost)
        {
            // spawn thread for crawling frontier
            Thread frontThread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    // if the critical lock is taken, wait for it to be released
                    if (criticalLock)
                    {
                        Thread.Sleep(1);
                        continue;
                    }
                    
                    if (webhost.Frontier.Count > 0)
                    {
                        string url = webhost.Frontier.Dequeue();
                        Webpage wp = Fetcher.FetchWebpage(url);
                        if (wp == null)
                        {
                            Console.WriteLine("dead link: " + url);
                            continue;
                        }
                        
                        webhost.SaveWebPage(wp);
                        backCount++;
                        List<string> links = WebParser.parse(wp);
                        links.ForEach(l =>
                        {
                            // check if duplicate
                            bool inFront = webhost.Frontier.Contains(l);
                            bool InBack = webhost.BackQueue.Any(w => w.Url == l);
                            bool InWiki = l.Contains("en.wikipedia.org");
                            if (!inFront && !InBack && InWiki)
                            {
                                // if not of this host, see if we can find its host
                                string linkhost = Utils.GetHost(l);
                                if (linkhost != null)
                                {
                                    
                                    if (linkhost != webhost.Host)
                                    {
                                        criticalLock = true;
                                        try
                                        {
                                            Webhost linkWebHost = webhosts.First(wh => wh.Host == linkhost);
                                            linkWebHost.EnqueueFrontier(l);
                                        }
                                        catch (InvalidOperationException)
                                        {
                                            // create new
                                            AddNewHost(l);
                                        }
                                        criticalLock = false;
                                    }
                                    else
                                    {
                                        webhost.Frontier.Enqueue(l);
                                    }
                                }
                            }
                        });
                        
                        // be nice to the hosts
                        Thread.Sleep(2000);
                    }
                }
            }));
            Threads.Enqueue(frontThread);
            frontThread.Start();
        }

        public void AddNewHost(string url)
        {
            Webpage newWebPage = Fetcher.FetchWebpage(url);
            if (newWebPage != null)
            {
                try
                {
                    Webhost newWebHost = new Webhost(newWebPage);
                    // It is checked again that the host doesn't exist to alleviate race conditions
                    if (!webhosts.Any(wh => wh.Host == newWebHost.Host))
                    {
                        newWebHost.Frontier.Enqueue(url);
                        webhosts.Add(newWebHost);
                        Console.WriteLine("new webhost added: " + newWebHost.Host);
                    
                        // start crawling it
                        SpawnHostCrawler(newWebHost);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void StartCrawl(string seed)
        {
            Webpage seedwp = Fetcher.FetchWebpage(seed);
            Webhost seedHost = new Webhost(seedwp);
            seedHost.Frontier.Enqueue(seed);
            webhosts.Add(seedHost);

            // start crawling!
            SpawnHostCrawler(seedHost);

            // when all some condition has been met, stop the crawlers
            while (true)
            {
                criticalLock = true;
                Console.WriteLine("pages processed: " + backCount);
                if (backCount > 1000)
                {
                    // kill all the threads and return
                    while (Threads.Count > 0)
                    {
                        Threads.Dequeue().Abort();
                    }
                    Console.WriteLine("----");
                    Console.WriteLine("crawling done");
                    return;
                }
                criticalLock = false;

                Thread.Sleep(1000);
            }
        }
    }
}