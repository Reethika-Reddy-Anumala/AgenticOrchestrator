# URL Shortener API - Testing Guide

Quick reference for testing all three API endpoints.

## 🚀 Quick Start

1. **Run the application:**
   ```bash
   dotnet run
   ```

2. **API will be available at:**
   - `http://localhost:5000` (HTTP)
   - `https://localhost:5001` (HTTPS)

3. **Database will be created automatically** as `urlshortener.db`

## 📋 Test Cases

### Test 1: Create a Shortened URL

#### Using cURL
```bash
curl -X POST http://localhost:5000/shorten \
  -H "Content-Type: application/json" \
  -d '{"url":"https://www.github.com/dotnet/aspnetcore","baseUrl":"http://localhost:5000"}'
```

#### Using PowerShell
```powershell
$body = @{
    url = "https://www.github.com/dotnet/aspnetcore"
    baseUrl = "http://localhost:5000"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/shorten" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body
```

#### Expected Response (201)
```json
{
  "code": "aB3xY2",
  "shortUrl": "http://localhost:5000/aB3xY2",
  "originalUrl": "https://www.github.com/dotnet/aspnetcore",
  "createdAt": "2024-01-15T10:30:45.1234567Z"
}
```

#### ✅ Success Criteria
- Status code: `201 Created`
- Response includes: code, shortUrl, originalUrl, createdAt
- Code is 6 characters alphanumeric

---

### Test 2: Invalid URL

#### Using cURL
```bash
curl -X POST http://localhost:5000/shorten \
  -H "Content-Type: application/json" \
  -d '{"url":"not a valid url"}'
```

#### Expected Response (400)
```json
{
  "error": "Invalid URL format"
}
```

#### ✅ Success Criteria
- Status code: `400 Bad Request`
- Error message indicates invalid format

---

### Test 3: Missing URL

#### Using cURL
```bash
curl -X POST http://localhost:5000/shorten \
  -H "Content-Type: application/json" \
  -d '{}'
```

#### Expected Response (400)
```json
{
  "error": "URL is required"
}
```

#### ✅ Success Criteria
- Status code: `400 Bad Request`
- Error message indicates URL is required

---

### Test 4: Redirect to Original URL

#### Using cURL (replace 'aB3xY2' with actual code from Test 1)
```bash
curl -L http://localhost:5000/aB3xY2
```

#### Expected Response (302)
- Redirects to original URL
- Browser follows redirect automatically (with -L flag)

#### ✅ Success Criteria
- Status code: `302 Found` (followed by 200 with final URL content)
- Final destination is the original URL
- Database AccessCount incremented
- Database LastAccessedAt updated

---

### Test 5: Invalid Short Code

#### Using cURL
```bash
curl -L http://localhost:5000/invalidCode
```

#### Expected Response (404)
```json
{
  "error": "Short URL not found"
}
```

#### ✅ Success Criteria
- Status code: `404 Not Found`
- Error message indicates code not found

---

### Test 6: Get Analytics

#### Using cURL (replace 'aB3xY2' with actual code)
```bash
curl http://localhost:5000/analytics/aB3xY2
```

#### Expected Response (200)
```json
{
  "code": "aB3xY2",
  "originalUrl": "https://www.github.com/dotnet/aspnetcore",
  "createdAt": "2024-01-15T10:30:45.1234567Z",
  "accessCount": 1,
  "lastAccessedAt": "2024-01-15T10:32:10.5678901Z"
}
```

#### ✅ Success Criteria
- Status code: `200 OK`
- Response includes: code, originalUrl, createdAt, accessCount, lastAccessedAt
- accessCount reflects number of times URL was accessed
- lastAccessedAt is the most recent access time

---

### Test 7: Analytics for Non-existent Code

#### Using cURL
```bash
curl http://localhost:5000/analytics/nonexistent
```

#### Expected Response (404)
```json
{
  "error": "Short URL not found"
}
```

#### ✅ Success Criteria
- Status code: `404 Not Found`
- Error message indicates code not found

---

### Test 8: Multiple Accesses to Same URL

#### Steps:
1. Create a short URL (use Test 1)
2. Record the code (e.g., 'aB3xY2')
3. Access it multiple times:
   ```bash
   curl -L http://localhost:5000/aB3xY2
   curl -L http://localhost:5000/aB3xY2
   curl -L http://localhost:5000/aB3xY2
   ```
4. Check analytics:
   ```bash
   curl http://localhost:5000/analytics/aB3xY2
   ```

#### Expected Response
```json
{
  "code": "aB3xY2",
  "originalUrl": "https://www.github.com/dotnet/aspnetcore",
  "createdAt": "2024-01-15T10:30:45.1234567Z",
  "accessCount": 3,
  "lastAccessedAt": "2024-01-15T10:35:20.1234567Z"
}
```

#### ✅ Success Criteria
- accessCount should be 3
- lastAccessedAt updated after each access

---

## 🧪 Automated Testing Script

