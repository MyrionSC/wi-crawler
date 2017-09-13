using System;
using System.Collections.Generic;
using System.Linq;

namespace we_crawler
{
    public class Jaccard
    {
        // testing strings
        // string string1 = "do not worry about your difficulties in mathematics";
        // string string2 = "i would not worry about your difficulties, you can easily learn what is needed";

        public static bool stump(string str1, string str2, int shingleLen)
        {
            return false;
        }
    
        // tag to strings, find near duplicate
        public static double nearDuplicateTrickOne(string str1, string str2, int shingleLen)
        {
            char[] del = {' '};
            str1 = str1.Replace(",", "");
            str2 = str2.Replace(",", "");
            string[] strarr1 = str1.Split(del);
            string[] strarr2 = str2.Split(del);

            var setOfSets1 = createSets(strarr1, shingleLen);
            var setOfSets2 = createSets(strarr2, shingleLen);

            var setOfHashVals1 = createHashMinVals(setOfSets1);
            var setOfHashVals2 = createHashMinVals(setOfSets2);

            var union = setOfHashVals1.Union(setOfHashVals2).ToList();
            var intersect = setOfHashVals1.Intersect(setOfHashVals2).ToList();

            double JaccardVal = (double) intersect.Count / (double) union.Count;

            Console.WriteLine("intersect: " + intersect.Count);
            Console.WriteLine("union: " + union.Count);
            Console.WriteLine("JaccardVal: " + JaccardVal);

            return JaccardVal;
        }

        private static HashSet<int> createHashMinVals(HashSet<HashSet<string>> setOfSets1)
        {
            var hashMins = new HashSet<int>();
            for (int i = 1; i < 101; i++)
            {
                var setHashVals = new HashSet<int>();
                var mod = i * 1986438 % 2874638;
                foreach (HashSet<string> s in setOfSets1)
                {
                    int val = Math.Abs(s.GetHashCode() % mod);
                    setHashVals.Add(val);
                }
                hashMins.Add(setHashVals.Min());
            }
            return hashMins;
        }


        // tag to strings, find near duplicate
        public static double nearDuplicateBasic(string str1, string str2, int shingleLen)
        {
            // split strings into arrays of strings whitespace seperated
            char[] del = {' '};
            str1 = str1.Replace(",", "");
            str2 = str2.Replace(",", "");
            string[] strarr1 = str1.Split(del);
            string[] strarr2 = str2.Split(del);

            var setOfSets1 = createSets(strarr1, shingleLen);
            var setOfSets2 = createSets(strarr2, shingleLen);

            printShingles(setOfSets1, setOfSets2);

            var hsc = new HashSetCompare();
            var union = setOfSets1.Union(setOfSets2, hsc).ToList();
            var intersect = setOfSets1.Intersect(setOfSets2, hsc).ToList();

            double JaccardVal = (double) intersect.Count / (double) union.Count;

            Console.WriteLine("intersect: " + intersect.Count);
            Console.WriteLine("union: " + union.Count);
            Console.WriteLine("JaccardVal: " + JaccardVal);

            return JaccardVal;
        }

        private static HashSet<HashSet<string>> createSets(string[] strarr, int shingleLen)
        {
            var setOfSets = new HashSet<HashSet<string>>();
            for (int i = 0; i + shingleLen - 1 < strarr.Length; i++)
            {
                var set = new HashSet<string>();
                for (int j = i, k = 0; j < shingleLen + i; j++, k++)
                {
                    var s = strarr[j];
                    set.Add(s);
                }
                setOfSets.Add(set);
            }
            return setOfSets;
        }

        private static HashSet<HashSet<string>> createSetofSets(List<List<string>> list)
        {
            var sets = new HashSet<HashSet<string>>();

            foreach (var sublist in list)
            {
                sets.Add(new HashSet<string>(sublist));
            }
            return sets;
        }

        private static void printShingles(HashSet<HashSet<string>> setOfSets1, HashSet<HashSet<string>> setOfSets2)
        {
            foreach (var list in setOfSets1)
            {
                foreach (var str in list)
                {
                    Console.Write(str + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("____________________________________________");

            foreach (var list in setOfSets2)
            {
                foreach (var str in list)
                {
                    Console.Write(str + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("___________________________________________");
        }
        

        private class HashSetCompare : IEqualityComparer<HashSet<string>>
        {
            public bool Equals(HashSet<string> x, HashSet<string> y)
            {
                return x.SetEquals(y);
            }

            public int GetHashCode(HashSet<string> obj)
            {
                return 1; // only calls equals if we do this
            }
        }
    }
}