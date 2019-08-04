using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Neon.Downloader.Enums;
using Neon.Downloader.Exceptions;

namespace Neon.Downloader
{
    public class DownloaderClient : IDownloader
    {
        private readonly DownloaderSettings _settings;
        private readonly HttpClient _client;
        private readonly long _maxDowloadSize;
        /// <summary>
        /// Instanciates download client with a max download size;
        /// </summary>
        /// <param name="maxDownloadSizeInBytes">Maximum download limit in bytes. Default is 10 MB.</param>
        public DownloaderClient(long maxDownloadSizeInBytes=10000000)
        {
            _maxDowloadSize = maxDownloadSizeInBytes;
            //Ignore bad certificate in .NET core 2.0
            var httpClientHandler = new HttpClientHandler {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
            };
            _settings = new DownloaderSettings();
            _client = new HttpClient(httpClientHandler);
        }

        public event DownloadEventHandler OnDownloading;
        public event DownloadCompletedEventHandler DownloadCompleted;
        public event DownloadErrorEventHandler OnError;

        public byte[] Download(Uri uri, CancellationToken cancellationToken)
        {
            return InternalDownloadAsync(uri, cancellationToken).Result;
        }

        public byte[] Download(string url, CancellationToken cancellationToken)
        {
            return InternalDownloadAsync(new Uri(url), cancellationToken).Result;
        }

        public Task<byte[]> DownloadAsync(Uri uri, CancellationToken cancellationToken)
        {
            return InternalDownloadAsync(uri, cancellationToken);
        }

        public Task<byte[]> DownloadAsync(string url, CancellationToken cancellationToken)
        {
            return InternalDownloadAsync(new Uri(url), cancellationToken);
        }
        public async void DownloadToFile(string url, CancellationToken cancellationToken, string folderPath=null)
        {
            _ = await InternalDownloadAsync(new Uri(url), cancellationToken, true, null, folderPath);
        }

        public async void DownloadToFile(string url, string filename, CancellationToken cancellationToken, string folderPath = null) {
            _ = await InternalDownloadAsync(new Uri(url), cancellationToken, true, filename, folderPath);
        }

        public async void DownloadToFile(Uri uri, CancellationToken cancellationToken, string folderPath = null)
        {
            _ = await InternalDownloadAsync(uri, cancellationToken, true, null, folderPath);
        }

        public async void DownloadToFile(Uri uri, string filename, CancellationToken cancellationToken, string folderPath = null)
        {
            _ = await InternalDownloadAsync(uri, cancellationToken, true, filename, folderPath);
        }


        internal async Task<byte[]> InternalDownloadAsync(Uri uri, CancellationToken cancellationToken, bool saveToDisk=false, string filename=null, string folderPath=null)
        {
            byte[] vs = new byte[0];
            try
            {
                HttpResponseMessage httpResponse = await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

                if (httpResponse.IsSuccessStatusCode)
                {
                    long? length = httpResponse.Content.Headers.ContentLength;
                    length = length.HasValue ? length 
                        : length.Value < _maxDowloadSize ? length
                        : _maxDowloadSize;
                    /*__________________________________________________________________________________
                      |                                                                                |
                      |  .NET runtime has a 2GB size limit for objects.                                |
                      |  ----------------------------------------------                                |
                      |  To adhere to this restriction, this module ONLY allows downloading files      |
                      |  less than 1GB. If the file is greater than 1GB, call DownloadToFile method    |
                      |  instead which downloads the file directly to disk or allow this application   |
                      |  to automatically save the file to disk for you.                               |
                      |  ----------------------------------------------                                |
                      |  1 GB = 1,000,000,000 (1 Billion Bytes).                                       |
                      |                                                                                |
                     *|________________________________________________________________________________|*/

                    long GB_bytes_size = 1000000000;

                    vs = (length >= GB_bytes_size || saveToDisk) ?
                        await ReadHttpResponseStreamAsync(httpResponse, length, cancellationToken, true, filename, folderPath) :
                        await ReadHttpResponseStreamAsync(httpResponse, length, cancellationToken, saveToDisk, null,folderPath);
                }
                else
                {
                    string message = await httpResponse.Content.ReadAsStringAsync();
                    OnError(new DownloadClientException("An error occured. Http request responded with a non 200-OK StatusCode.",
                        new HttpRequestException(message)));
                }
            }
            catch (OperationCanceledException e)
            {
                OnError?.Invoke(new DownloadClientException($"{nameof(OperationCanceledException)} thrown with message: {e.Message}"));
                return null;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(new DownloadClientException("Download operation failed & threw an exception. " +
                    "See inner exception for further details.", ex));
            }
            return vs;
        }

