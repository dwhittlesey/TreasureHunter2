# TreasureHunter2

Production-ready C# Treasure Hunter AR application using Clean Architecture principles.

## 🏗️ Architecture

This project follows **Clean Architecture** with clear separation of concerns:

```
TreasureHunter2/
├── src/
│   ├── TreasureHunter.Domain/         # Core business entities and interfaces
│   ├── TreasureHunter.Application/    # Business logic, DTOs, MediatR handlers
│   ├── TreasureHunter.Infrastructure/ # Data access, external services
│   └── TreasureHunter.API/           # Web API controllers and SignalR hubs
└── TreasureHunter.sln
```

## 🚀 Tech Stack

### Backend
- **ASP.NET Core 8.0** - Web API framework
- **Entity Framework Core 8.0** - ORM with SQL Server
- **MediatR** - CQRS pattern implementation
- **FluentValidation** - Input validation
- **JWT Bearer Authentication** - Secure authentication
- **SignalR** - Real-time treasure proximity updates

### Mobile
- **.NET MAUI Blazor Hybrid** - Cross-platform mobile app (see [MAUI_SETUP.md](MAUI_SETUP.md))

### Database
- **SQL Server** - Primary data store

## ✨ Features

### Core Functionality
1. **GPS-Based Treasure Placement** - Users can drop virtual items at their current location
2. **Proximity Detection** - Real-time distance and bearing calculations using Haversine formula
3. **Hot/Cold Feedback** - Dynamic proximity indicators (VERY_HOT, HOT, WARM, COOL, COLD)
4. **AR Visualization** - Ready for A-Frame or Three.js integration
5. **Gamification** - Points system with item collection and inventory
6. **Real-time Updates** - SignalR hub for live treasure proximity notifications

### Business Rules
- Users cannot collect their own treasures
- Discovery radius: 1-50 meters (configurable per item)
- Items have point values based on rarity
- Location updates every 5 seconds
- Inventory tracking with collection timestamps

## 🛠️ Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- SQL Server (or SQL Server Express)
- Visual Studio 2022 / VS Code / Rider

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/dwhittlesey/TreasureHunter2.git
cd TreasureHunter2
```

2. **Update database connection string**

Edit `src/TreasureHunter.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=TreasureHunterDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

3. **Apply database migrations**
```bash
cd src/TreasureHunter.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../TreasureHunter.API
dotnet ef database update --startup-project ../TreasureHunter.API
```

4. **Build the solution**
```bash
cd ../../
dotnet build
```

5. **Run the API**
```bash
cd src/TreasureHunter.API
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:5001/swagger`

## 📡 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token

### Treasure Management
- `POST /api/treasure` - Create/drop a treasure item
- `GET /api/treasure/nearby?latitude={lat}&longitude={lon}&radiusMeters={radius}` - Get nearby treasures
- `POST /api/treasure/{id}/collect` - Collect a treasure

### User
- `GET /api/user/me` - Get current user profile
- `GET /api/user/inventory` - Get user's collected items

### SignalR Hub
- `wss://localhost:5001/hubs/treasure` - Real-time treasure proximity updates

## 🔐 Authentication

The API uses JWT Bearer authentication. Include the token in requests:

```http
Authorization: Bearer {your-jwt-token}
```

Example registration:
```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "treasurehunter1",
    "email": "hunter@example.com",
    "password": "SecurePass123!",
    "displayName": "Treasure Hunter"
  }'
```

## 📱 Mobile App Setup

See [MAUI_SETUP.md](MAUI_SETUP.md) for detailed instructions on setting up the .NET MAUI Blazor Hybrid mobile application.

## 🗂️ Domain Entities

### TreasureItem
Core treasure entity with GPS coordinates, discovery radius, and point values.

### ItemType
Predefined treasure categories:
- **Common Coin** (100 points)
- **Rare Gem** (300 points)
- **Epic Treasure Chest** (500 points)

### ApplicationUser
Extended user profile with points and inventory tracking.

### UserInventory
Collection history with timestamps and points earned.

## 📊 Database Schema

```sql
TreasureItems (Id, Name, ItemTypeId, Latitude, Longitude, DiscoveryRadiusMeters, ...)
ItemTypes (Id, Name, BasePointValue, DefaultModelUrl, DefaultIconUrl)
ApplicationUsers (Id, DisplayName, TotalPoints, CreatedAt, LastActiveAt)
UserInventories (Id, UserId, TreasureItemId, CollectedAt, PointsEarned)
AspNetUsers (Identity tables)
```

## 🧪 Testing

### Example: Creating and Collecting a Treasure

1. Register a user
2. Login to get JWT token
3. Create a treasure at your location
4. Move within discovery radius
5. Collect the treasure via API or mobile app
6. Check your inventory and points

## 🌍 GPS Calculations

The application uses the **Haversine formula** for accurate distance calculations:

```csharp
// Calculate distance between two GPS coordinates
var distance = locationService.CalculateDistance(
    userLat, userLon, 
    treasureLat, treasureLon
); // Returns distance in meters

// Calculate bearing (direction)
var bearing = locationService.CalculateBearing(
    userLat, userLon,
    treasureLat, treasureLon
); // Returns bearing in degrees (0-360)
```

## 🔧 Configuration

### JWT Settings (appsettings.json)
```json
{
  "Jwt": {
    "Key": "YourVerySecretKeyThatShouldBeAtLeast32CharactersLong!",
    "Issuer": "TreasureHunter",
    "Audience": "TreasureHunterClients",
    "ExpiryMinutes": "60"
  }
}
```

### Game Constants
Modify `TreasureHunter.Domain/Constants/GameConstants.cs` to adjust:
- Discovery radius limits
- Point multipliers
- Location update intervals
- AR settings

## 🤝 Contributing

Contributions are welcome! Please follow the existing architecture patterns and code style.

## 📄 License

This project is licensed under the MIT License.

## 🎮 Game Mechanics

### Proximity Levels
- **VERY_HOT**: Within 25% of discovery radius
- **HOT**: Within 50% of discovery radius
- **WARM**: Within 75% of discovery radius
- **COOL**: Within 100% of discovery radius
- **COLD**: Within 200% of discovery radius
- **VERY_COLD**: Beyond 200% of discovery radius

### Point System
- Base points determined by ItemType
- Rare items: 3x multiplier
- Epic items: 5x multiplier
- Points awarded only on successful collection

## 🐛 Troubleshooting

**Database connection issues:**
- Verify SQL Server is running
- Check connection string in appsettings.json
- Ensure TrustServerCertificate=True if using local SQL Server

**JWT authentication errors:**
- Ensure JWT key is at least 32 characters
- Check token expiry settings
- Verify Bearer token is included in Authorization header

**SignalR connection issues:**
- Check CORS settings
- Verify WebSocket support on hosting platform
- Use WSS for secure connections in production

## 📞 Support

For issues and questions, please open an issue on GitHub.

---

Built with ❤️ using Clean Architecture and .NET 8.0