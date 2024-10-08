using Azure.Messaging.EventHubs.Processor;
using Azure.Messaging.EventHubs;
using Azure.Storage.Blobs;
using CropBox.Services;
using Firebase.Auth;
using System.Collections.Concurrent;
using Microsoft.Azure.Devices;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CropBox.Models;
using CropBox.Enums;
using System.Configuration;

namespace CropBox.Views.Shared;

/// <summary>
/// Team name : CropBox  
/// Team number : F
/// Winter 4/28/2023 
/// 420-6A6-AB
/// login page is used to login to the app using firebase authentication
/// </summary>
public partial class LoginPage : ContentPage
{
    const string OWNER_USER = "@owner.cropbox.com";
    const string TECH_USER = "@tech.cropbox.com";

    static UserTypes route;
    public static UserTypes Route { get { return route; } }

    /// <summary>
    /// login page constructor is used to initialize the login page
    /// </summary>
	public LoginPage()
	{
		InitializeComponent();
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
    }
    protected async override void OnAppearing()
    {
        try
        {
            if (App.telemetryHelper.processor.IsRunning == false)
                await App.telemetryHelper.Run();
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR! " + e.Message);
        }



    }

    /// <summary>
    /// LoginButton_Clicked is used to login to the app using firebase authentication
    /// </summary>
    /// <param name="sender"> sender is object represent the sender of the event</param>
    /// <param name="e">e is EventArgs represent the event arguments</param>
    // code from Assignment 2 Auth but modified to fit our need  
    private async void LoginButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            string username = ((Entry)FindByName("usernameInputed")).Text;

            IEnumerable<ConnectionProfile> profiles = Connectivity.Current.ConnectionProfiles;

            if (!profiles.Contains(ConnectionProfile.WiFi))
            {
                throw new Exception("Error. Not Connected to the internet.");
            }
            await AuthService
                .Client
                .FetchSignInMethodsForEmailAsync(username);
            AuthService.UserCreds = await AuthService
                .Client
                .SignInWithEmailAndPasswordAsync(username, ((Entry)FindByName("passwordInputed")).Text);
            await DisplayAlert("Login", "Logged in successfully.", "Ok");
            ((Grid)FindByName("login_gridview")).IsVisible = false;
            ((Grid)FindByName("logout_gridview")).IsVisible = true;
            if (username.Contains(OWNER_USER))
            {
                route = UserTypes.Owner;

                await Shell.Current.GoToAsync($"//{UserTypes.Owner}");
            }
            else if (username.Contains(TECH_USER)) {
                route = UserTypes.Technician;

                await Shell.Current.GoToAsync($"//{UserTypes.Technician}");
            }

            
        }
        catch (FirebaseAuthException exception)
        {
            await DisplayAlert("Alert", exception.Reason.ToString(), "Ok");
        }
        catch (Exception exception)
        {
            await DisplayAlert("Response Null", exception.Message, "Ok");
        }
    }
    /// <summary>
    /// LogoutButton_Clicked is used to logout from the app
    /// </summary>
    /// <param name="sender"> sender is object represent the sender of the event</param>
    /// <param name="e">e is EventArgs represent the event arguments</param>
    private async void LogoutButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            AuthService.Client.SignOut();
            AuthService.UserCreds = null;
            ((Entry)FindByName("usernameInputed")).Text = String.Empty;
            ((Entry)FindByName("passwordInputed")).Text = String.Empty;
            ((Grid)FindByName("logout_gridview")).IsVisible = false;
            ((Grid)FindByName("login_gridview")).IsVisible = true;
            DataPage.pickerUpdated = false;
            await Shell.Current.GoToAsync($"//Login");

        }

        catch (Exception exception)
        {
            await DisplayAlert("Alert", exception.Message, "Ok");
        }
    }
    private async void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess != NetworkAccess.Internet)
        {
            // Handle the network connection loss here
            // You can display an error message or perform any other necessary action
            await DisplayAlert("Network Error", "Connection lost. Please check your network settings.", "OK");
        }
    }
}