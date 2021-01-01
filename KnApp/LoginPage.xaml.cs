using System;
using System.Net.Http;
using Data;
using Flurl.Http;
using Xamarin.Forms;

namespace KnApp
{
	public partial class LoginPage : ContentPage
	{
		public LoginPage()
		{
			InitializeComponent();
		}

		async void OnSignUpButtonClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new SignUpPage());
		}

		async void OnLoginButtonClicked(object sender, EventArgs e)
		{
			var user = new User
			{
				Email = usernameEntry.Text,
				Password = passwordEntry.Text
			};

			try
			{
				
				var response = await (App.Host +"/api/user/login/").AllowAnyHttpStatus().PostUrlEncodedAsync(user);
				Console.WriteLine("Status", response.StatusCode);

				if (response.StatusCode == 200 || response.StatusCode == 201)
				{
					var result = await response.GetJsonAsync<Token>();
					App.Token = result.APIToken.ToString();

					Navigation.InsertPageBefore(new MainPage(), this);
					await Navigation.PopAsync();

					Console.WriteLine($"Success! {result}");
                    //await DisplayAlert("Ooops!", "Du är nu on, "OK");
                }
				else
				{
					var error = await response.GetStringAsync();
					Console.WriteLine($"We did something wrong! {error}");
					await DisplayAlert("Ooops!", error, "OK");
				}
			}
			catch (FlurlHttpException flurlexception)
			{
				var error = await flurlexception.GetResponseStringAsync();
                await DisplayAlert("Ooops!", error, "OK");

                Console.WriteLine(error);
			}
		}
	}
}
