using TreasureHunter.MauiAndroid.Services.Models;

namespace TreasureHunter.MauiAndroid.Services;

public interface ITreasureApiService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<List<TreasureItemDto>> GetNearbyItemsAsync(double lat, double lon, double radius);
    Task<PlaceItemResponse?> PlaceItemAsync(PlaceItemRequest request);
    Task<CollectItemResponse?> CollectItemAsync(CollectItemRequest request);
    string? GetAuthToken();
    void SetAuthToken(string token);
    void ClearAuthToken();
}
