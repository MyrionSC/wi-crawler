using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace we_crawler.model
{
    public class Webhost
    {
        public Queue<string> Frontier;
        public Queue<Webpage> BackQueue;
        public string Host;
        public string Protocol;
        public string ServerRoot;
        public string BaseDir;
        public string RobotstxtPath;
        public Politeness Politeness;

        public Webhost(Webpage wp)
        {   
            var regex = new Regex("^(http[s]?|ftp):\\/?\\/?([^:\\/\\s]+)");
            var match = regex.Match(wp.Url);

            if (match.Groups.Count == 3)
            {
                Protocol = match.Groups[1].Value;
                Host = match.Groups[2].Value;
                ServerRoot = Protocol + "://" + Host;
            }
            else
            {
                throw new Exception("input not valid url");
            }
            
            Frontier = new Queue<string>();
            BackQueue = new Queue<Webpage>();
            BackQueue.Enqueue(wp);

            // create directory for host files
            if (!Directory.Exists(Utils.GetBaseDir() + "/data"))
            {
                Directory.CreateDirectory(Utils.GetBaseDir() + "/data");

            }
            if (!Directory.Exists(Utils.GetBaseDir() + "/data/" + Host))
            {
                BaseDir = Utils.GetBaseDir() + "/data/" + Host;
                Directory.CreateDirectory(BaseDir);
            }
            
            // fetch robot.text and parse it, if it exists
            string robotstxt = Fetcher.FetchSrc(ServerRoot + "/robots.txt");            
            if (robotstxt != null)
            {
                RobotstxtPath = Utils.GetBaseDir() + "/data/" + Host + "/robots.txt";
                File.WriteAllText(RobotstxtPath, robotstxt);
                Politeness = RobotTxtParser.parse(RobotstxtPath);
            }
        }
    }
}