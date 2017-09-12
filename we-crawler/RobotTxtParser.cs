using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace we_crawler
{
    public class RobotTxtParser
    {
        public static List<string> parse(string robotfile)
        {
            List<string> allowList = new List<string>();

            if (File.Exists(robotfile))
            {
                string[] robottxt = File.ReadAllLines(robotfile);
                robottxt = robottxt.Select(s => s.ToLower()).ToArray();

                // find useragent
                for (int i = 0; i < robottxt.Length; i++)
                {
                    string s = robottxt[i];
//                    Console.WriteLine(s);
                    if (s.Contains("user-agent: *"))
                    {
                        int allowAllIndex = i;
                        int j = 1;
                        while (allowAllIndex + j < robottxt.Length &&
                               (robottxt[allowAllIndex + j].Contains("allow") ||
                                robottxt[allowAllIndex + j].Contains("disallow")))
                        {
                            allowList.Add(robottxt[allowAllIndex + j]);
                            j++;
                        }
                    }
                }
            }
            else
            {
                allowList.Add("File not found");
            }
            
            return allowList;
        }
    }
}