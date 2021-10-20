using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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

        /// <summary>
        /// Constructor for the middleware
        /// </summary>
        /// <param name="next">The next middleware to be executed</param>
        /// <param name="logger">The class logger</param>
        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
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
            // Write response
            context.Response.StatusCode = ((int)HttpStatusCode.InternalServerError);
            await context.Response.WriteAsync(JsonConvert.SerializeObject(ex.ToString()));

            // Return
            return;
        }
    }
}
