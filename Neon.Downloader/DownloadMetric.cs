using System;

namespace Neon.Downloader
{
    /// <summary>
    /// A time-series representation metric object that defines the state
    /// of a download operation/activity either before, during download, or
    /// after the download completes.
    /// 
    /// </summary>
    public struct DownloadMetric
    {
        /// <summary>
        /// Initializes a download metric.
        /// </summary>
        /// <param name="timeInSeconds">
        ///     Perform calculation using time in seconds or in milliseconds?
        ///     
        ///     To use seconds: <see cref="true"/>
        ///     To use milliseconds: <see cref="false"/>
        /// </param>
        public DownloadMetric(long? totalBytes=null, bool timeInSeconds=true)
        {
            TimeInSeconds = timeInSeconds;
            DownloadedBytes = 0;
            TotalBytes = totalBytes ?? int.MaxValue;
            ElapsedTime = new TimeSpan();
        }
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
        public double Progress
        {
            get
            {
                try
                {
                    return (DownloadedBytes / (double)TotalBytes) * 100;
                }
                catch (DivideByZeroException)
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// Remaining Bytes to complete download. (r)
        /// 
        /// <code>
        ///     = (b - d);
        /// </code>
        /// </summary>
        public long RemainingBytes => (TotalBytes - DownloadedBytes);
        /// <summary>
        /// Checks if the download is complete by looking at the size
        /// of remaining/pending bytes.
        /// </summary>
        public bool IsComplete => RemainingBytes <= 0;
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
        public double Speed
        {
            get
            {
                try
                {
                    return TimeInSeconds ?
                    (DownloadedBytes / ElapsedTime.TotalSeconds) :
                    (DownloadedBytes / ElapsedTime.TotalMilliseconds);
                }
                catch (DivideByZeroException)
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// Expiration or time remaining for download to complete. (e)
        /// 
        /// <code>
        ///     = (r / s);
        /// </code>
        /// </summary>
        public TimeSpan TimeRemaining
        {
            get
            {
                try
                {
                    double d = RemainingBytes / Speed;
                    return TimeInSeconds ? TimeSpan.FromSeconds(d) : TimeSpan.FromMilliseconds(d);
                }
                catch (DivideByZeroException)
                {
                    return TimeSpan.FromSeconds(0);
                }
                catch (Exception)
                {
                    return new TimeSpan();
                }
            }
        }
    }
}