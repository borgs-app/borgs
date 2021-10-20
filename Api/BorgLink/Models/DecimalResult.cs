using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models
{
    /// <summary>
    /// A wrapped decimal
    /// </summary>
    public class DecimalResult
    {
        /// <summary>
        /// The decimal
        /// </summary>
        public readonly decimal Result;

        /// <summary>
        /// COnstructor
        /// </summary>
        /// <param name="result"></param>
        public DecimalResult(decimal result)
        {
            Result = result;
        }
    }
}
