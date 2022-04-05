using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.Options
{
    /// <summary>
    /// Setting for twitter connection
    /// </summary>
    public class TwitterServiceOptions
    {
        /// <summary>
        /// The connections key
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// The client secret
        /// </summary>
        public string ApiSecret { get; set; }

        /// <summary>
        /// A secret pin code
        /// </summary>
        public string Pincode { get; set; }

        /// <summary>
        /// A secret access token
        /// </summary>
        public string AccessToken { get; set; }

        public bool Enabled { get; set; }

        /// <summary>
        /// The access token secret
        /// </summary>
        public string AccessTokenSecret { get; set; }
    }
}
