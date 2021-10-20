using System;
using System.Collections.Generic;
using System.Text;

namespace BorgLink.Api.Models
{
    /// <summary>
    /// The storage key
    /// </summary>
    public class StorageKey
    {
        /// <summary>
        /// Key value
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// THe connection details
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
