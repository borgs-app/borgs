using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.Enums
{
    /// <summary>
    /// List of fail reason codes
    /// </summary>
    public enum FailedReason
    {
        /// <summary>
        /// No failed reason (default)
        /// </summary>
        None = 0,

        /// <summary>
        /// System error
        /// </summary>
        SystemError = 1,

    }
}
