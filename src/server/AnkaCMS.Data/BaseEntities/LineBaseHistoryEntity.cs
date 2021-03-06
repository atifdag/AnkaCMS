﻿using AnkaCMS.Core;
using System;

namespace AnkaCMS.Data.BaseEntities
{
    /// <inheritdoc />
    /// <summary>
    /// Değişiklik geçmişi bilgisine sahip çoktan çoka ilişkli veri tabanı nesneleri için temel nesne
    /// </summary>
    public class LineBaseHistoryEntity : IEntity
    {
        /// <summary>
        /// Birincil anahtar
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Sıra No
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Sürüm No
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Oluşturulma zamanı
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Oluşturan kullanıcı
        /// </summary>
        public Guid CreatorId { get; set; }


        /// <summary>
        /// Kopyası tutulan referans satırın Id'si
        /// </summary>
        public Guid ReferenceId { get; set; }

        /// <summary>
        /// Geri dönüldüğü sürüm No
        /// </summary>
        public int RestoreVersion { get; set; }
    }

}
