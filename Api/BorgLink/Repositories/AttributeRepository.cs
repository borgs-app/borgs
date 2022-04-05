using BorgLink.Context.Contexts;
using BorgLink.Models;
using BorgLink.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Repositories
{
    /// <summary>
    /// Used for CRUD on the attributes table
    /// </summary>
    public class AttributeRepository : BaseRepository<Models.Attribute, BorgContext>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">The database context</param>
        public AttributeRepository(BorgContext context) :
            base(context)
        {
        }

        public IQueryable<AttributeCount> GetAttributeCounts(Condition condition)
        {
            var initialQuery =  _context.BorgAttributes
                .Include(x => x.Attribute)
                .Include(x => x.Borg);

            IQueryable<BorgAttribute> finalQuery = null;

            if (condition == Condition.Alive)
                finalQuery = initialQuery.Where(x => x.Borg.ChildId == null);
            else if (condition == Condition.Dead)
                finalQuery = initialQuery.Where(x => x.Borg.ChildId != null);

            return finalQuery.Select(x => x.Attribute.Name)
                .GroupBy(x => x)
                .Select(x => new AttributeCount() { Name = x.Key, Count = x.Count() });
        }
    }
}
