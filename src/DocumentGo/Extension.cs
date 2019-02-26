using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DocumentGo.Models;
using ExcelReport;

namespace DocumentGo
{
    public static class Extension
    {
        public static void Export(this MetadataAnalysis analysis, DocType docType)
        {
            switch (docType)
            {
                case DocType.Xls:
                    ExportXls(analysis, false);
                    break;

                case DocType.SimpleXls:
                    ExportXls(analysis, true);
                    break;

                case DocType.Rtf:
                    ExportRtf(analysis);
                    break;

                case DocType.Dot:
                    ExportDot(analysis);
                    break;

                case DocType.Png:
                    ExportPng(analysis);
                    break;
            }
        }

        private static void ExportRtf(MetadataAnalysis analysis)
        {
            ExportPng(analysis);
            new TextExporter(analysis).Export();
        }

        private static void ExportPng(MetadataAnalysis analysis)
        {
            foreach (var module in analysis.Config.Modules)
            {
                ProcessModule(module, analysis, true);
            }

            //Parallel.ForEach(analysis.Modules, (module) =>
            //{
            //    ProcessModule(module, analysis.Config, true);
            //});
        }

        private static void ExportXls(MetadataAnalysis analysis, bool isFilterAttribute)
        {
            var fileName = isFilterAttribute ? Path.Combine(analysis.OutPutPath,"数据结构-Simple.xls") : Path.Combine(analysis.OutPutPath, "数据结构.xls");
            var entities = analysis.MetadataEntityList;
            var relationShips = analysis.MetadataRelationShipList;
            if (isFilterAttribute)
            {
                XlsExport(fileName, entities, m => m.DbType == "uniqueidentifier", relationShips);
            }
            else
            {
                XlsExport(fileName, entities, m=>true, relationShips);
            }
        }

        private static void XlsExport(string fileName,List<MetadataEntity> entities, Func<MetadataAttribute,bool> attributeFilter,List<MetadataRelationShip> relationShips)
        {
            var workbookParameterContainer = new WorkbookParameterContainer();
            workbookParameterContainer.Load(@"Template\Template.xml");
            var sheetParameterContainer1 = workbookParameterContainer["数据库表格"];
            var sheetParameterContainer2 = workbookParameterContainer["主外键关系"];

            ExportHelper.ExportToLocal(@"Template\Template.xls",
                fileName,
                new SheetFormatter("数据库表格",
                    new RepeaterFormatter<MetadataEntity>(sheetParameterContainer1["rptTable_Start"],
                        sheetParameterContainer1["rptTable_End"], entities,
                        new CellFormatter<MetadataEntity>(sheetParameterContainer1["Index"], t => t.Index.ToString()),
                        new CellFormatter<MetadataEntity>(sheetParameterContainer1["Name"], t => t.Name),
                        new CellFormatter<MetadataEntity>(sheetParameterContainer1["DisplayName"], t => t.DisplayName),
                        new CellFormatter<MetadataEntity>(sheetParameterContainer1["EntityId"], t => t.EntityId),
                        new CellFormatter<MetadataEntity>(sheetParameterContainer1["Application"], t => t.Application),
                        new RepeaterFormatter<MetadataAttribute, MetadataEntity>(sheetParameterContainer1["rptColumn_Start"],
                            sheetParameterContainer1["rptColumn_End"],
                            t => t.Attributes.Where(attributeFilter).ToList(),
                            new CellFormatter<MetadataAttribute>(sheetParameterContainer1["ColumnName"], r => r.Name),
                            new CellFormatter<MetadataAttribute>(sheetParameterContainer1["ColumnDisplayName"],
                                r => r.DisplayName),
                            new CellFormatter<MetadataAttribute>(sheetParameterContainer1["ColumnAttributeType"],
                                r => r.AttributeType),
                            new CellFormatter<MetadataAttribute>(sheetParameterContainer1["ColumnDbType"], r => r.DbType),
                            new CellFormatter<MetadataAttribute>(sheetParameterContainer1["IsPrimaryAttribute"],
                                r => r.IsPrimaryAttribute == "true" ? "是" : ""),
                            new CellFormatter<MetadataAttribute>(sheetParameterContainer1["Remark"], r => r.Remark),
                            new CellFormatter<MetadataAttribute>(sheetParameterContainer1["IsNullable"],
                                r => r.IsNullable == "true" ? "是" : "")
                        )
                    )
                ),
                new SheetFormatter("主外键关系",
                    new RepeaterFormatter<MetadataRelationShip>(sheetParameterContainer2["rptRow_Start"],
                        sheetParameterContainer2["rptRow_End"], relationShips,
                        new CellFormatter<MetadataRelationShip>(sheetParameterContainer2["Index"], t => t.Index),
                        new CellFormatter<MetadataRelationShip>(sheetParameterContainer2["PrimaryEntityName"],
                            t => t.PrimaryEntityName),
                        new CellFormatter<MetadataRelationShip>(sheetParameterContainer2["PrimaryEntityDisplayName"],
                            t => t.PrimaryEntityDisplayName),
                        new CellFormatter<MetadataRelationShip>(sheetParameterContainer2["PrimaryAttributeName"],
                            t => t.PrimaryAttributeName),
                        new CellFormatter<MetadataRelationShip>(sheetParameterContainer2["PrimaryAttributeDisplayName"],
                            t => t.PrimaryAttributeDisplayName),
                        new CellFormatter<MetadataRelationShip>(sheetParameterContainer2["RelatedEntityName"],
                            t => t.RelatedEntityName),
                        new CellFormatter<MetadataRelationShip>(sheetParameterContainer2["RelatedEntityDisplayName"],
                            t => t.RelatedEntityDisplayName),
                        new CellFormatter<MetadataRelationShip>(sheetParameterContainer2["RelatedAttributeName"],
                            t => t.RelatedAttributeName),
                        new CellFormatter<MetadataRelationShip>(sheetParameterContainer2["RelatedAttributeDisplayName"],
                            t => t.RelatedAttributeDisplayName),
                        new CellFormatter<MetadataRelationShip>(sheetParameterContainer2["Type"], t => t.Type)
                    )
                )
            );
        }

