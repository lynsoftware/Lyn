# Lyn Website - Frontend (Blazor WebAssembly)

## 🌐 Live URL
- **Production:** https://www.lynsoftware.com
- **CloudFront Direct:** https://d309v9gr6vyyb1.cloudfront.net

## 🏗️ Architecture
```
User Browser
    ↓ HTTPS
CloudFront CDN (AWS)
    ↓
S3 Bucket (Static Files)
    ↓ API Calls (HTTPS)
Backend API (https://api.lynsoftware.com)
```

## 📦 Tech Stack

- **Framework:** Blazor WebAssembly (.NET 10)
- **UI Library:** Blazor.Bootstrap
- **State Management:** Blazored.LocalStorage, Blazored.SessionStorage
- **Internationalization:** Microsoft.Extensions.Localization
- **Logging:** Serilog with BrowserConsole sink

## 🚀 Deployment Infrastructure

### AWS Services Used

| Service | Purpose | Configuration |
|---------|---------|---------------|
| **S3** | Static file hosting | `lyn-website-blazor-bucket` |
| **CloudFront** | CDN & HTTPS | Distribution ID: `E3E7BJ3F96RN9` |
| **Route 53** | DNS management | Hosted zone: `lynsoftware.com` |
| **ACM** | SSL Certificate | Wildcard cert in `us-east-1` |
| **IAM** | Access control | `github-actions` user with S3/CloudFront permissions |

### S3 Bucket Configuration
```yaml
Bucket Name: lyn-website-blazor-bucket
Region: eu-north-1 (Stockholm)
Access: Public read via CloudFront
Versioning: Disabled
Static Website Hosting: Enabled
  - Index: index.html
  - Error: index.html (for SPA routing)
```

### CloudFront Distribution
```yaml
Distribution ID: E3E7BJ3F96RN9
Domain: d309v9gr6vyyb1.cloudfront.net
Custom Domains:
  - www.lynsoftware.com
  - lynsoftware.com
Origin: lyn-website-blazor-bucket.s3.eu-north-1.amazonaws.com
SSL Certificate: ACM (us-east-1)
Viewer Protocol: Redirect HTTP to HTTPS
Cache Behavior:
  - Default TTL: 86400 (24 hours)
  - Compress: Yes
Error Pages:
  - 403 → /index.html (200)
  - 404 → /index.html (200)
```

### Route 53 DNS Records
```yaml
lynsoftware.com:
  Type: A (Alias)
  Target: www.lynsoftware.com

www.lynsoftware.com:
  Type: A (Alias)
  Target: d309v9gr6vyyb1.cloudfront.net

Nameservers:
  - ns-219.awsdns-27.com
  - ns-1474.awsdns-56.org
  - ns-1807.awsdns-33.co.uk
  - ns-667.awsdns-19.net
```

## 🔄 CI/CD Pipeline

### GitHub Actions Workflow

**File:** `.github/workflows/deploy-frontend.yml`
```yaml
Trigger:
  - Push to main branch
  - Paths: Lyn.Web/**, Lyn.Shared/**

Steps:
  1. Build Blazor WebAssembly (Release mode)
  2. Publish to wwwroot
  3. Sync to S3 bucket
  4. Invalidate CloudFront cache (/*) 
  5. Deployment complete

Duration: ~3-5 minutes
```

### Deployment Flow
```
Developer Push → GitHub
    ↓
GitHub Actions triggered
    ↓
dotnet publish -c Release
    ↓
aws s3 sync → S3 Bucket
    ↓
CloudFront Invalidation
    ↓
Changes live in ~3-5 min
```

## 🔧 Local Development

### Prerequisites
```bash
- .NET 10 SDK
- Visual Studio 2022 / Rider / VS Code
```

### Run Locally
```bash
cd Lyn.Web
dotnet run
```

**Local URL:** http://localhost:5000

### Configuration Files

**`wwwroot/appsettings.json` (Development):**
```json
{
  "ApiBaseUrl": "http://localhost:8000"
}
```

**`wwwroot/appsettings.Production.json` (Production):**
```json
{
  "ApiBaseUrl": "https://api.lynsoftware.com"
}
```

