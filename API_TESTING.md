# API Testing Guide

This guide provides sample API requests for testing the TreasureHunter API.

## Base URL
- Development: `https://localhost:5001`
- HTTP: `http://localhost:5000`

## Testing with cURL

### 1. Register a New User

```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "hunter1",
    "email": "hunter1@example.com",
    "password": "SecurePass123!",
    "displayName": "Treasure Hunter 1"
  }'
```

**Expected Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "abc123...",
  "userName": "hunter1",
  "email": "hunter1@example.com"
}
```

### 2. Login

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "hunter1",
    "password": "SecurePass123!"
  }'
```

**Save the token from response for authenticated requests!**

### 3. Create a Treasure (Authenticated)

```bash
TOKEN="YOUR_JWT_TOKEN_HERE"

curl -X POST https://localhost:5001/api/treasure \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Golden Coin",
    "description": "A shiny golden coin hidden in the park",
    "itemTypeId": 1,
    "latitude": 37.7749,
    "longitude": -122.4194,
    "altitude": 10.5,
    "discoveryRadiusMeters": 5.0
  }'
```

**Response:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Golden Coin",
  "description": "A shiny golden coin hidden in the park",
  "itemTypeId": 1,
  "itemTypeName": "Common Coin",
  "latitude": 37.7749,
  "longitude": -122.4194,
  "altitude": 10.5,
  "discoveryRadiusMeters": 5.0,
  "pointValue": 100,
  "isCollected": false,
  "placedByUserId": "abc123...",
  "placedByUserName": "hunter1",
  "placedAt": "2024-01-15T10:30:00Z",
  "modelUrl": "/models/coin.gltf",
  "iconUrl": "/icons/coin.png"
}
```

### 4. Get Nearby Treasures (Authenticated)

```bash
curl -X GET "https://localhost:5001/api/treasure/nearby?latitude=37.7749&longitude=-122.4194&radiusMeters=100" \
  -H "Authorization: Bearer $TOKEN"
```

**Response:**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Golden Coin",
    "latitude": 37.7749,
    "longitude": -122.4194,
    "distanceMeters": 5.3,
    "bearingDegrees": 45.2,
    "pointValue": 100,
    "discoveryRadiusMeters": 5.0
  }
]
```

### 5. Collect a Treasure (Authenticated)

```bash
TREASURE_ID="550e8400-e29b-41d4-a716-446655440000"

curl -X POST "https://localhost:5001/api/treasure/$TREASURE_ID/collect" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "latitude": 37.7749,
    "longitude": -122.4194
  }'
```

**Response:**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440001",
  "userId": "abc123...",
  "treasureItemId": "550e8400-e29b-41d4-a716-446655440000",
  "treasureItemName": "Golden Coin",
  "collectedAt": "2024-01-15T10:35:00Z",
  "pointsEarned": 100
}
```

### 6. Get Current User Profile (Authenticated)

```bash
curl -X GET https://localhost:5001/api/user/me \
  -H "Authorization: Bearer $TOKEN"
```

**Response:**
```json
{
  "id": "abc123...",
  "userName": "hunter1",
  "email": "hunter1@example.com",
  "displayName": "Treasure Hunter 1",
  "totalPoints": 100
}
```

### 7. Get User Inventory (Authenticated)

```bash
curl -X GET https://localhost:5001/api/user/inventory \
  -H "Authorization: Bearer $TOKEN"
```

**Response:**
```json
[
  {
    "id": "660e8400-e29b-41d4-a716-446655440001",
    "userId": "abc123...",
    "treasureItemId": "550e8400-e29b-41d4-a716-446655440000",
    "treasureItemName": "Golden Coin",
    "collectedAt": "2024-01-15T10:35:00Z",
    "pointsEarned": 100
  }
]
```

## Testing with Postman

### Import Collection

Create a Postman collection with these requests:

1. **Register** - POST `{{baseUrl}}/api/auth/register`
2. **Login** - POST `{{baseUrl}}/api/auth/login`
3. **Create Treasure** - POST `{{baseUrl}}/api/treasure`
4. **Get Nearby** - GET `{{baseUrl}}/api/treasure/nearby?latitude=37.7749&longitude=-122.4194&radiusMeters=100`
5. **Collect Treasure** - POST `{{baseUrl}}/api/treasure/{{treasureId}}/collect`
6. **Get Profile** - GET `{{baseUrl}}/api/user/me`
7. **Get Inventory** - GET `{{baseUrl}}/api/user/inventory`

### Environment Variables
- `baseUrl`: `https://localhost:5001`
- `token`: (Set from login response)
- `treasureId`: (Set from create treasure response)

### Authorization
Add to collection settings:
- Type: Bearer Token
- Token: `{{token}}`

## Testing SignalR Connection

### Using JavaScript Client

