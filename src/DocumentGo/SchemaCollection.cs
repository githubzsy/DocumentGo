using System;
using System.Collections.Generic;
using DocumentGo.Models;

namespace DocumentGo
{
    [Serializable]
    public class SchemaCollection
    {
        /// <summary>
        /// 元数据实体集合
        /// </summary>
        public List<Table> TableList { get; set; } = new List<Table>();
        
        /// <summary>
        /// 元数据关系集合
        /// </summary>
        public List<RelationShip> RelationShipList { get; set; } = new List<RelationShip>();
    }
}