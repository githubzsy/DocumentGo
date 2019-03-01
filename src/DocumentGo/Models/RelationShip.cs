namespace DocumentGo.Models
{
    public class RelationShip
    {
        public bool IsMetadata { get; set; }
        public string PrimaryTableName { get; set; }
        public string PrimaryColumnName { get; set; }
        public string RelatedTableName { get; set; }
        public string RelatedColumnName { get; set; }
    }
}