### Bash Script (test.sh)
```bash
#!/bin/bash

API_URL="http://localhost:5000"

echo "=== URL Shortener API Test Suite ==="
echo ""

# Test 1: Create shortened URL
echo "Test 1: Creating shortened URL..."
RESPONSE=$(curl -s -X POST $API_URL/shorten \
  -H "Content-Type: application/json" \
  -d '{"url":"https://www.google.com","baseUrl":"'$API_URL'"}')

echo "Response: $RESPONSE"
CODE=$(echo $RESPONSE | grep -o '"code":"[^"]*' | cut -d'"' -f4)
echo "Generated code: $CODE"
echo ""

# Test 2: Get analytics
echo "Test 2: Getting analytics..."
curl -s -X GET $API_URL/analytics/$CODE | jq '.'
echo ""

# Test 3: Redirect
echo "Test 3: Testing redirect..."
curl -s -L -I -X GET $API_URL/$CODE | head -5
echo ""

# Test 4: Multiple accesses
echo "Test 4: Accessing URL multiple times..."
for i in {1..3}; do
  curl -s -L $API_URL/$CODE > /dev/null
  echo "  Access $i completed"
done
echo ""

# Test 5: Check updated analytics
echo "Test 5: Checking updated analytics..."
curl -s -X GET $API_URL/analytics/$CODE | jq '.'

echo ""
echo "=== Test Suite Complete ==="
```

### PowerShell Script (test.ps1)
```powershell
$API_URL = "http://localhost:5000"

Write-Host "=== URL Shortener API Test Suite ===" -ForegroundColor Cyan
Write-Host ""

# Test 1: Create shortened URL
Write-Host "Test 1: Creating shortened URL..." -ForegroundColor Yellow
$response = Invoke-RestMethod -Uri "$API_URL/shorten" `
  -Method Post `
  -ContentType "application/json" `
  -Body (@{
    url = "https://www.google.com"
    baseUrl = $API_URL
  } | ConvertTo-Json)

Write-Host "Response:" -ForegroundColor Green
$response | ConvertTo-Json
$code = $response.code
Write-Host "Generated code: $code" -ForegroundColor Green
Write-Host ""

# Test 2: Get analytics
Write-Host "Test 2: Getting analytics..." -ForegroundColor Yellow
$analytics = Invoke-RestMethod -Uri "$API_URL/analytics/$code" -Method Get
$analytics | ConvertTo-Json | Write-Host -ForegroundColor Green
Write-Host ""

# Test 4: Multiple accesses
Write-Host "Test 3: Accessing URL multiple times..." -ForegroundColor Yellow
for ($i = 1; $i -le 3; $i++) {
  $redirect = Invoke-RestMethod -Uri "$API_URL/$code" `
    -Method Get `
    -MaximumRedirection 0 `
    -ErrorAction SilentlyContinue
  Write-Host "  Access $i completed"
}
Write-Host ""

# Test 5: Check updated analytics
Write-Host "Test 4: Checking updated analytics..." -ForegroundColor Yellow
$analytics = Invoke-RestMethod -Uri "$API_URL/analytics/$code" -Method Get
$analytics | ConvertTo-Json | Write-Host -ForegroundColor Green

Write-Host ""
Write-Host "=== Test Suite Complete ===" -ForegroundColor Cyan
```

---

## 🧡 Manual Testing Checklist

Use this checklist when testing manually:

- [ ] **Test 1: Valid URL shortening**
  - [ ] Returns 201 status
  - [ ] Response includes code (6 chars)
  - [ ] Response includes shortUrl
  - [ ] Response includes originalUrl
  - [ ] Response includes createdAt

- [ ] **Test 2: Invalid URL format**
  - [ ] Returns 400 status
  - [ ] Error message about format

- [ ] **Test 3: Missing URL**
  - [ ] Returns 400 status
  - [ ] Error message about required field

- [ ] **Test 4: Redirect**
  - [ ] Returns 302 status
  - [ ] Redirects to original URL
  - [ ] Browser shows final destination

- [ ] **Test 5: Non-existent code**
  - [ ] Returns 404 status
  - [ ] Error message about not found

- [ ] **Test 6: Analytics for valid code**
  - [ ] Returns 200 status
  - [ ] Shows correct code
  - [ ] Shows original URL
  - [ ] Shows access count
  - [ ] Shows last accessed time

- [ ] **Test 7: Analytics for non-existent code**
  - [ ] Returns 404 status
  - [ ] Error message about not found

- [ ] **Test 8: Access tracking**
  - [ ] Create URL
  - [ ] Access it 3+ times
  - [ ] Analytics shows correct access count
  - [ ] lastAccessedAt is recent

---

## 📊 Testing Metrics

### Response Time Goals
- POST /shorten: < 50ms
- GET /{code}: < 20ms
- GET /analytics/{code}: < 20ms

### Database Validation
```bash
# Check database file created
ls -la urlshortener.db

# View database with sqlite3 (if installed)
sqlite3 urlshortener.db "SELECT * FROM ShortenedUrls;"
```

### Error Scenarios Tested
- [x] Invalid URL format
- [x] Missing URL parameter
- [x] Non-existent short code
- [x] Database operations
- [x] Multiple accesses
- [x] Collision detection (6-char limit)

---

## 🔍 Troubleshooting

### Issue: API not responding
**Solution:**
```bash
# Check if running
dotnet build
dotnet run

# Check listening ports
netstat -ano | findstr ":5000"  # Windows
lsof -i :5000                   # macOS/Linux
```

### Issue: Database locked
**Solution:**
- Stop the application
- Delete `urlshortener.db`
- Restart the application

### Issue: CORS errors
**Solution:**
- The API should allow all origins by default
- If testing from different domain, check CORS policy in Program.cs

### Issue: SSL certificate warning
**Solution:**
- Use HTTP (port 5000) instead of HTTPS during testing
- Or skip certificate validation: `curl -k https://localhost:5001/...`

---

## 📝 Notes

- Each test should be independent
- Short codes are case-sensitive (abc123 ≠ ABC123)
- Access count is incremented with each redirect
- Delete database file to reset all data
- Look at database file for verification of data

---

**Last Updated**: 2024  
**API Version**: 1.0.0  
**Status**: Ready for Testing ✅
