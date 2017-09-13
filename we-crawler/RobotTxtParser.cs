using System.IO;
using System.Linq;
using we_crawler.model;

namespace we_crawler
{
    public class RobotTxtParser
    {        
//            string basedir = Utils.getBaseDir(AppDomain.CurrentDomain.BaseDirectory);
//            string resdir = "dev/robots/";
//            
//            string[] robotsarr = {
//                "amazonrobot.txt",
//                "googlerobot.txt",
//                "teamliquidrobot.txt",
//                "twitchrobot.txt",
//                "wikipediarobot.txt"
//            };
//
//            foreach (var file in robotsarr)
//            {
//                string filesrc = basedir + resdir + file;
//                var pol = RobotTxtParser.parse(filesrc);
//                Console.WriteLine();
//                Console.WriteLine(file);
//                foreach (string s1 in pol.allows)
//                {
//                    Console.WriteLine(s1);
//                }
//                foreach (string s1 in pol.disallows)
//                {
//                    Console.WriteLine(s1);
//                }
//                Console.WriteLine("-------");
//            }
        
        public static Politeness parse(string robotfile)
        {
            Politeness pol = new Politeness();

            if (File.Exists(robotfile))
            {
                string[] robottxt = File.ReadAllLines(robotfile);
                robottxt = robottxt.Select(s => s.ToLower()).ToArray();

                for (int i = 0; i < robottxt.Length; i++)
                {
                    string s = robottxt[i];
                    if (s.Contains("user-agent: *"))
                    {
                        int allowAllIndex = i;
                        int j = 1;
                        while (allowAllIndex + j < robottxt.Length - 1 &&
                               (robottxt[allowAllIndex + j].StartsWith("allow: ") ||
                                robottxt[allowAllIndex + j].StartsWith("disallow: ")))
                        {
                            string l = robottxt[allowAllIndex + j];
                            if (robottxt[allowAllIndex + j].StartsWith("allow: "))
                            {
                                l = l.Substring(7, l.Length - 7);
                                pol.allows.Add(l);
                            }
                            if (robottxt[allowAllIndex + j].StartsWith("disallow: "))
                            {
                                l = l.Substring(10, l.Length - 10);
                                pol.disallows.Add(l);
                            }
                            j++;
                        }
                    }
                }
            }
            
            return pol;
        }
    }
}