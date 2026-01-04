using TreasureHunter.MauiAndroid.Services;
using TreasureHunter.MauiAndroid.Services.Models;

namespace TreasureHunter.MauiAndroid.Pages;

public partial class LoginPage : ContentPage
{
    private readonly ITreasureApiService _apiService;

    public LoginPage(ITreasureApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
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

        var request = new LoginRequest
        {
            UserName = email,
            Password = password
        };

        var response = await _apiService.LoginAsync(request);

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

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("RegisterPage");
    }

    private void SetLoading(bool isLoading)
    {
        LoginButton.IsEnabled = !isLoading;
        RegisterButton.IsEnabled = !isLoading;
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
    }

    private async Task ShowStatus(string message)
    {
        StatusLabel.Text = message;
        StatusLabel.IsVisible = true;
        
        await Task.Delay(3000);
        
        StatusLabel.IsVisible = false;
    }
}
