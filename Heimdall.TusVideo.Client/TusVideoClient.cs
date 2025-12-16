namespace Heimdall.TusVideo.Client;

/// <summary>
/// Client for uploading videos using the TUS protocol
/// </summary>
public class TusVideoClient
{
    private readonly HttpClient _httpClient;
    private readonly string _uploadUrl;

    public TusVideoClient(HttpClient httpClient, string uploadUrl)
    {
        _httpClient = httpClient;
        _uploadUrl = uploadUrl;
    }

    /// <summary>
    /// Upload a video file using TUS protocol
    /// </summary>
    /// <param name="filePath">Path to the video file</param>
    /// <param name="fileName">Name of the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Upload URL</returns>
    public async Task<string> UploadVideoAsync(string filePath, string fileName, CancellationToken cancellationToken = default)
    {
        var fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        // Create upload
        var createRequest = new HttpRequestMessage(HttpMethod.Post, _uploadUrl);
        createRequest.Headers.Add("Tus-Resumable", "1.0.0");
        createRequest.Headers.Add("Upload-Length", fileInfo.Length.ToString());
        createRequest.Headers.Add("Upload-Metadata", $"name {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(fileName))}");

        var createResponse = await _httpClient.SendAsync(createRequest, cancellationToken);
        createResponse.EnsureSuccessStatusCode();

        var uploadUrl = createResponse.Headers.Location?.ToString() 
                        ?? throw new InvalidOperationException("Upload URL not returned");

        // Upload file
        await using var fileStream = File.OpenRead(filePath);
        var uploadRequest = new HttpRequestMessage(new HttpMethod("PATCH"), uploadUrl);
        uploadRequest.Headers.Add("Tus-Resumable", "1.0.0");
        uploadRequest.Headers.Add("Upload-Offset", "0");
        uploadRequest.Content = new StreamContent(fileStream);
        uploadRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/offset+octet-stream");

        var uploadResponse = await _httpClient.SendAsync(uploadRequest, cancellationToken);
        uploadResponse.EnsureSuccessStatusCode();

        return uploadUrl;
    }

    /// <summary>
    /// Get upload progress
    /// </summary>
    /// <param name="uploadUrl">Upload URL returned from CreateUpload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Upload offset (bytes uploaded)</returns>
    public async Task<long> GetUploadProgressAsync(string uploadUrl, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Head, uploadUrl);
        request.Headers.Add("Tus-Resumable", "1.0.0");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        if (response.Headers.TryGetValues("Upload-Offset", out var values))
        {
            return long.Parse(values.First());
        }

        return 0;
    }
}
