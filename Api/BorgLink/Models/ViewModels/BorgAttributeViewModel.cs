using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.ViewModels
{
    /// <summary>
    /// A borgs attribute (link)
    /// </summary>
    public class BorgAttributeViewModel
    {
        /// <summary>
        /// The unique identifier
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// The borg link
        /// </summary>
        [JsonProperty("borgId")]
        public int BorgId { get; set; }

        /// <summary>
        /// The attribute link
        /// </summary>
        [JsonProperty("attributeId")]
        public int AttributeId { get; set; }

        /// <summary>
        /// The attribute link
        /// </summary>
        [JsonProperty("attribute")]
        public AttributeViewModel Attribute { get; set; }

        /// <summary>
        /// The borg link
        /// </summary>
        [JsonProperty("borg")]
        public BorgViewModel Borg { get; set; }
    }
}
