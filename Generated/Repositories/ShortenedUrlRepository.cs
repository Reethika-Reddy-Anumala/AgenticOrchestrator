using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Data;
using UrlShortenerApi.Models;

namespace UrlShortenerApi.Repositories;

/// <summary>
/// Implementation of IShortenedUrlRepository using Entity Framework Core.
/// </summary>
public class ShortenedUrlRepository : IShortenedUrlRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<ShortenedUrlRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the ShortenedUrlRepository.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public ShortenedUrlRepository(AppDbContext context, ILogger<ShortenedUrlRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ShortenedUrl?> GetByCodeAsync(string code)
    {
        try
        {
            return await _context.ShortenedUrls
                .FirstOrDefaultAsync(x => x.Code == code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shortened URL by code: {Code}", code);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ShortenedUrl?> GetByIdAsync(int id)
    {
        try
        {
            return await _context.ShortenedUrls
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shortened URL by ID: {Id}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ShortenedUrl> AddAsync(ShortenedUrl shortenedUrl)
    {
        try
        {
            _context.ShortenedUrls.Add(shortenedUrl);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Added shortened URL with code: {Code}", shortenedUrl.Code);
            return shortenedUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding shortened URL with code: {Code}", shortenedUrl.Code);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ShortenedUrl> UpdateAsync(ShortenedUrl shortenedUrl)
    {
        try
        {
            _context.ShortenedUrls.Update(shortenedUrl);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated shortened URL with ID: {Id}", shortenedUrl.Id);
            return shortenedUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shortened URL with ID: {Id}", shortenedUrl.Id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var shortenedUrl = await _context.ShortenedUrls.FindAsync(id);
            if (shortenedUrl == null)
            {
                _logger.LogWarning("Attempted to delete non-existent shortened URL with ID: {Id}", id);
                return false;
            }

            _context.ShortenedUrls.Remove(shortenedUrl);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted shortened URL with ID: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shortened URL with ID: {Id}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> CodeExistsAsync(string code)
    {
        try
        {
            return await _context.ShortenedUrls
                .AnyAsync(x => x.Code == code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if code exists: {Code}", code);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ShortenedUrl>> GetAllAsync(int skip = 0, int take = 100)
    {
        try
        {
            return await _context.ShortenedUrls
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all shortened URLs");
            throw;
        }
    }
}
