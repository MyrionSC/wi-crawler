using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using we_crawler.model;

namespace we_crawler
{
    public class Indexer
    {
        private Dictionary<int, Document> _documents = new Dictionary<int, Document>();
        private Dictionary<string, List<int>> wordDict = new Dictionary<string, List<int>>();
        private int _pagesProcessed; // expected to get some race conditions here but whatever

        public Indexer(List<Webpage> webpages)
        {
            // add stopwords from http://www.ranks.nl/stopwords
            string stopwordsstr = "a about above after again against all am an and any are aren't as at be because been before being below between both but by can't cannot could couldn't did didn't do does doesn't doing don't down during each few for from further had hadn't has hasn't have haven't having he he'd he'll he's her here here's hers herself him himself his how how's i i'd i'll i'm i've if in into is isn't it it's its itself let's me more most mustn't my myself no nor not of off on once only or other ought our ours ourselves out over own same shan't she she'd she'll she's should shouldn't so some such than that that's the their theirs them themselves then there there's these they they'd they'll they're they've this those through to too under until up very was wasn't we we'd we'll we're we've were weren't what what's when when's where where's which while who who's whom why why's with won't would wouldn't you you'd you'll you're you've your yours yourself yourselves";
            var stopwords = new HashSet<string>(stopwordsstr.Split(' '));

            int numberOfThreads = 4;
            int threadCounter = 0;
            int webPageListLength = webpages.Count / numberOfThreads;

            for (int i = 0; i < numberOfThreads; i++)
            {
                // split pages into equal parts
                List<Webpage> wps = webpages.GetRange(webPageListLength * threadCounter++, webPageListLength - 1);
                
                // start the computation
                StartParalelisedIndexing(wps, stopwords, webpages.Count);
            }
        }

        private void StartParalelisedIndexing(List<Webpage> wps, HashSet<string> stopwords, int totalCount)
        {
            new Thread(new ThreadStart(() =>
            {
                int i = 0;
                foreach (Webpage wp in wps)
                {
                    // get the body of the html and title (we don't want all these script tags and shit)
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(wp.Html);
                    if (doc.DocumentNode.SelectSingleNode("//title") == null || doc.DocumentNode.SelectSingleNode("//body") == null)
                        continue;
                    string title = doc.DocumentNode.SelectSingleNode("//title").InnerHtml;
                    string body = doc.DocumentNode.SelectSingleNode("//body").InnerHtml;
                
                    // remove html tags and symbols
                    title = ScrubHtml(title);
                    body = ScrubHtml(body);
                    title = removeSymbols(title);
                    body = removeSymbols(body);
                    string[] tokensWithTrash = (title + " " + body).Split(' ');
                    string[] tokens = tokensWithTrash.Where(item => item != "" && item != " " && !stopwords.Contains(item)).ToArray();

                    int left = Console.CursorLeft, top = Console.CursorTop;
                    Console.SetCursorPosition(0, 1);                    
                    Console.WriteLine(_pagesProcessed++ + " of " + totalCount + " processed");
                    Console.SetCursorPosition(left, top);

                    _documents.Add(wp.id, new Document(wp.id, wp.Url, tokens, wordDict));

                    // Add each token to the dictionary
                    foreach (string token in tokens)
                    {
                        try
                        {
                            AddTokenToDict(wp, token);
                        }
                        catch (Exception e)
                        {
//                            Console.WriteLine(e);
                            Thread.Sleep(10);
                            try
                            {
                                AddTokenToDict(wp, token);
                            }
                            catch (Exception exception)
                            {
//                                Console.WriteLine(exception);
                            }
                        }
                    }
                }
                Thread.CurrentThread.Abort();
            })).Start();
        }

        private void AddTokenToDict(Webpage wp, string token)
        {
            if (wordDict.ContainsKey(token))
            {
                wordDict[token].Add(wp.id);
            }
            else
            {
                wordDict.Add(token, new List<int> {wp.id});
            }
        }
        
