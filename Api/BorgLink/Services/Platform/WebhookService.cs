using BorgLink.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BorgLink.Services.Platform
{
    /// <summary>
    /// For webhook calls
    /// </summary>
    public class WebhookService : BaseHttpService
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        public WebhookService(HttpClient client) : base(client)
        {
        }

        /// <summary>
        /// Emit webhook event
        /// </summary>
        /// <returns>The operations success</returns>
        public async Task<bool> PropegateAsync()
        {
            try
            {
                return await PostAsync<bool, object>(string.Empty, null);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
