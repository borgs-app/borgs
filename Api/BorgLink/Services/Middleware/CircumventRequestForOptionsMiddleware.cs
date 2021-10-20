using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BorgLink.Services.Middleware
{
    /// <summary>
    /// Enables auto 200 response for CORS OPTIONS requests from browser
    /// </summary>
    public class CircumventRequestForOptionsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CircumventRequestForOptionsMiddleware> _logger;

        /// <summary>
        /// Constructor for the middleware
        /// </summary>
        /// <param name="next">The next middleware to be executed</param>
        /// <param name="logger">The class logger</param>
        public CircumventRequestForOptionsMiddleware(RequestDelegate next, ILogger<CircumventRequestForOptionsMiddleware> logger)
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
            // Return ok if its an options call (don't want to forward to post method)
            if (context.Request.Method == HttpMethods.Options)
            {
                await Ok(context);
                return;
            }

            await _next(context);  // Call next middleware if we can
        }

        /// <summary>
        /// Replies with a 200 response
        /// </summary>
        /// <param name="context">The context to make the response to</param>
        /// <returns>A completed task</returns>
        private async Task Ok(HttpContext context)
        {
            context.Response.StatusCode = ((int)HttpStatusCode.OK);
            await context.Response.WriteAsync(string.Empty);
            return;
        }
    }
}