        // search in indexed pages
        public List<KeyValuePair<double, string>> Search(string searchstring, int cutoffAmount)
        {            
            // split list into list of search terms by whitespace
            string[] searchTerms = searchstring.ToLower().Trim().Split(' ');
            
            // for each term, find results
            List<HashSet<int>> searchResultIds = new List<HashSet<int>>();
            foreach (string searchTerm in searchTerms)
            {
                if (wordDict.ContainsKey(searchTerm))
                {
                    searchResultIds.Add(new HashSet<int>(wordDict[searchTerm]));
                }
            }

            // perform union on ids
            HashSet<int> resultIdSet = new HashSet<int>();
            if (searchResultIds.Count == 0)
            {
                return new List<KeyValuePair<double, string>>();
            }
            resultIdSet = searchResultIds[0];
            for (int i = 1; i < searchResultIds.Count; i++)
            {
                resultIdSet.UnionWith(searchResultIds[i]);
            }
            
            // map ids to documents
            List<Document> resultDocuments = new List<Document>();
            foreach (int id in resultIdSet)
            {
                resultDocuments.Add(_documents[id]);
            }
            
            // rank and sort documents
            List<KeyValuePair<double, string>> rankedDocuments = new List<KeyValuePair<double, string>>();
            resultDocuments.ForEach(d =>
            {
                rankedDocuments.Add(new KeyValuePair<double, string>(d.GetRanking(searchTerms), d.url));
            });

            List<KeyValuePair<double, string>> results = rankedDocuments.OrderByDescending(d => d.Key).ToList();

            // return urls that match searchterm
            return results.GetRange(0, cutoffAmount > results.Count ? results.Count : cutoffAmount);
        }
        
        private string ScrubHtml(string value) {
            var step1 = Regex.Replace(value, @"<[^>]+>|&nbsp;", " ").Trim();
            var step2 = Regex.Replace(step1, @"\s{2,}", " ");
            return step2;
        }

        private string removeSymbols(string input)
        {
            var sb = new StringBuilder();
            foreach (char c in input)
            {
                if (char.IsLetterOrDigit(c))
                    sb.Append(c);
                else
                    sb.Append(' ');
            }
            return sb.ToString();
        }
        
        private class Document
        {
            public int id;
            public string url;
            public string[] tokens;
            private Dictionary<string, List<int>> _wordRefs;

            public Document(int _id, string _url, string[] _tokens, Dictionary<string, List<int>> wordRefs)
            {
                id = _id;
                url = _url;
                tokens = _tokens;
                _wordRefs = wordRefs;
            }

            public double GetRanking(string[] terms)
            {
                return Get_tdStar_IDF_Ranking(terms);
            }
            
            private double GetTermFrequency(string[] terms)
            {
                int[] res = new int[terms.Length];
                for (int i = 0; i < terms.Length; i++)
                {
                    res[i] = tokens.Count(t => t == terms[i]);
                }
                return res.Sum();
            }
            
            private double GetLogFrequency(string[] terms)
            {
                double[] res = new double[terms.Length];
                for (int i = 0; i < terms.Length; i++)
                {
                    string term = terms[i];
                    int count = tokens.Count(t => t == term);

                    if (count == 0)
                        res[i] = 0;
                    else
                        res[i] = 1 + Math.Log10(count);
                }
                return res.Sum();
            }    
            
            private double Get_tdStar_IDF_Ranking(string[] terms)
            {
                double[] res = new double[terms.Length];
                for (int i = 0; i < terms.Length; i++)
                {
                    string term = terms[i];
                    int count = tokens.Count(t => t == term);

                    if (count == 0)
                    {
                        res[i] = 0;
                    }
                    else
                    {
                        double inverseDocFrequency = Math.Log10((double)_wordRefs.Count / _wordRefs[term].Count);
                        double logFrequency = 1 + Math.Log10(count);
                        res[i] = logFrequency * inverseDocFrequency;
                    }
                }
                return res.Sum();
            }
        }
    }
}