using BorgLink.Api.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace BorgLink.Models.Options
{
    /// <summary>
    /// Options for storage
    /// </summary>
    public class StorageOptions
    {
        /// <summary>
        /// The storage account name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Account keys
        /// </summary>
        public List<StorageKey> AccessKeys { get; set; } = new List<StorageKey>();

        /// <summary>
        /// Endpoint url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// SaS expiry
        /// </summary>
        public string SasExpiry { get; set; }

        /// <summary>
        /// If the app is in test mode
        /// </summary>
        public bool TestMode { get; set; }
    }
}
