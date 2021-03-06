﻿using AnkaCMS.Core;
using System;

namespace AnkaCMS.Data.BaseEntities
{
    /// <inheritdoc />
    /// <summary>
    /// Değişiklik geçmişi bilgisine sahip veri tabanı nesneleri için temel nesne
    /// </summary>
    public class LanguageBaseHistoryEntity : IEntity
    {
        /// <summary>
        /// Birincil anahtar
        /// </summary>
        public Guid Id { get; set; }


        public string Code { get; set; }

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
        /// Silindi mı?
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Geri dönüldüğü sürüm No
        /// </summary>
        public int RestoreVersion { get; set; }
    }
}