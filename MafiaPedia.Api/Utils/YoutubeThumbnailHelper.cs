using System.Text.RegularExpressions;

namespace MafiaPedia.Api.Utils;

public static class YoutubeThumbnailHelper
{
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(5) };

    public static string? ExtractVideoId(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        var match = Regex.Match(url, @"youtube\.com/watch\?.*?v=([^&#]+)");
        if (match.Success) return match.Groups[1].Value;

        match = Regex.Match(url, @"youtu\.be/([^&#]+)");
        if (match.Success) return match.Groups[1].Value;

        match = Regex.Match(url, @"youtube\.com/embed/([^&#]+)");
        if (match.Success) return match.Groups[1].Value;

        match = Regex.Match(url, @"youtube\.com/shorts/([^&#]+)");
        if (match.Success) return match.Groups[1].Value;

        return null;
    }

    public static async Task<string?> DownloadThumbnailAsync(string videoId, string uploadsDir, ILogger logger)
    {
        var fileName = $"{Guid.NewGuid()}.jpg";
        var filePath = Path.Combine(uploadsDir, fileName);

        byte[]? bytes = null;

        try
        {
            var maxResUrl = $"https://i.ytimg.com/vi/{videoId}/maxresdefault.jpg";
            var response = await _httpClient.GetAsync(maxResUrl);
            if (response.IsSuccessStatusCode)
                bytes = await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to download maxresdefault thumbnail for video {VideoId}", videoId);
        }

        if (bytes is null || bytes.Length < 1000)
        {
            try
            {
                var hqUrl = $"https://i.ytimg.com/vi/{videoId}/hqdefault.jpg";
                var response = await _httpClient.GetAsync(hqUrl);
                if (response.IsSuccessStatusCode)
                    bytes = await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to download hqdefault thumbnail for video {VideoId}", videoId);
            }
        }

        if (bytes is null || bytes.Length < 1000)
        {
            logger.LogWarning("YouTube thumbnail too small or unavailable for video {VideoId}, discarding", videoId);
            return null;
        }

        await File.WriteAllBytesAsync(filePath, bytes);
        return $"/uploads/plays/{fileName}";
    }
}
