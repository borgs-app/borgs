using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.Options
{
    public class BorgEventSyncPingOptions
    {
        /// <summary>
        /// If the service is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The frequency of checking
        /// </summary>
        public string IntervalFrequency { get; set; }

        public string Url { get; set; }
    }
}
