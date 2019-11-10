using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Neon.Downloader.Enums;
using Neon.Downloader.Exceptions;

namespace Neon.Downloader
{
    public class DownloaderClient : IDownloader
    {
        private const long MaxSize = 1000000 * 1000;
        private readonly HttpClient _client;
        private readonly long _maxDowloadSize;
        private readonly CancellationToken nullToken = CancellationToken.None;
        /// <summary>
        /// Instanciates download client with a max download size of 1GB.
        /// </summary>
        public DownloaderClient()
        {
            _maxDowloadSize = MaxSize;
            //Ignore bad certificate in .NET core 2.0
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
            };
            _client = new HttpClient(httpClientHandler);
        }
        /// <summary>
        /// Instanciates download client with a max download size;
        /// </summary>
        /// <param name="maxDownloadSizeInBytes">Maximum download limit in bytes. Default is 500 MB.</param>
        public DownloaderClient(long maxDownloadSizeInBytes=MaxSize) :this()
        {
            _maxDowloadSize = maxDownloadSizeInBytes;
        }

        public event DownloadEventHandler OnDownloading;
        public event DownloadEventHandler OnDownloadStart;
        public event DownloadErrorEventHandler OnError;
        public event DownloadCompletedEventHandler DownloadCompleted;

        public byte[] Download(Uri uri)
        {
            return InternalDownloadAsync(uri, nullToken).Result;
        }
        public byte[] Download(string url)
        {
            return InternalDownloadAsync(new Uri(url), nullToken).Result;
        }
        public Task<byte[]> DownloadAsync(Uri uri)
        {
            return InternalDownloadAsync(uri, nullToken);
        }
        public Task<byte[]> DownloadAsync(string url)
        {
            return InternalDownloadAsync(new Uri(url), nullToken);
        }

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

        public void DownloadToFile(string url)
        {
            DownloadToFile(url, null);
        }
        public async void DownloadToFile(string url, string folderPath)
        {
            await InternalDownloadAsync(new Uri(url), nullToken, true, null, folderPath);
        }
        public Task DownloadToFileAsync(string url, string folderPath)
        {
            return InternalDownloadAsync(new Uri(url), nullToken, true, null, folderPath);
        }
        public Task<byte[]> DownloadToFileAsync(string url, string folder, string filename, CancellationToken ct)
        {
            return InternalDownloadAsync2(new Uri(url), folder, filename, ct);
        }
        public Task<byte[]> DownloadToFileAsync(string url, Stream output, CancellationToken ct) => InternalDownloadAsync2(new Uri(url), output,ct);
        public void DownloadToFile(Uri uri)
        {
            DownloadToFile(uri, null);
        }
        public void DownloadToFile(Uri uri, string folderPath)
        {
            DownloadToFile(uri, null, folderPath);
        }
        public async void DownloadToFile(Uri uri, string filename, string folderPath)
        {
            await InternalDownloadAsync(uri, nullToken, true, filename, folderPath);
        }

        public async void DownloadToFile(string url, CancellationToken cancellationToken, string folderPath=null)
        {
            await InternalDownloadAsync(new Uri(url), cancellationToken, true, null, folderPath);
        }
        public async void DownloadToFile(string url, string filename, CancellationToken cancellationToken, string folderPath = null) {
            await InternalDownloadAsync(new Uri(url), cancellationToken, true, filename, folderPath);
        }
        public async void DownloadToFile(Uri uri, CancellationToken cancellationToken, string folderPath = null)
        {
            await InternalDownloadAsync(uri, cancellationToken, true, null, folderPath);
        }
        public async void DownloadToFile(Uri uri, string filename, CancellationToken cancellationToken, string folderPath = null)
        {
            await InternalDownloadAsync(uri, cancellationToken, true, filename, folderPath);
        }


