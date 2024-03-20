using GenericSiteCrawlerLibrary;
using System.Windows.Forms;

namespace GenericSiteCrawlerGUI
{
    public partial class GenericSiteCrawlerForm : Form
    {
        private FolderBrowserDialog folderBrowserDialog;
        private TextBox urlTextBox;
        private TextBox outputFolderTextBox;
        private Button browseButton;
        private Button startButton;
        private ProgressBar progressBar;

        public GenericSiteCrawlerForm()
        {
            InitializeComponent();
            InitializeControls();
        }

        private void InitializeControls()
        {
            folderBrowserDialog = new FolderBrowserDialog();

            urlTextBox = new TextBox();
            urlTextBox.PlaceholderText = "https://siteToBeCrawled.com";
            urlTextBox.Location = new System.Drawing.Point(125, 50);
            urlTextBox.Size = new System.Drawing.Size(300, 30);

            outputFolderTextBox = new TextBox();
            outputFolderTextBox.PlaceholderText = "Path to save files";
            outputFolderTextBox.Location = new System.Drawing.Point(125, 100);
            outputFolderTextBox.Size = new System.Drawing.Size(300, 30);

            browseButton = new Button();
            browseButton.Location = new System.Drawing.Point(225, 135);
            browseButton.Size = new System.Drawing.Size(80, 30);
            browseButton.Text = "Browse";
            browseButton.Click += new EventHandler(browseButton_Click);

            startButton = new Button();
            startButton.Location = new System.Drawing.Point(225, 190);
            startButton.Size = new System.Drawing.Size(80, 40);
            startButton.Text = "Start";
            startButton.Click += new EventHandler(startButton_ClickAsync);

            progressBar = new ProgressBar();
            progressBar.Location = new System.Drawing.Point(125, 200);
            progressBar.Size = new System.Drawing.Size(300, 20);
            progressBar.Style = ProgressBarStyle.Marquee; 
            progressBar.MarqueeAnimationSpeed = 50;
            progressBar.Visible = false;

            Controls.Add(urlTextBox);
            Controls.Add(outputFolderTextBox);
            Controls.Add(browseButton);
            Controls.Add(startButton);
            Controls.Add(progressBar);
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                outputFolderTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private async void startButton_ClickAsync(object sender, EventArgs e)
        {
            string rootUrl = urlTextBox.Text;
            string outputFolder = outputFolderTextBox.Text;

            if (string.IsNullOrWhiteSpace(rootUrl) || string.IsNullOrWhiteSpace(outputFolder))
            {
                MessageBox.Show("Please provide a valid URL.");
                return;
            }

            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                MessageBox.Show("Please provide a valid output folder.");
                return;
            }

            startButton.Visible = false;
            progressBar.Visible = true;

            try
            {
                if (!rootUrl.Contains("http"))
                    rootUrl = $@"https://" + rootUrl;

                Uri myUri = new Uri(rootUrl);
                string host = myUri.Host;
                outputFolder += $@"\SitesCrawled\" + host;

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

                await Task.Run(() =>
                {
                    crawler.Crawl();
                });


                var totalPagesVisited = crawler.visitedUrls.Where(l => l.Value).Count();


                startButton.Visible = true;
                progressBar.Visible = false;

                MessageBox.Show($"Crawling complete. A total of {totalPagesVisited} were visited.");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Please provide a valid URL.");
                return;
            }

        }
    }
}
