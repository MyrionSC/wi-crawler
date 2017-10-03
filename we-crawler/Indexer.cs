using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using we_crawler.model;

namespace we_crawler
{
    public class Indexer
    {
        private List<Webpage> _webpages;
        private Dictionary<int, Document> _documentsById = new Dictionary<int, Document>();
        private Dictionary<string, Document> _documentsByUrl = new Dictionary<string, Document>();
        private Dictionary<string, List<int>> wordDict = new Dictionary<string, List<int>>();

        public Indexer(List<Webpage> webpages)
        {
            _webpages = webpages;

            // add stopwords from http://www.ranks.nl/stopwords
            string stopwordsstr = "a about above after again against all am an and any are aren't as at be because been before being below between both but by can't cannot could couldn't did didn't do does doesn't doing don't down during each few for from further had hadn't has hasn't have haven't having he he'd he'll he's her here here's hers herself him himself his how how's i i'd i'll i'm i've if in into is isn't it it's its itself let's me more most mustn't my myself no nor not of off on once only or other ought our ours ourselves out over own same shan't she she'd she'll she's should shouldn't so some such than that that's the their theirs them themselves then there there's these they they'd they'll they're they've this those through to too under until up very was wasn't we we'd we'll we're we've were weren't what what's when when's where where's which while who who's whom why why's with won't would wouldn't you you'd you'll you're you've your yours yourself yourselves";
            var stopwords = new HashSet<string>(stopwordsstr.Split(' '));

            int i = 0;
            foreach (Webpage wp in webpages)
            {
                if (i > 300) break;
                
                // get the body of the html and title (we don't want all these script tags and shit)
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(wp.Html);
                if (doc.DocumentNode.SelectSingleNode("//title") == null || doc.DocumentNode.SelectSingleNode("//body") == null)
                    continue;
                string title = doc.DocumentNode.SelectSingleNode("//title").InnerHtml;
                string body = doc.DocumentNode.SelectSingleNode("//body").InnerHtml;
                
                // find all outgoing links for page
                List<string> OutgoingLinks = WebParser.Parse(wp);
                
                // remove html tags and symbols
                title = ScrubHtml(title);
                body = ScrubHtml(body);
                title = removeSymbols(title);
                body = removeSymbols(body);
                string[] tokensWithTrash = (title + " " + body).Split(' ');
                string[] tokens = tokensWithTrash.Where(item => item != "" && item != " " && !stopwords.Contains(item)).ToArray();
                
//                Console.WriteLine(wp.Url + ": pages processed: " + i++ + ", tokens in page: " + tokens.Length);
                Console.SetCursorPosition(0, 2);
                Console.WriteLine(i++ + " of " + webpages.Count + " pages processed");

                var document = new Document(wp.id, wp.Url, tokens, wordDict, OutgoingLinks);
                _documentsById.Add(wp.id, document);
                _documentsByUrl.Add(wp.Url, document);

                // Add each token to the dictionary
                foreach (string token in tokens)
                {
                    if (wordDict.ContainsKey(token))
                    {
                        wordDict[token].Add(wp.id);
                    }
                    else
                    {
                        wordDict.Add(token, new List<int>() {wp.id});
                    }
                }
            }
            
            // 
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
                resultDocuments.Add(_documentsById[id]);
            }
            
            // rank and sort documents
            List<KeyValuePair<double, string>> rankedDocuments = new List<KeyValuePair<double, string>>();
            resultDocuments.ForEach(d =>
            {
                rankedDocuments.Add(new KeyValuePair<double, string>(d.GetRanking(searchTerms), d.Url));
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
            public int Id;
            public string Url;
            public string[] Tokens;
            private Dictionary<string, List<int>> _wordRefs;
            private HashSet<String> _outgoingLinks;
            private HashSet<String> _ingoingLinks = new HashSet<string>();

            public Document(int id, string url, string[] tokens, Dictionary<string, List<int>> wordRefs, List<string> outgoingLinks)
            {
                Id = id;
                Url = url;
                Tokens = tokens;
                _wordRefs = wordRefs;
                _outgoingLinks = new HashSet<string>(outgoingLinks);
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
                    res[i] = Tokens.Count(t => t == terms[i]);
                }
                return res.Sum();
            }
            
            private double GetLogFrequency(string[] terms)
            {
                double[] res = new double[terms.Length];
                for (int i = 0; i < terms.Length; i++)
                {
                    string term = terms[i];
                    int count = Tokens.Count(t => t == term);

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
                    int count = Tokens.Count(t => t == term);

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