using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using AngleSharp;
using AngleSharp.Html.Parser;

namespace GenericSiteCrawlerLibrary
{
    public class Crawler
    {
        private string rootUrl;
        private Action<Uri, string> customAction;
        public Dictionary<string, bool> visitedUrls;

        public Crawler(string rootUrl, Action<Uri, string> customAction)
        {
            this.rootUrl = rootUrl;
            this.customAction = customAction;
            visitedUrls = new Dictionary<string, bool>();
        }

        public void Crawl()
        {
            Uri uri = new Uri(rootUrl);
            CrawlPage(uri);
        }

        private void CrawlPage(Uri uri)
        {
            string absoluteUri = uri.AbsoluteUri;

            if (visitedUrls.ContainsKey(absoluteUri) && visitedUrls[absoluteUri])
                return;

            visitedUrls[absoluteUri] = true;

            try
            {
                WebClient client = new WebClient();
                string html = client.DownloadString(uri);

                customAction(uri, html);

                var config = Configuration.Default;
                var context = BrowsingContext.New(config);
                var parser = context.GetService<IHtmlParser>();

                var document = parser.ParseDocument(html);
                
                foreach (var link in document.QuerySelectorAll("a[href]"))
                {
                    string linkUrl = link.GetAttribute("href");
                    if (Uri.TryCreate(uri, linkUrl, out Uri newUri))
                    {
                        if (newUri.Host == uri.Host)
                           CrawlPage(newUri);
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error crawling {uri.AbsoluteUri}: {ex.Message}");
            }
        }
    }
}
