using System;
using System.Linq;
using System.Net.Http;
using Xamarin.Forms;
using Flurl.Http;
using System.Threading.Tasks;

namespace KnApp
{
	public partial class SignUpPage : ContentPage
	{


        //public bool ButtonEnabled { get; set; }

        public SignUpPage()
		{
			InitializeComponent();
			//ButtonEnabled = false;
		}

        async void OnSignUpButtonClicked(object sender, EventArgs e)
		{

			var user = new User()
			{
				Email = emailEntry.Text,
				Password = passwordEntry.Text,
				PasswordConfirm = passwordConfirmEntry.Text,
			};

			// Sign up logic goes here

			var signupValid = AreDetailsValid(user);
			var registered = false;

			SignupButton.IsBusy = true;

			//return;

			try
			{
				//var resp = await "http://192.168.0.50:5000/api/items".PostMultipartAsync(mp => mp
				//	.AddStringParts(new { Lat = location.Latitude, Long = location.Longitude })
				//	.AddFile("file1", PhotoPath));

				var response = await (App.Host + "/api/user/register/").AllowAnyHttpStatus().PostUrlEncodedAsync(new
				{
					Email = user.Email,
					Password = user.Password,
					PasswordConfirm = user.PasswordConfirm
				});


				Console.WriteLine("Status", response.StatusCode);


				if (response.StatusCode == 200)
				{
					var result = await response.GetStringAsync();
					Console.WriteLine($"Success! {result}");
					await DisplayAlert("Ooops!", result, "OK");
					registered = true;
				}

				else if (response.StatusCode < 300 && response.StatusCode > 200)
				{
					SignupButton.IsBusy = false;
					var result = await response.GetStringAsync();
					Console.WriteLine($"Success! {result}");
					await DisplayAlert("Ooops!", "Något fel inträffade: {result}", "OK");
				}
				else if (response.StatusCode < 500)
				{
					SignupButton.IsBusy = false;
					var error = await response.GetStringAsync();
					Console.WriteLine($"You did something wrong! {error}");
					await DisplayAlert("Ooops!", error, "OK");


				}
				else
				{
					SignupButton.IsBusy = false;
					var error = await response.GetStringAsync();
					Console.WriteLine($"We did something wrong! {error}");
					await DisplayAlert("Ooops!", error, "OK");
				}


				Console.Write(response);

            }
			catch (FlurlHttpException flurlexception)
			{
                //var error = await flurlexception.GetResponseStringAsync<TError>();
                //await DisplayAlert("Ooops!", "Emailadressen finns redan registrerad", "OK");

                //var a = flurlexception.GetResponseStringAsync();

                Console.WriteLine(flurlexception);
                //Console.WriteLine(flurlexception.GetResponseStringAsync());
            }


			if (signupValid && registered)
			{
				var rootPage = Navigation.NavigationStack.FirstOrDefault();
				if (rootPage != null)
				{
					//App.IsUserLoggedIn = true;
					Navigation.InsertPageBefore(new LoginPage(), Navigation.NavigationStack.First());
					await Navigation.PopToRootAsync();
				}
			}
			else
			{
				SignupButton.IsBusy = false;
				messageLabel.Text = "Sign up failed";
			}
		}

		void FormIsValid(object sender, EventArgs e)
		{
			var user = new User()
			{
				Email = emailEntry.Text,
				Password = passwordEntry.Text,
				PasswordConfirm = passwordConfirmEntry.Text,
			};

			var text = ((Entry)sender).Text; //cast sender to access the properties of the Entry

			var valid = (!string.IsNullOrWhiteSpace(user.Email) && !string.IsNullOrWhiteSpace(user.Password) && !string.IsNullOrWhiteSpace(user.Email) && user.Email.Contains("@") && (user.Password == user.PasswordConfirm) );

			if (valid)
            {
				SignupButton.IsEnabled = true;
			} else
            {
				SignupButton.IsEnabled = false;
			}
		}

		bool AreDetailsValid(User user)
		{
			
			return (!string.IsNullOrWhiteSpace(user.Email) && !string.IsNullOrWhiteSpace(user.Password) && !string.IsNullOrWhiteSpace(user.Email) && user.Email.Contains("@"));
		}
	}
}
