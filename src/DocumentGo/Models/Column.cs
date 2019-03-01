namespace DocumentGo.Models
{
    public class Column
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string AttributeType { get; set; }
        public string DbType { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsNullable { get; set; }

        public string Length { get; set; }
        public string Remark { get; set; }
        public string DecimalPrecision { get; set; }
    }
}