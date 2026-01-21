# Lyn Website - Backend API (ASP.NET Core)

## 🌐 Live URL
- **Production API:** https://api.lynsoftware.com
- **Health Check:** https://api.lynsoftware.com/health

## 🏗️ Architecture
```
User Request (HTTPS)
    ↓
Nginx (Reverse Proxy + SSL)
    ↓ HTTP (localhost only)
Backend API (ASP.NET Core in Docker)
    ↓
PostgreSQL Database (Docker)
```

## 📦 Tech Stack

- **Framework:** ASP.NET Core 10.0
- **Database:** PostgreSQL 18 (Alpine)
- **ORM:** Entity Framework Core
- **Authentication:** JWT with Argon2id password hashing
- **Email:** Resend API
- **Containerization:** Docker + Docker Compose
- **Reverse Proxy:** Nginx 1.24.0
- **SSL:** Let's Encrypt (Certbot)

## 🚀 Deployment Infrastructure

### AWS Services Used

| Service | Purpose | Configuration |
|---------|---------|---------------|
| **EC2** | Application hosting | Instance: `i-01001a1fb9bb9977d` |
| **VPC** | Network isolation | `lyn-website-vpc` (10.0.0.0/16) |
| **Security Groups** | Firewall rules | `lyn-backend-sg` |
| **IAM** | Access control | `EC2LynBackendRole` |
| **Systems Manager** | Remote access | Session Manager (no SSH) |
| **Parameter Store** | Secrets management | SecureString parameters |
| **Route 53** | DNS | `api.lynsoftware.com` |

### EC2 Instance Details
```yaml
Instance ID: i-01001a1fb9bb9977d
Instance Type: t3.small
  - vCPU: 2
  - RAM: 2 GB
  - Cost: ~$15/month
AMI: Ubuntu Server 24.04 LTS
Region: eu-north-1 (Stockholm)
Availability Zone: eu-north-1a
VPC: lyn-website-vpc
Subnet: lyn-website-subnet-public1-eu-north-1a
Public IP: 16.16.63.35
Storage: 8 GB gp3 SSD (~$0.64/month)
IAM Role: EC2LynBackendRole
Tags:
  - Name: lyn-backend-server
  - Environment: production
  - Project: lyn-website
```

### VPC Configuration
```yaml
VPC: lyn-website-vpc
CIDR: 10.0.0.0/16

Public Subnets:
  - lyn-website-subnet-public1-eu-north-1a (10.0.1.0/24)
  - lyn-website-subnet-public2-eu-north-1b (10.0.2.0/24)

Private Subnets (reserved for future RDS):
  - lyn-website-subnet-private1-eu-north-1a (10.0.3.0/24)
  - lyn-website-subnet-private2-eu-north-1b (10.0.4.0/24)

Internet Gateway: Yes
NAT Gateway: No (cost savings)
```

### Security Group Rules
```yaml
Security Group: lyn-backend-sg (sg-0050120b79045355ea)

Inbound Rules:
  - HTTP (80):
      Source: 0.0.0.0/0
      Purpose: Let's Encrypt SSL validation
  
  - HTTPS (443):
      Source: 0.0.0.0/0
      Purpose: API traffic from frontend
  
  - Custom TCP (8000):
      Source: 0.0.0.0/0
      Purpose: Direct backend access (temporary)
  
  Note: SSH (22) is NOT open - using SSM instead

Outbound Rules:
  - All traffic: 0.0.0.0/0
```

### IAM Role Permissions
```yaml
Role: EC2LynBackendRole

Policies:
  - AmazonSSMManagedInstanceCore:
      Purpose: SSH-less access via Session Manager
      Permissions:
        - ssm:UpdateInstanceInformation
        - ssm:ListAssociations
        - ssmmessages:CreateControlChannel
        - ssmmessages:CreateDataChannel
  
  - AmazonSSMReadOnlyAccess:
      Purpose: Read secrets from Parameter Store
      Permissions:
        - ssm:GetParameter
        - ssm:GetParameters
        - ssm:GetParameterHistory
```

### Secrets in Parameter Store
```yaml
Region: eu-north-1
Type: SecureString (encrypted with AWS KMS)

Parameters:
  /lyn/prod/postgres-password:
    Description: PostgreSQL database password
    Type: SecureString
  
  /lyn/prod/jwt-secret:
    Description: JWT secret key (min 32 characters)
    Type: SecureString
  
  /lyn/prod/admin-password:
    Description: Admin user password
    Type: SecureString
  
  /lyn/prod/resend-api-key:
    Description: Resend API key for email
    Type: SecureString
```

