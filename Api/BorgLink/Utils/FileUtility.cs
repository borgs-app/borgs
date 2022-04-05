using BorgLink.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace BorgLink.Utils
{
    /// <summary>
    /// For file related functions
    /// </summary>
    public static class FileUtility
    {
        /// <summary>
        /// Generates Azure app settings from KV Pairs
        /// </summary>
        /// <param name="fileName">Config to create KV Pairs from</param>
        /// <returns>KV Pairs config</returns>
        public static List<AzureSetting> GenerateAzureAppSettings(string fileName = "appsettings.json")
        {
            // Define out app settings
            var file = File.ReadAllText(fileName);

            // Parse the file as JSON
            var obj = JObject.Parse(file);

            // Convert to KV Pairs
            var propertyName = string.Empty;
            var azureJsonSettings = AppSettingsUtility.ToAzureSettings(obj, ref propertyName, null);

            // Write to output file in bin
            using (StreamWriter writer = File.CreateText("bin\\azuresettings.txt"))
            {
                writer.WriteLine(JsonConvert.SerializeObject(azureJsonSettings));
            }

            return azureJsonSettings;
        }

        /// <summary>
        /// Validate extension
        /// </summary>
        /// <returns></returns>
        public static bool ValidateExtension(string[] extensions, IFormFile file)
        {
            if (file != null)
            {
                // Get extension
                var extension = Path.GetExtension(file.FileName);

                // If it is not in out whitelist then error
                if (extensions.Contains(extension.ToLower()))
                    return true;
            }

            return false;
        }
    }
}