        internal async Task<byte[]> InternalDownloadAsync(Uri uri, CancellationToken cancellationToken, bool saveToDisk=false, string filename=null, string folderPath=null)
        {
            byte[] vs = Array.Empty<byte>();
            try
            {
                HttpResponseMessage httpResponse = await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

                if (httpResponse.IsSuccessStatusCode)
                {
                    long? length = httpResponse.Content.Headers.ContentLength;
                    if (length == null)
                        length = _maxDowloadSize;
                    else
                    {
                        if (length.Value > _maxDowloadSize)
                            length = _maxDowloadSize;
                    }
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
                    OnError(new DownloadClientException("Non 200-OK StatusCode Received. See inner exception for details",
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
                OnError?.Invoke(new DownloadClientException("Download failed. See inner exception for details ", ex));
            }
            return vs;
        }
        public Task<byte[]> InternalDownloadAsync2(Uri uri, string folder, string filename, CancellationToken ct)
        {
            return InternalDownloadAsync2(uri, File.Open(Path.Combine(folder, filename), FileMode.OpenOrCreate, FileAccess.Write), ct);
        }
        public async Task<byte[]> InternalDownloadAsync2(Uri uri, Stream output, CancellationToken ct)
        {
            byte[] vs = Array.Empty<byte>();
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                #region Get file size  
                WebRequest webRequest = WebRequest.Create(uri);
                webRequest.Method = "HEAD";
                long? bytes;
                Stopwatch watch = new Stopwatch();
                watch.Start();
                DownloadMetric m = new DownloadMetric();
                using (WebResponse webResponse = webRequest.GetResponse())
                {
                    bytes = long.Parse(webResponse.Headers.Get("Content-Length"));
                    m.TotalBytes = bytes.Value;
                    m.ElapsedTime = watch.Elapsed;
                    OnDownloadStart?.Invoke(m);
                }
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
                if (bytes == null)
                    bytes = _maxDowloadSize;
                else
                {
                    if (bytes.Value > _maxDowloadSize)
                        bytes = _maxDowloadSize;
                }
                #endregion

                long? l = bytes;
                HttpWebRequest req = WebRequest.Create(uri) as HttpWebRequest;
                req.Method = "GET";
                req.AddRange(0, bytes.Value);
                vs = await Task.Run(async () =>
                {
                    using (StreamReader sr = new StreamReader((await req.GetResponseAsync()).GetResponseStream()))
                    {
                        var a = FromReaderToStream(sr, output, ref m, ref watch, ref l, ct);
                        DownloadCompleted?.Invoke(m, null);
                        return a;
                    }
                }, ct);
            }
            catch (OperationCanceledException)
            {
                OnError?.Invoke(new DownloadClientException($"Download cancelled by user."));
            }
            catch (Exception ex)
            {
                OnError?.Invoke(new DownloadClientException("Download failed. See inner exception for details ", ex));
            }
            return vs;
        }

        public Task<DownloadResult> ProcessParallel(Uri uri, long bytes, int threads, string destinationFilePath, DownloadResult result)
        {
            return Task.Run(() =>
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                DownloadMetric m = new DownloadMetric();
                using (FileStream destinationStream = new FileStream(destinationFilePath, FileMode.Append))
                {
                    ConcurrentDictionary<int, string> temps = new ConcurrentDictionary<int, string>();

                    #region Calculate ranges  
                    List<Range> ranges = new List<Range>();
                    for (int i = 0; i < threads - 1; i++)
                    {
                        var range = new Range()
                        {
                            Start = i * (bytes / threads),
                            End = ((i + 1) * (bytes / threads)) - 1
                        };
                        ranges.Add(range);
                    }
                    ranges.Add(new Range()
                    {
                        Start = ranges.Any() ? ranges.Last().End + 1 : 0,
                        End = bytes - 1
                    });
                    #endregion

                    #region Parallel download  
                    int index = 0;
                    Parallel.ForEach(ranges, new ParallelOptions() { MaxDegreeOfParallelism = threads }, (range) =>
                    {
                        long? range_length = range.End - range.Start;
                        HttpWebRequest httpWebRequest = WebRequest.Create(uri) as HttpWebRequest;
                        httpWebRequest.Method = "GET";
                        httpWebRequest.AddRange(range.Start, range.End);
                        using (StreamReader sr = new StreamReader(httpWebRequest.GetResponse().GetResponseStream()))
                        {
                            string tempFilePath = Path.GetTempFileName();
                            using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.Write))
                            {
                                FromReaderToStream(sr, fileStream,
                                        ref m, ref watch, ref range_length, CancellationToken.None);
                                temps.TryAdd(index, tempFilePath);
                            }
                        }
                    });
                    result.ParallelDownloads = index;
                    #endregion

                    result.TimeTaken = watch.Elapsed;

                    #region Merge to single file  
                    foreach (var tempFile in temps.OrderBy(b => b.Key))
                    {
                        byte[] tempFileBytes = File.ReadAllBytes(tempFile.Value);
                        destinationStream.Write(tempFileBytes, 0, tempFileBytes.Length);
                        File.Delete(tempFile.Value);
                    }
                    #endregion
                    return result;
                }
            });
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
                    OnDownloadStart?.Invoke(metric);
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
            //int kb = metric.Speed > Globals.PageSize ? (int)metric.Speed : Globals.PageSize;
            int kb = 1024*20;
            int toDownload = kb;
            var buffer = new byte[toDownload];
            int bytesRead;

