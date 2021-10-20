using BorgLink.Api.Models;
using BorgLink.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Controllers
{
    /// <summary>
    /// Shared controller functionality
    /// </summary>
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// The cache
        /// </summary>
        private readonly MemoryCacheService _cacheService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cacheService">The cache</param>
        public BaseController(MemoryCacheService cacheService) 
        {
            _cacheService = cacheService;
        }

        /// <summary>
        /// Gets or sets item from cache if doesnt already exist
        /// </summary>
        /// <typeparam name="T">The type to cache</typeparam>
        /// <param name="cacheKey">What to cahce the object under</param>
        /// <param name="action">The action to get the item</param>
        /// <param name="expiresInSeconds">When to expire item from cache (in seconds)</param>
        /// <returns>Cached item</returns>
        protected T GetCachedItem<T>(string cacheKey, Func<T> action, long? expiresInSeconds = null)
            where T : class
        {
            var cachedResult = _cacheService.GetValue<CachedResultViewModel<T>>(cacheKey);

            if (cachedResult == null)
            {
                // Get and return user data
                var itemToCache = action();

                if (itemToCache != null)
                {
                    var cachedItem = new CachedResultViewModel<T>() { Item = itemToCache, DateCached = DateTime.UtcNow };
                    _cacheService.SetValue<CachedResultViewModel<T>>(cacheKey, cachedItem,
                        TimeSpan.FromSeconds(expiresInSeconds.Value));

                    return itemToCache;
                }

                return null;
            }

            return cachedResult.Item;
        }
    }
}
