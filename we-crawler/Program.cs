
using System;
using System.IO;

namespace we_crawler
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string basedir = Utils.getBaseDir(AppDomain.CurrentDomain.BaseDirectory);
            string resdir = "dev/robots/";
            
            string[] robotsarr = {
                "amazonrobot.txt",
                "googlerobot.txt",
                "teamliquidrobot.txt",
                "twitchrobot.txt",
                "wikipediarobot.txt"
            };

            foreach (var file in robotsarr)
            {
                string filesrc = basedir + resdir + file;
                var list = RobotTxtParser.parse(filesrc);
                Console.WriteLine();
                Console.WriteLine(file);
                foreach (string s1 in list)
                {
                    Console.WriteLine(s1);
                }
                Console.WriteLine("-------");
            }
            
        }
    }
}