            while ((bytesRead = sr.BaseStream.Read(buffer, 0, buffer.Length)) > 0){
                // Poll on this property if you have to do
                // other cleanup before throwing.
                if (ct.IsCancellationRequested)
                {
                    // Clean up here, then...
                    destinationStream.SetLength(0);
                    destinationStream.Close();
                    if (destinationStream is FileStream)
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
                OnDownloading?.Invoke(metric);

                length -= bytesRead;
                toDownload = length >= kb ? kb : (int)length;
                buffer = new byte[toDownload];
            }
            stopwatch.Stop();
            //DownloadCompleted?.Invoke(metric, destinationStream);
            //stopwatch.Reset();
            
            if(destinationStream is MemoryStream) {
                return ((MemoryStream)destinationStream).ToArray();
            }
            else {
                destinationStream.Flush();
                destinationStream.Close();
                destinationStream.Dispose();
                return null;
            }
        }
        internal string ToRelativeFilePath(Uri uri, string filename = null, string folderPath = null)
        {
            string path;
            string folder = folderPath.IsValid() ? folderPath : Globals.DownloadFolder;
            if (filename.IsValid())
            {
                /* First, check if file extensions match. If they don't, replace
                 * the file extension supplied by the user with the extension from
                 * the uri.
                 * 
                 ***************************************/
                if (!filename.Contains('.'))
                {
                    string uri_ext = uri.OriginalString.Split('/').Last().Split('.').Last();
                    string file_ext = filename.Split('.').Last();

                    if (!uri_ext.IsValid())
                    {
                        if (!file_ext.IsValid())
                            throw new ArgumentException("No file extension specified.");
                        else
                            path = folder.IsValid() ? Path.Combine(folder, filename) : filename;
                    }
                    else
                    {
                        if (!file_ext.Equals(uri_ext))
                            filename = filename.Replace(file_ext, uri_ext);
                        if (new Uri(filename).IsAbsoluteUri)
                            path = folder.IsValid() ? Path.Combine(folder, filename) : filename;
                        else
                            path = filename;
                    }
                }
                else
                {
                    path = folder.IsValid() ? Path.Combine(folder, filename) : filename;
                }
            }
            else
            {
                path = folder.IsValid() ? Path.Combine(folder, filename) : filename;
            }

            /* Creates all directories and sub-directories in the specified path
             * unless they already exist.
             * 
             *****************************************/
            if (folder.IsValid()) {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }

            return path;
        }
        public string GetFileNameFrom(string url) => GetFileNameFrom(new Uri(url));
        public string GetFileNameFrom(Uri uri) => Helpers.Extensions.GetFileNameFrom(uri);
        public FileType GetFileTypeFrom(string url) => GetFileTypeFrom(new Uri(url));
        public FileType GetFileTypeFrom(Uri uri) => Helpers.Extensions.GetFileTypeFrom(uri);
    }
    public class DownloadResult
    {
        public long Size { get; set; }
        public String FilePath { get; set; }
        public TimeSpan TimeTaken { get; set; }
        public int ParallelDownloads { get; set; }
    }
    internal class Range
    {
        public Range()
        { }
        public Range(long start, long end) :this()
        {
            Start = start;
            End = end;
        }
        public long Start { get; set; }
        public long End { get; set; }
        public long Length => End - Start;
    }
}