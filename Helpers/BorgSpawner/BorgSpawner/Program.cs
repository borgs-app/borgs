using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace BorgSpawner
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
            var tokenCount = configuration.GetValue<int>("TokenCount");

            Console.WriteLine("Loading config.");

            // Create service
            var service = new BorgService(key, contractAddress, endpointAddress, adminAddress, abiCode, chainId);

            Console.WriteLine("Service created. Now generating token");

            // Generate the tokens
            service.GenerateTokensAsync(tokenCount).GetAwaiter().GetResult();

            Console.WriteLine("Completed.");
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
}