## 🐳 Docker Configuration

### Docker Compose Architecture

**File:** `docker-compose.ec2.yml`
```yaml
Services:
  backend:
    Container: lyn-backend
    Build: Lyn.Backend/Dockerfile
    Port: 8000:8080 (host:container)
    Network: lyn-network (bridge)
    Environment: Production
    Depends on: database (health check)
    Restart: unless-stopped
    
  database:
    Container: lyn-database
    Image: postgres:18-alpine
    Port: 127.0.0.1:5432:5432 (localhost only!)
    Volume: postgres_data (persistent)
    Network: lyn-network
    Restart: unless-stopped
    Health Check: pg_isready

Networks:
  lyn-network: bridge

Volumes:
  postgres_data: /var/lib/docker/volumes/postgres_data
```

### Container Details

**Backend Container:**
```yaml
Name: lyn-backend
Base Image: mcr.microsoft.com/dotnet/aspnet:10.0
Working Dir: /app
Exposed Port: 8080 (internal)
Mapped Port: 8000 (external, localhost only)
Environment: Production
Auto-restart: Yes
Health Check: wget http://localhost:8080/health
```

**Database Container:**
```yaml
Name: lyn-database
Image: postgres:18-alpine
Data Volume: postgres_data (persistent across restarts)
Port Binding: 127.0.0.1:5432:5432 (NOT exposed to internet)
Database: lyndb
User: postgres
Health Check: pg_isready -U postgres -d lyndb
```

## 🔒 Nginx Reverse Proxy

### Configuration

**File:** `/etc/nginx/sites-available/lyn-api`
```nginx
server {
    listen 443 ssl http2;
    server_name api.lynsoftware.com;

    # SSL Certificate (Let's Encrypt)
    ssl_certificate /etc/letsencrypt/live/api.lynsoftware.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.lynsoftware.com/privkey.pem;
    
    # SSL Configuration
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;

    # Proxy to Docker container
    location / {
        proxy_pass http://localhost:8000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

server {
    listen 80;
    server_name api.lynsoftware.com;
    
    # Redirect HTTP to HTTPS
    return 301 https://$server_name$request_uri;
}
```

### SSL Certificate
```yaml
Provider: Let's Encrypt
Tool: Certbot
Domain: api.lynsoftware.com
Certificate: /etc/letsencrypt/live/api.lynsoftware.com/fullchain.pem
Private Key: /etc/letsencrypt/live/api.lynsoftware.com/privkey.pem
Expiry: 2026-04-20 (90 days from issue)
Auto-renewal: Yes (via systemd timer)
  - Check: certbot renew --dry-run
  - Timer: /etc/systemd/system/certbot.timer
```

## 🔄 CI/CD Pipeline

### GitHub Actions Workflow

**File:** `.github/workflows/deploy-backend.yml`
```yaml
Trigger:
  - Push to main branch
  - Paths: Lyn.Backend/**, Lyn.Shared/**, docker-compose.ec2.yml
  - Manual dispatch

IAM User: github-actions
Permissions:
  - ec2:DescribeInstances
  - ssm:SendCommand
  - ssm:GetCommandInvocation

Steps:
  1. Configure AWS credentials
  2. Get EC2 Instance ID (via tag: Name=lyn-backend-server)
  3. Send SSM command to EC2:
     - cd /home/ubuntu/Lyn
     - git pull origin main
     - docker compose -f docker-compose.ec2.yml down
     - docker compose -f docker-compose.ec2.yml up -d --build
     - echo Deployment completed
  4. Wait for completion (10 seconds)
  5. Retrieve command output

Duration: ~2-3 minutes (first build: 3-6 min)
```

### Deployment Flow
```
Developer Push → GitHub
    ↓
GitHub Actions triggered
    ↓
AWS SSM send-command
    ↓
EC2: git pull
    ↓
EC2: docker compose down
    ↓
EC2: docker compose build
    ↓
EC2: docker compose up
    ↓
Backend live (~2-3 min)
```

### Git Configuration on EC2
```yaml
Method: SSH Key
Location: /home/ubuntu/.ssh/id_ed25519
GitHub: Deploy key added to repository
Remote: git@github.com:lynsoftware/Lyn.git
Branch: main
Auto-pull: Via GitHub Actions SSM command
```

## 🔧 Local Development

### Prerequisites
```bash
- .NET 10 SDK
- Docker Desktop
- PostgreSQL (or use Docker)
```

### Run Locally
```bash
# With Docker Compose
cd Lyn
docker compose up

# Or run directly
cd Lyn.Backend
dotnet run
```

