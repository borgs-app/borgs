using BorgLink.Models.Enums;
using BorgLink.Services.Platform;
using BorgLink.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BorgLink.Services.Middleware
{
    /// <summary>
    /// Middleware to intercept 500 errors as to log and format proper response
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly FailedResponseMappingService _failedResponseMappingService;
        private readonly TelegramService _telegramService;

        /// <summary>
        /// Constructor for the middleware
        /// </summary>
        /// <param name="next">The next middleware to be executed</param>
        /// <param name="failedResponseMappingService"></param>
        /// <param name="logger">The class logger</param>
        public ErrorHandlingMiddleware(RequestDelegate next, FailedResponseMappingService failedResponseMappingService, TelegramService telegramService,
            ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _failedResponseMappingService = failedResponseMappingService;
            _telegramService = telegramService;
            _logger = logger;
        }

        /// <summary>
        /// Invoked via reflection on request passthrough
        /// </summary>
        /// <param name="context">The request context</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context); // Call next middleware
            }
            catch (Exception ex)
            {
                // Generate bad request log and return (stop any further execution of middleware stack/request
                await GenerateInternalServerError(context, ex);
                return;
            }
        }

        /// <summary>
        /// Replies with a 500 response and a special code for admin to identify in the logs
        /// </summary>
        /// <param name="context">The context to make the response to</param>
        /// <param name="ex">The current exception thrown</param>
        /// <returns>A completed task</returns>
        private async Task GenerateInternalServerError(HttpContext context, Exception ex)
        {
            // Create hash (unique)
            var randomHash = StringUtility.RandomString(20);

            // Get the request being made
            var url = UriHelper.GetDisplayUrl(context.Request);

            // Build up the log request
            var requestUrlString = $"Request url: {url},Request Method: {context.Request.Method},Request Schem: {context.Request.Scheme}, UserAgent: {context.Request.Headers[HeaderNames.UserAgent]}";

            // Add to log (so we can cross check
            _logger.LogError($"Code[{randomHash}]: {ex} {Environment.NewLine}{requestUrlString}");

            // Create error message
            var wrappedError = _failedResponseMappingService.Map(FailedReason.SystemError, HttpStatusCode.InternalServerError, randomHash, null, ex.StackTrace);

            // Output to telegram
            await _telegramService.EchoAsync($"{this.GetType()} {ex.Message}");

            // Write response
            context.Response.StatusCode = ((int)HttpStatusCode.InternalServerError);
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(wrappedError));

            // Return
            return;
        }
    }
}
