using UrlShortenerApi.Models;

namespace UrlShortenerApi.Repositories;

/// <summary>
/// Repository interface for ShortenedUrl operations.
/// Defines the contract for data access operations on shortened URLs.
/// </summary>
public interface IShortenedUrlRepository
{
    /// <summary>
    /// Retrieves a shortened URL by its short code.
    /// </summary>
    /// <param name="code">The short code to look up.</param>
    /// <returns>The ShortenedUrl if found; otherwise null.</returns>
    Task<ShortenedUrl?> GetByCodeAsync(string code);

    /// <summary>
    /// Retrieves a shortened URL by its ID.
    /// </summary>
    /// <param name="id">The ID to look up.</param>
    /// <returns>The ShortenedUrl if found; otherwise null.</returns>
    Task<ShortenedUrl?> GetByIdAsync(int id);

    /// <summary>
    /// Adds a new shortened URL to the repository.
    /// </summary>
    /// <param name="shortenedUrl">The ShortenedUrl entity to add.</param>
    /// <returns>The added ShortenedUrl with its ID populated.</returns>
    Task<ShortenedUrl> AddAsync(ShortenedUrl shortenedUrl);

    /// <summary>
    /// Updates an existing shortened URL.
    /// </summary>
    /// <param name="shortenedUrl">The ShortenedUrl entity with updated values.</param>
    /// <returns>The updated ShortenedUrl.</returns>
    Task<ShortenedUrl> UpdateAsync(ShortenedUrl shortenedUrl);

    /// <summary>
    /// Deletes a shortened URL by its ID.
    /// </summary>
    /// <param name="id">The ID of the ShortenedUrl to delete.</param>
    /// <returns>True if deletion was successful; false if not found.</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks if a code already exists in the repository.
    /// </summary>
    /// <param name="code">The code to check.</param>
    /// <returns>True if the code exists; otherwise false.</returns>
    Task<bool> CodeExistsAsync(string code);

    /// <summary>
    /// Gets all shortened URLs (with pagination support).
    /// </summary>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to take.</param>
    /// <returns>A collection of ShortenedUrl entities.</returns>
    Task<IEnumerable<ShortenedUrl>> GetAllAsync(int skip = 0, int take = 100);
}
