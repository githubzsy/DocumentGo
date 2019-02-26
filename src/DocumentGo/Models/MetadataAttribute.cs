namespace DocumentGo.Models
{
    public class MetadataAttribute
    {
        public string AttributeId { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string AttributeType { get; set; }
        public string DbType { get; set; }
        public string IsPrimaryAttribute { get; set; }
        public string IsNullable { get; set; }

        public string Length { get; set; }
        public string Remark { get; set; }
        public string DecimalPrecision { get; set; }
    }
}