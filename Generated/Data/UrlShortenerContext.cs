using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Models;

namespace UrlShortenerApi.Data;

/// <summary>
/// Entity Framework Core DbContext for the URL Shortener Service.
/// Manages the database connection and entity mappings.
/// </summary>
public class UrlShortenerContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the UrlShortenerContext.
    /// </summary>
    /// <param name="options">DbContext options for configuration.</param>
    public UrlShortenerContext(DbContextOptions<UrlShortenerContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// DbSet for ShortenedUrl entities.
    /// </summary>
    public DbSet<ShortenedUrl> ShortenedUrls { get; set; }

    /// <summary>
    /// Configures the model on context initialization.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure the model.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure ShortenedUrl entity
        modelBuilder.Entity<ShortenedUrl>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.OriginalUrl)
                .IsRequired()
                .HasMaxLength(2048); // Max URL length

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(10); // Short codes are typically 6-10 chars

            entity.HasIndex(e => e.Code)
                .IsUnique(); // Code must be unique

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.AccessCount)
                .IsRequired()
                .HasDefaultValue(0);
        });
    }
}
