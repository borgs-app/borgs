using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace BorgLink.Utils
{
    /// <summary>
    /// String formatting functions
    /// </summary>
    public static class StringUtility
    {
        /// <summary>
        /// Gets a randon string of length n
        /// </summary>
        /// <param name="n">length of random string to be produced</param>
        /// <returns></returns>
        public static string RandomString(int n, string possibilities = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!&$_+=")
        {
            return new string(Enumerable.Repeat(possibilities, n)
              .Select(s => s[new Random().Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Lower case the first letter of a string provided
        /// </summary>
        /// <param name="text">The text to lower first letter of</param>
        /// <returns>The value with the first letter in lower case</returns>
        public static string LowercaseFirstLetter(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            if (Char.IsUpper(text[0]) == true)
            {
                text = text.Replace(text[0], char.ToLower(text[0]));
                return text;
            }

            return text;
        }

        public static IEnumerable<string> SplitIntoChunks(this string str, int chunkSize)
        {
            IEnumerable<string> retVal = Enumerable.Range(0, str.Length / chunkSize)
                 .Select(i => str.Substring(i * chunkSize, chunkSize));

             if (str.Length % chunkSize > 0)
                retVal = retVal.Append(str.Substring(str.Length / chunkSize * chunkSize, str.Length % chunkSize));

            return retVal;
        }
    }
}
