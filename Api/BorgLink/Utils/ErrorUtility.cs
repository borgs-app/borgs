using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BorgLink.Utils
{
    /// <summary>
    /// Manage error related logic
    /// </summary>
    public class ErrorUtility
    {
        /// <summary>
        /// Build errors (JObject) with a property-value
        /// </summary>
        /// <param name="propertyName">The property name</param>
        /// <param name="propertyValue">The property value</param>
        /// <returns>Error object</returns>
        public static object BuildErrors(params (string propertyName, string propertyValue)[] errors)
        {
            var errorObject = @"{}";

            var errorObjectToBuild = JsonConvert.DeserializeObject<Dictionary<string, object>>(errorObject);

            foreach (var error in errors)
            {
                // Check if it already exists - if so then ignore
                var alreadyExists = errorObjectToBuild.TryGetValue(error.propertyName, out _);
                if(!alreadyExists)
                    errorObjectToBuild.Add(error.propertyName, error.propertyValue);
            }

            return errorObjectToBuild;
        }

        public static dynamic BuildErrors((object, string) p)
        {
            throw new NotImplementedException();
        }
    }
}
