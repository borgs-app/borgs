using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models
{
    /// <summary>
    /// Report of attribute usage
    /// </summary>
    public class AttributeCount
    {
        /// <summary>
        /// The attribute being counted
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The total number of times the attribute has been used
        /// </summary>
        public int Count { get; set; }
    }
}
