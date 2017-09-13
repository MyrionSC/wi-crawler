using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using we_crawler.model;

namespace we_crawler
{
    public class WebParser
    {
        public List<string> parse(Webpage wp)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(wp.Html);
            IEnumerable<string> linkedPages = doc.DocumentNode.Descendants("a")
                .Select(a => a.GetAttributeValue("href", null))
                .Where(u => !String.IsNullOrEmpty(u));
            
            List<string> refinedLinkedPages = new List<string>();
            foreach (string linkedPage in linkedPages)
            {
                if (!linkedPage.StartsWith("#"))
                {
                    string lp = linkedPage;
                    if (lp.StartsWith("//"))
                    {
                        lp = wp.Protocol + ":" + lp;
                    } else if (!lp.StartsWith("http"))
                    {
                        lp = wp.getProtocalAndHost() + lp;
                    }
                    refinedLinkedPages.Add(lp);
                }
            }
            
            return refinedLinkedPages;
        }
    }
}