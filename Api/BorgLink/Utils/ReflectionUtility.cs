using System;
using System.Collections.Generic;
using System.Text;

namespace BorgLink.Utils
{
    /// <summary>
    /// For reflection related logic
    /// </summary>
    public static class ReflectionUtility
    {
        /// <summary>
        /// Gets an object value by property name
        /// </summary>
        /// <param name="obj">The object to get the property value from</param>
        /// <param name="propName">The property name of the value we want to get</param>
        /// <returns>The property value for a specified name</returns>
        /// <author>Matt Sharp</author>
        /// <date>17 February 2020</date>
        public static dynamic GetPropertyValue(this object obj, string propName)
        {
            return obj.GetType().GetProperty(propName).GetValue(obj);
        }

        /// <summary>
        /// Gets an object value by property name
        /// </summary>
        /// <param name="obj">The object to get the property value from</param>
        /// <param name="propName">The property name of the value we want to get</param>
        /// <returns>The property value for a specified name</returns>
        /// <author>Matt Sharp</author>
        /// <date>17 February 2020</date>
        public static T GetPropertyValue<T>(this object obj, string propName)
        {
            return (T)obj.GetType().GetProperty(propName).GetValue(obj);
        }
    }
}
