using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.ViewModels
{
    /// <summary>
    /// Borg to be displayed on OpenSea
    /// </summary>
    public class OpenseaBorgViewModel
    {
        /// <summary>
        /// The borgs description
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// The external url
        /// </summary>
        [JsonProperty("external_url")]
        public string ExternalUrl { get; set; }

        /// <summary>
        /// The hosted image url
        /// </summary>
        [JsonProperty("image")]
        public string Image { get; set; }

        /// <summary>
        /// The external url
        /// </summary>
        [JsonProperty("background_color")]
        public string Color { get; set; }

        /// <summary>
        /// The borgs name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// What makes up a borg
        /// </summary>
        [JsonProperty("attributes")]
        public List<OpenSeaAttributeViewModel> Attributes { get; set; }
    }
}