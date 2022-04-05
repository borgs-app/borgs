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
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BorgLink.Services
{
    /// <summary>
    /// Event sync service
    /// </summary>
    public class BorgEventSyncPingService : BackgroundService
    {
        private BorgEventSyncPingOptions _options;
        private ILogger<BorgEventSyncPingService> _logger;
        private IServiceScopeFactory _serviceScopeFactory;
        private TelegramService _telegramService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceScopeFactory">The service scope (from DI)</param>
        public BorgEventSyncPingService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Setup all the required services to use
        /// </summary>
        public void SetupServices()
        {
            var scope = _serviceScopeFactory.CreateScope();
            _options = scope.ServiceProvider.GetService<IOptions<BorgEventSyncPingOptions>>()?.Value;
            _logger = scope.ServiceProvider.GetService<ILogger<BorgEventSyncPingService>>();
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
                _logger.LogDebug($"{DateTime.UtcNow}|Sleep time for event sync ping is: {time}");

                // Ping url
                var web = new WebClient();
                var responseString = web.DownloadString(_options.Url);
            }
            catch (Exception ex)
            {
                // _logger.LogCritical(ex.ToString());

                // Telegram loggers
                // await _telegramService.EchoAsync($"{this.GetType()} {ex.Message}");
            }
            finally
            {
                // Delay until next run (cron job)
                await Task.Delay((int)time);
            }
        }
    }
}
