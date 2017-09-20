using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using we_crawler.model;

namespace we_crawler
{
    public class Indexer
    {
        private List<Webpage> _webpages;
        private HashSet<string> _stopwords;
        private HashSet<SearchWord> _searchWords = new HashSet<SearchWord>();

        public Indexer(List<Webpage> webpages)
        {
            _webpages = webpages;

            // add stopwords from http://www.ranks.nl/stopwords
            string stopwordsstr = "a about above after again against all am an and any are aren't as at be because been before being below between both but by can't cannot could couldn't did didn't do does doesn't doing don't down during each few for from further had hadn't has hasn't have haven't having he he'd he'll he's her here here's hers herself him himself his how how's i i'd i'll i'm i've if in into is isn't it it's its itself let's me more most mustn't my myself no nor not of off on once only or other ought our ours ourselves out over own same shan't she she'd she'll she's should shouldn't so some such than that that's the their theirs them themselves then there there's these they they'd they'll they're they've this those through to too under until up very was wasn't we we'd we'll we're we've were weren't what what's when when's where where's which while who who's whom why why's with won't would wouldn't you you'd you'll you're you've your yours yourself yourselves";
            _stopwords = new HashSet<string>(stopwordsstr.Split(' '));
            
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
                Console.WriteLine(i++);
                // get the body of the html and title (we don't want all these script tags and shit)
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(wp.Html);
                string title = doc.DocumentNode.SelectSingleNode("//title").InnerHtml;
                string body = doc.DocumentNode.SelectSingleNode("//body").InnerHtml;
                body = ScrubHtml(body);
                
                // remove anything other than letters and text
                Regex pattern = new Regex("[^A-Za-z0-9 \n]*");
                title = pattern.Replace(title, "").ToLower();
                body = pattern.Replace(body, "").ToLower();
                string[] tokens = (title + " " + body).Split(' ');
                
                // throw each token into the meatgrinder
                foreach (string token in tokens)
                {
                    var searchWord = _searchWords.SingleOrDefault(sw => sw.word == token);
                    if (searchWord != null)
                    {
                        searchWord.refs.Add(wp.id);
                    }
                    else
                    {
                        _searchWords.Add(new SearchWord(token, wp.id));
                    }
                }
            }
        }
        
        // search in indexed pages
        public List<string> Search(string searchstring)
        {
            List<string> results = new List<string>();
            
            // split list into list of search terms by whitespace
            
            // for each term, find results
            // todo: match part of word as well
            // todo: toLower() on searchstring items
            HashSet<int> result = new HashSet<int>();
            var searchWordRes = _searchWords.SingleOrDefault(sw => sw.word == searchstring);
            if (searchWordRes != null)
            {
                result = new HashSet<int>(searchWordRes.refs);
                foreach (int id in result)
                {
                    results.Add(_webpages.Single(wp => wp.id == id).Url);
                }
            }
            else
            {
                results.Add("No results found");
            }
            
            // perform union on ids
            
            // return urls that match searchterm
            return results;
        }
        
        private string ScrubHtml(string value) {
            var step1 = Regex.Replace(value, @"<[^>]+>|&nbsp;", "").Trim();
            var step2 = Regex.Replace(step1, @"\s{2,}", " ");
            return step2;
        }
        
        private class SearchWord
        {
            public string word;
            public List<int> refs = new List<int>();

            public SearchWord(string word, int wpref)
            {
                this.word = word;
                refs.Add(wpref);
            }
        }
    }
}