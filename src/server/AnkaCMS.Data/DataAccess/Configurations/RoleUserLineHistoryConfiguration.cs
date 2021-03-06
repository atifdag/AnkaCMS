using AnkaCMS.Data.DataEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnkaCMS.Data.DataAccess.Configurations
{

    /// <inheritdoc />
    /// <summary>
    /// Veri tabanı PersonHistory tablosu konfigürasyonu
    /// </summary>
    internal class RoleUserLineHistoryConfiguration : IEntityTypeConfiguration<RoleUserLineHistory>
    {
        public void Configure(EntityTypeBuilder<RoleUserLineHistory> builder)
        {
            builder.ToTable("RoleUserLineHistories");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).IsRequired();
            builder.Property(x => x.DisplayOrder).IsRequired();
            builder.Property(x => x.Version).IsRequired();
            builder.Property(x => x.CreationTime).IsRequired();
            builder.Property(x => x.CreatorId).IsRequired();
            builder.Property(x => x.ReferenceId).IsRequired();
            builder.Property(x => x.RestoreVersion).IsRequired();


            builder.Property(x => x.RoleId).IsRequired();
            builder.Property(x => x.UserId).IsRequired();

        }
    }
}
