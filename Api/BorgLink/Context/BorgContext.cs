using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;
using BorgLink.Models;
using BorgLink.Models.Maps;

namespace BorgLink.Context.Contexts
{
    /// <summary>
    /// Database context
    /// </summary>
    public class BorgContext : DbContext
    {
        /// <summary>
        /// The "[dbo].[Borgs]" table
        /// </summary>
        public DbSet<Borg> Borgs { get; set; }

        /// <summary>
        /// The "[dbo].[Attributes]" table
        /// </summary>
        public DbSet<Models.Attribute> Attributes { get; set; }

        /// <summary>
        /// The "[dbo].[Borgattributes]" table
        /// </summary>
        public DbSet<BorgAttribute> BorgAttributes { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">The database options</param>
        public BorgContext(DbContextOptions<BorgContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        /// <summary>
        /// Called on model creation (mapping can be done here)
        /// </summary>
        /// <param name="builder">Maps the relationships etc. once models are built</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            _ = new BorgsMap(builder.Entity<Borg>());
            _ = new AttributeMap(builder.Entity<Models.Attribute>());
            _ = new BorgAttributeMap(builder.Entity<BorgAttribute>());

            base.OnModelCreating(builder);
        }
    }
}