        private static void ExportDot(MetadataAnalysis analysis )
        {

            foreach (var module in analysis.Config.Modules)
            {
                ProcessModule(module, analysis, false);
            }
        }

        private static void ProcessModule(Module module, MetadataAnalysis analysis, bool isGenPng)
        {
            

            // 需要绘图的节点
            var children = module.Children.Where(m => m.DrawObjectEnum != DrawObjectEnum.Table).ToList();

            List<MetadataEntity> entities;
            List<MetadataRelationShip> metadataRelationShipList;

            foreach (var child in children)
            {
                entities =
                    analysis.MetadataEntityList.Where(m => child.Entities.Contains(m.Name)).ToList();

                metadataRelationShipList = analysis.MetadataRelationShipList
                    .Where(m => entities.Any(e => e.Name == m.PrimaryEntityName) && entities.Any(e => e.Name == m.RelatedEntityName)).ToList();

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

                    
                    foreach (var attr in entity.Attributes.Where(m => m.DbType == "uniqueidentifier")
                        .OrderByDescending(m => m.IsPrimaryAttribute == "true"))
                    {
                        if (attr.IsPrimaryAttribute == "true")
                        {
                            content += "  			<TR><TD BORDER=\"1\" PORT=\"" +
                                       attr.Name + "\">PK</TD><TD BORDER=\"1\" ALIGN=\"LEFT\" WIDTH=\"250\"> " + attr.Name + " </TD></TR>" + Environment.NewLine;
                        }
                        else if (metadataRelationShipList.Any(m =>
                            m.RelatedEntityName == entity.Name && m.RelatedAttributeName == attr.Name))
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
                    content += string.Format("    {0}:{1}->{2}:{3};" + Environment.NewLine, ship.RelatedEntityName,
                        ship.RelatedAttributeName, ship.PrimaryEntityName, ship.PrimaryAttributeName);
                }

                content += "}";

                var fileName = Path.Combine(analysis.OutPutPath, module.Name+"_"+child.Name)+ ".dot";

                File.WriteAllText(fileName, content, Encoding.UTF8);

                if (isGenPng)
                {
                    ConvertDot2Png(analysis.Config.DotExe, fileName);
                }
            }
        }



        public static void Dot2Png(string dotExe,string folder)
        {
            var dotFiles=Directory.EnumerateFiles(folder, "*.dot").ToList();

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
