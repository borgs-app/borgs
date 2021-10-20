using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.ViewModels
{
    /// <summary>
    /// An attribute (that makes up a borg)
    /// </summary>
    public class AttributeViewModel
    {
        /// <summary>
        /// The unique identifier
        /// </summary>

        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// The attribute name
        /// </summary>

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
