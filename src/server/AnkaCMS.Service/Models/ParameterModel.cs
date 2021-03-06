﻿using AnkaCMS.Core;
using AnkaCMS.Core.ValueObjects;
using System;

namespace AnkaCMS.Service.Models
{
    public class ParameterModel : IServiceModel
    {
        public Guid Id { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsApproved { get; set; }
        public int Version { get; set; }
        public DateTime CreationTime { get; set; }
        public IdCodeName Creator { get; set; }
        public DateTime LastModificationTime { get; set; }
        public IdCodeName LastModifier { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public bool Erasable { get; set; }
        public string Description { get; set; }
        public IdCodeName ParameterGroup { get; set; }

    }
}
