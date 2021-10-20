using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models
{
    /// <summary>
    /// An attribute (that makes up a borg)
    /// </summary>
    public class Attribute
    {
        /// <summary>
        /// The unique identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The attribute name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The owning borgs
        /// </summary>
        public List<BorgAttribute> BorgAttributes { get; set; }
    }
}