**Local URL:** http://localhost:8000

### Environment Variables

**`.env` file (on EC2):**
```bash
# Database
POSTGRES_DB=lyndb
POSTGRES_USER=postgres
POSTGRES_PASSWORD=[from Parameter Store]

# Application
ASPNETCORE_ENVIRONMENT=Production
BACKEND_URL=http://localhost:8000

# CORS
CORS_ALLOWED_ORIGIN=https://www.lynsoftware.com

# JWT
JWT_KEY=[from Parameter Store]
JWT_ISSUER=LynBackend
JWT_AUDIENCE=LynWebUser
JWT_TOKEN_VALIDITY_MINUTES=60

# Admin
ADMIN_USER_EMAIL=admin@lynwebsite.com
ADMIN_USER_PASSWORD=[from Parameter Store]

# Email
RESEND_API_KEY=[from Parameter Store]
```

## 🗄️ Database

### PostgreSQL Configuration
```yaml
Version: 18 (Alpine)
Container: lyn-database
Port: 127.0.0.1:5432 (localhost only - NOT exposed to internet)
Database: lyndb
User: postgres
Password: Stored in Parameter Store
Volume: postgres_data (persistent)
Character Set: UTF8
Collation: en_US.utf8
```

### Connection String
```csharp
Host=database;Port=5432;Database=lyndb;Username=postgres;Password=${POSTGRES_PASSWORD}
```

Note: `database` is the Docker service name (resolves via Docker network)

### Migrations
```bash
# Run migrations (automatic on startup)
# Handled by Entity Framework Core in Program.cs

# Manual migration
cd Lyn.Backend
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Backup Strategy
```bash
# Manual backup
docker exec lyn-database pg_dump -U postgres lyndb > backup.sql

# Restore
docker exec -i lyn-database psql -U postgres lyndb < backup.sql

# Automated backups (TODO)
# - Cron job on EC2
# - Upload to S3 bucket
# - Retention: 30 days
```

### Database Schema

**Main Tables:**
- `AspNetUsers` - User accounts
- `AspNetRoles` - User roles
- `AppDownloads` - Download statistics
- `__EFMigrationsHistory` - Migration tracking

## 🔒 Security

### Authentication & Authorization
```yaml
Method: JWT (JSON Web Tokens)
Algorithm: HS256
Issuer: LynBackend
Audience: LynWebUser
Token Validity: 60 minutes
Password Hashing: Argon2id (timing-attack resistant)
Admin Seeding: Automatic on first run
```

### CORS Policy
```csharp
Allowed Origins:
  - https://www.lynsoftware.com
  - https://lynsoftware.com (if added)
  - https://d309v9gr6vyyb1.cloudfront.net (if added)

Allowed Methods: All
Allowed Headers: All
Credentials: Supported
```

### SSL/TLS
```yaml
Provider: Let's Encrypt
Certificate: Wildcard NOT used (single domain)
Domain: api.lynsoftware.com
Protocols: TLSv1.2, TLSv1.3
Auto-renewal: Yes (Certbot systemd timer)
Expiry Check: certbot certificates
```

### Network Security
```yaml
Database Port: 127.0.0.1:5432 (localhost only)
Backend Port: 127.0.0.1:8000 (localhost only)
Public Access: Via Nginx on 443 (HTTPS) only
SSH: Disabled (using AWS SSM instead)
```

## 💰 Cost Breakdown
```
EC2 t3.small:              ~$15.20/month
EBS Storage (8 GB gp3):    ~$0.64/month
Data Transfer (outbound):  ~$1/month (first 100 GB free)
Route 53:                  $0.50/month (hosted zone)
Parameter Store:           $0 (standard tier)
IAM:                       $0
SSM:                       $0
────────────────────────────────────────
Total:                     ~$17.50/month
```

### Cost Optimization Options
```yaml
Current: t3.small (2 GB RAM) - $15/month
  ↓ Downgrade
Option 1: t3.micro (1 GB RAM) - $8/month (⚠️ may OOM)
  ↓ Upgrade
Option 2: RDS db.t3.micro - +$15/month (managed database)
Option 3: t3.medium (4 GB RAM) - $30/month (if scaling needed)
```

## 📊 Monitoring

### Health Check Endpoint
```bash
GET https://api.lynsoftware.com/health

Response:
{
  "status": "healthy",
  "timestamp": "2026-01-20T21:58:11.0908058Z"
}
```

### Docker Health Checks
```bash
# Check container status
docker compose -f docker-compose.ec2.yml ps

