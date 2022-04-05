using BorgLink.Models.Options;
using BorgLink.Utils;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BorgLink.Services
{
    public class TelegramService
    {
        private readonly IDictionary<long, TelegramBotClient> _botClients = new Dictionary<long, TelegramBotClient>();
        private readonly TelegramOptions _options;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="token"></param>
        public TelegramService(IOptions<TelegramOptions> options)
        {
            _options = options.Value;

            // Setup accounts
            foreach (var account in _options.Accounts)
            {
                _botClients.Add(account.Id, new TelegramBotClient(account.Token));
            }
        }

        /// <summary>
        /// Send a message to the telegram client
        /// </summary>
        /// <param name="message">The message to send the telegram client</param>
        /// <returns>The task</returns>
        public async Task EchoAsync(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            message = $"{DateTime.UtcNow} [{Environment.MachineName}] | {message}";
            // message = message.Substring(0, message.Length - 1 > 1000 ? 1000 : message.Length);

            // Echo to each account
            foreach (var botClient in _botClients)
            {
                // Split sets
                var splitSets = message.SplitIntoChunks(999);

                // Send each set
                foreach (var splitSet in splitSets)
                {
                    await botClient.Value.SendTextMessageAsync(new ChatId(botClient.Key), splitSet);
                }
            }
        }
    }
}
