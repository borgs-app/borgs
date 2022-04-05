using BorgLink.Models.Options;
using BorgLink.Services.Platform;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BorgLink.Services
{
    public class OpenSeaService : BaseHttpService
    {
        private OpenSeaOptions _options;

        public OpenSeaService(IOptions<OpenSeaOptions> options, HttpClient client) : 
            base(client)
        {
            _options = options.Value;
        }

        public bool RefreshOpenseaImage(int id)
        {
            /*
            var uri = $"/api/v1/asset/0x375df763cd7b87e3ffb8efad812aae088553664c/{id}/?force_update=true";

            try
            {
                return await PostAsync<bool, object>(uri, null);
            }
            catch (Exception ex)
            {
                return false;
            }
            */

            return true;
        }
    }
}
