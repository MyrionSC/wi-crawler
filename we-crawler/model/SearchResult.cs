namespace we_crawler.model
{
    public class SearchResult
    {
        public string Url;
        public double ContentRank;
        public double PageRank;
        public double TotalRank;

        public SearchResult(string url, double contentRank, double pageRank, double totalRank)
        {
            Url = url;
            ContentRank = contentRank;
            PageRank = pageRank;
            TotalRank = totalRank;
        }
    }
}