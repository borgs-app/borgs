using BorgLink.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BorgLink.Utils
{
    /// <summary>
    /// For frequency related functions
    /// </summary>
    public static class FrequencyUtility
    {
        /// <summary>
        /// Parses frequency string ie. 1s -> 1000, 1m -> 60000, 1h -> 3600000.. if conversion is set to milliseconds
        /// </summary>
        /// <param name="frequency">The frequency to parse</param>
        /// <param name="returnFormat">What to return the time period in</param>
        /// <returns>Frequency</returns>
        public static long ParseFrequency(string frequency, TimeUnit returnFormat = TimeUnit.MilliSeconds)
        {
            // Check there anything to parse
            if (string.IsNullOrEmpty(frequency))
                throw new Exception("No value defined to parse (frequency)");

            // break up the input string
            var strTimePeriod = frequency.Substring(frequency.Length - 1, 1);
            var value = frequency.Substring(0, frequency.Length - 1);
            var timePeriodInSeconds = 0L;

            // Parse the time unit
            switch (strTimePeriod.ToLower())
            {
                // Seconds
                case "s":
                    timePeriodInSeconds = long.Parse(value);
                    break;
                // Months
                case "m":
                    timePeriodInSeconds = long.Parse(value) * 60;
                    break;
                // Hours
                case "h":
                    timePeriodInSeconds = long.Parse(value) * 60 * 60;
                    break;
                // Days
                case "d":
                    timePeriodInSeconds = long.Parse(value) * 60 * 60 * 24;
                    break;
                // Weeks
                case "w":
                    timePeriodInSeconds = long.Parse(value) * 60 * 60 * 24 * 7;
                    break;
            }

            return ConvertSecondsTo(returnFormat, timePeriodInSeconds);
        }

        /// <summary>
        /// Parses frequency string ie. 1s -> 1000, 1m -> 60000, 1h -> 3600000.. if conversion is set to milliseconds
        /// </summary>
        /// <param name="frequency"></param>
        /// <param name="returnFormat"></param>
        /// <returns>Frequency</returns>
        public static long ParseFrequencyConfig(this string frequency, TimeUnit returnFormat = TimeUnit.MilliSeconds)
        {
            return ParseFrequency(frequency, returnFormat);
        }

        /// <summary>
        /// Convert seconds to another format
        /// </summary>
        /// <param name="period"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        private static long ConvertSecondsTo(TimeUnit period, long seconds)
        {
            switch (period)
            {
                case TimeUnit.Minutes: 
                    return seconds / 60;
                case TimeUnit.Hours:
                    return seconds / (60 * 60);
                case TimeUnit.Days:
                    return seconds / (60 * 60 * 24);
                case TimeUnit.MilliSeconds:
#pragma warning disable CS0078 // The 'l' suffix is easily confused with the digit '1' -- use 'L' for clarity
                    return seconds * 1000l;
#pragma warning restore CS0078 // The 'l' suffix is easily confused with the digit '1' -- use 'L' for clarity
                case TimeUnit.Seconds:
                    return seconds;
            }

            throw new Exception("Period not supported");
        }
    }
}
