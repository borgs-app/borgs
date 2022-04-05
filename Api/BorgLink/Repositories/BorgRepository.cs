using System.Linq;
using BorgLink.Models;
using System.Collections.Generic;
using BorgLink.Utils;
using BorgLink.Context.Contexts;
using System;
using Microsoft.EntityFrameworkCore;

namespace BorgLink.Repositories
{
    /// <summary>
    /// Used for CRUD on the borgs table
    /// </summary>
    public class BorgRepository : BaseRepository<Borg, BorgContext>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">The database context</param>
        public BorgRepository(BorgContext context) :
            base(context)
        {
        }

        /// <summary>
        /// Gets all borgs with full mappings to attributes
        /// </summary>
        /// <returns></returns>
        public IQueryable<Borg> GetAllWithAttributes()
        {
            return _context.Borgs
                .Include(x => x.BorgAttributes)
                .ThenInclude(x => x.Attribute)
                .AsNoTracking();
        }
    }
}
