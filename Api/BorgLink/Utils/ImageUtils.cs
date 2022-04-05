using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BorgLink.Utils
{
    /// <summary>
    /// For image related functions
    /// </summary>
    public class ImageUtils
    {
        /// <summary>
        /// This is used to convert an array of hex pixals back into an argb image
        /// </summary>
        /// <param name="hexValues">The string hex values to convert to image (flat)</param>
        /// <returns>A 2d bitmap</returns>
        public static Bitmap ConvertBorgToBitmap(List<byte[]> hexValues)
        {
            // Assuming regular image ie. 24x24, 12x12, 48x48 etc.
            var sqrt = (int)Math.Sqrt(hexValues.Count());
            var bitmap = new Bitmap(sqrt, sqrt, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int i = 0; i < hexValues.Count(); i++)
            {
                // Default white
                var pixal = Color.Transparent;

                // If specified then isnt white
                if (hexValues[i].Any())
                {
                    // Get a string from bytes
                    var strValues = System.Text.Encoding.Default.GetString(hexValues[i]);

                    // We dont want the null values - so take everything before
                    strValues = strValues.Split(new[] { '\0' }, 2)?.FirstOrDefault()?.Trim();

                    // If we have a string to parse, parse
                    if (!string.IsNullOrEmpty(strValues))
                    {
                        // Parse and turn to int32
                        var convertedHexValue = Convert.ToInt32(strValues, 16);

                        // Create color from argb int32
                        pixal = Color.FromArgb(convertedHexValue);
                    }
                }

                // Define 2d coords
                var y = i / sqrt;
                var x = i % sqrt;

                // Set in place
                bitmap.SetPixel(x, y, pixal);
            }

            // Return the built up image
            return bitmap;
        }

        /// <summary>
        /// Method to "convert" an Image object into a byte array, formatted in PNG file format, which 
        /// provides lossless compression. This can be used together with the GetImageFromByteArray() 
        /// method to provide a kind of serialization / deserialization. 
        /// </summary>
        /// <param name="theImage">Image object, must be convertable to PNG format</param>
        /// <returns>byte array image of a PNG file containing the image</returns>
        public static byte[] CopyImageToByteArray(Image theImage)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                theImage.Save(memoryStream, ImageFormat.Png);
                return memoryStream.ToArray();
            }
        }

        public static Bitmap Transparent2Color(Bitmap bmp1, Color target)
        {
            Bitmap bmp2 = new Bitmap(bmp1.Width, bmp1.Height);
            Rectangle rect = new Rectangle(Point.Empty, bmp1.Size);
            using (Graphics G = Graphics.FromImage(bmp2))
            {
                G.Clear(target);
                G.DrawImageUnscaledAndClipped(bmp1, rect);
            }
            return bmp2;
        }

        public static Bitmap Crop(Bitmap b, Rectangle r)
        {
            Bitmap nb = new Bitmap(r.Width, r.Height);
            using (Graphics g = Graphics.FromImage(nb))
            {
                g.DrawImage(b, -r.X, -r.Y);
                return nb;
            }
        }

        public static Bitmap ResizeBitmap(Bitmap sourceBMP, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.DrawImage(sourceBMP, 0, 0, width, height);
            }
            return result;
        }

        /// <summary>
        /// Gets an image from a url
        /// </summary>
        /// <param name="uri">The uri to download image from</param>
        /// <param name="transparentFill">Replaces transparent values in image - leave null to ignore</param>
        /// <returns></returns>
        public static Bitmap GetImageFromUri(string uri, string transparentFill = null)
        {
            // Get image as bytes
            using var webClient = new WebClient();
            byte[] imageBytes = webClient.DownloadData(uri);

            // Add blue background
            using var memoryStream = new MemoryStream(imageBytes);
            var image = new Bitmap(memoryStream);

            var color = ColorTranslator.FromHtml(transparentFill);
            image = Transparent2Color(image, color);

            return image;
        }
    }
}
