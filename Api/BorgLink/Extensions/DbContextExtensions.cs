using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorgLink.Extensions
{
    /// <summary>
    /// Extension methods for DbContext
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// Returns success if any changes have been persited - (anything > 0)
        /// </summary>
        /// <param name="context">The context to call this from</param>
        /// <returns></returns>
        public static bool EnsureSaveChanges(this DbContext context)
        {
            // Convert the number of saved records to bool
            return Convert.ToBoolean(context.SaveChanges());
        }

        /// <summary>
        /// Returns success if any changes have been persited asynchronously - (anything > 0)
        /// </summary>
        /// <param name="context">The context to call this from</param>
        /// <returns></returns>
        public static async Task<bool> EnsureSaveChangesAsync(this DbContext context)
        {
            // Convert the number of saved records to bool
            return Convert.ToBoolean(await context.SaveChangesAsync());
        }

        /// <summary>
        /// Detach all attached entities
        /// </summary>
        public static void DetachAllEntities(this DbContext context)
        {
            var changedEntriesCopy = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in changedEntriesCopy)
                entry.State = EntityState.Detached;
        }
    }
}
