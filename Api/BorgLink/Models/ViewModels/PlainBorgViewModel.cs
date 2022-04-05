using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.ViewModels
{
    public class PlainBorgViewModel
    {
        /// <summary>
        /// The unique identifier (also known as tokenId)
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// The link to a hosted image
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// The first parent
        /// </summary>
        [JsonProperty("parentId1")]
        public int? ParentId1 { get; set; }

        /// <summary>
        /// The second parent
        /// </summary>
        [JsonProperty("parentId2")]
        public int? ParentId2 { get; set; }

        /// <summary>
        /// A child
        /// </summary>
        [JsonProperty("childId")]
        public int? ChildId { get; set; }

        /// <summary>
        /// A user set name - won't be populated in most instances so ignore (not included if null too)
        /// </summary>
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

    }
}
