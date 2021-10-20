using BorgLink.Extensions;
using BorgLink.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BorgLink.Repositories
{
    /// <summary>
    /// The base functionality of a repository - implements an interface
    /// </summary>
    /// <typeparam name="T">The type of repository</typeparam>
    /// <typeparam name="T2">The context (X:DbContext)</typeparam>
    public abstract class BaseRepository<T, T2> : IRepository<T>
       where T2 : DbContext where T : class
    {
        /// <summary>
        /// Database context
        /// </summary>
        protected readonly T2 _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">The database context</param>
        public BaseRepository(T2 context)
        {
            // Setup the context
            _context = context;
        }

        /// <summary>
        /// Add an item to the context
        /// </summary>
        /// <param name="item">Item to add to context</param>
        /// <returns>Added item to context</returns>
        public T Add(T item)
        {
            _context.Add(item);
            return item;
        }

        /// <summary>
        /// Add items to the context
        /// </summary>
        /// <param name="items">Items to add to context</param>
        /// <returns>Added items to context</returns>
        public List<T> AddRange(List<T> items)
        {
            _context.AddRange(items);
            return items;
        }

        /// <summary>
        /// Remove items to the context
        /// </summary>
        /// <returns>Removed items to context</returns>
        public List<T> RemoveRange(List<T> items)
        {
            _context.RemoveRange(items);
            return items;
        }

        /// <summary>
        /// Gets all items from context - is overrideable
        /// </summary>
        /// <param name="tableName">Optional tablename param</param>
        /// <returns>List of items</returns>
        public virtual IEnumerable<T> GetAll(string tableName = null)
        {
            return _context.GetPropertyValue(tableName ?? $"{typeof(T).Name}s");
        }

        /// <summary>
        /// Updates a tracked item in the context
        /// </summary>
        /// <param name="item">The item to update</param>
        /// <returns>The updated item</returns>
        public T Update(T item)
        {
            _context.Attach(item);
            _context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            return item;
        }

        /// <summary>
        /// Updates items in context
        /// </summary>
        /// <param name="items">Items to update in context</param>
        /// <returns>Updated items in context</returns>
        public IEnumerable<T> UpdateRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                _context.Attach(item);
                _context.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }

            return items;
        }

        /// <summary>
        /// Removes a tracked item in the context
        /// </summary>
        /// <param name="item">The item to remove</param>
        public void Remove(T item)
        {
            _context.Remove(item);
        }

        /// <summary>
        /// Saves the context
        /// </summary>
        /// <returns>The number of records inserted/removed/updated</returns>
        public int Save()
        {
            return _context.SaveChanges();
        }

        /// <summary>
        /// Returns true if the number of updated entities > 0
        /// </summary>
        /// <returns>True/false</returns>
        public bool EnsureSaveChanges()
        {
            return _context.EnsureSaveChanges();
        }

        /// <summary>
        /// Returns true if the number of updated entities > 0 - asynchronously
        /// </summary>
        /// <returns>True/false</returns>
        public async Task<bool> EnsureSaveChangesAsync()
        {
            return await _context.EnsureSaveChangesAsync();
        }

        /// <summary>
        /// Detaches all current entities
        /// </summary>
        public void DetachAllEntities()
        {
            _context.DetachAllEntities();
        }
    }
}
