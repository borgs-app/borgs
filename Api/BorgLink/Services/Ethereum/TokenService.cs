using Nethereum.StandardTokenEIP20;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Text;

namespace BorgLink.Services.Ethereum
{
    /// <summary>
    /// Token service - enables contract communication
    /// </summary>
    public class TokenService : StandardTokenService
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="web3">The web3 connection</param>
        /// <param name="contractAddress">The contracts address</param>
        public TokenService(Web3 web3, string contractAddress)
            : base(web3, contractAddress) { }

        /// <summary>
        /// Get the web3 obj
        /// </summary>
        /// <returns>Web3</returns>
        public Web3 GetWeb3()
        {
            return this.Web3;
        }
    }
}
