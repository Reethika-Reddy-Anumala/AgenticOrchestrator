using Microsoft.EntityFrameworkCore;
using UrlShortenerApi.Data;
using UrlShortenerApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(o => { });

// Add services to the container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=urlshortener.db"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseHttpLogging();
app.UseCors("AllowAll");

// POST /shorten - Create a shortened URL
app.MapPost("/shorten", async (ShortenRequest request, AppDbContext db) =>
{
    // Validate request
    if (string.IsNullOrWhiteSpace(request.Url))
    {
        return Results.BadRequest(new { error = "URL is required" });
    }

    try
    {
        // Validate URL format
        new Uri(request.Url);
    }
    catch
    {
        return Results.BadRequest(new { error = "Invalid URL format" });
    }

    // Generate short code
    var code = GenerateShortCode();

    // Check if code already exists (very unlikely)
    while (await db.ShortenedUrls.AnyAsync(x => x.Code == code))
    {
        code = GenerateShortCode();
    }

    var shortenedUrl = new ShortenedUrl
    {
        Code = code,
        OriginalUrl = request.Url,
        CreatedAt = DateTime.UtcNow,
        AccessCount = 0
    };

    db.ShortenedUrls.Add(shortenedUrl);
    await db.SaveChangesAsync();

    return Results.Created($"/{code}", new
    {
        code = code,
        shortUrl = $"{request.BaseUrl ?? "http://localhost"}/{code}",
        originalUrl = request.Url,
        createdAt = shortenedUrl.CreatedAt
    });
});

// GET /{code} - Redirect to original URL
app.MapGet("/{code}", async (string code, AppDbContext db, HttpContext context) =>
{
    var shortenedUrl = await db.ShortenedUrls
        .FirstOrDefaultAsync(x => x.Code == code);

    if (shortenedUrl == null)
    {
        return Results.NotFound(new { error = "Short URL not found" });
    }

    // Update access count and last accessed time
    shortenedUrl.AccessCount++;
    shortenedUrl.LastAccessedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();

    return Results.Redirect(shortenedUrl.OriginalUrl);
});

// GET /analytics/{code} - Get analytics for a shortened URL
app.MapGet("/analytics/{code}", async (string code, AppDbContext db) =>
{
    var shortenedUrl = await db.ShortenedUrls
        .FirstOrDefaultAsync(x => x.Code == code);

    if (shortenedUrl == null)
    {
        return Results.NotFound(new { error = "Short URL not found" });
    }

    return Results.Ok(new
    {
        code = shortenedUrl.Code,
        originalUrl = shortenedUrl.OriginalUrl,
        createdAt = shortenedUrl.CreatedAt,
        accessCount = shortenedUrl.AccessCount,
        lastAccessedAt = shortenedUrl.LastAccessedAt
    });
});

app.Run();

// Helper function to generate a random short code
static string GenerateShortCode()
{
    const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    var random = new Random();
    var codeLength = 6;
    var code = new char[codeLength];

    for (int i = 0; i < codeLength; i++)
    {
        code[i] = chars[random.Next(chars.Length)];
    }

    return new string(code);
}

// Request/Response DTOs
public class ShortenRequest
{
    public string Url { get; set; } = string.Empty;
    public string? BaseUrl { get; set; }
}

public class ShortenResponse
{
    public string Code { get; set; } = string.Empty;
    public string ShortUrl { get; set; } = string.Empty;
    public string OriginalUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AnalyticsResponse
{
    public string Code { get; set; } = string.Empty;
    public string OriginalUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int AccessCount { get; set; }
    public DateTime? LastAccessedAt { get; set; }
}
