using BorgImageReader.Ethereum;
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
            await AddLayersAsync(layers);

            // Add colours
            await AddBorgColoursAsync(layers);

            // Add the items to the layers
            await AddLayerItemsAsync(layers);

            return;
        }

        /// <summary>
        /// Add layers 
        /// </summary>
        /// <param name="layers"></param>
        /// <returns></returns>
        public async Task AddLayersAsync(List<Layer> layers)
        {
            for (int i = 0; i < layers.Count(); i++)
            {
                // Get layer
                var layer = layers[i];

                // Add layer
                var layerTransaction = await _borgService.AddLayerAsync($"Layer_{i}");

                // Wait for transaction
                await _borgService.WaitForTransaction(layerTransaction);
            }
        }

        /// <summary>
        /// Add the layer items colours
        /// </summary>
        /// <param name="layers"></param>
        /// <returns></returns>
        public async Task AddBorgColoursAsync(List<Layer> layers)
        {
            // Add all of the layers colours
            for (int i = 0; i < layers.Count(); i++)
            {
                // Get layer
                var layer = layers[i];

                foreach (var layerItem in layer.LayerItems)
                {
                    foreach (var borgColor in layerItem.Histogram.Data)
                    {
                        // Add the colour/positions for the attribute
                        var borgAttributeTransaction = await _borgService.AddBorgAttributeColorAsync(layerItem.Name, borgColor.Key, borgColor.Value.ToArray());

                        // Wait for transaction
                        await _borgService.WaitForTransaction(borgAttributeTransaction);
                    }
                }
            }
        }

        /// <summary>
        /// Add the layer items
        /// </summary>
        /// <param name="layers"></param>
        /// <returns></returns>
        public async Task AddLayerItemsAsync(List<Layer> layers)
        {
            for (int i = 0; i < layers.Count(); i++)
            {
                // Get layer
                var layer = layers[i];

                foreach (var layerItem in layer.LayerItems)
                {
                    // Add the items to each layer
                    var layerItemTransaction = await _borgService.AddLayerItemAsync($"Layer_{i}", (int)layerItem.Chance, layerItem.Name);

                    // Wait for transaction
                    await _borgService.WaitForTransaction(layerItemTransaction);
                }
            }
        }
    }
}
