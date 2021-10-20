using BorgLink.Ethereum;
using BorgLink.Models.Options;
using BorgLink.Services.Ethereum;
using BorgLink.Services.Platform;
using BorgLink.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.ABI.FunctionEncoding.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BorgLink.Services
{
    /// <summary>
    /// Event listener service
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BorgEventListenerService<T> : BackgroundService
        where T : IEventDTO, new()
    {
        private BorgEventListenerOptions _options;
        private BorgTokenService _borgTokenService;
        private BorgService _borgService;
        private WebhookService _WebhookService;
        private ILogger<BorgEventListenerService<T>> _logger;
        private IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceScopeFactory">The service scope (from DI)</param>
        public BorgEventListenerService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Setup all the required services to use
        /// </summary>
        public void SetupServices()
        {
            var scope = _serviceScopeFactory.CreateScope();
            _options = scope.ServiceProvider.GetService<IOptions<BorgEventListenerOptions>>()?.Value;
            _borgTokenService = scope.ServiceProvider.GetService<BorgTokenService>();
            _borgService = scope.ServiceProvider.GetService<BorgService>();
            _WebhookService = scope.ServiceProvider.GetService<WebhookService>();
            _logger = scope.ServiceProvider.GetService<ILogger<BorgEventListenerService<T>>>();
        }

        /// <summary>
        /// This is called on kick-off of applicaition
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    await SubscribeToEvent();
                }
            });
        }

        /// <summary>
        /// Check for new events
        /// </summary>
        /// <returns></returns>
        private async Task SubscribeToEvent()
        {
            // Setup required services
            SetupServices();

            // The time to wait between syncs
            var time = _options.IntervalFrequency.ParseFrequencyConfig();

            try
            {
                // Calculate how long sleep for
                _logger.LogDebug($"{DateTime.UtcNow}|Sleep time for {typeof(T)} request service is: {time}");

                // Subscribe to the contract event
                await _borgTokenService.SubscribeToBorgEvent<T>(_borgService.SaveBorgAsync);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());
            }
            finally
            {
                // Delay until next run (cron job)
                await Task.Delay((int)time);
            }
        }
    }
}
