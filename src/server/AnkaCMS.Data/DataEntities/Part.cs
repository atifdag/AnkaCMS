using AnkaCMS.Data.BaseEntities;
using System.Collections.Generic;

namespace AnkaCMS.Data.DataEntities
{
    public class Part : LanguageBaseEntity
    {
        public int MaxItemCount { get; set; }
        public virtual ICollection<PartContentLine> PartContentLines { get; set; }
        public virtual ICollection<PartLanguageLine> PartLanguageLines { get; set; }
    }
}
