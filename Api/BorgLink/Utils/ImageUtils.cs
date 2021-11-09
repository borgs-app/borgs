using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
    }
}
