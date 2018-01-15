using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using we_crawler.model;

namespace we_crawler
{
    public class Indexer
    {
        private Dictionary<int, Document> _documentsById = new Dictionary<int, Document>();
        private Dictionary<string, Document> _documentsByUrl = new Dictionary<string, Document>();
        private Dictionary<string, List<int>> wordDict = new Dictionary<string, List<int>>();
        private int _pagesProcessed;
        private static Mutex _mutex = new Mutex();

        public Indexer(List<Webpage> webpages)
        {
            // add stopwords from http://www.ranks.nl/stopwords
            string stopwordsstr = "a about above after again against all am an and any are aren't as at be because been before being below between both but by can't cannot could couldn't did didn't do does doesn't doing don't down during each few for from further had hadn't has hasn't have haven't having he he'd he'll he's her here here's hers herself him himself his how how's i i'd i'll i'm i've if in into is isn't it it's its itself let's me more most mustn't my myself no nor not of off on once only or other ought our ours ourselves out over own same shan't she she'd she'll she's should shouldn't so some such than that that's the their theirs them themselves then there there's these they they'd they'll they're they've this those through to too under until up very was wasn't we we'd we'll we're we've were weren't what what's when when's where where's which while who who's whom why why's with won't would wouldn't you you'd you'll you're you've your yours yourself yourselves";
            var stopwords = new HashSet<string>(stopwordsstr.Split(' '));

            int numberOfThreads = 4;
            int threadCounter = 0;
            int webPageListLength = webpages.Count / numberOfThreads;
            Thread[] threads = new Thread[numberOfThreads];

            for (int i = 0; i < numberOfThreads; i++)
            {
                // split pages into equal parts
                List<Webpage> wps;
                wps = threadCounter + 1 != numberOfThreads
                    ? webpages.GetRange(webPageListLength * threadCounter++, webPageListLength)
                    : webpages.GetRange(webPageListLength * threadCounter,
                        webpages.Count - webPageListLength * threadCounter);
                
                // start the computation
                threads[i] = StartParalelisedIndexing(wps, stopwords, webpages.Count);
            }

            while (true)
            {
                Thread.Sleep(200);
                if (threads.All(t => !t.IsAlive)) break;
            }

            // compile in and out edges in each page
            foreach (KeyValuePair<int,Document> keyValuePair in _documentsById)
            {
                var document = keyValuePair.Value;
                document.OutgoingLinks.ForEach(l =>
                {
                    if (_documentsByUrl.ContainsKey(l) && _documentsByUrl[l].id != document.id)
                    {
                        _documentsByUrl[l].IngoingLinkIds.Add(document.id);
                        document.OutgoingLinkIds.Add(_documentsByUrl[l].id);
                    }
                });
            }
            
            
            
            // --- PageRank generation - Markov Chain
            Console.WriteLine();
            Console.WriteLine("Starting markov chain PageRanker");
            
            // markov chain
            int runSteps = 100;
            Random random = new Random();
            double[] markovChainOld = new double[_documentsById.Count];
            double[] markovChainNew = new double[_documentsById.Count];
            for (int i = 0; i < _documentsById.Count; i++)
            {
                markovChainOld[i] = 1d / _documentsById.Count;
                markovChainNew[i] = 0d;
            }
//            markovChainNew[0] = markovChainOld[0] = 1d;

            // start calculating // we have foregone the transition matrix because we have an inverted index of links from each document
            for (int step = 0; step < runSteps; step++)
            {
                foreach (KeyValuePair<int,Document> idDocPair in _documentsById)
                {
                    int id = idDocPair.Key;
                    
                    if (markovChainOld[id] == 0) 
                        continue;
                    
                    if (idDocPair.Value.OutgoingLinkIds.Count == 0)
                    {
                        // go to random link
                        markovChainNew[random.Next(0, _documentsById.Count)] += markovChainOld[id];
                    }
                    else
                    {
                        double transitionValue = markovChainOld[id] / idDocPair.Value.OutgoingLinkIds.Count;
                        foreach (int linkId in idDocPair.Value.OutgoingLinkIds)
                        {
                            markovChainNew[linkId] += transitionValue;
                        }
                    }
                }
                markovChainOld = markovChainNew.Clone() as double[];
                
                for (int i = 0; i < _documentsById.Count; i++)
                {
                    markovChainNew[i] = 0;
                }
            }

            Console.WriteLine("PageRanker done");
            
            // assign pageranks to documents
            for (int index = 0; index < markovChainOld.Length; index++)
            {
                _documentsById[index].PageRank = Math.Round(markovChainOld[index], 10);
            }
        }

        private Thread StartParalelisedIndexing(List<Webpage> wps, HashSet<string> stopwords, int totalCount)
        {
            Thread t =  new Thread(new ThreadStart(() =>
            {
                foreach (Webpage wp in wps)
                {
                    // get the body and title of the html (we don't want the meta tags)
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
                    string[] tokensLower = new string[tokens.Length];
                    for (var j = 0; j < tokens.Length; j++)
                    {
                        tokensLower[j] = tokens[j].ToLower();
                    }

                    _mutex.WaitOne();
                    Console.WriteLine(_pagesProcessed++ + " of " + totalCount + " processed");
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    _mutex.ReleaseMutex();

//                    var document = new Document(wp.id, wp.Url, new string[0], wordDict, WebParser.parse(wp));
                    var document = new Document(wp.id, wp.Url, tokensLower, wordDict, WebParser.parse(wp));
                    _documentsById.Add(wp.id, document);
                    _documentsByUrl.Add(wp.Url, document);

                    // Add each token to the dictionary
                    foreach (string token in tokensLower)
                    {
                        _mutex.WaitOne();
                        AddTokenToDict(wp, token);
                        _mutex.ReleaseMutex();
                    }
                }
                Thread.CurrentThread.Abort();
            }));
            t.Start();
            return t;
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
        public List<SearchResult> Search(string searchstring, int cutoffAmount)
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
                return new List<SearchResult>();
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
                resultDocuments.Add(_documentsById[id]);
            }
            
            // rank and sort documents
            List<SearchResult> results = new List<SearchResult>();
            resultDocuments.ForEach(document =>
            {
                double contentRank = document.GetRanking(searchTerms);
                results.Add(new SearchResult(document.url, contentRank, document.PageRank, contentRank + Math.Log((1 + document.PageRank * 10000))));
            });

            List<SearchResult> orderedResults = results.OrderByDescending(d => d.TotalRank).ToList();

            // return urls that match searchterm
            return orderedResults.GetRange(0, cutoffAmount > orderedResults.Count ? orderedResults.Count : cutoffAmount);
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
            public List<string> OutgoingLinks;
            public HashSet<int> IngoingLinkIds = new HashSet<int>();
            public HashSet<int> OutgoingLinkIds = new HashSet<int>();
            public double PageRank = 0;

            public Document(int _id, string _url, string[] _tokens, Dictionary<string, List<int>> wordRefs, List<string> outgoingLinks)
            {
                id = _id;
                url = _url;
                tokens = _tokens;
                _wordRefs = wordRefs;
                OutgoingLinks = outgoingLinks;
            }

            public double GetRanking(string[] terms)
            {
                return Get_tfStar_IDF_Ranking(terms);
            }
            
            private double Get_tfStar_IDF_Ranking(string[] terms)
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