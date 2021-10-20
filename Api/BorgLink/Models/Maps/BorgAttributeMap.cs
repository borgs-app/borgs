using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.Maps
{
	/// <summary>
	/// Used to map the BorgAttributes table
	/// </summary>
	public class BorgAttributeMap
	{
		/// <summary>
		/// Map the table
		/// </summary>
		/// <param name="entityTypeBuilder">The builder</param>
		public BorgAttributeMap(EntityTypeBuilder<BorgAttribute> entityTypeBuilder)
		{
			entityTypeBuilder.ToTable("BorgAttributes", "dbo");
			entityTypeBuilder.HasKey(t => t.Id);
			entityTypeBuilder.Property(t => t.Id).UseIdentityColumn();

			entityTypeBuilder.HasOne(x => x.Attribute).WithMany(x => x.BorgAttributes);
			entityTypeBuilder.HasOne(x => x.Borg).WithMany(x => x.BorgAttributes);
		}
	}
}
