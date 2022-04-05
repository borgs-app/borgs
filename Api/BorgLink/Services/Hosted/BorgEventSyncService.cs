using BorgLink.Ethereum;
using BorgLink.Models;
using BorgLink.Models.Options;
using BorgLink.Models.Paging;
using BorgLink.Services.Ethereum;
using BorgLink.Services.Platform;
using BorgLink.Utils;
using Hangfire;
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
        private TelegramService _telegramService;
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
            _telegramService = scope.ServiceProvider.GetService<TelegramService>();
        }

        /// <summary>
        /// This is called on kick-off of applicaition
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Setup required services
            SetupServices();

            // Check is enabled
            if (!_options.Enabled)
                return;

            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    await SyncEvents();
                    await Task.Delay(1000);
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
                var borgIds = await _borgService.GetMissingBorgIdsAsync();

                if (borgIds != null)
                {
                    // For each missing, add
                    foreach (var borgId in borgIds)
                    {
                        // Save Borg
                        var borg = new Borg(borgId);

                        // Queue job
                        _borgService.SaveBorg(borg, false);
                    }

                    // Trigger site rebuild
                    if ((borgIds?.Any() ?? false))
                        await _webhookService.PropegateAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.ToString());

                // Telegram loggers
                await _telegramService.EchoAsync($"{this.GetType()} {ex.Message}");
            }
            finally
            {
                // Delay until next run (cron job)
                await Task.Delay((int)time);
            }
        }
    }
}
