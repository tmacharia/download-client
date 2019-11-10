# Download Client

[![Build status](https://ci.appveyor.com/api/projects/status/5rpvg2q10vc5so4v?svg=true)](https://ci.appveyor.com/project/tmacharia/download-client)
[![Nuget](https://img.shields.io/nuget/vpre/Download.Client.svg?logo=nuget&link=https://www.nuget.org/packages/Download.Client//left)](https://www.nuget.org/packages/Download.Client/)
![SDK Downloads on Nuget](https://img.shields.io/nuget/dt/Download.Client.svg?label=downloads&logo=nuget&link=https://www.nuget.org/packages/Download.Client//left)

.NET lightweight library for download operations offering download statistics/metrics and events while downloading.

### Usage

Install package from Nuget by running the following command.

**Package Manager Console**

```bash
Install-Package Download.Client
```
**.NET CLI**

```bash
dotnet add package Download.Client
```

Then go ahead import the library in your target class.

```c#
using Neon.Downloader;
```
Initialize a new instance of download client where you can optionally set the maximum size per download in the constructor, the default is 1GB.

```c#
IDownloader _downloader = new DownloadClient();
_downloader.DownloadTrace += (string trace) => // trace event logs
_downloader.OnDownloadStart += (DownloadMetric metric) => // indicates that download has started
_downloader.DownloadCompleted += (DownloadMetric metric, Stream stream) => // last metric with downloaded stream
_downloader.OnError += (Exception ex) => // when an error occurs
_downloader.Download("http://example.com/video.mp4");
```

### DownloadMetric

A time-series representation metric object that defines the state of a download operation/activity either before, during download, or after the download completes.

This is how each download metric looks like

```c#
    public struct DownloadMetric
    {
        /// <summary>
        /// Checks whether the time measurement/calculation unit is seconds
        /// or milliseconds. 
        /// 
        /// Returns <see cref="true"/> if it's using seconds
        /// and <see cref="false"/> if it uses milliseconds.
        /// </summary>
        public bool TimeInSeconds { get; }
        /// <summary>
        /// Received/Downloaded Bytes. (d)
        /// </summary>
        public long DownloadedBytes { get; set; }
        /// <summary>
        /// Total Bytes to download. (b)
        /// </summary>
        public long TotalBytes { get; set; }
        /// <summary>
        /// Download progress percentage. (p)
        /// 
        /// <code>
        ///     = (d / b) * 100;
        /// </code>
        /// </summary>
        public double Progress { get; }
        /// <summary>
        /// Remaining Bytes to complete download. (r)
        /// 
        /// <code>
        ///     = (b - d);
        /// </code>
        /// </summary>
        public long RemainingBytes { get; }
        /// <summary>
        /// Checks if the download is complete by looking at the size
        /// of remaining/pending bytes.
        /// </summary>
        public bool IsComplete { get; }
        /// <summary>
        /// Elapsed time at the current moment. (t)
        /// </summary>
        public TimeSpan ElapsedTime { get; set; }
        /// <summary>
        /// Download speed. (s)
        /// 
        /// <code>
        ///     = (d / t);
        /// </code>.
        /// 
        /// The result is in (bytes/sec) or (bytes/ms) depending on the unit
        /// chosen for representing time.
        /// </summary>
        public double Speed { get; }
        /// <summary>
        /// Expiration or time remaining for download to complete. (e)
        /// 
        /// <code>
        ///     = (r / s);
        /// </code>
        /// </summary>
        public TimeSpan TimeRemaining { get; }
    }
```