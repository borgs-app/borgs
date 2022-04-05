using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BorgLink.Repositories
{
    /// <summary>
    /// Interface of a repository
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepository<T>
    {
        /// <summary>
        /// Add an item
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <returns>Added item (with updated id)</returns>
        T Add(T item);

        /// <summary>
        /// Update an item
        /// </summary>
        /// <param name="item">Item to update (must be trackd)</param>
        /// <returns>Updated item</returns>
        T Update(T item);

        /// <summary>
        /// Update items
        /// </summary>
        /// <param name="items">Items to update (must be trackd)</param>
        /// <returns>Updated items</returns>
        IEnumerable<T> UpdateRange(IEnumerable<T> items);

        /// <summary>
        /// Gets all items
        /// </summary>
        /// <param name="tableName">Optional tablename param</param>
        /// <returns>List of items</returns>
        IQueryable<T> GetAll(string tableName = null);

        /// <summary>
        /// Save context
        /// </summary>
        /// <returns>Items updatd/inserted/removed in save</returns>
        int Save();
    }
}
