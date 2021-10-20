using BorgLink.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.Options
{
    /// <summary>
    /// Options (settings) for borg service
    /// </summary>
    public class BorgServiceOptions
    {
        /// <summary>
        /// The container location
        /// </summary>
        public ContainerLocation FolderLocation { get; set; }

        /// <summary>
        /// Base Azure storage account url
        /// </summary>
        public string BaseStorageUrl { get; set; }

        /// <summary>
        /// If the app is in test mode
        /// </summary>
        public bool TestMode { get; set; }
    }
}
