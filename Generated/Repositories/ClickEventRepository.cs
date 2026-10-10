using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Data;
using UrlShortenerApi.Models;

namespace UrlShortenerApi.Repositories;

/// <summary>
/// Implementation of IClickEventRepository using Entity Framework Core.
/// </summary>
public class ClickEventRepository : IClickEventRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<ClickEventRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the ClickEventRepository.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public ClickEventRepository(AppDbContext context, ILogger<ClickEventRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task RecordClickAsync(AnalyticsData analyticsData)
    {
        try
        {
            _logger.LogDebug("Recording click event for code: {Code}", analyticsData.Code);
            // Implementation for future use
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording click event");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ClickAnalyticsSummary> GetAnalyticsSummaryAsync(int shortenedUrlId)
    {
        try
        {
            var shortenedUrl = await _context.ShortenedUrls
                .FirstOrDefaultAsync(x => x.Id == shortenedUrlId);

            if (shortenedUrl == null)
            {
                return new ClickAnalyticsSummary();
            }

            return new ClickAnalyticsSummary
            {
                TotalClicks = shortenedUrl.AccessCount,
                UniqueClicks = shortenedUrl.AccessCount,
                FirstClickDate = shortenedUrl.CreatedAt,
                LastClickDate = shortenedUrl.LastAccessedAt,
                TopReferrer = "N/A",
                ReferrerBreakdown = new Dictionary<string, int>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analytics summary for shortened URL ID: {ShortenedUrlId}", shortenedUrlId);
            throw;
        }
    }
}
