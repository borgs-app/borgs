using BorgLink.Context.Contexts;
using BorgLink.Models;
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
    }
}
