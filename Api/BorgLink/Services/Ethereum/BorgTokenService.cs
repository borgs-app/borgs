using AutoMapper;
using BorgLink.Ethereum;
using BorgLink.Models;
using BorgLink.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Reactive.Eth;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BorgLink.Services.Ethereum
{
    /// <summary>
    /// The token service
    /// </summary>
    public class BorgTokenService
    {
        private readonly ILogger<BorgTokenService> _logger;
        private readonly IMapper _mapper;
        private readonly BorgTokenServiceOptions _options;
        private Web3 _web3;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mapper">The mapper</param>
        /// <param name="options">Options</param>
        /// <param name="logger">The class logger</param>
        public BorgTokenService(IMapper mapper, IOptions<BorgTokenServiceOptions> options, ILogger<BorgTokenService> logger)
        {
            _logger = logger;
            _options = options.Value;
            _mapper = mapper;
        }

        /// <summary>
        /// Gets a standard token service
        /// </summary>
        /// <returns>Morality token service</returns>
        private TokenService GetStandardTokenService()
        {
            // If web3 hasnt be set, set it (singleton as will only ever get created once)
            if (_web3 == null)
            {
                // Create account with callers key
                Nethereum.RPC.Accounts.IAccount pk = new Account(_options.Key, _options.ChainId);

                // Build web3
                _web3 = new Web3(pk, _options.EndpointAddress, null, null);
            }

            // Return a token service with the web3 connection
            return new TokenService(_web3, _options.ContractAddress);
        }

        /// <summary>
        /// Get Borg from blockchain
        /// </summary>
        /// <param name="borgId">The Borg to get</param>
        /// <returns>A Borg</returns>
        public async Task<GetBorgOutputDTO> GetBorgAsync(int borgId)
        {
            // Build the function call
            var getBorgFunction = new GetBorgFunction() { BorgId = borgId };

            // Get client
            var client = GetStandardTokenService();

            // Call function
            return await client.ContractHandler.QueryDeserializingToObjectAsync<GetBorgFunction, GetBorgOutputDTO>(getBorgFunction, null);
        }

        /// <summary>
        /// Wait for a transaction to finish
        /// </summary>
        /// <param name="txHash">THe transaction to make sure is completed</param>
        /// <param name="retryCount">Retry count</param>
        /// <param name="timeoutInMilliseconds">The timeout count (milli seconds)</param>
        /// <returns>If the transaction is a success</returns>
        public async Task<bool> WaitForTransaction(string txHash, int retryCount = 0, int timeoutInMilliseconds = 1000)
        {
            // Get web client
            var client = GetStandardTokenService().GetWeb3();

            // Get transaction by id
            var transaction = (await client.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txHash));

            // If the transaction is null or block numer not set, delay and try again. Can only retry x number of times
            if ((transaction == null || transaction.BlockNumber == new HexBigInteger(new BigInteger(0))) && retryCount < 3)
            {
                // Wait an alloted time
                await Task.Delay(timeoutInMilliseconds);

                // Continue to wait for transaction
                return await WaitForTransaction(txHash, retryCount++);
            }

            // Return if the transaction exists and has been broadcast on the blockchain
            return (transaction != null && transaction.BlockNumber != new HexBigInteger(new BigInteger(0)));
        }

        /// <summary>
        /// Get a borgs parents
        /// </summary>
        /// <param name="borgId">The borg to get parents for</param>
        /// <returns>The borgs parent ids</returns>
        public async Task<(BigInteger? ParentAId, BigInteger? ParentBId)> GetBorgsParentsAsync(int borgId)
        {
            // Build the function call
            var borgImageFunction = new BorgParentsFunction() { BorgId = borgId };

            // Get client
            var client = GetStandardTokenService();

            // Call function
            return await client.ContractHandler.QueryAsync<BorgParentsFunction, (BigInteger? parentAId, BigInteger? parentBId)>(borgImageFunction, null);
        }

        /// <summary>
        /// Get a Borgs image from blockchain
        /// </summary>
        /// <param name="borgId">The borg to get</param>
        /// <returns>The Borg image (flat list of colours in hex)</returns>
        public async Task<List<string>> GetBorgImageAsync(int borgId)
        {
            // Build the function call
            var borgImageFunction = new BorgImageFunction() { BorgId = borgId };

            // Get client
            var client = GetStandardTokenService();

            // Call function
            return await client.ContractHandler.QueryAsync<BorgImageFunction, List<string>>(borgImageFunction, null);
        }

        /// <summary>
        /// Get a Borgs attributes (what its made up of)
        /// </summary>
        /// <param name="borgId">The Borg to get attributes of</param>
        /// <returns>List of Borg attributes</returns>
        public async Task<List<string>> GetBorgsAttributesAsync(int borgId)
        {
            // Build the function call
            var getBorgAttributesFunction = new GetBorgsAttributesFunction() { BorgId = borgId };

            // Get client
            var client = GetStandardTokenService();

            // Call function
            return await client.ContractHandler.QueryAsync<GetBorgsAttributesFunction, List<string>>(getBorgAttributesFunction, null);
        }

        /// <summary>
        /// Subscribe to a borg event
        /// </summary>
        /// <typeparam name="T">The type of event to subscribe to</typeparam>
        /// <param name="action">The action to execute on callback from event firing</param>
        /// <returns>An async task</returns>
        public async Task SubscribeToBorgEvent<T>(Func<Borg, bool, Task<Borg>> action)
            where T : IEventDTO, new()
        {
            // Get client
            var web3 = GetStandardTokenService().GetWeb3();

            // Define subscription
            EthLogsObservableSubscription subscription = null;

            // Open web socket connection
            using (var client = new StreamingWebSocketClient(_options.WebsocketEndpointAddress))
            {
                // Get event filter
                var filter = web3.Eth.GetEvent<T>(_options.ContractAddress).CreateFilterInput();

                // create the subscription
                subscription = new EthLogsObservableSubscription(client);

                try
                {
                    // Attach a handler for event logs
                    subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(async log =>
                    {
                        try
                        {
                            // Decode the log into a typed event log
                            var decodedEvent = log.DecodeEvent<T>();

                            // Map to common object (Borg)
                            if (decodedEvent != null)
                            {
                                //Map
                                var mappedBorg = _mapper.Map<Borg>(decodedEvent.Event);

                                // Save in db (and create image url)
                                var actionedItem = await action(mappedBorg, true);
                            }
                        }
                        catch (Exception ex)
                        {
                            //Log exception
                            _logger.LogError(ex.StackTrace);
                        }
                    });

                    // Open the web socket connection
                    await client.StartAsync();

                    // Begin receiving subscription data
                    await subscription.SubscribeAsync(filter);

                    // Keep alive
                    while (true)
                    {
                        // Create handler
                        var handler = new EthBlockNumberObservableHandler(client);

                        // Make small call to get block count
                        handler.GetResponseAsObservable().Subscribe(x =>
                            Console.WriteLine(x.Value)
                        );

                        // Send request
                        await handler.SendRequestAsync();

                        // Wait some time
                        await Task.Delay(TimeSpan.FromSeconds(30));
                    }
                }
                catch (Exception ex)
                {
                    // Unsubscribe
                    await subscription.UnsubscribeAsync();

                    //Log exception
                    _logger.LogError(ex.StackTrace);

                    // Allow time to unsubscribe
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }
    }
}
