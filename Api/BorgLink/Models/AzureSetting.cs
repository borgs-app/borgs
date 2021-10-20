using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BorgLink.Models
{

    /// <summary>
    /// The Azure KV Pair
    /// </summary>
    public class AzureSetting
    {
        /// <summary>
        /// The setting name
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The setting value
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        /// <summary>
        /// If it is slot setting
        /// </summary>
        [JsonProperty(PropertyName = "slotSetting")]
        public bool SlotSetting { get; set; } = false;
    }
}
