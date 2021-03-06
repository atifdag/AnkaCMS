﻿using AnkaCMS.Data.BaseEntities;
using System.Collections.Generic;

namespace AnkaCMS.Data.DataEntities
{
    public class Role : BaseEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public int Level { get; set; }

        public virtual ICollection<RolePermissionLine> RolePermissionLines { get; set; }
        public virtual ICollection<RoleUserLine> RoleUserLines { get; set; }

    }
}
