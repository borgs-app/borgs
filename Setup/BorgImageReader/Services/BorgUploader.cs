using BorgImageReader.Ethereum;
using BorgImageReader.Models;
using Nethereum.Hex.HexConvertors.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BorgImageReader
{
    public class BorgUploader
    {
        private BorgService _borgService;

        private string _abiCode;
        public string _contractAddress;
        public string _endpointAddress;
        public string _adminAddress;
        public string _key;
        public BigInteger? _chainId;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="contractAddress"></param>
        /// <param name="endpointAddress"></param>
        /// <param name="adminAddress"></param>
        /// <param name="abiCode"></param>
        /// <param name="chainId"></param>
        public BorgUploader(string key, string contractAddress, string endpointAddress, string adminAddress, string abiCode, BigInteger? chainId)
        {
            _key = key;
            _contractAddress = contractAddress;
            _endpointAddress = endpointAddress;
            _adminAddress = adminAddress;
            _abiCode = abiCode;
            _chainId = chainId;
        }

        /// <summary>
        /// Sertup service
        /// </summary>
        private void InitializeServices()
        {
            // Initilize borg service
            _borgService = new BorgService(_key, _contractAddress, _endpointAddress, _adminAddress, _abiCode, _chainId);
        }

        /// <summary>
        /// Run the import
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            // Setup
            InitializeServices();

            // Get the layers
            var layers = BorgHelpers.ReadLayers("Images");

            // Add layers
            await AddLayersAsync(layers.Count());

            // Add all the blank layers in 1 fell swoop
            await AddBlanks(layers);

            // Add colours
            await AddBorgAttributes(layers);

            // Add the items to the layers
            await AddLayerItemsAsync(layers);

            return;
        }

        /// <summary>
        /// Add layers 
        /// </summary>
        /// <param name="layers"></param>
        /// <returns></returns>
        public async Task AddLayersAsync(int layerCount)
        {
            // Add layer
            var layerTransaction = await _borgService.AddLayersAsync(layerCount);

            // Wait for transaction
            await _borgService.WaitForTransactionAsync(layerTransaction);
        }

        /// <summary>
        /// Add the layer items colours
        /// </summary>
        /// <param name="layers"></param>
        /// <returns></returns>
        public async Task AddBorgAttributes(List<Layer> layers)
        {
            // Add all of the layers colours
            for (int i = 0; i < layers.Count(); i++)
            {
                // Get layer
                var layer = layers[i];

                foreach (var layerItem in layer.LayerItems)
                {
                    // Dont add blanks as they are already added in 1 fell swoop with another action
                    if (!layerItem.Name.Contains("blank"))
                    {
                        // Get colors
                        var colors = layerItem.LayerPositions.Data.Select(x => x.Key)
                            .Select(y => string.Join(string.Empty, y.Select(c => ((int)c).ToString("X2"))).HexToByteArray())
                            .ToArray();

                        // Get positions
                        var positions = layerItem.LayerPositions.Data.Values.Select(x => x.ToArray()).ToArray();

                        // Add the colour/positions for the attribute
                        var borgAttributeTransaction = await _borgService.CreateBorgAttribute(
                            layerItem.Name,
                            colors,
                            positions
                        );

                        // Wait for transaction
                        await _borgService.WaitForTransactionAsync(borgAttributeTransaction);
                    }
                    else
                    {
                        var s = "";
                    }
                }
            }
        }

        /// <summary>
        /// Add black attributes
        /// </summary>
        /// <param name="layers"></param>
        /// <returns></returns>
        public async Task AddBlanks(List<Layer> layers)
        {
            // We dont want to add blanks for layer 0 (the main borg part)
            var layersToUse = layers.Where((x, y) => y > 0);

            // Get the blank names
            var blankAttributeNames = layers.Select((x, y) => $"blank{y}").ToList();

            // Add blank for each layer but the first
            var transaction = await _borgService.AddBlanksAsync(blankAttributeNames);

            // Wait for transaction
            await _borgService.WaitForTransactionAsync(transaction);

            return;
        }

        /// <summary>
        /// Add the layer items
        /// </summary>
        /// <param name="layers"></param>
        /// <returns></returns>
        public async Task AddLayerItemsAsync(List<Layer> layers)
        {
            // Define layer items
            List<CreateLayerItem> allLayerItems = new List<CreateLayerItem>();

            // For the layer name
            var layerCounter = 0;

            // Add to layer items
            foreach (var layer in layers)
            {
                // Convert to out new format
                var layersItems = layer.LayerItems.Select(x => new CreateLayerItem()
                {
                    Chance = new BigInteger(x.Chance),
                    LayerIndex = layerCounter,
                    AttributeName = x.Name,
                }).ToList();

                allLayerItems.AddRange(layersItems);

                layerCounter++;
            }

            // Add the items to each layer
            var layerItemTransaction = await _borgService.AddLayerItemsAsync(allLayerItems);

            // Wait for transaction
            await _borgService.WaitForTransactionAsync(layerItemTransaction);
        }
    }
}
