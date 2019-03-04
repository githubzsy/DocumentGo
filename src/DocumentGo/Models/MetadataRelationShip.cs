namespace DocumentGo.Models
{
    public class MetadataRelationShip
    {
        public string Type { get; set; }

        /// <summary>
        /// 主表实体
        /// </summary>
        public string PrimaryEntityId { get; set; }
        /// <summary>
        /// 主表属性
        /// </summary>
        public string PrimaryAttributeId { get; set; }
        /// <summary>
        /// 关联表实体
        /// </summary>
        public string RelatedEntityId { get; set; }
        /// <summary>
        /// 关联表属性
        /// </summary>
        public string RelatedAttributeId { get; set; }
    }
}