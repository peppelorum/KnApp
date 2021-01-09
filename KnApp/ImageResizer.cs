using System;
using System.IO;
using System.Threading.Tasks;

#if __IOS__
using System.Drawing;
using UIKit;
using CoreGraphics;
#endif

#if __ANDROID__
using Android.Graphics;
#endif

#if WINDOWS_UWP
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
#endif

namespace KnApp
{
    public static class ImageResizer
    {
        static ImageResizer()
        {
        }

        public static async Task<byte[]> ResizeImage(byte[] imageData, float width)
        {
#if __IOS__
            return ResizeImageIOS(imageData, width);
#endif
#if __ANDROID__
			return ResizeImageAndroid(imageData, width);
#endif
#if WINDOWS_UWP
            return await ResizeImageWindows(imageData, width);
#endif
        }


#if __IOS__
        public static byte[] ResizeImageIOS(byte[] imageData, float width)
        {
            UIImage originalImage = ImageFromByteArray(imageData);
            UIImageOrientation orientation = originalImage.Orientation;
            float height;

            if (originalImage.Size.Height > originalImage.Size.Width)
            {
                float aspect = ((float)(originalImage.Size.Width / originalImage.Size.Height));
                height = width * aspect;
            }
            else
            {
                float aspect = ((float)(originalImage.Size.Height / originalImage.Size.Width));
                height = width * aspect;
            }

            //create a 24bit RGB image
            using (CGBitmapContext context = new CGBitmapContext(IntPtr.Zero,
                                                 (int)width, (int)height, 8,
                                                 4 * (int)width, CGColorSpace.CreateDeviceRGB(),
                                                 CGImageAlphaInfo.PremultipliedFirst))
            {

                RectangleF imageRect = new RectangleF(0, 0, width, height);

                // draw the image
                context.DrawImage(imageRect, originalImage.CGImage);

                UIKit.UIImage resizedImage = UIKit.UIImage.FromImage(context.ToImage(), 0, orientation);

                // save the image as a jpeg
                return resizedImage.AsJPEG().ToArray();
            }
        }

        public static UIKit.UIImage ImageFromByteArray(byte[] data)
        {
            if (data == null)
            {
                return null;
            }

            UIKit.UIImage image;
            try
            {
                image = new UIKit.UIImage(Foundation.NSData.FromArray(data));
            }
            catch (Exception e)
            {
                Console.WriteLine("Image load failed: " + e.Message);
                return null;
            }
            return image;
        }
#endif

#if __ANDROID__
		
		public static byte[] ResizeImageAndroid (byte[] imageData, float width)
		{
			// Load the bitmap
			Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
            float height;

            if (originalImage.Height > originalImage.Width)
            {
                float aspect = ((float)(originalImage.Width / originalImage.Height));
                height = width * aspect;
            }
            else
            {
                float aspect = ((float)(originalImage.Height / originalImage.Width));
                height = width * aspect;
            }

            Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)width, (int)height, false);

			using (MemoryStream ms = new MemoryStream())
			{
				resizedImage.Compress (Bitmap.CompressFormat.Jpeg, 100, ms);
				return ms.ToArray ();
			}
		}

#endif

#if WINDOWS_UWP

        public static async Task<byte[]> ResizeImageWindows(byte[] imageData, float width, float height)
        {
            byte[] resizedData;

            using (var streamIn = new MemoryStream(imageData))
            {
                using (var imageStream = streamIn.AsRandomAccessStream())
                {
                    var decoder = await BitmapDecoder.CreateAsync(imageStream);
                    var resizedStream = new InMemoryRandomAccessStream();
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(resizedStream, decoder);
                    encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;
                    encoder.BitmapTransform.ScaledHeight = (uint)height;
                    encoder.BitmapTransform.ScaledWidth = (uint)width;
                    await encoder.FlushAsync();
                    resizedStream.Seek(0);
                    resizedData = new byte[resizedStream.Size];
                    await resizedStream.ReadAsync(resizedData.AsBuffer(), (uint)resizedStream.Size, InputStreamOptions.None);                  
                }                
            }

            return resizedData;
        }

#endif
    }
}

