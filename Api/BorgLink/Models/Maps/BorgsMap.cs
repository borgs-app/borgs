using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BorgLink.Models.Maps
{
	/// <summary>
	/// Used to map the Borgs table
	/// </summary>
	public class BorgsMap
	{
		/// <summary>
		/// Map the table
		/// </summary>
		/// <param name="entityTypeBuilder">The builder</param>
		public BorgsMap(EntityTypeBuilder<Borg> entityTypeBuilder)
		{
			// Setup the table mapping - Borg ID must be set as its not Identity(1,1)
			entityTypeBuilder.ToTable("Borgs", "dbo");
			entityTypeBuilder.HasKey(t => t.BorgId);
			entityTypeBuilder.Property(t => t.BorgId);

			// Ignore the attributes since its not a db property
			entityTypeBuilder.Ignore(x => x.Attributes);

			// Map the attributes
			entityTypeBuilder.HasMany(x => x.BorgAttributes).WithOne(x => x.Borg);
		}
	}
}
