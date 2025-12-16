# Heimdall - TUS Video Upload Solution

This is an Application which will monitor everything

## Solution Structure

This repository contains a .NET 10 solution for handling video uploads using the TUS (Tus Upload Protocol) protocol. The solution consists of two projects:

### Projects

1. **Heimdall.TusVideo.Server** - ASP.NET Core Web API
   - TUS protocol server implementation for handling resumable video uploads
   - Built with .NET 10.0
   - Uses `tusdotnet` library for TUS protocol support
   - Stores uploaded videos in the `uploads` directory

2. **Heimdall.TusVideo.Client** - Class Library
   - TUS protocol client library for uploading videos
   - Built with .NET 10.0
   - Provides `TusVideoClient` class for easy video uploads
   - Supports upload progress tracking

## Getting Started

### Prerequisites
- .NET 10 SDK

### Building the Solution
```bash
dotnet build Heimdall.TusVideo.sln
```

### Running the Server
```bash
cd Heimdall.TusVideo.Server
dotnet run
```

The server will start and listen on the configured ports (default: http://localhost:5000 and https://localhost:5001).

### Using the Client
```csharp
using Heimdall.TusVideo.Client;

var httpClient = new HttpClient();
var client = new TusVideoClient(httpClient, "http://localhost:5000/files");

// Upload a video
var uploadUrl = await client.UploadVideoAsync("path/to/video.mp4", "video.mp4");
Console.WriteLine($"Upload complete: {uploadUrl}");

// Check upload progress
var progress = await client.GetUploadProgressAsync(uploadUrl);
Console.WriteLine($"Uploaded: {progress} bytes");
```

## TUS Protocol

TUS (Tus Upload Protocol) is an open protocol for resumable file uploads. It's particularly useful for:
- Large file uploads (like videos)
- Unreliable network connections
- Resuming interrupted uploads
- Progress tracking

Learn more at: https://tus.io/
 
