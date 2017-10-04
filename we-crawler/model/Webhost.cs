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
        public readonly int id;
        private Queue<string> _queueFrontier;
        private HashSet<string> _setFrontier;
        public Queue<Webpage> BackQueue;
        public string Host;
        public string Protocol;
        public string ServerRoot;
        public string BaseDir;
        public string RobotstxtPath;
        public Politeness Politeness;

        private static int idIterator = 0;

        public Webhost(Webpage wp)
        {
            id = idIterator++;
            
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
            
            _queueFrontier = new Queue<string>();
            _setFrontier = new HashSet<string>();
            BackQueue = new Queue<Webpage>();
            BackQueue.Enqueue(wp);

            // create directory for host files
            if (!Directory.Exists(Utils.GetBaseDir() + "/data"))
            {
                Directory.CreateDirectory(Utils.GetBaseDir() + "/data");

            }
            BaseDir = Utils.GetBaseDir() + "data/" + Utils.EncodeUrl(ServerRoot);
            if (!Directory.Exists(BaseDir))
            {
                Directory.CreateDirectory(BaseDir);
            }
            
            // fetch robot.text and parse it, if it doesn't exist
            if (!File.Exists(BaseDir + "/robots.txt"))
            {
                string robotstxt = Fetcher.FetchSrc(ServerRoot + "/robots.txt");
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
        
        public Webhost(string url)
        {
            id = idIterator++;
            
            try
            {
                Uri uri = new Uri(url);
                Protocol = uri.Scheme;
                Host = uri.Host;
                ServerRoot = Protocol + "://" + Host;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
            
            _queueFrontier = new Queue<string>();
            _setFrontier = new HashSet<string>();
            BackQueue = new Queue<Webpage>();

            // create directory for host files, if it doesn't exist
            if (!Directory.Exists(Utils.GetBaseDir() + "/data"))
            {
                Directory.CreateDirectory(Utils.GetBaseDir() + "/data");

            }
            BaseDir = Utils.GetBaseDir() + "data/" + Utils.EncodeUrl(ServerRoot);
            if (!Directory.Exists(BaseDir))
            {
                Directory.CreateDirectory(BaseDir);
            }
            
            // fetch robot.text and parse it, if it doesn't exist
            RobotstxtPath = BaseDir + "/robots.txt";
            if (!File.Exists(RobotstxtPath))
            {
                string robotstxt = Fetcher.FetchSrc(ServerRoot + "/robots.txt");
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
            else
            {
                Politeness = RobotTxtParser.parse(RobotstxtPath);
            }
        }

        public void EnqueueFrontier(string url)
        {
            bool duplicate = _setFrontier.Contains(url) & BackQueue.Any(w => w.Url == url);
            if (!duplicate)
            {
                _queueFrontier.Enqueue(url);
                _setFrontier.Add(url);
            }
        }
        public string DequeueFrontier()
        {
            string url = _queueFrontier.Dequeue();
            _setFrontier.Remove(url);
            return url;
        }
        public bool ExistsInFrontier(string url)
        {
            return _setFrontier.Contains(url);
        }

        public int CountFronter()
        {
            return _queueFrontier.Count;
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