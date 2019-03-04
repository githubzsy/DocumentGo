using DocumentGo.Import;
using DocumentGo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DocumentGo
{
    public class MetadataImport: BaseImport
    {
        /// <summary>
        /// 查找文件
        /// </summary>
        private const string SEARCH_PATTERN = "*.Metadata.config";

        /// <summary>
        /// 实体元数据目录
        /// </summary>
        private const string ENTITY_FOLDER = @"_metadata\Entity\";

        /// <summary>
        /// 实体关系元数据目录
        /// </summary>
        private const string RELATIONSHIP_FOLDER = @"_metadata\MetadataRelationship\";

        /// <summary>
        /// 需要排除的字段
        /// </summary>
        private readonly string[] _excludeFields = new[]
        {
            "CreatedTime",
            "CreatedGUID",
            "CreatedName",
            "ModifiedTime",
            "ModifiedGUID",
            "ModifiedName",
            "VersionNumber"
        };

        public MetadataImport(Config config):base (config)
        {            
        }

        /// <summary>
        /// 加载元数据
        /// </summary>
        /// <returns></returns>
        private SchemaCollection GetMetadataCollection()
        {
            SchemaCollection metadataCollection = new SchemaCollection();

            List<MetadataEntity> metadataEntityList = LoadEntities(Path.Combine(Config.WebRoot, ENTITY_FOLDER));

            List<MetadataRelationShip> metadataRelationShipList = LoadRelationShips(Path.Combine(Config.WebRoot, RELATIONSHIP_FOLDER));

            foreach (var entity in metadataEntityList)
            {
                metadataCollection.TableList.Add(Convert(entity));
            }

            foreach (var metadataRelationShip in metadataRelationShipList)
            {
                var relationShip = Convert(metadataRelationShip, metadataEntityList);

                if (relationShip != null)
                {
                    metadataCollection.RelationShipList.Add(relationShip);
                }
            }

            metadataEntityList.ForEach((entity) =>
            {
                if (false == entity.Attributes.Any(e => e.IsPrimary == "true"))
                {
                    return;
                }

                var key = entity.Attributes.First(e => e.IsPrimary == "true");

                // 根据主键名称寻找对应关系
                var rEntities = metadataEntityList
                    .Where(m => m.Attributes.Any(a => a.Name.Equals(key.Name, StringComparison.OrdinalIgnoreCase)) && m.EntityId != entity.EntityId).ToList();

                foreach (var r in rEntities)
                {
                    var rAttr = r.Attributes.First(m => m.Name.Equals(key.Name, StringComparison.OrdinalIgnoreCase));

                    metadataCollection.RelationShipList.Add(new RelationShip
                    {
                        IsMetadata = false,
                        PrimaryTableName = entity.Name,
                        PrimaryColumnName = key.Name,
                        RelatedTableName = r.Name,
                        RelatedColumnName = rAttr.Name
                    });
                }

            });

            return metadataCollection;
        }

        /// <summary>
        /// 加载元数据关系
        /// </summary>
        /// <param name="relationShipPath"></param>
        /// <returns></returns>
        private List<MetadataRelationShip> LoadRelationShips(string relationShipPath)
        {
            List<MetadataRelationShip> result = new List<MetadataRelationShip>();

            List<string> files = Directory.EnumerateFiles(relationShipPath, SEARCH_PATTERN).ToList();

            foreach (var file in files)
            {
                MetadataRelationShip ship = ReadMetadataRelationShipFile(file);
                if (ship != null)
                {
                    result.Add(ship);
                }
            }

            return result;
        }

        /// <summary>
        /// 读取实体关系
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private MetadataRelationShip ReadMetadataRelationShipFile(string file)
        {
            XDocument doc = XDocument.Load(file);
            XElement rootEle = doc.Root;
            if (rootEle == null)
            {
                return null;
            }

            if (rootEle.Attribute("application").Value != Config.ApplicationCode)
            {
                return null;
            }

            MetadataRelationShip ship = new MetadataRelationShip()
            {
                Type = rootEle.Element("Type").Value,
                PrimaryEntityId = rootEle.Element("PrimaryEntityId").Value,
                PrimaryAttributeId = rootEle.Element("PrimaryAttributeId").Value,
                RelatedEntityId = rootEle.Element("RelatedEntityId").Value,
                RelatedAttributeId = rootEle.Element("RelatedAttributeId").Value
            };

            return ship;
        }

        private List<MetadataEntity> LoadEntities(string entityPath)
        {
            List<MetadataEntity> result = new List<MetadataEntity>();

            List<string> files = Directory.EnumerateFiles(entityPath, SEARCH_PATTERN).ToList();

            foreach (var file in files)
            {
                MetadataEntity entity = ReadMetadataEntityFile(file);
                if (entity != null)
                {
                    result.Add(entity);
                }
            }

            return result;
        }

        private MetadataEntity ReadMetadataEntityFile(string file)
        {
            XDocument doc = XDocument.Load(file);
            XElement rootEle = doc.Root;
            if (rootEle == null)
            {
                return null;
            }

            if (rootEle.Attribute("Application").Value != Config.ApplicationCode)
            {
                return null;
            }

            MetadataEntity entity = new MetadataEntity
            {
                EntityId = rootEle.Attribute("EntityId").Value,
                Name = rootEle.Attribute("Name").Value,
                DisplayName = rootEle.Attribute("DisplayName").Value,
                Application = rootEle.Attribute("Application").Value,
                Attributes = new List<MetadataAttribute>()
            };

            List<MetadataAttribute> attrs = new List<MetadataAttribute>();

            foreach (XElement metadataAttribute in rootEle.Element("Attributes").Elements())
            {
                string name = metadataAttribute.Element("Name").Value;

                if (_excludeFields.Contains(name))
                {
                    continue;
                }

                string dbType = metadataAttribute.Element("DbType").Value;
                string length = metadataAttribute.Element("Length").Value;
                string decimalPrecision = metadataAttribute.Element("DecimalPrecision").Value;

                string colType = string.Empty;
                switch (dbType)
                {
                    case "nvarchar":

                        if (length == "-1")
                        {
                            length = "Max";
                        }
                        colType = $"{dbType}({length})";
                        break;
                    case "decimal":
                        colType = $"{dbType}({length},{decimalPrecision})";
                        break;
                    default:
                        colType = dbType;
                        break;
                }

                MetadataAttribute attr = new MetadataAttribute
                {
                    AttributeId = metadataAttribute.Element("AttributeId").Value,
                    Name = metadataAttribute.Element("Name").Value,
                    DisplayName = metadataAttribute.Element("DisplayName").Value,
                    AttributeType = metadataAttribute.Element("AttributeType").Value,
                    DbType = colType,
                    IsPrimary = metadataAttribute.Element("IsPrimaryAttribute").Value,
                    IsNullable = metadataAttribute.Element("IsNullable").Value,
                    Remark = metadataAttribute.Element("Remark") == null ? "" : metadataAttribute.Element("Remark").Value,
                    Length = metadataAttribute.Element("Length").Value,
                    DecimalPrecision = metadataAttribute.Element("DecimalPrecision").Value
                };

                if (attrs.Any(m => m.DisplayName == attr.DisplayName))
                {
                    attr.DisplayName += "_" + DateTime.Now.Ticks;
                }

                attrs.Add(attr);
            }

            entity.Attributes = attrs.OrderByDescending(m => m.IsPrimary == "true").ThenBy(m => m.Name)
                .ToList();

            return entity;
        }

        private Table Convert(MetadataEntity entity)
        {
            Table table = new Table { Name = entity.Name, DisplayName = entity.DisplayName };

            foreach (MetadataAttribute attr in entity.Attributes)
            {
                table.Columns.Add(Convert(attr));
            }

            return table;
        }

        private Column Convert(MetadataAttribute attr)
        {
            Column column = new Column
            {
                AttributeType = attr.AttributeType,
                DbType = attr.DbType,
                //DecimalPrecision = attr.DecimalPrecision,
                DisplayName = attr.DisplayName,
                IsNullable = attr.IsNullable == "true",
                IsPrimary = attr.IsPrimary == "true",
                //Length = attr.Length,
                Name = attr.Name,
                Remark = attr.Remark
            };

            return column;
        }

        private RelationShip Convert(MetadataRelationShip metadataRelationShip, List<MetadataEntity> metadataEntities)
        {
            RelationShip relationShip = null;

            if (metadataEntities.All(m => m.EntityId != metadataRelationShip.PrimaryEntityId) ||
                metadataEntities.All(m => m.EntityId != metadataRelationShip.RelatedEntityId))
            {
                return null;
            }

            MetadataEntity primaryEntity = metadataEntities.Find(m => m.EntityId == metadataRelationShip.PrimaryEntityId);

            MetadataEntity relatedEntity = metadataEntities.Find(m => m.EntityId == metadataRelationShip.RelatedEntityId);

            if (primaryEntity.Attributes.All(m => m.AttributeId != metadataRelationShip.PrimaryAttributeId) ||
                relatedEntity.Attributes.All(m => m.AttributeId != metadataRelationShip.RelatedAttributeId))
            {
                return null;
            }

            MetadataAttribute primaryAttribute =
                    primaryEntity.Attributes.Find(m => m.AttributeId == metadataRelationShip.PrimaryAttributeId);
            MetadataAttribute relatedAttribute =
                    relatedEntity.Attributes.Find(m => m.AttributeId == metadataRelationShip.RelatedAttributeId);

            relationShip = new RelationShip
            {
                IsMetadata = true,
                PrimaryTableName = primaryEntity.Name,
                PrimaryColumnName = primaryAttribute.Name,
                RelatedTableName = relatedEntity.Name,
                RelatedColumnName = relatedAttribute.Name
            };

            return relationShip;
        }

        public override object Import()
        {
            var result= GetMetadataCollection();

            return result;
        }
    }
}