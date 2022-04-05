using BorgLink.Api.Models;
using BorgLink.Services.Platform;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace BorgLink.Extensions
{
    /// <summary>
    /// Extension methods for IServiceProvider
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Sets a database context into the service collection
        /// </summary>
        /// <typeparam name="T">The type of context (T:DbContext)</typeparam>
        /// <param name="services">The service provider</param>
        /// <param name="connectionString">The connection string of the connection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection SetContext<T>(this IServiceCollection services, string connectionString)
           where T : DbContext
        {
            // Add db context pool using SqlServer to the service collection
            return services.AddDbContextPool<T>(optionsBuilder =>
                           optionsBuilder.UseSqlServer(
                                  connectionString,
                                  options => options
                                  // Enable retry with max count of 10
                                     .EnableRetryOnFailure(
                                         maxRetryCount: 10,
                                         maxRetryDelay: TimeSpan.FromSeconds(5),
                                         errorNumbersToAdd: null)
                                     // Timeout occours after 30 seconds
                                     .CommandTimeout(30)).UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
                           );

        }

        /// <summary>
        /// Adds gzip response compression to API
        /// </summary>
        /// <param name="services">The service provider</param>
        /// <param name="config">Platform configuration</param>
        public static IServiceCollection AddGZipResponseCompression(this IServiceCollection services, IConfiguration config)
        {
            // Get the response compression mime types
            var mimeTypes = new List<string>();
            config.GetSection("ResponseCompressionMimeTypes").Bind(mimeTypes);

            // Add the response compression
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = mimeTypes;
            });

            return services;
        }


        /// <summary>
        /// Sets up the error mapping service
        /// </summary>
        /// <param name="services">The service provider</param>
        public static IServiceCollection LoadErrorMappings(this IServiceCollection services)
        {
            // Get the mappings
            var mappingJson = File.ReadAllText("failedresponsemappings.json");

            // Convert to obj
            var mappings = JsonConvert.DeserializeObject<List<ErrorMap>>(mappingJson);

            services.AddTransient<FailedResponseMappingService>(s =>
                new FailedResponseMappingService(s.GetService<ILogger<FailedResponseMappingService>>(), Options.Create(mappings), "Development")
            );

            return services;
        }
    }
}
