using System.Collections.Generic;

namespace DocumentGo.Models
{
    public class MetadataEntity
    {
        public string EntityId { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Application { get; set; }

        public List<MetadataAttribute> Attributes { get; set; }
    }
}