```html
<!DOCTYPE html>
<html>
<head>
    <script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@latest/dist/browser/signalr.min.js"></script>
</head>
<body>
    <button onclick="updateLocation()">Update Location</button>
    <div id="output"></div>

    <script>
        const token = "YOUR_JWT_TOKEN";
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("https://localhost:5001/hubs/treasure", {
                accessTokenFactory: () => token
            })
            .build();

        connection.on("ProximityUpdate", (treasures) => {
            document.getElementById("output").innerHTML = 
                JSON.stringify(treasures, null, 2);
        });

        connection.on("TreasureDiscoverable", (treasures) => {
            alert("Treasure in range: " + treasures[0].name);
        });

        connection.start()
            .then(() => console.log("Connected to hub"))
            .catch(err => console.error(err));

        function updateLocation() {
            navigator.geolocation.getCurrentPosition((position) => {
                connection.invoke("UpdateLocation", 
                    position.coords.latitude, 
                    position.coords.longitude);
            });
        }
    </script>
</body>
</html>
```

## Complete Testing Scenario

### Scenario: Two Users Finding Each Other's Treasures

```bash
# Terminal 1: User 1 (Place treasure)
# Register user 1
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"userName": "alice", "email": "alice@test.com", "password": "Test123!", "displayName": "Alice"}'

# Save token as TOKEN1
TOKEN1="<token_from_response>"

# Alice places a treasure at Golden Gate Park
curl -X POST https://localhost:5001/api/treasure \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN1" \
  -d '{
    "name": "Alice'\''s Golden Coin",
    "description": "Found near the bridge",
    "itemTypeId": 1,
    "latitude": 37.7695,
    "longitude": -122.4868,
    "discoveryRadiusMeters": 10.0
  }'

# Terminal 2: User 2 (Find and collect)
# Register user 2
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"userName": "bob", "email": "bob@test.com", "password": "Test123!", "displayName": "Bob"}'

# Save token as TOKEN2
TOKEN2="<token_from_response>"

# Bob searches for treasures near Golden Gate Park
curl -X GET "https://localhost:5001/api/treasure/nearby?latitude=37.7695&longitude=-122.4868&radiusMeters=50" \
  -H "Authorization: Bearer $TOKEN2"

# Bob collects Alice's treasure (within 10m)
TREASURE_ID="<id_from_nearby_response>"
curl -X POST "https://localhost:5001/api/treasure/$TREASURE_ID/collect" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN2" \
  -d '{"latitude": 37.7695, "longitude": -122.4868}'

# Bob checks his inventory
curl -X GET https://localhost:5001/api/user/inventory \
  -H "Authorization: Bearer $TOKEN2"

# Bob checks his profile (should have 100 points)
curl -X GET https://localhost:5001/api/user/me \
  -H "Authorization: Bearer $TOKEN2"
```

## Error Scenarios to Test

### 1. Cannot collect own treasure
```bash
# User tries to collect their own treasure
# Expected: 400 Bad Request - "You cannot collect your own treasure items"
```

### 2. Too far away to collect
```bash
# User tries to collect from 100m away
# Expected: 400 Bad Request - "You are too far away. Distance: 100.00m, Required: 5.00m"
```

### 3. Treasure already collected
```bash
# Another user tries to collect the same treasure
# Expected: 400 Bad Request - "This item has already been collected"
```

### 4. Invalid authentication
```bash
# Request without token
# Expected: 401 Unauthorized
```

### 5. Invalid treasure ID
```bash
# Request with non-existent ID
# Expected: 404 Not Found - "Treasure item not found"
```

## Load Testing

### Using Apache Bench
```bash
# Test login endpoint (100 requests, 10 concurrent)
ab -n 100 -c 10 -p login.json -T application/json \
  https://localhost:5001/api/auth/login

# login.json:
# {"userName":"hunter1","password":"SecurePass123!"}
```

### Using k6
```javascript
import http from 'k6/http';
import { check } from 'k6';

export default function () {
  const payload = JSON.stringify({
    userName: 'hunter1',
    password: 'SecurePass123!'
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  let response = http.post('https://localhost:5001/api/auth/login', payload, params);
  
  check(response, {
    'is status 200': (r) => r.status === 200,
    'has token': (r) => r.json('token') !== '',
  });
}
```

## Swagger UI

Access interactive API documentation:
- **URL**: `https://localhost:5001/swagger`
- Test all endpoints with a UI
- View request/response schemas
- Try out authentication flow

## Common HTTP Status Codes

- `200 OK` - Successful GET/POST
- `201 Created` - Resource created successfully
- `400 Bad Request` - Invalid input or business rule violation
- `401 Unauthorized` - Missing or invalid authentication token
- `404 Not Found` - Resource doesn't exist
- `500 Internal Server Error` - Server error

## Next Steps

1. Test all endpoints with valid data
2. Test error scenarios
3. Test with SignalR for real-time updates
4. Perform load testing
5. Monitor database for data integrity
6. Test on mobile devices with actual GPS coordinates
