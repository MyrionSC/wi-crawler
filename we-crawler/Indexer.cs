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
        private Dictionary<int, Document> _documents;
        private Dictionary<string, List<int>> wordDict = new Dictionary<string, List<int>>();

        public Indexer(List<Webpage> webpages)
        {
            _webpages = webpages;

            // add stopwords from http://www.ranks.nl/stopwords
            string stopwordsstr = "a about above after again against all am an and any are aren't as at be because been before being below between both but by can't cannot could couldn't did didn't do does doesn't doing don't down during each few for from further had hadn't has hasn't have haven't having he he'd he'll he's her here here's hers herself him himself his how how's i i'd i'll i'm i've if in into is isn't it it's its itself let's me more most mustn't my myself no nor not of off on once only or other ought our ours ourselves out over own same shan't she she'd she'll she's should shouldn't so some such than that that's the their theirs them themselves then there there's these they they'd they'll they're they've this those through to too under until up very was wasn't we we'd we'll we're we've were weren't what what's when when's where where's which while who who's whom why why's with won't would wouldn't you you'd you'll you're you've your yours yourself yourselves";
            var stopwords = new HashSet<string>(stopwordsstr.Split(' '));
            
            // --- index the webpages
            // foreach webpage
                // isolate content (remove html tags)
                // remove stopwords
                // foreach word in webpage
                    // if word exists in searchwords, add wp id to its refs
                    // if not add new searchword and add wp ref to it
            
            int i = 0;
            foreach (Webpage wp in webpages)
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
                
                Console.WriteLine(wp.Url + ": pages processed: " + i++ + ", tokens in page: " + tokens.Length);
                
                _documents.Add(wp.id, new Document(wp.id, wp.Url, tokens, _webpages.Count));

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
        }
        
        // search in indexed pages
        public List<string> Search(string searchstring)
        {
            List<string> intersectResults = new List<string>();
            
            // split list into list of search terms by whitespace
            string[] searchTerms = searchstring.ToLower().Trim().Split(' ');
            
            
            // for each term, find results
            // todo: match part of word as well
            List<HashSet<string>> searchResults = new List<HashSet<string>>();
            foreach (string searchTerm in searchTerms)
            {
                var searchTermRes = wordDict.SingleOrDefault(sw => sw.Key == searchTerm);
                if (searchTermRes.Key != null)
                {
                    var resultIdSet = new HashSet<int>(searchTermRes.Value);
                    var resultSet = new HashSet<string>();
                    foreach (int id in resultIdSet)
                    {
                        resultSet.Add(_webpages.Single(wp => wp.id == id).Url);
                    }
                    searchResults.Add(resultSet);
                }
            }

            // perform union on ids
            if (searchResults.Count == 0)
            {
                intersectResults.Add("No results found");
            } 
            else if (searchResults.Count == 1)
            {
                intersectResults = searchResults[0].ToList();
            }
            else
            {
                var resultSet = searchResults[0];
                for (int i = 1; i < searchResults.Count; i++)
                {
                    resultSet.IntersectWith(searchResults[i]);
                }
                intersectResults = resultSet.ToList();
            }

            // return urls that match searchterm
            return intersectResults;
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
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append(' ');
                }
            }
            return sb.ToString();
        }
        
        private class Document
        {
            public int id;
            public string url;
            public string[] tokens;
            private int docCount;

            public Document(int _id, string _url, string[] _tokens, int _docCount)
            {
                id = _id;
                url = _url;
                tokens = _tokens;
                docCount = _docCount;
            }

            public double GetTermFrequency(string[] terms)
            {
                int[] res = new int[terms.Length];
                for (int i = 0; i < terms.Length; i++)
                {
                    res[i] = tokens.Select(t => t == terms[i]).Count();

                }
                return res.Sum();
            }
            public double GetLogFrequency(string term)
            {
//                int count = tokens.Select(t => t == term).Count();
//                if (count == 0) return 0;
//                return 1 + Math.Log10(count);
                return 0;
            }
        }
    }
}