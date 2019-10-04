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
Initialize the downloader and enjoy.

```c#
IDownloader _downloader = new DownloadClient();
_downloader.Download("http://example.com/video.mp4");
```
