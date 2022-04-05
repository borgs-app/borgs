using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Api.Models
{
    /// <summary>
    /// Key value map for error
    /// </summary>
    public class ErrorMap
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The value
        /// </summary>
        public string Value { get; set; }
    }
}
