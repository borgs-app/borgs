using Nethereum.StandardTokenEIP20;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Text;

namespace BorgImageReader.Ethereum
{
    /// <summary>
    /// Token service
    /// </summary>
    public class TokenService : StandardTokenService
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="web3"></param>
        /// <param name="contractAddress"></param>
        public TokenService(Web3 web3, string contractAddress)
            : base(web3, contractAddress) { }

        /// <summary>
        /// Get the services web3 connection
        /// </summary>
        /// <returns></returns>
        public Web3 GetWeb3()
        {
            return this.Web3;
        }
    }
}
