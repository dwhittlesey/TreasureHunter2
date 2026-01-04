using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TreasureHunter.MauiAndroid.Services.Models;

namespace TreasureHunter.MauiAndroid.Services;

public class TreasureApiService : ITreasureApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPreferences _preferences;
    private const string AuthTokenKey = "auth_token";

    public TreasureApiService(IHttpClientFactory httpClientFactory, IPreferences preferences)
    {
        _httpClientFactory = httpClientFactory;
        _preferences = preferences;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("TreasureHunterAPI");
        
        var token = GetAuthToken();
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        return client;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var client = CreateClient();
            Debug.WriteLine($"Login request: {System.Text.Json.JsonSerializer.Serialize(request)}");
            
            var response = await client.PostAsJsonAsync("api/Auth/login", request);
            
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null && !string.IsNullOrEmpty(authResponse.Token))
                {
                    SetAuthToken(authResponse.Token);
                }
                return authResponse;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Login failed: {response.StatusCode} - {errorContent}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Login error: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var client = CreateClient();
            Debug.WriteLine($"Register request: {System.Text.Json.JsonSerializer.Serialize(request)}");
            
            var response = await client.PostAsJsonAsync("api/Auth/register", request);
            
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null && !string.IsNullOrEmpty(authResponse.Token))
                {
                    // Auto-login after successful registration
                    SetAuthToken(authResponse.Token);
                }
                return authResponse;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Registration failed: {response.StatusCode} - {errorContent}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Registration error: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    public async Task<List<TreasureItemDto>> GetNearbyItemsAsync(double lat, double lon, double radius)
    {
        try
        {
            var client = CreateClient();
            var url = $"/api/treasure/nearby?latitude={lat}&longitude={lon}&radiusMeters={radius}";
            Debug.WriteLine($"Get nearby items: {url}");
            
            var response = await client.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var items = await response.Content.ReadFromJsonAsync<List<TreasureItemDto>>();
                return items ?? new List<TreasureItemDto>();
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Get nearby items failed: {response.StatusCode} - {errorContent}");
            return new List<TreasureItemDto>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Get nearby items error: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return new List<TreasureItemDto>();
        }
    }

    public async Task<PlaceItemResponse?> PlaceItemAsync(PlaceItemRequest request)
    {
        try
        {
            var client = CreateClient();
            Debug.WriteLine($"Place item request: {System.Text.Json.JsonSerializer.Serialize(request)}");
            
            var response = await client.PostAsJsonAsync("/api/treasure", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TreasureItemDto>();
                if (result != null)
                {
                    return new PlaceItemResponse
                    {
                        ItemId = result.Id,
                        Success = true,
                        Message = "Treasure placed successfully!"
                    };
                }
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Place item failed: {response.StatusCode} - {errorContent}");
            return new PlaceItemResponse
            {
                Success = false,
                Message = $"Failed to place treasure: {errorContent}"
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Place item error: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return new PlaceItemResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public async Task<CollectItemResponse?> CollectItemAsync(CollectItemRequest request)
    {
        try
        {
            var client = CreateClient();
            var locationData = new { Latitude = request.UserLatitude, Longitude = request.UserLongitude };
            Debug.WriteLine($"Collect item {request.ItemId} at location: {System.Text.Json.JsonSerializer.Serialize(locationData)}");
            
            var response = await client.PostAsJsonAsync($"/api/treasure/{request.ItemId}/collect", locationData);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UserInventoryDto>();
                if (result != null)
                {
                    return new CollectItemResponse
                    {
                        Success = true,
                        Message = "Treasure collected!",
                        PointsEarned = result.PointsEarned,
                        TotalPoints = 0, // Will be updated from user profile
                        PlaySound = true
                    };
                }
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Collect item failed: {response.StatusCode} - {errorContent}");
            return new CollectItemResponse
            {
                Success = false,
                Message = errorContent
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Collect item error: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return new CollectItemResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public string? GetAuthToken()
    {
        return _preferences.Get<string?>(AuthTokenKey, null);
    }

    public void SetAuthToken(string token)
    {
        _preferences.Set(AuthTokenKey, token);
    }

    public void ClearAuthToken()
    {
        _preferences.Remove(AuthTokenKey);
    }

    // Helper DTO for collect response
    private class UserInventoryDto
    {
        public int PointsEarned { get; set; }
    }
}
