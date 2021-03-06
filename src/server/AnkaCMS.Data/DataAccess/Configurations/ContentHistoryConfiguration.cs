﻿using AnkaCMS.Data.DataEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnkaCMS.Data.DataAccess.Configurations
{

    /// <inheritdoc />
    /// <summary>
    /// Veri tabanı ContentHistory tablosu konfigürasyonu
    /// </summary>
    internal class ContentHistoryConfiguration : IEntityTypeConfiguration<ContentHistory>
    {
        public void Configure(EntityTypeBuilder<ContentHistory> builder)
        {
            // Tablo adı
            builder.ToTable("ContentHistories");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).IsRequired();
            builder.Property(x => x.Code).IsRequired().HasColumnType("varchar(512)");
            builder.Property(x => x.CreationTime).IsRequired();
            builder.Property(x => x.CreatorId).IsRequired();
            builder.Property(x => x.ReferenceId).IsRequired();
            builder.Property(x => x.IsDeleted).IsRequired();
            builder.Property(x => x.RestoreVersion).IsRequired();

            builder.Property(x => x.CategoryId).IsRequired();

        }
    }
}
