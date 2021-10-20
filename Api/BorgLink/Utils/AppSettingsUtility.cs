using BorgLink.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BorgLink.Utils
{
    /// <summary>
    /// For app setting related functions
    /// </summary>
    public static class AppSettingsUtility
    {
        /// <summary>
        /// Convert app settings (JSON objects) to azure KV pairs
        /// </summary>
        /// <param name="obj">The object to convert</param>
        /// <param name="propertyName">property name used in recurrsion</param>
        /// <param name="settings">The output settings used in recursion</param>
        /// <returns>KV Pair representation of app settings</returns>
        public static List<AzureSetting> ToAzureSettings(JObject obj, ref string propertyName, List<AzureSetting> settings = null)
        {
            settings = (settings == null) ? new List<AzureSetting>() : settings;

            var properties = obj.Properties();

            foreach (var property in properties)
            {
                propertyName += $"{property.Name}";

                if (property.Value.Type == JTokenType.Object)
                {
                    propertyName += ":";
                    settings = ToAzureSettings((JObject)property.Value, ref propertyName, settings);
                }
                else
                {
                    settings.Add(new AzureSetting()
                    {
                        Name = propertyName,
                        Value = property.Value.ToString()
                    });

                    propertyName = RemovePreviousProperty(propertyName);
                }
            }

            propertyName = string.Empty;

            return settings;
        }

        /// <summary>
        /// Removes the previous property (value before :)
        /// </summary>
        /// <param name="propertyName">The current property name</param>
        /// <returns>A property string with the previous one removed</returns>
        private static string RemovePreviousProperty(string propertyName)
        {
            var lastIndex = propertyName.LastIndexOf(":") + 1;
            if (lastIndex > 0)
                propertyName = propertyName.Substring(0, lastIndex);
            else
                propertyName = string.Empty;

            return propertyName;
        }
    }
}