## 📝 Environment Variables

Frontend uses configuration from `appsettings.{Environment}.json`:

| Variable | Development | Production |
|----------|-------------|------------|
| `ApiBaseUrl` | `http://localhost:8000` | `https://api.lynsoftware.com` |

## 🛠️ Maintenance

### Manual Deployment
```bash
# Build
cd Lyn.Web
dotnet publish -c Release -o ./publish

# Deploy to S3
aws s3 sync ./publish/wwwroot/ s3://lyn-website-blazor-bucket/ --delete

# Invalidate CloudFront
aws cloudfront create-invalidation \
  --distribution-id E3E7BJ3F96RN9 \
  --paths "/*"
```

### CloudFront Cache Invalidation

**When to invalidate:**
- After deploying critical fixes
- When static assets change
- When appsettings.Production.json is updated

**Cost:** First 1000 invalidations/month are free

### Rollback Procedure
```bash
# S3 versioning is disabled, so manual rollback required:
# 1. Checkout previous commit
git checkout <previous-commit-hash>

# 2. Build and deploy
dotnet publish -c Release -o ./publish
aws s3 sync ./publish/wwwroot/ s3://lyn-website-blazor-bucket/ --delete

# 3. Invalidate CloudFront
aws cloudfront create-invalidation --distribution-id E3E7BJ3F96RN9 --paths "/*"
```

## 🔒 Security

### SSL/TLS

- **Certificate:** AWS Certificate Manager (ACM)
- **Type:** Wildcard (`*.lynsoftware.com`, `lynsoftware.com`, `www.lynsoftware.com`)
- **Region:** us-east-1 (required for CloudFront)
- **Auto-renewal:** Yes (managed by AWS)

### Content Security

- HTTPS enforced (HTTP → HTTPS redirect)
- CORS handled by backend
- No sensitive data in frontend code
- API keys and secrets stored in backend only

## 💰 Cost Breakdown
```
S3 Storage (1 GB):           ~$0.23/month
CloudFront (first year):     $0 (free tier)
CloudFront (after):          ~$1-5/month (depends on traffic)
Route 53 Hosted Zone:        $0.50/month
────────────────────────────────────────
Total (first year):          ~$1/month
Total (after free tier):     ~$2-6/month
```

## 📊 Monitoring

### CloudWatch Metrics

- CloudFront: Requests, Bytes downloaded, Error rate
- S3: Storage size, Request count

### Logs

- CloudFront access logs: Disabled (can be enabled if needed)
- S3 server access logs: Disabled

## 🐛 Troubleshooting

### Issue: 403 Forbidden

**Cause:** CloudFront can't access S3 bucket

**Fix:**
```bash
# Check bucket policy allows CloudFront
aws s3api get-bucket-policy --bucket lyn-website-blazor-bucket
```

### Issue: Mixed Content Error

**Cause:** API URL is HTTP instead of HTTPS

**Fix:** Update `appsettings.Production.json`:
```json
{
  "ApiBaseUrl": "https://api.lynsoftware.com"
}
```

### Issue: 404 on refresh

**Cause:** CloudFront error pages not configured

**Fix:** Verify CloudFront error pages:
- 403 → /index.html (200)
- 404 → /index.html (200)

### Issue: Changes not visible

**Cause:** CloudFront cache

**Fix:**
```bash
# Invalidate cache
aws cloudfront create-invalidation --distribution-id E3E7BJ3F96RN9 --paths "/*"

# Or hard refresh browser
# Ctrl + Shift + R (Windows/Linux)
# Cmd + Shift + R (Mac)
```

## 🔗 Related Documentation

- [Backend README](../Lyn.Backend/README.md)
- [AWS CloudFront Docs](https://docs.aws.amazon.com/cloudfront/)
- [Blazor WASM Docs](https://docs.microsoft.com/en-us/aspnet/core/blazor/)

## 📞 Support

**Repository:** https://github.com/lynsoftware/Lyn
**Issues:** https://github.com/lynsoftware/Lyn/issues

---

**Last Updated:** January 20, 2026
**Deployed By:** GitHub Actions
**Status:** ✅ Production