namespace UrlShortenerApi.Models;

/// <summary>
/// Represents analytics data for a shortened URL.
/// Tracks click count and last access time.
/// </summary>
public class AnalyticsData
{
    /// <summary>
    /// The shortened URL code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// The original URL.
    /// </summary>
    public string OriginalUrl { get; set; } = string.Empty;

    /// <summary>
    /// When this shortened URL was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Number of times this short URL has been accessed.
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    /// When this short URL was last accessed.
    /// </summary>
    public DateTime? LastAccessedAt { get; set; }
}
