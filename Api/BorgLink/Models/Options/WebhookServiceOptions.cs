using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.Options
{
    /// <summary>
    /// The options for the post webhook to regenerate site
    /// </summary>
    public class WebhookServiceOptions
    {
        /// <summary>
        /// The endpoint 
        /// </summary>
        public string Endpoint { get; set; }
    }
}
