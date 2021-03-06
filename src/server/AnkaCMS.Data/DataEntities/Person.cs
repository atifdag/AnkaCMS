﻿using AnkaCMS.Core;
using System;
using System.Collections.Generic;

namespace AnkaCMS.Data.DataEntities
{
    public class Person : IEntity
    {
        public Guid Id { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsApproved { get; set; }
        public int Version { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime LastModificationTime { get; set; }
        public Guid LastModifierId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName => FirstName + " " + LastName;
        public string IdentityCode { get; set; }
        public DateTime BirthDate { get; set; }
        public string Biography { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
