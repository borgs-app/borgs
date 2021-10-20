using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.Enums
{
    /// <summary>
    /// Filter for borg
    /// </summary>
    public enum Condition
    {
        /// <summary>
        /// Both alive and dead
        /// </summary>
        Both = 0,

        /// <summary>
        /// Child == null
        /// </summary>
        Alive = 1,

        /// <summary>
        /// Child != null
        /// </summary>
        Dead = 2
    }
}
