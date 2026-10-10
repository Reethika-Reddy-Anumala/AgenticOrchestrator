namespace UrlShortenerService.Services
{
    /// <summary>
    /// Service interface for URL shortening operations.
    /// Handles the business logic for creating and managing shortened URLs.
    /// </summary>
    public interface IUrlShorteningService
    {
        /// <summary>
        /// Shortens a long URL and returns the shortened URL information.
        /// </summary>
        /// <param name="originalUrl">The long URL to shorten.</param>
        /// <param name="expiresAt">Optional expiration date for the shortened URL.</param>
        /// <param name="customCode">Optional custom short code (if custom codes are allowed).</param>
        /// <returns>A UrlShortenResponse containing the shortened URL information.</returns>
        Task<UrlShortenResponse> ShortenUrlAsync(string originalUrl, DateTime? expiresAt = null, string? customCode = null);

        /// <summary>
        /// Retrieves the original URL for a given short code.
        /// </summary>
        /// <param name="code">The short code.</param>
        /// <returns>The original URL if found and not expired; otherwise null.</returns>
        Task<string?> GetOriginalUrlAsync(string code);

        /// <summary>
        /// Deletes a shortened URL entry.
        /// </summary>
        /// <param name="code">The short code to delete.</param>
        /// <returns>True if deletion was successful; false if not found.</returns>
        Task<bool> DeleteShortenedUrlAsync(string code);
    }

    /// <summary>
    /// Response DTO for URL shortening requests.
    /// </summary>
    public class UrlShortenResponse
    {
        /// <summary>
        /// The unique short code for this URL.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// The shortened URL (full URL including domain).
        /// </summary>
        public string ShortenedUrl { get; set; } = string.Empty;

        /// <summary>
        /// The original long URL that was shortened.
        /// </summary>
        public string OriginalUrl { get; set; } = string.Empty;

        /// <summary>
        /// When this shortened URL was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When this shortened URL expires (if applicable).
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// Request DTO for URL shortening.
    /// </summary>
    public class UrlShortenRequest
    {
        /// <summary>
        /// The long URL to shorten.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Optional custom short code.
        /// </summary>
        public string? CustomCode { get; set; }

        /// <summary>
        /// Optional expiration date for the shortened URL.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }
}
