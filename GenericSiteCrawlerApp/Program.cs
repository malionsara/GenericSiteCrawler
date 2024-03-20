using GenericSiteCrawlerLibrary;


//string rootUrl = "https://softsolution.al/";
//string rootUrl = "https://facebook.com/";

Console.WriteLine("Please provide url of site to be crawled. \n");

var rootUrl = Console.ReadLine();

Console.WriteLine("\nStarted crawling. \n");


Uri myUri = new Uri(rootUrl);
string host = myUri.Host;
string outputFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $@"\SitesCrawled\" + host;

if (!Directory.Exists(outputFolder))
    Directory.CreateDirectory(outputFolder);

//In our case the custom action will be saving crawled pages as local files
Action<Uri, string> SaveCrawledPageAsHtml = (uri, html) =>
{
    string fileName = uri.AbsolutePath.Replace("/", "_").TrimStart('_').TrimEnd('_');
    fileName = fileName == "" ? "index.html" : fileName + ".html";

    string filePath = Path.Combine(outputFolder, fileName);
    File.WriteAllText(filePath, html);

    Console.WriteLine($"Saved: {uri.AbsoluteUri} -> {filePath}");
};

Crawler crawler = new(rootUrl, SaveCrawledPageAsHtml);

crawler.Crawl();

var totalPagesVisited = crawler.visitedUrls.Where(l => l.Value).Count();

Console.WriteLine("\nCrawling complete.\n");

Console.WriteLine($"Visited a total of {totalPagesVisited}.");

