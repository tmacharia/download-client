using System;

namespace Neon.Downloader.Console
{
    class Program
    {
        private static IDownloader _downloader;
        static void Main(string[] args)
        {
            System.Console.WriteLine("Hello World!");
            _downloader = new DownloaderClient(250000);
            _downloader.OnDownloading += _downloader_OnDownloading;
            _downloader.OnError += _downloader_OnError;

            Start("http://91.165.88:8116/stream/1/");

            System.Console.ReadKey();
        }

        private static void _downloader_OnDownloading(DownloadMetric e)
        {
            System.Console.WriteLine(e.Progress.ToString("##.##") + "%");
        }

        private async static void Start(string url)
        {
            await _downloader.DownloadToFileAsync(url,"homeboyz.mp3", @"F:\downloads");
            string x = null;
        }
        private static void _downloader_OnError(Exception ex)
        {
            System.Console.WriteLine(ex.Message);
        }

        private static void _downloader_DownloadCompleted(DownloadMetric d)
        {
            System.Console.WriteLine("COMPLETE!!");
        }
    }
}
