using BorgImageReader.Ethereum;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
namespace BorgImageReader
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create service collection
            var serviceCollection = new ServiceCollection();
            var configuration = ConfigureServices(serviceCollection);

            // Build provider
            serviceCollection.BuildServiceProvider();

            // Print connection string to demonstrate configuration object is populated
            var key = configuration.GetValue<string>("Key");
            var contractAddress = configuration.GetValue<string>("ContractAddress");
            var endpointAddress = configuration.GetValue<string>("EndpointAddress");
            var adminAddress = configuration.GetValue<string>("AdminAddress");
            var abiCode = configuration.GetValue<string>("AbiCode");
            var chainId = configuration.GetValue<long>("ChainId");

            var uploader = new BorgUploader(key, contractAddress, endpointAddress, adminAddress, abiCode, chainId);
            uploader.StartAsync().GetAwaiter().GetResult();

        }

        /// <summary>
        /// Configure services with config
        /// </summary>
        /// <param name="serviceCollection">The service collection</param>
        private static IConfiguration ConfigureServices(IServiceCollection serviceCollection)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            // Add access to generic IConfigurationRoot
            serviceCollection.AddSingleton<IConfigurationRoot>(configuration);

            return configuration;
        }
    }

    /*      
     // Generate token
     var borgService = new BorgService(Key, ContractAddress, EndpointAddress, AdminAddress, AbiCode, ChainId);
     borgService.GenerateTokensAsync(3000).GetAwaiter().GetResult();

     // Breed
     borgService.BreedAsync(1).GetAwaiter().GetResult();

     /*
     // Save image/s
     var images = new List<string>();
     for(int i=4;i<5;i++)
     {
         var rawImage = borgService.GetBorgImageAsync(i).GetAwaiter().GetResult();
         var convertedImage = ConvertBorgToBitmap(rawImage);
         //convertedImage.Save($"test{i}.png");
         var newImg = ResizeBitmap(convertedImage, 384, 384);
         newImg.Save($"test_{i}.png");
         images.Add(string.Join(",", rawImage));
     }
     */
}

