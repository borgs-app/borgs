using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Api.Models
{
    /// <summary>
    /// Body of response when an error has occoured
    /// </summary>
    public class BorgsProblemDetails
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "https://tools.ietf.org/html/rfc7231#section-6.5.1";

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("traceId")]
        public string TraceId { get; set; }

        [JsonProperty("errors")]
        public dynamic Errors { get; set; }

        [JsonProperty("stackTrace", NullValueHandling = NullValueHandling.Ignore)]
        public string Stacktrace { get;set; }
    }
}
