using System;

namespace BorgLink.Api.Models
{
    /// <summary>
    /// The cache item (can be any object - T)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CachedResultViewModel<T>
    {
        /// <summary>
        /// The cached item
        /// </summary>
        public T Item { get; set; }

        /// <summary>
        /// The date time the item was cahced
        /// </summary>
        public DateTime DateCached { get; set; }
    }
}
