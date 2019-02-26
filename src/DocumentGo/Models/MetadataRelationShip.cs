namespace DocumentGo.Models
{
    public class MetadataRelationShip
    {
        public int Index { get; set; }
        public string RelationShipId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public string PrimaryEntityId { get; set; }
        public string PrimaryAttributeId { get; set; }
        public string RelatedEntityId { get; set; }
        public string RelatedAttributeId { get; set; }

        public string PrimaryEntityName { get; set; }
        public string PrimaryEntityDisplayName { get; set; }
        public string PrimaryAttributeName { get; set; }
        public string PrimaryAttributeDisplayName { get; set; }
        public string RelatedEntityName { get; set; }
        public string RelatedEntityDisplayName { get; set; }
        public string RelatedAttributeName { get; set; }
        public string RelatedAttributeDisplayName { get; set; }
    }
}