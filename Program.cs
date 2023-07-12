using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Drawing;

namespace WebPageAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter a URL: ");
            string url = Console.ReadLine();

            string htmlContent = GetWebpageContent(url);
            List<string> images = ExtractImages(htmlContent);
            DisplayImages(images);

            List<string> words = ExtractWords(htmlContent);
            DisplayWordStatistics(words);

            Console.ReadLine();
        }

        static string GetWebpageContent(string url)
        {
            using (WebClient client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }

        static List<string> ExtractImages(string html)
        {
            List<string> images = new List<string>();

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            foreach (HtmlNode imgNode in doc.DocumentNode.Descendants("img"))
            {
                string imgUrl = imgNode.GetAttributeValue("src", "");
                if (!string.IsNullOrEmpty(imgUrl))
                {
                    images.Add(imgUrl);
                }
            }

            return images;
        }

        static List<string> ExtractWords(string html)
        {
            List<string> words = new List<string>();

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            string text = doc.DocumentNode.InnerText;
            string[] splitWords = text.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in splitWords)
            {
                string cleanedWord = Regex.Replace(word, @"[^a-zA-Z0-9\s]", "").ToLower();
                words.Add(cleanedWord);
            }

            return words;
        }

        static void DisplayImages(List<string> images)
        {
            if (images.Count == 0)
            {
                Console.WriteLine("No images found.");
                return;
            }

            foreach (string imageUrl in images)
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        byte[] imageData = client.DownloadData(imageUrl);
                        using (MemoryStream stream = new MemoryStream(imageData))
                        {
                            Image img = Image.FromStream(stream);
                            Process.Start(imageUrl); // Open the image in the default viewer
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Failed to display image: " + imageUrl);
                }
            }
        }

        static void DisplayWordStatistics(List<string> words)
        {
            Dictionary<string, int> wordCounts = words.GroupBy(w => w)
                .ToDictionary(g => g.Key, g => g.Count());

            int totalWords = words.Count;
            Console.WriteLine("Total words: " + totalWords);

            var topWords = wordCounts.OrderByDescending(wc => wc.Value).Take(7);

            Console.WriteLine("Top 7 occurring words:");
            foreach (var pair in topWords)
            {
                Console.WriteLine($"{pair.Key}: {pair.Value}");
            }

            // Generate a bar chart
            var chart = new Chart();
            var chartArea = new ChartArea();
            chart.ChartAreas.Add(chartArea);

            var series = new Series();
            series.ChartType = SeriesChartType.Column;
            series.Points.DataBindXY(topWords.Select(wc => wc.Key), topWords.Select(wc => wc.Value));
            chart.Series.Add(series);

            var chartForm = new Form();
            var chartControl = new System.Windows.Forms.DataVisualization.Charting.Chart();
            chartControl.Dock = DockStyle.Fill;
            chartControl.ChartAreas.Add(chartArea);
            chartControl.Series.Add(series);

            chartForm.Controls.Add(chartControl);
            chartForm.Size = new Size(600, 400);
            chartForm.ShowDialog();
        }
    }
}
