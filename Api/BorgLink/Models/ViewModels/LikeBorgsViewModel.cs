using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.ViewModels
{
    public class LikeBorgsViewModel
    {
        [JsonProperty("attributes")]
        public List<AttributeViewModel> Attributes { get; set; }

        [JsonProperty("borgs")]
        public List<PlainBorgViewModel> Borgs { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
