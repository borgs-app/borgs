using BorgLink.Context.Contexts;
using BorgLink.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Repositories
{
    /// <summary>
    /// Used for CRUD on the borg attributes table
    /// </summary>
    public class BorgAttributeRepository : BaseRepository<BorgAttribute, BorgContext>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">The database context</param>
        public BorgAttributeRepository(BorgContext context) :
            base(context)
        {
        }

        /// <summary>
        /// Gets all borg attributes with the attribute attached
        /// </summary>
        /// <returns></returns>
        public IQueryable<BorgAttribute> GetAllWithAttribute()
        {
            return _context.BorgAttributes
                .Include(x => x.Attribute);
        }

        /// <summary>
        /// Gets all borg attributes with the attribute attached
        /// </summary>
        /// <returns></returns>
        public IQueryable<BorgAttribute> GetAllWithBorgAndAttribute()
        {
            return _context.BorgAttributes
                .Include(x => x.Borg)
                .Include(x => x.Attribute);
        }

        public IQueryable<BorgAttribute> GetAllWithBorg()
        {
            return _context.BorgAttributes
                .Include(x => x.Borg);
        }
    }
}
