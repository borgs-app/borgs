using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.Enums
{
    /// <summary>
    /// Sub folders to store different resolution images
    /// </summary>
    public enum ResolutionContainer
    {
        /// <summary>
        /// 24x24
        /// </summary>
        Default = 0,

        /// <summary>
        /// 600x600
        /// </summary>
        Medium = 1,

        /// <summary>
        /// 1400x1400
        /// </summary>
        Large = 2
    }
}
