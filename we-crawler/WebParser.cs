using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using HtmlAgilityPack;
using we_crawler.model;

namespace we_crawler
{
    public class WebParser
    {
        public static List<string> parse(Webpage wp)
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
                        lp = new Uri(wp.Url).Scheme + ":" + lp;
                    } else if (!lp.StartsWith("http"))
                    {
                        Uri uri = new Uri(wp.Url);
                        lp = uri.Scheme + "://" + uri.Host + lp;
                    }
                    refinedLinkedPages.Add(lp);
                }
            }
            
            return refinedLinkedPages;
        }
    }
}