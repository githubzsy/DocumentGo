using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

            if(!Directory.Exists(OutPutPath))
            {
                Directory.CreateDirectory(OutPutPath);
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
                if (false == entity.Attributes.Any(e => e.IsPrimary == "true"))
                {
                    return;
                }

                var key = entity.Attributes.First(e => e.IsPrimary == "true");

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

        
    }

    public abstract class ExportBase
    {
        public Config Config { get; }

        public string Output { get; }

        public SchemaCollection SchemaCollection { get; }

        protected ExportBase(Config config, SchemaCollection schemaCollection)
        {
            Config = config;
            SchemaCollection = schemaCollection;

            Output = Path.Combine(Directory.GetCurrentDirectory() + "\\Output");

            if (!Directory.Exists(Output))
            {
                Directory.CreateDirectory(Output);
            }
        }

        public abstract void Export();
    }

    /// <summary>
    /// 导出Dot文件
    /// </summary>
    public class ExportDot : ExportBase
    {
        public ExportDot(Config config, SchemaCollection schemaCollection) : base(config, schemaCollection)
        {
        }

        public override void Export()
        {
            foreach (var module in Config.Modules)
            {
                ProcessModule(module);
            }
        }

        private void ProcessModule(Module module)
        {
            // 需要绘图的节点
            var children = module.Children.Where(m => m.DrawObjectEnum != DrawObjectEnum.Table).ToList();

            foreach (var child in children)
            {
                var entities = SchemaCollection.TableList.Where(m => child.Entities.Contains(m.Name)).ToList();

                var metadataRelationShipList = SchemaCollection.RelationShipList
                    .Where(m => entities.Any(e => e.Name == m.PrimaryTableName) && entities.Any(e => e.Name == m.RelatedTableName)).ToList();

                var content = "  digraph structs {" + Environment.NewLine
                                                       + "    graph [fontname=\"Microsoft YaHei\" rankdir = \"LR\"];" +
                                                       Environment.NewLine
                                                       + "    edge [fontname=\"Microsoft YaHei\"]; " + Environment.NewLine
                                                       + "    node [fontname=\"Microsoft YaHei\" fontsize = \"16\" shape = \"box\"];" +
                                                       Environment.NewLine;

                foreach (var entity in entities)
                {
                    content += "    " + entity.Name + " [label=<" + Environment.NewLine;
                    content += "		<TABLE BORDER=\"0\">" + Environment.NewLine;
                    content += "  			<TR><TD COLSPAN=\"2\" BORDER=\"1\" BGCOLOR=\"grey\"> " + entity.Name + " </TD></TR>" + Environment.NewLine;


                    foreach (var attr in entity.Columns.Where(m => m.DbType == "uniqueidentifier")
                        .OrderByDescending(m => m.IsPrimary))
                    {
                        if (attr.IsPrimary)
                        {
                            content += "  			<TR><TD BORDER=\"1\" PORT=\"" +
                                       attr.Name + "\">PK</TD><TD BORDER=\"1\" ALIGN=\"LEFT\" WIDTH=\"250\"> " + attr.Name + " </TD></TR>" + Environment.NewLine;
                        }
                        else if (metadataRelationShipList.Any(m =>
                            m.RelatedTableName == entity.Name && m.RelatedColumnName == attr.Name))
                        {
                            content += "  			<TR><TD BORDER=\"1\">FK</TD><TD BORDER=\"1\" PORT=\"" + attr.Name +
                                       "\" ALIGN=\"LEFT\" WIDTH=\"250\"> " + attr.Name + " </TD></TR>" + Environment.NewLine;
                        }
                        else
                        {
                            content += "  			<TR><TD BORDER=\"1\"></TD><TD BORDER=\"1\" ALIGN=\"LEFT\" WIDTH=\"250\"> " +
                                       attr.Name + " </TD></TR>" + Environment.NewLine;
                        }
                    }

                    content += "        </TABLE>" + Environment.NewLine;
                    content += "    >];" + Environment.NewLine + Environment.NewLine;
                }

                foreach (var ship in metadataRelationShipList)
                {
                    content += string.Format("    {0}:{1}->{2}:{3};" + Environment.NewLine, ship.RelatedTableName,
                        ship.RelatedColumnName, ship.PrimaryTableName, ship.PrimaryColumnName);
                }

                content += "}";

                var fileName = Path.Combine(this.Output, module.Name + "_" + child.Name) + ".dot";

                File.WriteAllText(fileName, content, Encoding.UTF8);
            }
        }
    }

    public static class DotUtil
    {
        public static void Exec(string dotExe,string dotFolder)
        {
            var dotFiles = Directory.EnumerateFiles(dotFolder, "*.dot").ToList();

            foreach (var dot in dotFiles)
            {
                ConvertDot2Png(dotExe, dot);
            }
        }

        private static void ConvertDot2Png(string dotExe, string dotFile)
        {
            var cmdStr = string.Format(@"""{1}"" ""{0}.dot"" -T png -o ""{0}.png""", dotFile.Remove(dotFile.Length - 4, 4), dotExe);

            try
            {
                using (var myPro = new Process())
                {
                    myPro.StartInfo.FileName = "cmd.exe";
                    myPro.StartInfo.UseShellExecute = false;
                    myPro.StartInfo.RedirectStandardInput = true;
                    myPro.StartInfo.RedirectStandardOutput = true;
                    myPro.StartInfo.RedirectStandardError = true;
                    myPro.StartInfo.CreateNoWindow = true;
                    myPro.Start();
                    //如果调用程序路径中有空格时，cmd命令执行失败，可以用双引号括起来 ，在这里两个引号表示一个引号（转义）
                    var str = $"{cmdStr} {"&exit"}";

                    myPro.StandardInput.WriteLine(str);
                    myPro.StandardInput.AutoFlush = true;
                    myPro.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}