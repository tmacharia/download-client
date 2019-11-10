using System;
using System.IO;
using System.Linq;
using Common;
using static System.Environment;

namespace Neon.Downloader
{
    public static class Globals
    {
        public const int MaxThreads = 4;
        public const int MaxParallelDownloads = 4;
        public const int PageSize = 1024 * 200;
        public static string RootFolder = GetFolderPath(SpecialFolder.LocalApplicationData);
        public static string DownloadFolder = $"{RootFolder}{Path.DirectorySeparatorChar}Downloads";
        public static string AppFolder = $"{RootFolder}";
        public static string SettingsFile = $"{AppFolder}{Path.DirectorySeparatorChar}settings.json";

        private const string videoFileExts = "mp4,mpg,mpeg,avi,mov,qt,wmv,mkv";

        public static string[] VideoFileExts = videoFileExts.Split(',');
        public static string[] VideoExts = SplitFileExts(videoFileExts);

        private static string[] SplitFileExts(string flatFile)
        {
            if (flatFile.IsValid())
                return flatFile.Split(',').Select(x => $".{x}").ToArray();
            return new string[0];
        }
    }

    public delegate void DownloadTraceEventHandler(string message);
    public delegate void DownloadEventHandler(DownloadMetric metric);
    public delegate void DownloadCompletedEventHandler(DownloadMetric metric, Stream stream);
    public delegate void DownloadErrorEventHandler(Exception ex);
}