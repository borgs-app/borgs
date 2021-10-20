using BorgLink.Ethereum;
using BorgLink.Models;
using BorgLink.Models.Options;
using BorgLink.Models.Paging;
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
    /// Event sync service
    /// </summary>
    public class BorgEventSyncService : BackgroundService
    {
        private BorgEventSyncOptions _options;
        private BorgService _borgService;
        private WebhookService _webhookService;
        private ILogger<BorgEventSyncService> _logger;
        private IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceScopeFactory">The service scope (from DI)</param>
        public BorgEventSyncService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Setup all the required services to use
        /// </summary>
        public void SetupServices()
        {
            var scope = _serviceScopeFactory.CreateScope();
            _options = scope.ServiceProvider.GetService<IOptions<BorgEventSyncOptions>>()?.Value;
            _borgService = scope.ServiceProvider.GetService<BorgService>();
            _logger = scope.ServiceProvider.GetService<ILogger<BorgEventSyncService>>();
            _webhookService = scope.ServiceProvider.GetService<WebhookService>();
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
                    await SyncEvents();
                }
            });
        }

        /// <summary>
        /// Check for new events
        /// </summary>
        /// <returns></returns>
        private async Task SyncEvents()
        {
            // Setup required services
            SetupServices();

            // The time to wait between syncs
            var time = _options.IntervalFrequency.ParseFrequencyConfig();

            try
            {
                // Calculate how long sleep for
                _logger.LogDebug($"{DateTime.UtcNow}|Sleep time for event sync is: {time}");

                // Get all borgs missing from firebase
                var borgIds = _borgService.GetMissingBorgIds();

                // For each missing, add
                foreach (var borgId in borgIds)
                {
                    // Save Borg
                    await _borgService.SaveBorgAsync(new Borg(borgId), false);
                }

                // Trigger site rebuild
                await _webhookService.PropegateAsync();
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
