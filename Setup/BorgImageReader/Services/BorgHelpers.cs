using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace BorgImageReader
{
    public class BorgHelpers
    {
        /// <summary>
        /// Read layers from a directory/file structure
        /// </summary>
        /// <param name="folderName">The parent directory holding the sub directory/file structure required to parse</param>
        /// <returns>Borg layers</returns>
        public static List<Layer> ReadLayers(string folderName)
        {
            // Define layers
            var layers = new List<Layer>();

            // Get all directories to read images from
            var folders = Directory.GetDirectories(folderName)
                .OrderBy(x => int.Parse(x.Substring(x.LastIndexOf("_") + 1, x.Length - (x.LastIndexOf("_") + 1))));

            // Define position
            var position = 0;

            // For each directory (acting as a layer) we read its files/images into layer items
            foreach (var folder in folders)
            {
                // Get the files/images to turn to historgrams
                var files = Directory.GetFiles(folder)
                    .Where(x => !x.Contains("DS_Store"))
                    .ToList();

                // Add the layer
                layers.Add(new Layer() { Position = position++, LayerItems = BuildLayerItems(files.ToList()) });

                position++;
            }

            // Return the built layers
            return layers;
        }

        /// <summary>
        /// Build the layer items for a layer
        /// </summary>
        /// <param name="files">The files to build the layer items from</param>
        /// <returns>A layers items</returns>
        public static List<LayerItem> BuildLayerItems(List<string> files)
        {
            // Define the items
            var layersItems = new List<LayerItem>();

            // For each of the files we want to build a histogram to add to the layer
            foreach (var file in files)
            {
                // Get the image from file location
                var bitmap = new Bitmap(file);

                // Define histogram
                var histogram = new LayersPositions();

                // Process the image
                for (int i = 0; i < bitmap.Size.Height; i++)
                {
                    for (int j = 0; j < bitmap.Size.Width; j++)
                    {
                        // Get the pixal for the current position
                        var pixal = bitmap.GetPixel(j, i);

                        // Determine the color
                        var color = Color.FromArgb(pixal.A, pixal.R, pixal.G, pixal.B);

                        // Convert to hex value
                        var hex = color.A.ToString("X2") + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");

                        // Calculate the position as if it were in a flat array
                        var imgPosition = (i * bitmap.Size.Height) + j;

                        // Set in histogram
                        if (hex != "FFFFFFFF")
                            histogram.AddData(hex, imgPosition);
                    }
                }

                // Add mandatory white (as nothing)
                if (!histogram.Data.Any())
                    histogram.AddData("FFFFFFFF", null);

                // Get the chance of the layer item being selected (from file name)
                var chance = ParseChance(Path.GetFileName(file));
                var name = ParseName(Path.GetFileName(file));

                // Once the image has been processed we can add the image (as a histogram) to the collection of layer items
                layersItems.Add(new LayerItem() { Name = name, Chance = chance, LayerPositions = histogram });
            }

            // Return the items
            return layersItems;
        }

        /// <summary>
        /// Parse a file name to get the name it can be selected ie. fileName="test_35_.png" -> test
        /// NOTE: "_" works as space in string
        /// </summary>
        /// <param name="file">A full file locations</param>
        /// <returns>A decimal value representing chance for the respective file</returns>
        public static string ParseName(string file)
        {
            // We get the file name (which holds chance data)
            var actualFileName = Path.GetFileName(file);

            var lastIndexOfUnderscore = file.LastIndexOf("_");

            // return the name
            return actualFileName.Substring(0, lastIndexOfUnderscore).Replace("_", " ");
        }

        /// <summary>
        /// Parse a file name to get the chance it can be selected ie. fileName="35_.png" -> 35
        /// NOTE: "_" allows multiple file names with same chance
        /// </summary>
        /// <param name="file">A full file locations</param>
        /// <returns>A decimal value representing chance for the respective file</returns>
        public static decimal ParseChance(string file)
        {
            // We get the file name (which holds chance data)
            var actualFileName = Path.GetFileName(file);

            var lastIndexOfUnderscore = file.LastIndexOf("_");
            var lastIndexOfDot = actualFileName.LastIndexOf('.');

            // Parse the chance
            var parsedChance = actualFileName.Substring(lastIndexOfUnderscore, lastIndexOfDot - lastIndexOfUnderscore).Replace("_", string.Empty);

            // Return parsed value
            return decimal.Parse(parsedChance);
        }

        /// <summary>
        /// This is used to convert an array of hex pixals back into an argb image
        /// </summary>
        /// <param name="hexValues">The string hex values to convert to image (flat)</param>
        /// <returns>A 2d bitmap</returns>
        public static Bitmap ConvertBorgToBitmap(List<string> hexValues)
        {
            // Assuming regular image ie. 24x24, 12x12, 48x48 etc.
            var sqrt = (int)Math.Sqrt(hexValues.Count());
            var bitmap = new Bitmap(sqrt, sqrt, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int i = 0; i < hexValues.Count(); i++)
            {
                // Default white
                var pixal = Color.White;

                // If specified then isnt white
                if (!string.IsNullOrEmpty(hexValues[i]))
                {
                    var convertedHexValue = Convert.ToInt32(hexValues[i], 16);
                    pixal = Color.FromArgb(convertedHexValue);
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
    }
}
