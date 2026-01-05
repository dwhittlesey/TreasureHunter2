using TreasureHunter.MauiAndroid.Services;
using TreasureHunter.MauiAndroid.Services.Models;

namespace TreasureHunter.MauiAndroid.Pages;

public partial class LoginPage : ContentPage
{
    private readonly ITreasureApiService _apiService;
    private CancellationTokenSource? _navigationCts;

    public LoginPage(ITreasureApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _navigationCts = new CancellationTokenSource();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Cancel any pending operations
        _navigationCts?.Cancel();
        _navigationCts?.Dispose();
        _navigationCts = null;
        
        // Explicitly unfocus controls to prevent race condition
        EmailEntry?.Unfocus();
        PasswordEntry?.Unfocus();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            await ShowStatus("Please enter email and password");
            return;
        }

        SetLoading(true);

        try
        {
            var request = new LoginRequest
            {
                UserName = email,
                Password = password
            };

            var response = await _apiService.LoginAsync(request);

            // Check if page was navigated away during async operation
            if (_navigationCts?.IsCancellationRequested == true)
            {
                return;
            }

            SetLoading(false);

            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                // Navigate to MainPage
                await Shell.Current.GoToAsync("///MainPage");
            }
            else
            {
                await ShowStatus("Login failed. Please check your credentials.");
            }
        }
        catch (Exception ex) when (_navigationCts?.IsCancellationRequested == true)
        {
            // Silently handle cancellation due to navigation
            return;
        }
        catch (Exception ex)
        {
            if (_navigationCts?.IsCancellationRequested == false)
            {
                SetLoading(false);
                await ShowStatus($"Error: {ex.Message}");
            }
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("RegisterPage");
    }

    private void SetLoading(bool isLoading)
    {
        // Check if controls are still available
        if (LoginButton != null)
            LoginButton.IsEnabled = !isLoading;
        if (RegisterButton != null)
            RegisterButton.IsEnabled = !isLoading;
        if (LoadingIndicator != null)
        {
            LoadingIndicator.IsRunning = isLoading;
            LoadingIndicator.IsVisible = isLoading;
        }
    }

    private async Task ShowStatus(string message)
    {
        if (StatusLabel == null || _navigationCts?.IsCancellationRequested == true)
            return;

        StatusLabel.Text = message;
        StatusLabel.IsVisible = true;
        
        try
        {
            await Task.Delay(3000, _navigationCts?.Token ?? CancellationToken.None);
            
            if (StatusLabel != null && _navigationCts?.IsCancellationRequested == false)
            {
                StatusLabel.IsVisible = false;
            }
        }
        catch (TaskCanceledException)
        {
            // Expected when navigating away
        }
    }
}
