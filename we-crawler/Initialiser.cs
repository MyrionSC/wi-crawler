using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using we_crawler.model;

namespace we_crawler
{
    public class Initialiser
    {
        // loads a list of files into webpages in webhosts
        public static List<Webhost> LoadWebhosts ()
        {
            List<Webhost> webhosts = new List<Webhost>();
            
            // foreach dir in data
                // make a webhost object
                // parse all files in dir to webpages and add to webhost
            
            // get dirs in folder data
            string datadir = Utils.GetBaseDir() + "data";
            string[] dirs = Directory.GetDirectories(datadir);
            
            foreach (var dir in dirs)
            {
                // init the webhost with url from first webpage in it
                string[] files = Directory.GetFiles(dir);
                string firstPage = Utils.DecodeUrl(files[0].Substring(dir.Length + 1, files[0].Length - dir.Length - 1)); // get url
                Webhost wh = new Webhost(firstPage);
                webhosts.Add(wh);

                // add remaining webpages to it
                for (var i = 0; i < files.Length; i++)
                {
                    var f = files[i];
                    if (!f.Contains("robots.txt"))
                    {
                        string url = Utils.DecodeUrl(f.Substring(dir.Length + 1, f.Length - dir.Length - 1));
                        string html = File.ReadAllText(f);
                        wh.BackQueue.Enqueue(new Webpage(url, html));
                    }
                }
                Console.WriteLine("Initter: Created webhost " + wh.Host + ", containing " + wh.BackQueue.Count + " pages");
            }
            
            return webhosts;
        }
    }
}