using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BorgSpawner
{
    public class BorgService
    {
        private readonly string _abiCode;
        private readonly string _endpointAddress;
        private readonly string _contractAddress;
        private readonly string _adminAddress;
        private readonly string _key;
        private readonly BigInteger? _chainId;
        private Web3 _web3;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="contractAddress"></param>
        /// <param name="endpointAddress"></param>
        /// <param name="adminAddress"></param>
        /// <param name="abiCode"></param>
        /// <param name="chainId"></param>
        public BorgService(string key, string contractAddress, string endpointAddress, string adminAddress, string abiCode, BigInteger? chainId)
        {
            _chainId = chainId;
            _endpointAddress = endpointAddress;
            _contractAddress = contractAddress;
            _adminAddress = adminAddress;
            _abiCode = abiCode;
            _key = key;
        }

        /// <summary>
        /// Gets a standard token service
        /// </summary>
        /// <returns>Morality token service</returns>
        private TokenService GetStandardTokenService()
        {
            // Setups up web3 if null (singleton)
            if (_web3 == null)
            {
                // Builds pk
                Nethereum.RPC.Accounts.IAccount pk = new Account(_key, _chainId);

                // Makes connection
                _web3 = new Web3(pk, _endpointAddress, null, null);
            }

            // Returns token service with connection
            return new TokenService(_web3, _contractAddress);
        }

        /// <summary>
        /// Used to wait for a transaction
        /// </summary>
        /// <param name="txHash">The transaction to wait for</param>
        /// <param name="count">The current retry count</param>
        /// <param name="maxCount">Max number of times to try for</param>
        /// <returns></returns>
        public async Task<bool> WaitForTransactionAsync(string txHash, int count = 0, int maxCount = 2, int delayTimeInMillis = 10000)
        {
            // Get connection
            var client = GetStandardTokenService().GetWeb3();

            // Run transaction
            var transaction = (await client.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txHash));

            // Check if the transaction has completed or we have reached completion due to the 
            if ((transaction == null || transaction.BlockNumber == new HexBigInteger(new BigInteger(0))) && count < maxCount)
            {
                // Wait
                await Task.Delay(delayTimeInMillis);

                // Try again
                return await WaitForTransactionAsync(txHash, count++);
            }

            // If we have reached max retry count or the tranaction has been confirmed then we can return
            return transaction != null;
        }

        /// <summary>
        /// Generate borg/s
        /// </summary>
        /// <param name="tokenCount">How many tokens to generate</param>
        /// <returns>List of transactions completed</returns>
        public async Task<List<string>> GenerateTokensAsync(int tokenCount)
        {
            // Define transactions sent
            var transactions = new List<string>();

            // Get connection
            var contract = GetStandardTokenService().GetWeb3().Eth.GetContract(_abiCode, _contractAddress);

            // Get function
            var generateBorg = contract.GetFunction("generateBorg");

            // Estimate generation
            var rawGas = await generateBorg.EstimateGasAsync(_adminAddress, null, null);
            var gas = rawGas.Value + 261995; // Add some because i think the randomness invalidates the estimate?

            // Run for tokenCount
            for (var i = 0; i < tokenCount; i++)
            {
                Console.WriteLine($"Generating token: {i}");

                // Send transaction
                var hex = await generateBorg.SendTransactionAsync(_adminAddress, new HexBigInteger(gas), new HexBigInteger(3000000000), (HexBigInteger)null); //new HexBigInteger(100000000000000000));

                // Wait for completion
                await WaitForTransactionAsync(hex);

                // Add the transaction hash to completed transaction list
                transactions.Add(hex);

                Console.WriteLine($"Token: {i} generated with txHash: {hex}");

                //await Task.Delay(2000);
            }

            // Return the completed transactions
            return transactions;
        }
    }
}
