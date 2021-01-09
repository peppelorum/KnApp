using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Xamarin.Forms;
using Xamarin.Essentials;
using FFImageLoading;

namespace KnApp
{
	public partial class MainPage : ContentPage
	{

        CancellationTokenSource cts;
        public String PhotoPath { get; private set; }
        Location location;

        public MainPage()
		{
			InitializeComponent();
		}


        public async Task GetCurrentLocation()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                cts = new CancellationTokenSource();
                location = await Geolocation.GetLocationAsync(request, cts.Token);

                if (location != null)
                {
                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                Console.WriteLine(fnsEx.Message);
                await DisplayAlert("Ooops!", fnsEx.Message, "OK");
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException fneEx)
            {
                Console.WriteLine(fneEx.Message);
                await DisplayAlert("Ooops!", fneEx.Message, "OK");
                // Handle not enabled on device exception
            }
            catch (PermissionException pEx)
            {
                Console.WriteLine(pEx.Message);
                await DisplayAlert("Ooops!", pEx.Message, "OK");
                // Handle permission exception
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await DisplayAlert("Ooops!", ex.Message, "OK");
                // Unable to get location
            }
        }

        async Task PostLocation()
        {
            //indicator.Hidden = false;
            //indicator.StartAnimating();

            await GetCurrentLocation();

            try
            {
                var resp = await (App.Host + "/api/items").PostMultipartAsync(mp => mp
                    .AddStringParts(new { Lat = location.Latitude, Long = location.Longitude }));
            }
            catch (FlurlHttpException e)
            {
                Console.WriteLine(e);
            }
            //indicator.StopAnimating();
            //indicator.Hidden = true;
        }

        async Task PostImageAndLocation()
        {
            //photoIndicator.Hidden = false;
            //photoIndicator.StartAnimating();

            await GetCurrentLocation();

            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();
                await LoadPhotoAsync(photo);
                Console.WriteLine($"CapturePhotoAsync COMPLETED: {PhotoPath}");

                //await GetCurrentLocation();

                try
                {
                    var resp = await (App.Host + "/api/items").PostMultipartAsync(mp => mp
                        .AddStringParts(new { Lat = location.Latitude, Long = location.Longitude })
                        .AddFile("file1", PhotoPath));
                }
                catch (FlurlHttpException e)
                {
                    Console.WriteLine(e);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"CapturePhotoAsync THREW: {ex.Message}");
            }

            //photoIndicator.StopAnimating();
            //photoIndicator.Hidden = true;
        }

        async Task LoadPhotoAsync(FileResult photo)
        {
            // canceled
            if (photo == null)
            {
                PhotoPath = null;
                return;
            }
            // save the file into local storage

            byte[] imageData;

            //Stream stream = 
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    photo.CopyTo(ms);
            //    imageData = ms.ToArray();
            //}

            //byte[] resizedImage = await ImageResizer.ResizeImage(imageData, 400, 400);

            //this._photo.Source = ImageSource.FromStream(() => new MemoryStream(resizedImage));



            //var newFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            //using (var stream = await photo.OpenReadAsync())
            //using (var newStream = File.OpenWrite(newFile))
            //    await stream.CopyToAsync(newStream);




            //await ImageService.Instance.LoadFile()


            //var newFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            //var stream = await ImageService.Instance.LoadFile(newFile)
            //    .DownSample(width: 800)
            //    .AsJPGStreamAsync();

            //using (MemoryStream ms = new MemoryStream())
            //{
            //    stream.CopyTo(ms);
            //    imageData = ms.ToArray();
            //}

            //using (var newStream = File.OpenWrite(newFile))
            //    await stream.CopyToAsync(stream);




            var newFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            var stream = await photo.OpenReadAsync();

            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                imageData = ms.ToArray();
            }

            byte[] resizedImage = await ImageResizer.ResizeImage(imageData, 800);
            File.WriteAllBytes(newFile, resizedImage);

            PhotoPath = newFile;
        }

        private async void OnImageLocationClicked(object sender, EventArgs e)
        {
            ImageButtonWithSpinner.IsBusy = true;

            await PostImageAndLocation();

            ImageButtonWithSpinner.IsBusy = false;
        }

        private async void OnLocationClicked(object sender, EventArgs e)
        {
            ButtonWithSpinner.IsBusy = true;

            await PostLocation();

            ButtonWithSpinner.IsBusy = false;
        }


        async void OnLogoutButtonClicked(object sender, EventArgs e)
		{
			App.Token = null;
			Navigation.InsertPageBefore(new LoginPage(), this);
			await Navigation.PopAsync();
		}
	}
}
