using System;
using System.Linq;
using Common;
using Common.IO;
using Neon.Downloader.Enums;

namespace Neon.Downloader.Helpers
{
    public static class Extensions
    {
        public static string GetFileNameFrom(Uri uri)
        {
            string filename;
            filename = uri.LocalPath.Split('/').Last();
            filename = FileExts.ToSafeFileName(filename);
            return filename;
        }
        public static FileType GetFileTypeFrom(Uri uri)
        {
            string filename = GetFileNameFrom(uri);
            string ext = filename.Split('.').Last();

            if (ext.MatchesAny(Globals.VideoFileExts))
                return FileType.Video;
            else if (ext.MatchesAny("gif", "png", "jpg", "jpeg", "tif", "jpe", "bmp"))
                return FileType.Image;
            else if (ext.MatchesAny("pdf"))
                return FileType.Pdf;
            else if (ext.MatchesAny("mp3", "wma", "waw"))
                return FileType.Music;
            else if (ext.MatchesAny("zip", "rar", "tar", "gz", "tgz"))
                return FileType.Zipped;
            else if (ext.MatchesAny("docx", "doc"))
                return FileType.Document;
            else
                return FileType.Unknown;
        }
    }
}