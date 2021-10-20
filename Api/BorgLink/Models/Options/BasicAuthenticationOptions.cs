using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.Options
{
    /// <summary>
    /// Options for a Basoc authentication connection
    /// </summary>
    public class BasicAuthenticationOptions
    {
        /// <summary>
        /// The username
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The password
        /// </summary>
        public string Password { get; set; }
    }
}
