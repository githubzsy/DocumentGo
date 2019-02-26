using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DocumentGo.Models;

namespace DocumentGo
{
    public sealed class MetadataAnalysis
    {

        private readonly string _outPutPath = "\\OutPut";

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

        public Config Config { get; }

        public string OutPutPath { get; }
        public List<MetadataEntity> MetadataEntityList { get; } = new List<MetadataEntity>();
        public List<MetadataRelationShip> MetadataRelationShipList { get; } = new List<MetadataRelationShip>();

        public MetadataAnalysis(Config config)
        {
            OutPutPath =Path.Combine(Directory.GetCurrentDirectory()+_outPutPath);

            Config = config;

            if(!Directory.Exists(_outPutPath))
            {
                Directory.CreateDirectory(_outPutPath);
            }

            Load();
        }

        private void Load()
        {
            LoadEntity(Path.Combine(Config.WebRoot, @"_metadata\Entity\"));
            
            // 根据字段名称构建外键关系
            LoadRelationShip();
        }

        private void LoadRelationShip()
        {
            MetadataEntityList.ForEach((entity) =>
            {
                if (false == entity.Attributes.Any(e => e.IsPrimaryAttribute == "true"))
                {
                    return;
                }

                var key = entity.Attributes.First(e => e.IsPrimaryAttribute == "true");

                var rEntities = MetadataEntityList
                    .Where(m => m.Attributes.Any(a => a.Name.Equals(key.Name, StringComparison.OrdinalIgnoreCase))&&m.EntityId!=entity.EntityId).ToList();


                foreach (var r in rEntities)
                {
                    var id = Guid.NewGuid().ToString();

                    var rAttr = r.Attributes.First(m => m.Name.Equals(key.Name, StringComparison.OrdinalIgnoreCase));

                    MetadataRelationShipList.Add(new MetadataRelationShip
                    {
                        Index = -1,
                        RelationShipId = id,
                        Name = id,
                        Type = "OneToOne",
                        PrimaryEntityId = entity.EntityId,
                        PrimaryAttributeId = key.AttributeId,
                        RelatedEntityId = r.EntityId,
                        RelatedAttributeId = rAttr.AttributeId,

                        PrimaryEntityName = entity.Name,
                        PrimaryEntityDisplayName = entity.DisplayName,
                        PrimaryAttributeName = key.Name,
                        PrimaryAttributeDisplayName = key.DisplayName,
                        RelatedEntityName = r.Name,
                        RelatedAttributeName = rAttr.Name,
                        RelatedAttributeDisplayName = rAttr.DisplayName,

                    });
                }

            });
        }

        private void LoadEntity(string entityPath)
        {
            var files = Directory.EnumerateFiles(entityPath, "*.Metadata.config").ToList();
            for (var i = 1; i <= files.Count(); i++)
            {
                var entity = ReadMetadataEntityFile(files[i - 1], i);
                if (entity != null)
                {
                    MetadataEntityList.Add(entity);
                }
            }
        }

        private MetadataEntity ReadMetadataEntityFile(string file, int index)
        {
            var doc = XDocument.Load(file);
            var rootEle = doc.Root;
            if (rootEle == null)
            {
                return null;
            }

            if (rootEle.Attribute("Application").Value != "0201")
            {
                return null;
            }

            var entity = new MetadataEntity
            {
                Index = index,
                EntityId = rootEle.Attribute("EntityId").Value,
                Name = rootEle.Attribute("Name").Value,
                DisplayName = rootEle.Attribute("DisplayName").Value,
                Application = rootEle.Attribute("Application").Value,
                Attributes = new List<MetadataAttribute>()
            };

            var attrs = new List<MetadataAttribute>();

            foreach (var metadataAttribute in rootEle.Element("Attributes").Elements())
            {
                var name = metadataAttribute.Element("Name").Value;

                if (_excludeFields.Contains(name))
                {
                    continue;
                }

                var dbType = metadataAttribute.Element("DbType").Value;
                var length = metadataAttribute.Element("Length").Value;
                var decimalPrecision = metadataAttribute.Element("DecimalPrecision").Value;

                var colType = string.Empty;
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

                var attr = new MetadataAttribute
                {
                    AttributeId = metadataAttribute.Element("AttributeId").Value,
                    Name = metadataAttribute.Element("Name").Value,
                    DisplayName = metadataAttribute.Element("DisplayName").Value,
                    AttributeType = metadataAttribute.Element("AttributeType").Value,
                    DbType = colType,
                    IsPrimaryAttribute = metadataAttribute.Element("IsPrimaryAttribute").Value,
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

            entity.Attributes = attrs.OrderByDescending(m => m.IsPrimaryAttribute == "true").ThenBy(m => m.Name)
                .ToList();

            return entity;
        }

        
    }
}