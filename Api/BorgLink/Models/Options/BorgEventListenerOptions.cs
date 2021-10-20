using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.Options
{
    /// <summary>
    /// Options for the listener worker thread
    /// </summary>
    public class BorgEventListenerOptions
    {
        /// <summary>
        /// If the service is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The frequency of checking
        /// </summary>
        public string IntervalFrequency { get; set; }
    }
}
