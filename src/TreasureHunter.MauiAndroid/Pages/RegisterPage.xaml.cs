using TreasureHunter.MauiAndroid.Services;
using TreasureHunter.MauiAndroid.Services.Models;

namespace TreasureHunter.MauiAndroid.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly ITreasureApiService _apiService;

    public RegisterPage(ITreasureApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var displayName = DisplayNameEntry.Text?.Trim();
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;
        var confirmPassword = ConfirmPasswordEntry.Text;

        // Validate all fields filled
        if (string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(email) || 
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            await ShowStatus("Please fill in all fields");
            return;
        }

        // Check passwords match
        if (password != confirmPassword)
        {
            await ShowStatus("Passwords do not match");
            return;
        }

        // Check password length
        if (password.Length < 8)
        {
            await ShowStatus("Password must be at least 8 characters");
            return;
        }

        SetLoading(true);

        var request = new RegisterRequest
        {
            Email = email,
            UserName = email, // Using email as username
            Password = password,
            DisplayName = displayName
        };

        var response = await _apiService.RegisterAsync(request);

        SetLoading(false);

        if (response != null && !string.IsNullOrEmpty(response.Token))
        {
            // Auto-login successful, navigate to MainPage
            await Shell.Current.GoToAsync("//MainPage");
        }
        else
        {
            await ShowStatus("Registration failed. Email may already be in use.");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void SetLoading(bool isLoading)
    {
        RegisterButton.IsEnabled = !isLoading;
        BackButton.IsEnabled = !isLoading;
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
