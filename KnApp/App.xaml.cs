using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace KnApp
{
    public partial class App : Application
    {
        public static string Host = "https://knapp.azurewebsites.net";
        public static bool IsUserLoggedIn {
            get {
                return Token != null;
            }
        }

        public static string Token {
            get {
                if (Preferences.ContainsKey("token"))
                {
                    return Preferences.Get("token", null);
                }
                return null;

            }
            set {
                Preferences.Set("token", value);
            }
        }

        public App()
        {
            if (!IsUserLoggedIn)
            {
                MainPage = new NavigationPage(new LoginPage());
            }
            else
            {
                MainPage = new NavigationPage(new KnApp.MainPage());
            }
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
