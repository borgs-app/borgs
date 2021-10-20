using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.ViewModels
{
    /// <summary>
    /// Report of attribute usage
    /// </summary>
    public class AttributeCountViewModel
    {
        /// <summary>
        /// The attribute being counted
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The total number of times the attribute has been used
        /// </summary>
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
