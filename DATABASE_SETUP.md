# Database Setup Guide

## Prerequisites
- SQL Server 2019+ or SQL Server Express
- .NET 8.0 SDK
- Entity Framework Core Tools

## Install EF Core Tools

```bash
dotnet tool install --global dotnet-ef
```

## Step-by-Step Setup

### 1. Update Connection String

Edit `src/TreasureHunter.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=TreasureHunterDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Common Connection Strings:**

**Local SQL Server (Windows Authentication):**
```
Server=localhost;Database=TreasureHunterDB;Trusted_Connection=True;TrustServerCertificate=True;
```

**SQL Server Express:**
```
Server=localhost\\SQLEXPRESS;Database=TreasureHunterDB;Trusted_Connection=True;TrustServerCertificate=True;
```

**SQL Server with Username/Password:**
```
Server=YOUR_SERVER;Database=TreasureHunterDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;
```

**Azure SQL Database:**
```
Server=tcp:YOUR_SERVER.database.windows.net,1433;Database=TreasureHunterDB;User Id=YOUR_USER@YOUR_SERVER;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;
```

### 2. Create Initial Migration

From the repository root:

```bash
cd src/TreasureHunter.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../TreasureHunter.API --output-dir Persistence/Migrations
```

### 3. Apply Migration to Database

```bash
dotnet ef database update --startup-project ../TreasureHunter.API
```

This will:
- Create the `TreasureHunterDB` database
- Create all tables (TreasureItems, ItemTypes, UserInventories, AspNetUsers, etc.)
- Seed initial ItemTypes data

### 4. Verify Database

Connect to SQL Server and verify tables were created:

```sql
USE TreasureHunterDB;
GO

-- List all tables
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE';

-- Check seeded data
SELECT * FROM ItemTypes;
```

Expected ItemTypes:
- ID 1: Common Coin (100 points)
- ID 2: Rare Gem (300 points)
- ID 3: Epic Treasure Chest (500 points)

## Adding New Migrations

After modifying entities in the Domain project:

```bash
cd src/TreasureHunter.Infrastructure
dotnet ef migrations add YourMigrationName --startup-project ../TreasureHunter.API
dotnet ef database update --startup-project ../TreasureHunter.API
```

## Rollback Migration

```bash
# List migrations
dotnet ef migrations list --startup-project ../TreasureHunter.API

# Remove last migration
dotnet ef migrations remove --startup-project ../TreasureHunter.API

# Rollback to specific migration
dotnet ef database update PreviousMigrationName --startup-project ../TreasureHunter.API
```

## Drop and Recreate Database

**WARNING: This will delete all data!**

```bash
dotnet ef database drop --startup-project ../TreasureHunter.API --force
dotnet ef database update --startup-project ../TreasureHunter.API
```

## Troubleshooting

### Error: "Cannot open database"
- Verify SQL Server is running
- Check connection string server name
- Test connection with SQL Server Management Studio

### Error: "Login failed for user"
- Verify SQL Server authentication mode (Windows or Mixed)
- Check username/password in connection string
- Ensure user has CREATE DATABASE permission

### Error: "A network-related error"
- Enable TCP/IP in SQL Server Configuration Manager
- Check Windows Firewall settings
- Verify SQL Server Browser service is running

### Error: "Trust relationship failed"
- Add `TrustServerCertificate=True` to connection string
- Or install proper SSL certificate on SQL Server

## Production Considerations

### Security
1. **Never commit production connection strings** to source control
2. Use **User Secrets** for development:
   ```bash
   cd src/TreasureHunter.API
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING"
   ```

3. Use **Environment Variables** in production:
   ```bash
   export ConnectionStrings__DefaultConnection="YOUR_CONNECTION_STRING"
   ```

4. Use **Azure Key Vault** or similar for cloud deployments

### Performance
- Enable connection pooling (enabled by default)
- Use appropriate indexes (already configured in ApplicationDbContext)
- Monitor query performance with SQL Profiler
- Consider read replicas for high-traffic scenarios

### Backup
Regular database backups are essential:
```sql
BACKUP DATABASE TreasureHunterDB
TO DISK = 'C:\Backups\TreasureHunterDB.bak'
WITH FORMAT, COMPRESSION;
```

## Database Schema Overview

```
AspNetUsers (Identity)
├── AspNetUserRoles
├── AspNetUserClaims
├── AspNetUserLogins
└── AspNetUserTokens

ApplicationUsers (Extended Profile)
├── PlacedItems → TreasureItems
├── CollectedItems → TreasureItems
└── Inventory → UserInventories

ItemTypes
└── TreasureItems

TreasureItems
├── ItemType (FK)
├── PlacedByUser (FK)
├── CollectedByUser (FK)
└── UserInventories

UserInventories
├── User (FK)
└── TreasureItem (FK)
```

## Sample Data Scripts

### Create Test Users
```sql
-- After running the API register endpoint
-- Users will be created in AspNetUsers and ApplicationUsers
```

### Create Test Treasures
```sql
INSERT INTO TreasureItems (Id, Name, Description, ItemTypeId, Latitude, Longitude, 
                          DiscoveryRadiusMeters, PointValue, IsCollected, 
                          PlacedByUserId, PlacedAt)
VALUES 
(NEWID(), 'Test Coin', 'A test treasure', 1, 37.7749, -122.4194, 5.0, 100, 0, 
 'YOUR_USER_ID', GETUTCDATE()),
(NEWID(), 'Test Gem', 'A rare test gem', 2, 37.7750, -122.4195, 10.0, 300, 0, 
 'YOUR_USER_ID', GETUTCDATE());
```

## Monitoring

### Check Active Connections
```sql
SELECT 
    DB_NAME(dbid) as DatabaseName,
    COUNT(dbid) as NumberOfConnections,
    loginame as LoginName
FROM sys.sysprocesses
WHERE dbid > 0
GROUP BY dbid, loginame
ORDER BY DB_NAME(dbid);
```

### View Recent Changes
```sql
-- Recent treasure placements
SELECT TOP 10 Name, PlacedAt, Latitude, Longitude 
FROM TreasureItems 
ORDER BY PlacedAt DESC;

-- Recent collections
SELECT TOP 10 ui.CollectedAt, ti.Name, au.DisplayName, ui.PointsEarned
FROM UserInventories ui
JOIN TreasureItems ti ON ui.TreasureItemId = ti.Id
JOIN ApplicationUsers au ON ui.UserId = au.Id
ORDER BY ui.CollectedAt DESC;
```
