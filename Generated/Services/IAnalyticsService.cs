namespace UrlShortenerService.Services
{
    /// <summary>
    /// Service interface for analytics operations.
    /// Handles tracking and reporting of URL access statistics.
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Records a click event for a shortened URL.
        /// </summary>
        /// <param name="code">The short code that was clicked.</param>
        /// <param name="referrer">HTTP Referer header value (if provided).</param>
        /// <param name="userAgent">User-Agent header value (if provided).</param>
        /// <param name="ipAddress">IP address of the requester (if available).</param>
        /// <returns>True if the click was successfully recorded; false if the code was not found.</returns>
        Task<bool> RecordClickAsync(string code, string? referrer = null, string? userAgent = null, string? ipAddress = null);

        /// <summary>
        /// Retrieves analytics statistics for a shortened URL.
        /// </summary>
        /// <param name="code">The short code to get analytics for.</param>
        /// <returns>An AnalyticsResponse containing statistics; null if code not found.</returns>
        Task<AnalyticsResponse?> GetAnalyticsAsync(string code);

        /// <summary>
        /// Retrieves click statistics within a date range.
        /// </summary>
        /// <param name="code">The short code.</param>
        /// <param name="startDate">Start date (inclusive).</param>
        /// <param name="endDate">End date (inclusive).</param>
        /// <returns>An AnalyticsResponse containing statistics for the date range.</returns>
        Task<AnalyticsResponse?> GetAnalyticsByDateRangeAsync(string code, DateTime startDate, DateTime endDate);
    }

    /// <summary>
    /// Response DTO for analytics queries.
    /// </summary>
    public class AnalyticsResponse
    {
        /// <summary>
        /// The short code being analyzed.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// The original URL.
        /// </summary>
        public string OriginalUrl { get; set; } = string.Empty;

        /// <summary>
        /// Total number of clicks.
        /// </summary>
        public int TotalClicks { get; set; }

        /// <summary>
        /// Approximate number of unique visitors.
        /// </summary>
        public int UniqueClicks { get; set; }

        /// <summary>
        /// When the shortened URL was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date of first click.
        /// </summary>
        public DateTime? FirstClickDate { get; set; }

        /// <summary>
        /// Date of most recent click.
        /// </summary>
        public DateTime? LastClickDate { get; set; }

        /// <summary>
        /// Top referrer for clicks.
        /// </summary>
        public string? TopReferrer { get; set; }

        /// <summary>
        /// Breakdown of clicks by referrer.
        /// </summary>
        public Dictionary<string, int> ReferrerBreakdown { get; set; } = new();
    }
}
