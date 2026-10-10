namespace UrlShortenerApi.Models;

public class ShortenedUrl
{
    public int Id { get; set; }
    
    public string Code { get; set; } = string.Empty;
    
    public string OriginalUrl { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public int AccessCount { get; set; } = 0;
    
    public DateTime? LastAccessedAt { get; set; }
}
