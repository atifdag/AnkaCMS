﻿using AnkaCMS.Data.DataEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnkaCMS.Data.DataAccess.Configurations
{

    /// <inheritdoc />
    /// <summary>
    /// Veri tabanı LanguageHistory tablosu konfigürasyonu
    /// </summary>
    internal class LanguageHistoryConfiguration : IEntityTypeConfiguration<LanguageHistory>
    {
        public void Configure(EntityTypeBuilder<LanguageHistory> builder)
        {
            // Tablo adı
            builder.ToTable("LanguageHistories");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).IsRequired();
            builder.Property(x => x.DisplayOrder).IsRequired();
            builder.Property(x => x.IsApproved).IsRequired();
            builder.Property(x => x.Version).IsRequired();
            builder.Property(x => x.CreationTime).IsRequired();
            builder.Property(x => x.CreatorId).IsRequired();
            builder.Property(x => x.ReferenceId).IsRequired();
            builder.Property(x => x.IsDeleted).IsRequired();
            builder.Property(x => x.RestoreVersion).IsRequired();


            builder.Property(x => x.Code).IsRequired().HasColumnType("varchar(36)");
            builder.Property(x => x.Name).IsRequired().HasColumnType("varchar(100)");
            builder.Property(x => x.Description).HasColumnType("varchar(512)");

        }
    }
}
