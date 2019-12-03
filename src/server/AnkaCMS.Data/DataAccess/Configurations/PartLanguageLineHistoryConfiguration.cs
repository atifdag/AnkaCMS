using AnkaCMS.Data.DataEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnkaCMS.Data.DataAccess.Configurations
{

    /// <inheritdoc />
    /// <summary>
    /// Veri taban� PersonHistory tablosu konfig�rasyonu
    /// </summary>
    internal class PartLanguageLineHistoryConfiguration : IEntityTypeConfiguration<PartLanguageLineHistory>
    {
        public void Configure(EntityTypeBuilder<PartLanguageLineHistory> builder)
        {
            builder.ToTable("PartLanguageLineHistories");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).IsRequired();
            builder.Property(x => x.DisplayOrder).IsRequired();
            builder.Property(x => x.IsApproved).IsRequired();
            builder.Property(x => x.Version).IsRequired();
            builder.Property(x => x.CreationTime).IsRequired();
            builder.Property(x => x.CreatorId).IsRequired();
            builder.Property(x => x.ReferenceId).IsRequired();
            builder.Property(x => x.RestoreVersion).IsRequired();

            builder.Property(x => x.Code).IsRequired().HasColumnType("varchar(400)");
            builder.Property(x => x.Name).IsRequired().HasColumnType("varchar(400)");
            builder.Property(x => x.Description).HasColumnType("varchar(4000)");
            builder.Property(x => x.Keywords).HasColumnType("varchar(4000)");
            builder.Property(x => x.PartId).IsRequired();


            builder.Property(x => x.ReferenceId).IsRequired();
            builder.Property(x => x.LanguageId).IsRequired();

        }
    }
}
