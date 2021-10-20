using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BorgLink.Models.Maps
{
	/// <summary>
	/// Used to map the Attributes table
	/// </summary>
	public class AttributeMap
	{
		/// <summary>
		/// Map the table
		/// </summary>
		/// <param name="entityTypeBuilder">The builder</param>
		public AttributeMap(EntityTypeBuilder<Attribute> entityTypeBuilder)
		{
			entityTypeBuilder.ToTable("Attributes", "dbo");
			entityTypeBuilder.HasKey(t => t.Id);
			entityTypeBuilder.Property(t => t.Id).UseIdentityColumn();

			entityTypeBuilder.HasMany(x => x.BorgAttributes).WithOne(x => x.Attribute);
		}
	}
}
