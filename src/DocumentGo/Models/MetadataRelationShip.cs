namespace DocumentGo.Models
{
    public class MetadataRelationShip
    {
        public string Type { get; set; }
        public string PrimaryEntityId { get; set; }
        public string PrimaryAttributeId { get; set; }
        public string RelatedEntityId { get; set; }
        public string RelatedAttributeId { get; set; }
    }
}