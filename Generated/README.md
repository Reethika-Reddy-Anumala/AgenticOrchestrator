# URL Shortener API

A simple and efficient REST API for shortening long URLs into compact, shareable links.

## Features

- **URL Shortening**: Convert long URLs into short, easy-to-share links
- **URL Retrieval**: Redirect from short codes to original URLs
- **Statistics Tracking**: Monitor usage and click counts
- **RESTful API**: Clean and intuitive REST endpoints
- **In-Memory Storage**: Fast access with simple data persistence

## Getting Started

### Prerequisites

- .NET 6.0 or higher
- Visual Studio or Visual Studio Code

### Installation

1. Clone the repository
```bash
git clone https://github.com/yourusername/url-shortener-api.git
cd url-shortener-api
```

2. Build the project
```bash
dotnet build
```

3. Run the application
```bash
dotnet run
```

The API will be available at `http://localhost:5000`

## API Endpoints

### Create Short URL

**POST** `/api/urls`

Creates a new shortened URL.

**Request Body:**
```json
{
  "originalUrl": "https://www.example.com/very/long/url/path"
}
```

**Response (201 Created):**
```json
{
  "id": "abc123",
  "originalUrl": "https://www.example.com/very/long/url/path",
  "shortCode": "abc123",
  "shortUrl": "http://localhost:5000/abc123",
  "createdDate": "2024-01-15T10:30:00Z",
  "clickCount": 0
}
```

### Redirect to Original URL

**GET** `/{shortCode}`

Redirects to the original long URL and increments the click counter.

**Response:** Redirect (301/302) to original URL

### Get URL Details

**GET** `/api/urls/{shortCode}`

Retrieves information about a shortened URL including statistics.

**Response (200 OK):**
```json
{
  "id": "abc123",
  "originalUrl": "https://www.example.com/very/long/url/path",
  "shortCode": "abc123",
  "shortUrl": "http://localhost:5000/abc123",
  "createdDate": "2024-01-15T10:30:00Z",
  "clickCount": 42
}
```

### List All URLs

**GET** `/api/urls`

Retrieves a list of all shortened URLs.

**Response (200 OK):**
```json
[
  {
    "id": "abc123",
    "originalUrl": "https://www.example.com/path1",
    "shortCode": "abc123",
    "shortUrl": "http://localhost:5000/abc123",
    "createdDate": "2024-01-15T10:30:00Z",
    "clickCount": 42
  },
  {
    "id": "def456",
    "originalUrl": "https://www.example.com/path2",
    "shortCode": "def456",
    "shortUrl": "http://localhost:5000/def456",
    "createdDate": "2024-01-16T14:20:00Z",
    "clickCount": 15
  }
]
```

### Delete URL

**DELETE** `/api/urls/{shortCode}`

Deletes a shortened URL mapping.

**Response:** 204 No Content

## Error Responses

### Bad Request (400)
```json
{
  "error": "Invalid URL format",
  "message": "The provided URL is not valid"
}
```

### Not Found (404)
```json
{
  "error": "URL not found",
  "message": "The short code 'invalid' does not exist"
}
```

### Internal Server Error (500)
```json
{
  "error": "Internal server error",
  "message": "An unexpected error occurred"
}
```

## Usage Examples

### Using cURL

Create a short URL:
```bash
curl -X POST http://localhost:5000/api/urls \
  -H "Content-Type: application/json" \
  -d '{"originalUrl":"https://www.github.com/some/very/long/repository/path"}'
```

Redirect to original URL:
```bash
curl -L http://localhost:5000/abc123
```

Get URL details:
```bash
curl http://localhost:5000/api/urls/abc123
```

### Using PowerShell

Create a short URL:
```powershell
$body = @{originalUrl="https://www.github.com/some/very/long/repository/path"} | ConvertTo-Json
Invoke-WebRequest -Uri "http://localhost:5000/api/urls" -Method Post -Body $body -ContentType "application/json"
```

## Project Structure

```
url-shortener-api/
├── Controllers/
│   └── UrlsController.cs
├── Models/
│   └── UrlMapping.cs
├── Services/
│   └── UrlService.cs
├── Program.cs
├── README.md
└── url-shortener-api.csproj
```

## Configuration

You can customize the application by modifying `Program.cs`:

- **Port**: Change the port in `launchSettings.json`
- **Short Code Length**: Adjust the length of generated short codes in `UrlService.cs`
- **Base URL**: Update the `baseUrl` in the service to match your deployment

## Performance Considerations

- Short codes are generated using a random alphanumeric algorithm
- URLs are stored in memory for fast access
- Click counts are incremented efficiently
- Consider implementing a database for production use with large datasets

## Future Enhancements

- [ ] Database persistence (SQL Server, PostgreSQL)
- [ ] User authentication and authorization
- [ ] Custom short code selection
- [ ] URL expiration policies
- [ ] Advanced analytics dashboard
- [ ] Rate limiting and throttling
- [ ] API key management
- [ ] URL preview functionality

## Security Considerations

- Validate all input URLs
- Implement rate limiting in production
- Use HTTPS in production environments
- Consider adding authentication for administrative endpoints
- Sanitize error messages to prevent information leakage

## Testing

Run the unit tests:
```bash
dotnet test
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## Support

For support, please open an issue on the GitHub repository or contact the maintainers.

## Acknowledgments

- Built with ASP.NET Core
- Inspired by popular URL shortening services
- Community feedback and contributions
