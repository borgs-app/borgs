using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.ViewModels
{
    /// <summary>
    /// The opensea attribute
    /// </summary>
    public class OpenSeaAttributeViewModel
    {
        /// <summary>
        /// The trait type (layer)
        /// </summary>
        [JsonProperty("trait_type")]
        public string TraitType { get; set; }

        /// <summary>
        /// The attribute name
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
