using AnkaCMS.Data.DataEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnkaCMS.Data.DataAccess.Configurations
{

    /// <inheritdoc />
    /// <summary>
    /// Veri tabanı Person tablosu konfigürasyonu
    /// </summary>
    internal class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.ToTable("Persons");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).IsRequired();
            builder.Property(x => x.DisplayOrder).IsRequired();
            builder.HasIndex(x => x.DisplayOrder).IsUnique(false).HasName("IX_PersonDisplayOrder");
            builder.Property(x => x.IsApproved).IsRequired();
            builder.Property(x => x.Version).IsRequired();
            builder.Property(x => x.CreationTime).IsRequired();
            builder.Property(x => x.CreatorId).IsRequired();
            builder.Property(x => x.LastModifierId).IsRequired();
            builder.Property(x => x.LastModificationTime).IsRequired();

            builder.Property(x => x.FirstName).IsRequired().HasColumnType("varchar(100)");
            builder.Property(x => x.LastName).IsRequired().HasColumnType("varchar(100)");
            builder.Property(x => x.IdentityCode).HasColumnType("varchar(36)");
            builder.HasIndex(x => x.IdentityCode).IsUnique().HasName("UK_PersonIdentityCode");
            builder.Property(x => x.BirthDate).IsRequired();
            builder.Property(x => x.Biography).HasColumnType("text");
        }
    }
}
