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
        private Mutex _mutex = new Mutex();

        private void SpawnHostCrawler(Webhost webhost)
        {
            // spawn thread for crawling frontier
            Thread frontThread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    // if the critical lock is taken, wait for it to be released
                    if (webhost.CountFronter() > 0)
                    {
                        string url = webhost.DequeueFrontier();
                        Webpage wp = Fetcher.FetchWebpage(url);
                        if (wp == null)
                        {
                            Console.WriteLine("dead link: " + url);
                            continue;
                        }

                        if (Jaccard.CheckNearDuplicate(wp, webhost.BackQueue, 4))
                        {
                            Console.WriteLine("webpage is duplicate: " + url);
                            continue;
                        }
                        
                        webhost.SaveWebPage(wp);
                        Console.WriteLine("Page saved: " + wp.Url + ", total: " + ++backCount);
                        List<string> links = WebParser.parse(wp);
                        links.ForEach(l =>
                        {
                            // check if duplicate
                            bool valid = !webhost.ExistsInFrontier(l) & !webhost.BackQueue.Any(w => w.Url == l); // to get some shortcircutting
                            string host = Utils.GetHost(l);
                            if (host == null) return; // return equals continue in Linq foreach
//                            valid = valid & host == "en.wikipedia.org";
//                            valid = valid & host.StartsWith("wiki.");
                            if (valid)
                            {
                                // if not of this host, see if we can find its host
                                string linkhost = Utils.GetHost(l);
                                if (linkhost != null)
                                {
                                    
                                    if (linkhost != webhost.Host)
                                    {
                                        _mutex.WaitOne();
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
                                        _mutex.Dispose();
                                    }
                                    else
                                    {
                                        webhost.EnqueueFrontier(l);
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
            // lets keep it at 100 threads, shall we?
            if (Threads.Count < 300)
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
                            newWebHost.EnqueueFrontier(url);
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
        }

        public void StartCrawl(string seed)
        {
            Webpage seedwp = Fetcher.FetchWebpage(seed);
            Webhost seedHost = new Webhost(seedwp);
            seedHost.EnqueueFrontier(seed);
            webhosts.Add(seedHost);

            // start crawling!
            SpawnHostCrawler(seedHost);

            // when all some condition has been met, stop the crawlers
            while (true)
            {
                _mutex.WaitOne();
                if (backCount > 10000)
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
                _mutex.Dispose();

                Thread.Sleep(1000);
            }
        }
    }
}