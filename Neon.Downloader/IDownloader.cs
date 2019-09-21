using System;
using System.Threading;
using System.Threading.Tasks;
using Neon.Downloader.Enums;

namespace Neon.Downloader
{
    public interface IDownloader
    {
        /// <summary>
        /// Event handler or function/method to call passing along a <see cref="DownloadMetric"/>
        /// for when a download starts.
        /// </summary>
        event DownloadEventHandler OnDownloadStart;
        /// <summary>
        /// Event handler or function/method to call passing along a <see cref="DownloadMetric"/>
        /// for every time progress occurs during a download operation.
        /// </summary>
        event DownloadEventHandler OnDownloading;
        /// <summary>
        /// Event handler to call after a download operation successfully completes!
        /// </summary>
        event DownloadCompletedEventHandler DownloadCompleted;
        /// <summary>
        /// Event handler to call when an error occurs while processing a download 
        /// operation. This event passes along an <see cref="DownloadClientException"/>
        /// </summary>
        event DownloadErrorEventHandler OnError;

        /// <summary>
        /// Returns all the bytes read from a HTTP resource.
        /// </summary>
        /// <param name="uri">Path to the resource to download.</param>
        /// <returns>
        ///     A byte array of the contents read from the specified <paramref name="uri"/>
        /// </returns>
        byte[] Download(Uri uri);
        /// <summary>
        /// Returns all the bytes read from a HTTP resource.
        /// </summary>
        /// <param name="uri">Path to the resource to download.</param>
        /// <returns>
        ///     A byte array of the contents read from the specified <paramref name="uri"/>
        /// </returns>
        byte[] Download(Uri uri, CancellationToken cancellationToken);
        /// <summary>
        /// Returns all the bytes read from a HTTP resource.
        /// </summary>
        /// <param name="url">Path to the resource to download.</param>
        /// <returns>
        ///     A byte array of the contents read from the specified <paramref name="uri"/>
        /// </returns>
        byte[] Download(string url);
        /// <summary>
        /// Returns all the bytes read from a HTTP resource.
        /// </summary>
        /// <param name="url">Path to the resource to download.</param>
        /// <returns>
        ///     A byte array of the contents read from the specified <paramref name="uri"/>
        /// </returns>
        byte[] Download(string url, CancellationToken cancellationToken);
        /// <summary>
        /// Asynchronously reads and returns all the bytes read from a HTTP resource.
        /// </summary>
        /// <param name="uri">Path to the resource to download.</param>
        /// <returns>
        ///     An awaitable <see cref="Task"/> that finally returns the resource's content
        ///     as a byte array once the <see cref="Task"/> completes.
        /// </returns>
        Task<byte[]> DownloadAsync(Uri uri);
        /// <summary>
        /// Asynchronously reads and returns all the bytes read from a HTTP resource.
        /// </summary>
        /// <param name="uri">Path to the resource to download.</param>
        /// <returns>
        ///     An awaitable <see cref="Task"/> that finally returns the resource's content
        ///     as a byte array once the <see cref="Task"/> completes.
        /// </returns>
        Task<byte[]> DownloadAsync(Uri uri, CancellationToken cancellationToken);
        /// <summary>
        /// Asynchronously reads and returns all the bytes read from a HTTP resource.
        /// </summary>
        /// <param name="url">Path to the resource to download.</param>
        /// <returns>
        ///     An awaitable <see cref="Task"/> that finally returns the resource's content
        ///     as a byte array once the <see cref="Task"/> completes.
        /// </returns>
        Task<byte[]> DownloadAsync(string url);
        /// <summary>
        /// Asynchronously reads and returns all the bytes read from a HTTP resource.
        /// </summary>
        /// <param name="url">Path to the resource to download.</param>
        /// <returns>
        ///     An awaitable <see cref="Task"/> that finally returns the resource's content
        ///     as a byte array once the <see cref="Task"/> completes.
        /// </returns>
        Task<byte[]> DownloadAsync(string url, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously downloads the contents of a remote resource/file and saves it 
        /// to a Local file in the Local ApplicationData Folder with the name
        /// of the file found in the <paramref name="url"/>.
        /// 
        /// </summary>
        /// <param name="url">Path to the resource to download.</param>
        /// <param name="folderPath">Path to folder where to save the file.</param>
        void DownloadToFile(string url, string folderPath = null);
        /// <summary>
        /// Asynchronously downloads the contents of a remote resource/file and saves it 
        /// to a Local file in the Local ApplicationData Folder with the name
        /// of the file found in the <paramref name="url"/>.
        /// 
        /// </summary>
        /// <param name="url">Path to the resource to download.</param>
        /// <param name="folderPath">Path to folder where to save the file.</param>
        void DownloadToFile(string url, CancellationToken cancellationToken, string folderPath=null);
        /// <summary>
        /// Asynchronously downloads the contents of a remote resource/file and saves it
        /// to the specified Folder with a name extracted from the url.
        /// 
        /// </summary>
        /// <param name="url">Path to the resource to download.</param>
        /// <param name="folderPath">Path to folder where to save the file.</param>
        Task DownloadToFileAsync(string url, string folderPath);
        /// <summary>
        /// Asynchronously downloads the contents of a remote resource/file and saves it
        /// to a Local file in the Local ApplicationData Folder using the 
        /// <paramref name="filename"/> specified.
        /// 
        /// </summary>
        /// <param name="url">Path to the resource to download.</param>
        /// <param name="filename">
        ///     Name to use in saving the file or basically a path of where to save the file.
        /// </param>
        /// <param name="folderPath">Path to folder where to save the file.</param>
        Task DownloadToFileAsync(string url, string filename, string folderPath);
        /// <summary>
        /// Asynchronously downloads the contents of a remote resource/file and saves it
        /// to a Local file in the Local ApplicationData Folder using the 
        /// <paramref name="filename"/> specified.
        /// 
        /// </summary>
        /// <param name="url">Path to the resource to download.</param>
        /// <param name="filename">
        ///     Name to use in saving the file or basically a path of where to save the file.
        /// </param>
        /// <param name="folderPath">Path to folder where to save the file.</param>
        void DownloadToFile(string url, string filename, CancellationToken cancellationToken, string folderPath=null);
        /// <summary>
        /// Asynchronously downloads the contents of a remote resource/file and saves it 
        /// to a Local file in the Local ApplicationData Folder with the name
        /// of the file found in the <paramref name="uri"/>.
        /// 
        /// </summary>
        /// <param name="url">Path to the resource to download.</param>
        /// <param name="folderPath">Path to folder where to save the file.</param>
        void DownloadToFile(Uri uri, string folderPath = null);
        /// <summary>
        /// Asynchronously downloads the contents of a remote resource/file and saves it 
        /// to a Local file in the Local ApplicationData Folder with the name
        /// of the file found in the <paramref name="uri"/>.
        /// 
        /// </summary>
        /// <param name="url">Path to the resource to download.</param>
        /// <param name="folderPath">Path to folder where to save the file.</param>
        void DownloadToFile(Uri uri, CancellationToken cancellationToken, string folderPath=null);
        /// <summary>
        /// Asynchronously downloads the contents of a remote resource/file and saves it
        /// to a Local file in the Local ApplicationData Folder using the 
        /// <paramref name="filename"/> specified.
        /// 
        /// </summary>
        /// <param name="uri">Path to the resource to download.</param>
        /// <param name="filename">
        ///     Name to use in saving the file or basically a path of where to save the file.
        /// </param>
        /// <param name="folderPath">Path to folder where to save the file.</param>
        void DownloadToFile(Uri uri, string filename, string folderPath = null);
        /// <summary>
        /// Asynchronously downloads the contents of a remote resource/file and saves it
        /// to a Local file in the Local ApplicationData Folder using the 
        /// <paramref name="filename"/> specified.
        /// 
        /// </summary>
        /// <param name="uri">Path to the resource to download.</param>
        /// <param name="filename">
        ///     Name to use in saving the file or basically a path of where to save the file.
        /// </param>
        /// <param name="folderPath">Path to folder where to save the file.</param>
        void DownloadToFile(Uri uri, string filename, CancellationToken cancellationToken, string folderPath=null);

        string GetFileNameFrom(string url);
        string GetFileNameFrom(Uri uri);
        FileType GetFileTypeFrom(string url);
        FileType GetFileTypeFrom(Uri uri);
    }
}