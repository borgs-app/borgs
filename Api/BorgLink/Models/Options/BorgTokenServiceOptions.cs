using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.Options
{
    /// <summary>
    /// The token service settings
    /// </summary>
    public class BorgTokenServiceOptions
    {
        /// <summary>
        /// Private key for application
        /// </summary>
        public string Key { get; set; } 

        /// <summary>
        /// Borg contract address
        /// </summary>
        public string ContractAddress { get; set; }

        /// <summary>
        /// A node endpoint address
        /// </summary>
        public string EndpointAddress { get; set; }

        /// <summary>
        /// The abi code of the contract
        /// </summary>
        public string AbiCode { get; set; }

        /// <summary>
        /// The chain id
        /// </summary>
        public int ChainId { get; set; }

        /// <summary>
        /// Address might be different from node endpoint address (one that supports ws://)
        /// </summary>
        public string WebsocketEndpointAddress { get; set; }
    }
}
