using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models
{
    /// <summary>
    /// A borgs attribute (link)
    /// </summary>
    public class BorgAttribute
    {
        /// <summary>
        /// The unique identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The borg link
        /// </summary>
        public int BorgId { get; set; }

        /// <summary>
        /// The attribute link
        /// </summary>
        public int AttributeId { get; set; }

        /// <summary>
        /// The borg link
        /// </summary>
        public Borg Borg { get; set; }

        /// <summary>
        /// The attribute link
        /// </summary>
        public Attribute Attribute { get; set; }
    }
}
