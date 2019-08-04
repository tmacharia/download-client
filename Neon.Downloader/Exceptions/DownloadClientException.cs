using System;

namespace Neon.Downloader.Exceptions
{
    public class DownloadClientException : Exception
    {
        public DownloadClientException()
            :base()
        {

        }
        public DownloadClientException(string message)
            :base(message)
        {

        }
        public DownloadClientException(string message, Exception innerException)
            :base(message, innerException)
        {

        }
    }
}