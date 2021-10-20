using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BorgImageReader.Ethereum
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
        /// Add layers to borgs contract
        /// </summary>
        /// <param name="layerName">The layer to add</param>
        /// <returns>The transaction</returns>
        public async Task<string> AddLayerAsync(string layerName)
        {
            // Get connection
            var contract = GetStandardTokenService().GetWeb3().Eth.GetContract(_abiCode, _contractAddress);

            // Get function
            var addLayer = contract.GetFunction("addLayer");

            // Estimate gas
            var gas = await addLayer.EstimateGasAsync(_adminAddress, null, null, layerName);

            // Send transaction
            var hex = await addLayer.SendTransactionAsync(_adminAddress, gas, (HexBigInteger)null, layerName);

            // Return transaction hash
            return hex;
        }

        /// <summary>
        /// Add an item to a layer
        /// </summary>
        /// <param name="layerName">The layer to add item to</param>
        /// <param name="chance">The chance for the item to be selected</param>
        /// <param name="borgAttributeName">The name (unique) for the layer item/attribute</param>
        /// <returns>The transaction hash</returns>
        public async Task<string> AddLayerItemAsync(string layerName, BigInteger chance, string borgAttributeName)
        {
            // Get connection
            var contract = GetStandardTokenService().GetWeb3().Eth.GetContract(_abiCode, _contractAddress);

            // Get function
            var addLayer = contract.GetFunction("addLayerItem");

            // Estimate gas
            var gas = await addLayer.EstimateGasAsync(_adminAddress, null, null, layerName, chance, borgAttributeName);

            // Send transaction
            var hex = await addLayer.SendTransactionAsync(_adminAddress, gas, (HexBigInteger)null, layerName, chance, borgAttributeName);

            // Return transaction hash
            return hex;
        }

        /// <summary>
        /// Add a new colour (and positions) for an attribute
        /// </summary>
        /// <param name="borgAttributeName">The attribute to add colour for</param>
        /// <param name="hexColour">The colour to add (in hex)</param>
        /// <param name="positions">The positions to add (linear array positions)</param>
        /// <returns>The transaction</returns>
        public async Task<string> AddBorgAttributeColorAsync(string borgAttributeName, string hexColour, BigInteger[] positions)
        {
            // Get connection
            var contract = GetStandardTokenService().GetWeb3().Eth.GetContract(_abiCode, _contractAddress);

            // Get function
            var addLayer = contract.GetFunction("addColorToBorgAttribute");

            // Estimate gas
            var gas = await addLayer.EstimateGasAsync(_adminAddress, null, null, borgAttributeName, hexColour, positions);

            // Send transaction
            var hex = await addLayer.SendTransactionAsync(_adminAddress, gas, (HexBigInteger)null, borgAttributeName, hexColour, positions);

            // Return transaction hash
            return hex;
        }

        /// <summary>
        /// Used to wait for a transaction
        /// </summary>
        /// <param name="txHash">The transaction to wait for</param>
        /// <param name="count">The current retry count</param>
        /// <param name="maxCount">Max number of times to try for</param>
        /// <returns></returns>
        public async Task<bool> WaitForTransaction(string txHash, int count = 0, int maxCount = 2, int delayTimeInMillis = 10000)
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
                return await WaitForTransaction(txHash, count++);
            }

            // If we have reached max retry count or the tranaction has been confirmed then we can return
            return transaction != null;
        }

        /// <summary>
        /// Get a borg as a 1D array of hex pixels
        /// </summary>
        /// <param name="borgId">The borg to get</param>
        /// <returns>A borgs pixels in 1D (576)</returns>
        public async Task<List<string>> GetBorgImageAsync(int borgId)
        {
            var borgImageFunction = new BorgImageFunction() { BorgId = borgId };
            return await GetStandardTokenService().ContractHandler.
                QueryAsync<BorgImageFunction, List<string>>(borgImageFunction, null);
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
                // Send transaction
                var hex = await generateBorg.SendTransactionAsync(_adminAddress, new HexBigInteger(gas), new HexBigInteger(3000000000), new HexBigInteger(100000000000000000));

                // Wait for completion
                await WaitForTransaction(hex);

                // Add the transaction hash to completed transaction list
                transactions.Add(hex);
            }

            // Return the completed transactions
            return transactions;
        }

        /// <summary>
        /// Breed borg/s
        /// </summary>
        /// <param name="tokenCount">The amount of borgs to breed</param>
        /// <returns>A list of completed transactions</returns>
        public async Task<List<string>> BreedAsync(int tokenCount)
        {
            // Define completed transactions
            var transactions = new List<string>();

            // Get connection
            var contract = GetStandardTokenService().GetWeb3().Eth.GetContract(_abiCode, _contractAddress);

            // Get function
            var generateBorg = contract.GetFunction("breedBorgs");

            // Estimate gas
            var gas = await generateBorg.EstimateGasAsync(_adminAddress, null, null, 2, 3);

            // Generate borgs
            for (var i = 0; i < tokenCount; i++)
            {
                try
                {
                    // Send transaction
                    var hex = await generateBorg.SendTransactionAsync(_adminAddress, new HexBigInteger(gas), new HexBigInteger(3000000000), new HexBigInteger(0), 2, 3);

                    // Wait for completion
                    await WaitForTransaction(hex);

                    // Add transaction to completed transactions
                    transactions.Add(hex);
                }
                catch (Exception ex)
                {
                    // TODO: log the error
                }
            }

            // Return the completed transaction hashes
            return transactions;
        }
    }
}