# Backend health
docker compose -f docker-compose.ec2.yml exec backend wget -q -O- http://localhost:8080/health

# Database health
docker compose -f docker-compose.ec2.yml exec database pg_isready -U postgres -d lyndb
```

### Logs
```bash
# View all logs
docker compose -f docker-compose.ec2.yml logs

# Follow backend logs
docker compose -f docker-compose.ec2.yml logs -f backend

# Last 100 lines
docker compose -f docker-compose.ec2.yml logs --tail=100 backend

# Nginx logs
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log
```

### AWS CloudWatch (TODO)
```yaml
Metrics to monitor:
  - EC2 CPU Utilization
  - EC2 Memory Usage
  - EC2 Disk Space
  - Container restart count
  - API response time
  - Database connection count

Alarms:
  - CPU > 80% for 5 minutes
  - Disk space < 20%
  - Container unhealthy
```

## 🛠️ Maintenance

### Access EC2 Instance
```bash
# Via AWS SSM (no SSH key needed)
aws ssm start-session --target i-01001a1fb9bb9977d

# Then switch to ubuntu user
sudo su - ubuntu
cd ~/Lyn
```

### Update Application
```bash
# Pull latest code
git pull origin main

# Rebuild and restart
docker compose -f docker-compose.ec2.yml down
docker compose -f docker-compose.ec2.yml up -d --build

# Verify
docker compose -f docker-compose.ec2.yml ps
docker compose -f docker-compose.ec2.yml logs -f backend
```

### Update System Packages
```bash
sudo apt update
sudo apt upgrade -y

# If kernel updated, reboot
sudo reboot
```

### Renew SSL Certificate (manual)
```bash
# Test renewal
sudo certbot renew --dry-run

# Force renewal
sudo certbot renew --force-renewal

# Reload Nginx
sudo systemctl reload nginx
```

### Backup Database
```bash
# Create backup
docker exec lyn-database pg_dump -U postgres lyndb | gzip > backup-$(date +%Y%m%d).sql.gz

# Upload to S3 (TODO: create backup bucket)
# aws s3 cp backup-*.sql.gz s3://lyn-backups/database/
```

### Rollback Procedure
```bash
# 1. SSH to EC2
aws ssm start-session --target i-01001a1fb9bb9977d

# 2. Switch user
sudo su - ubuntu
cd ~/Lyn

# 3. Checkout previous version
git log --oneline  # Find commit hash
git checkout <previous-commit-hash>

# 4. Rebuild
docker compose -f docker-compose.ec2.yml down
docker compose -f docker-compose.ec2.yml up -d --build

# 5. Verify
curl http://localhost:8000/health
```

## 🐛 Troubleshooting

### Issue: Container won't start
```bash
# Check logs
docker compose -f docker-compose.ec2.yml logs backend

# Check if port 8000 is in use
sudo lsof -i :8000

# Restart Docker
sudo systemctl restart docker
```

### Issue: Database connection failed
```bash
# Check database container
docker compose -f docker-compose.ec2.yml ps database

# Check database logs
docker compose -f docker-compose.ec2.yml logs database

# Test database connection
docker compose -f docker-compose.ec2.yml exec database psql -U postgres -d lyndb -c "SELECT 1;"
```

### Issue: SSL certificate expired
```bash
# Check certificate status
sudo certbot certificates

# Renew
sudo certbot renew

# If renewal fails, check:
# 1. Port 80 is open in Security Group
# 2. Nginx is running
# 3. Domain DNS is correct
```

### Issue: Out of disk space
```bash
# Check disk usage
df -h

# Clean Docker
docker system prune -a

# Clean old logs
sudo journalctl --vacuum-time=7d
```

### Issue: High CPU usage
```bash
# Check processes
top

# Check Docker stats
docker stats

# Scale vertically: Upgrade to t3.medium
# Scale horizontally: Add load balancer + more instances (advanced)
```

## 🔗 Related Documentation

- [Frontend README](../Lyn.Web/README.md)
- [AWS EC2 Docs](https://docs.aws.amazon.com/ec2/)
- [Docker Compose Docs](https://docs.docker.com/compose/)
- [ASP.NET Core Docs](https://docs.microsoft.com/en-us/aspnet/core/)
- [PostgreSQL Docs](https://www.postgresql.org/docs/)

## 📞 Support

**Repository:** https://github.com/lynsoftware/Lyn
**Issues:** https://github.com/lynsoftware/Lyn/issues

---

**Last Updated:** January 20, 2026
**Deployed By:** GitHub Actions
**Status:** ✅ Production