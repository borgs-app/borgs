using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BorgLink.Models.Enums;
using BorgLink.Api.Models;
using BorgLink.Utils;

namespace BorgLink.Services.Platform
{
    /// <summary>
    /// Maps requests/operations using app config settings
    /// </summary>
    public class FailedResponseMappingService
    {
        /// <summary>
        /// Configuration for the failed response reasons
        /// </summary>
        private Dictionary<FailedReason, string> _failedResponseMappings;

        /// <summary>
        /// Class logger
        /// </summary>
        private readonly ILogger<FailedResponseMappingService> _loggingService;

        /// <summary>
        /// Instance of API
        /// </summary>
        private readonly string _instance;

        /// <summary>
        /// Constructor
        /// </summary>
        public FailedResponseMappingService(ILogger<FailedResponseMappingService> loggingService, IOptions<List<ErrorMap>> failedResponseMappings, string instance)
        {
            _loggingService = loggingService;
            _instance = instance;
            _failedResponseMappings = failedResponseMappings.Value?.ToDictionary(x => ConvertToFailedReason(x.Key), y => y.Value);
        }

        /// <summary>
        /// Safely convert a string to a failed reason. If for any reason it cant be mapped, it is returned as none
        /// </summary>
        /// <param name="key">The value to parse and convert to enum</param>
        /// <returns>Mapped enum</returns>
        private FailedReason ConvertToFailedReason(string key)
        {
            return (FailedReason)Enum.Parse(typeof(FailedReason), key);
        }

        /// <summary>
        /// Maps failed responses from configuration (text string)
        /// </summary>
        /// <param name="reason">The generic reason to map</param>
        /// <param name="status">The failed response status</param>
        /// <param name="traceId">The trace code</param>
        /// <param name="relatedProperty">The related property</param>
        /// <param name="stackTrace">The error debug message (if running in dev)</param>
        /// <returns>A human readable response string for type of error (if exists in configuration)</returns>
        public BorgsProblemDetails Map(FailedReason reason, HttpStatusCode status, string traceId, Models.Enums.Property? relatedProperty, string stackTrace = null)
        {
            var problemDetails = new BorgsProblemDetails();

            if (_failedResponseMappings.TryGetValue(reason, out var errorMessageMapping))
            {
                problemDetails.Title = reason.ToString();
                problemDetails.Stacktrace = stackTrace;
                problemDetails.Status = (int)status;
                problemDetails.TraceId = traceId;

                problemDetails.Errors = ErrorUtility.BuildErrors((relatedProperty?.ToString()?.LowercaseFirstLetter() ?? reason.ToString(), errorMessageMapping));

                return problemDetails;
            }

            return null;
        }

        /// <summary>
        /// Maps failed responses from configuration (text string)
        /// </summary>
        /// <param name="reasons">The generic reasons to map</param>
        /// <param name="status">The failed response status</param>
        /// <param name="traceId">The trace code</param>
        /// <param name="relatedProperty">A related property to the error (if exists). If it doesn't exist then the message mapping code is provided</param>
        /// <param name="stackTrace">The error debug message (if running in dev)</param>
        /// <returns>A human readable response string for type of error (if exists in configuration)</returns>
        public BorgsProblemDetails Map(FailedReason parentFailedReason, List<(int index, FailedReason failedReason)> reasons, HttpStatusCode status, string traceId, Models.Enums.Property? relatedProperty, string stackTrace = null)
        {
            var problemDetails = new BorgsProblemDetails();
            var errors = new List<(string, string)>();

            foreach (var reason in reasons)
            {
                if (_failedResponseMappings.TryGetValue(reason.failedReason, out var errorMessageMapping))
                {
                    errors.Add(($"{relatedProperty?.ToString()?.LowercaseFirstLetter() ?? reason.ToString()}[{reason.index}]",errorMessageMapping));
                }
            }

            problemDetails.Title = parentFailedReason.ToString();
            problemDetails.Stacktrace = stackTrace;
            problemDetails.Status = (int)status;
            problemDetails.TraceId = traceId;

            problemDetails.Errors = ErrorUtility.BuildErrors(errors.ToArray());

            return problemDetails;

            return null;
        }
    }
}
