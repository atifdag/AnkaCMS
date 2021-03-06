using AnkaCMS.Data.DataEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnkaCMS.Data.DataAccess.Configurations
{

    /// <inheritdoc />
    /// <summary>
    /// Veri tabanı CategoryLanguageLine tablosu konfigürasyonu
    /// </summary>
    internal class CategoryLanguageLineConfiguration : IEntityTypeConfiguration<CategoryLanguageLine>
    {
        public void Configure(EntityTypeBuilder<CategoryLanguageLine> builder)
        {
            builder.ToTable("CategoryLanguageLines");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).IsRequired();
            builder.Property(x => x.DisplayOrder).IsRequired();
            builder.HasIndex(x => x.DisplayOrder).IsUnique(false).HasName("IX_CategoryLanguageLineDisplayOrder");
            builder.Property(x => x.IsApproved).IsRequired();
            builder.Property(x => x.Version).IsRequired();
            builder.Property(x => x.CreationTime).IsRequired();
            builder.HasOne(x => x.Creator).WithMany(y => y.CategoryLanguageLinesCreatedBy).IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.Property(x => x.LastModificationTime).IsRequired();
            builder.HasOne(x => x.LastModifier).WithMany(y => y.CategoryLanguageLinesLastModifiedBy).IsRequired().OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.Code).IsRequired().HasColumnType("varchar(400)");
            builder.HasIndex(x => x.Code).IsUnique().HasName("UK_CategoryLanguageLineCode");
            builder.Property(x => x.Name).IsRequired().HasColumnType("varchar(400)");
            builder.Property(x => x.Description).HasColumnType("varchar(4000)");
            builder.Property(x => x.Keywords).HasColumnType("varchar(4000)");

            builder.HasOne(x => x.Category).WithMany(y => y.CategoryLanguageLines).IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Language).WithMany(y => y.CategoryLanguageLines).IsRequired().OnDelete(DeleteBehavior.Restrict);


        }
    }
}