        internal async Task<byte[]> ReadHttpResponseStreamAsync(HttpResponseMessage httpResponse, long? length, CancellationToken ct, bool saveToFile=false, string filename=null, string folderPath=null)
        {
            return await Task.Run(async () =>
            {
                // Were we already canceled?
                ct.ThrowIfCancellationRequested();
                using (StreamReader sr = new StreamReader(await httpResponse.Content.ReadAsStreamAsync()))
                {
                    DownloadMetric metric = new DownloadMetric(length);
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    if (!saveToFile)
                        return FromReaderToStream(sr, new MemoryStream((int)length), ref metric, ref stopwatch, ref length, ct);
                    else
                    {
                        filename = ToRelativeFilePath(httpResponse.RequestMessage.RequestUri, filename, folderPath);

                        return FromReaderToStream(sr, File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite),
                            ref metric, ref stopwatch, ref length, ct);
                    }
                }
            }, ct);
        }

        internal byte[] FromReaderToStream(StreamReader sr, Stream destinationStream,
            ref DownloadMetric metric, ref Stopwatch stopwatch, ref long? length, CancellationToken ct)
        {
            var buffer = new byte[1024];
            int bytesRead;
            while ((bytesRead = sr.BaseStream.Read(buffer, 0, buffer.Length)) > 0 && !metric.IsComplete)
            {
                // Poll on this property if you have to do
                // other cleanup before throwing.
                if (ct.IsCancellationRequested)
                {
                    // Clean up here, then...
                    destinationStream.SetLength(0);
                    destinationStream.Close();
                    if(destinationStream is FileStream)
                    {
                        var fs = destinationStream as FileStream;
                        if (File.Exists(fs.Name))
                            File.Delete(fs.Name);
                    }
                    ct.ThrowIfCancellationRequested();
                }

                destinationStream.Write(buffer, 0, bytesRead);
                metric.DownloadedBytes += bytesRead;
                metric.ElapsedTime = stopwatch.Elapsed;
                OnDownloading(metric);
            }
            stopwatch.Stop();
            stopwatch.Reset();
            DownloadCompleted(metric);
            
            if(destinationStream is MemoryStream) {
                return ((MemoryStream)destinationStream).ToArray();
            }else {
                destinationStream.Flush();
                destinationStream.Close();
                destinationStream.Dispose();
                return null;
            }
        }

        internal string ToRelativeFilePath(Uri uri, string filename = null, string folderPath = null)
        {
            string path;
            string folder = folderPath.IsValid() ? folderPath : _settings.DefaultDownloadFolder;
            if (filename.IsValid())
            {
                /* First, check if file extensions match. If they don't, replace
                 * the file extension supplied by the user with the extension from
                 * the uri.
                 * 
                 ***************************************/
                string uri_ext = uri.OriginalString.Split('.').Last();
                string file_ext = filename.Split('.').Last();

                if (!file_ext.Equals(uri_ext))
                    filename = filename.Replace(file_ext, uri_ext);

                if(new Uri(filename).IsAbsoluteUri)
                {
                    path = Path.Combine(folder, filename);
                }
                else
                {
                    path = filename;
                }
            }
            else
            {
                path = Path.Combine(folder, GetFileNameFrom(uri));
            }

            /* Creates all directories and sub-directories in the specified path
             * unless they already exist.
             * 
             *****************************************/
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return path;
        }

        public string GetFileNameFrom(string url) => GetFileNameFrom(new Uri(url));
        public string GetFileNameFrom(Uri uri) => Helpers.Extensions.GetFileNameFrom(uri);
        public FileType GetFileTypeFrom(string url) => GetFileTypeFrom(new Uri(url));
        public FileType GetFileTypeFrom(Uri uri) => Helpers.Extensions.GetFileTypeFrom(uri);
    }
}