using CropBox.Config;
using Firebase.Auth.Providers;
using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CropBox.Services
{
    /// <summary>
    /// Team name : CropBox  
    /// Team number : F
    /// Winter 4/28/2023 
    /// 420-6A6-AB
    /// AuthService class is used to store the firebase authentication configuration
    /// </summary>
    // this code was provide in the lab instructions
    internal class AuthService
    {
        // Configure...
        private static FirebaseAuthConfig config = new FirebaseAuthConfig
        {
            ApiKey = ResourceStrings.Apikey,
            AuthDomain = ResourceStrings.AuthorizedDomain,
            Providers = new FirebaseAuthProvider[]
            {
                // Add and configure individual providers
                new EmailProvider()
            },
        };
        // ...and create your FirebaseAuthClient
        public static FirebaseAuthClient Client { get; } = new FirebaseAuthClient(config);
        public static UserCredential UserCreds { get; set; }
    }
}
