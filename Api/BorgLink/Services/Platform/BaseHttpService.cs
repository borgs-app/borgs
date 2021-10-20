using Microsoft.AspNetCore.Authentication.JwtBearer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BorgLink.Services.Platform
{
    /// <summary>
    /// Base Http functions
    /// </summary>
    public class BaseHttpService
    {
        /// <summary>
        /// THe web client used to make Http calls
        /// </summary>
        protected readonly HttpClient _httpClient;

        /// <summary>
        /// Base Http service
        /// </summary>
        /// <param name="client">Web client</param>
        public BaseHttpService(HttpClient client)
        {
            _httpClient = client;
        }

        /// <summary>
        /// Http GET
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="url">The extension</param>
        /// <param name="additionalHeaders">Additional headers to set in request</param>
        /// <returns>T</returns>
        protected async Task<T> GetAsync<T>(string url, List<KeyValuePair<string, string>> additionalHeaders = null)
        {
            AddHeaders(additionalHeaders);

            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<T>(content);

            throw new Exception($"[{response.StatusCode}]:{response.ReasonPhrase}");
        }

        /// <summary>
        /// Http POST
        /// </summary>
        /// <param name="uri">The extension</param>
        /// <param name="model">The request param</param>
        /// <typeparam name="T1">Response type</typeparam>
        /// <typeparam name="T2">Request param</typeparam>
        /// <param name="additionalHeaders">Additional headers to set in request</param>
        /// <returns>T1</returns>
        protected async Task<T1> PostAsync<T1, T2>(string uri, T2 model, List<KeyValuePair<string, string>> additionalHeaders = null)
            where T2 : class
        {
            AddHeaders(additionalHeaders);

            var serializedObject = JsonConvert.SerializeObject(model);
            var content = new StringContent(serializedObject, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(uri, content);
            var responseAsString = await response?.Content?.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(responseAsString))
                return JsonConvert.DeserializeObject<T1>(responseAsString);

            return default(T1);
        }

        /// <summary>
        /// Http PUT
        /// </summary>
        /// <param name="uri">The extension</param>
        /// <param name="model">The request param</param>
        /// <typeparam name="T1">Response type</typeparam>
        /// <typeparam name="T2">Request param</typeparam>
        /// <param name="additionalHeaders">Additional headers to set in request</param>
        /// <returns>T1</returns>
        public async Task<T1> PutAsync<T1, T2>(string uri, T2 model, List<KeyValuePair<string, string>> additionalHeaders)
        {
            AddHeaders(additionalHeaders);

            var serializedObject = JsonConvert.SerializeObject(model);
            var content = new StringContent(serializedObject, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(uri, content);
            var responseAsString = await response?.Content?.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T1>(responseAsString);
        }

        /// <summary>
        /// Http PATCH
        /// </summary>
        /// <param name="uri">The extension</param>
        /// <param name="model">The request param</param>
        /// <typeparam name="T1">Response type</typeparam>
        /// <typeparam name="T2">Request param</typeparam>
        /// <param name="additionalHeaders">Additional headers to set in request</param>
        /// <returns>T1</returns>
        protected async Task<T1> PatchAsync<T1, T2>(string uri, T2 model, List<KeyValuePair<string, string>> additionalHeaders = null)
        {
            AddHeaders(additionalHeaders);

            var serializedObject = JsonConvert.SerializeObject(model);
            var content = new StringContent(serializedObject, Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync(uri, content);
            var responseAsString = await response?.Content?.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T1>(responseAsString);
        }

        /// <summary>
        /// Http DELETE
        /// </summary>
        /// <param name="uri">The extension</param>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="additionalHeaders">Additional headers to set in request</param>
        /// <returns>T</returns>
        protected async Task<T> DeleteAsync<T>(string uri, List<KeyValuePair<string, string>> additionalHeaders = null)
        {
            AddHeaders(additionalHeaders);

            var response = await _httpClient.DeleteAsync(uri);
            var responseAsString = await response?.Content?.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseAsString);
        }

        /// <summary>
        /// Add headers to a request
        /// </summary>
        /// <param name="additionalHeaders"></param>
        private void AddHeaders(List<KeyValuePair<string, string>> additionalHeaders = null)
        {
            // Check if exist
            if (additionalHeaders == null)
                additionalHeaders = new List<KeyValuePair<string, string>>();

            // Add headers
            foreach (var header in additionalHeaders)
            {
                if (header.Key == "Authorization")
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, header.Value);
                else
                    _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }
}
