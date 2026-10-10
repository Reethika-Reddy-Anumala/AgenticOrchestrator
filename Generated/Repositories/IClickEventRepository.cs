using UrlShortenerApi.Models;

namespace UrlShortenerApi.Repositories;

/// <summary>
/// Repository interface for ClickEvent operations.
/// Defines the contract for data access operations on click events.
/// </summary>
public interface IClickEventRepository
{
    /// <summary>
    /// Records a new click event (for future use - not currently implemented).
    /// </summary>
    /// <param name="analyticsData">The analytics data.</param>
    /// <returns>Task.</returns>
    Task RecordClickAsync(AnalyticsData analyticsData);

    /// <summary>
    /// Gets analytics summary data for a shortened URL.
    /// </summary>
    /// <param name="shortenedUrlId">The ID of the shortened URL.</param>
    /// <returns>An object containing summary statistics.</returns>
    Task<ClickAnalyticsSummary> GetAnalyticsSummaryAsync(int shortenedUrlId);
}

/// <summary>
/// Summary of click analytics for a shortened URL.
/// </summary>
public class ClickAnalyticsSummary
{
    /// <summary>
    /// Total number of clicks.
    /// </summary>
    public int TotalClicks { get; set; }

    /// <summary>
    /// Number of unique clicks (approximate, based on IP addresses).
    /// </summary>
    public int UniqueClicks { get; set; }

    /// <summary>
    /// Date of the first click.
    /// </summary>
    public DateTime? FirstClickDate { get; set; }

    /// <summary>
    /// Date of the most recent click.
    /// </summary>
    public DateTime? LastClickDate { get; set; }

    /// <summary>
    /// Top referrer for clicks.
    /// </summary>
    public string? TopReferrer { get; set; }

    /// <summary>
    /// Dictionary of referrers to click counts.
    /// </summary>
    public Dictionary<string, int> ReferrerBreakdown { get; set; } = new();
}
