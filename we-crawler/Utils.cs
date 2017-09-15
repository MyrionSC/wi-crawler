using System;
using System.Collections;
using System.Collections.Generic;

namespace we_crawler
{
    public class Utils
    {
        public static string getBaseDir(string basedir)
        {
            if (basedir.Substring(basedir.Length - 10, 10) == "bin/Debug/")
            {
                basedir = basedir.Substring(0, basedir.Length - 10);
            }
            return basedir;
        }
        
        public class MutablePair<TFirst, TSecond>
        {
            public TFirst First { get; set; }
            public TSecond Second { get; set; }

            public MutablePair()
            {
            }

            public MutablePair(TFirst first, TSecond second)
            {
                First = first;
                Second = second;
            }
        }
    }
}