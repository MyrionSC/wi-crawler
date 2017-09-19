using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;

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
//            var regex = new Regex("^(http[s]?|ftp):\\/?\\/?([^:\\/\\s]+)");
//            var match = regex.Match(wp.Url);
//
//            if (match.Groups.Count == 3)
//            {
//                Protocol = match.Groups[1].Value;
//                Host = match.Groups[2].Value;
//                ServerRoot = Protocol + "://" + Host;
//            }
//            else
//            {
//                throw new Exception("input not valid url");
//            }

            try
            {
                Uri uri = new Uri(wp.Url);
                Protocol = uri.Scheme;
                Host = uri.Host;
                ServerRoot = Protocol + "://" + Host;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
            
            Frontier = new Queue<string>();
            BackQueue = new Queue<Webpage>();
            BackQueue.Enqueue(wp);

            // create directory for host files
            if (!Directory.Exists(Utils.GetBaseDir() + "/data"))
            {
                Directory.CreateDirectory(Utils.GetBaseDir() + "/data");

            }
            BaseDir = Utils.GetBaseDir() + "data/" + Host;
            if (!Directory.Exists(Utils.GetBaseDir() + "/data/" + Host))
            {
                Directory.CreateDirectory(BaseDir);
            }
            
            // fetch robot.text and parse it, if it exists
            string robotstxt = Fetcher.FetchSrc(ServerRoot + "/robots.txt");            
            if (robotstxt != null)
            {
                RobotstxtPath = BaseDir + "/robots.txt";
                try
                {
                    File.WriteAllText(RobotstxtPath, robotstxt);
                    Politeness = RobotTxtParser.parse(RobotstxtPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("No robots.txt for host: " + Host);
                    Politeness = null;
                }
            }
        }

        public void EnqueueFrontier(string url)
        {
            bool inFront = Frontier.Contains(url);
            bool InBack = BackQueue.Any(w => w.Url == url);
            if (!inFront && !InBack)
            {
                Frontier.Enqueue(url);
            }
        }

        public void SaveWebPage(Webpage wp)
        {
            if (BaseDir != null)
            {
                try
                {
                    string path = BaseDir + "/" + Utils.EncodeUrl(wp.Url);
                    File.WriteAllText(path, wp.Html);
                }
                catch (PathTooLongException e)
                {
                    // fuck this file
                }
                catch (IOException ioe)
                {
                    Console.WriteLine("IO ex");
                    Console.WriteLine(ioe.Message);
                    Thread.Sleep(10);
                    SaveWebPage(wp);
                }
                catch (Exception e)
                {
                    Console.WriteLine("savewebpage ex");
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}