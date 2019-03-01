using DocumentGo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DocumentGo
{
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
            foreach (Module module in Config.Modules)
            {
                ProcessModule(module);
            }
            Console.WriteLine("Dot已生成");
        }

        private void ProcessModule(Module module)
        {
            // 需要绘图的节点
            List<Child> children = module.Children.Where(m => m.DrawObjectEnum != DrawObjectEnum.Table).ToList();

            foreach (Child child in children)
            {
                List<Models.Table> entities = SchemaCollection.TableList.Where(m => child.Entities.Contains(m.Name)).ToList();

                List<RelationShip> metadataRelationShipList = SchemaCollection.RelationShipList
                    .Where(m => entities.Any(e => e.Name == m.PrimaryTableName) && entities.Any(e => e.Name == m.RelatedTableName)).ToList();

                string content = "  digraph structs {" + Environment.NewLine
                                                    + "    graph [fontname=\"Microsoft YaHei\" rankdir = \"LR\"];" +
                                                    Environment.NewLine
                                                    + "    edge [fontname=\"Microsoft YaHei\"]; " + Environment.NewLine
                                                    + "    node [fontname=\"Microsoft YaHei\" fontsize = \"16\" shape = \"box\"];" +
                                                    Environment.NewLine;

                foreach (Models.Table entity in entities)
                {
                    content += "    " + entity.Name + " [label=<" + Environment.NewLine;
                    content += "		<TABLE BORDER=\"0\">" + Environment.NewLine;
                    content += "  			<TR><TD COLSPAN=\"2\" BORDER=\"1\" BGCOLOR=\"grey\"> " + entity.Name + " </TD></TR>" + Environment.NewLine;


                    foreach (Column attr in entity.Columns.Where(m => m.DbType == "uniqueidentifier")
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

                foreach (RelationShip ship in metadataRelationShipList)
                {
                    content += string.Format("    {0}:{1}->{2}:{3};" + Environment.NewLine, ship.RelatedTableName,
                        ship.RelatedColumnName, ship.PrimaryTableName, ship.PrimaryColumnName);
                }

                content += "}";

                string fileName = Path.Combine(Output, module.Name + "_" + child.Name) + ".dot";

                File.WriteAllText(fileName, content, Encoding.UTF8);
            }
        }
    }
}