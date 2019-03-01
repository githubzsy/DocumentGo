using System;
using System.IO;
using System.Linq;
using DocumentGo.Models;
using ExcelReport;

namespace DocumentGo
{
    /// <summary>
    /// 导出XLS
    /// </summary>
    public class ExcelExport : BaseExport
    {
        public ExcelExport(Config config, SchemaCollection schemaCollection) : base(config, schemaCollection)
        {
        }

        public override void Export()
        {
            WorkbookParameterContainer workbookParameterContainer = new WorkbookParameterContainer();
            workbookParameterContainer.Load(@"Template\Template.xml");
            SheetParameterContainer sheetParameterContainer1 = workbookParameterContainer["数据库表格"];
            SheetParameterContainer sheetParameterContainer2 = workbookParameterContainer["主外键关系"];

            ExportHelper.ExportToLocal(@"Template\Template.xls",
                Path.Combine(Config.Output, "Report.xls"),
                new SheetFormatter("数据库表格",
                    new RepeaterFormatter<Models.Table>(sheetParameterContainer1["rptTable_Start"],sheetParameterContainer1["rptTable_End"], SchemaCollection.TableList,
                        new CellFormatter<Models.Table>(sheetParameterContainer1["Name"], t => t.Name),
                        new CellFormatter<Models.Table>(sheetParameterContainer1["DisplayName"], t => t.DisplayName),
                        new CellFormatter<Models.Table>(sheetParameterContainer1["EmptyLine"], t => string.Empty),

                        new RepeaterFormatter<Column, Models.Table>(sheetParameterContainer1["rptColumn_Start"], sheetParameterContainer1["rptColumn_End"], t => t.Columns.ToList(),
                            new CellFormatter<Column>(sheetParameterContainer1["ColumnName"], r => r.Name),
                            new CellFormatter<Column>(sheetParameterContainer1["ColumnDisplayName"],r => r.DisplayName),
                            new CellFormatter<Column>(sheetParameterContainer1["ColumnAttributeType"],r => r.AttributeType),
                            new CellFormatter<Column>(sheetParameterContainer1["ColumnDbType"], r => r.DbType),
                            new CellFormatter<Column>(sheetParameterContainer1["IsPrimary"], r => r.IsPrimary ? "是" : ""),
                            new CellFormatter<Column>(sheetParameterContainer1["Remark"], r => r.Remark),
                            new CellFormatter<Column>(sheetParameterContainer1["IsNullable"], r => r.IsNullable ? "是" : "")
                        )
                    )
                ),
                new SheetFormatter("主外键关系",
                    new RepeaterFormatter<RelationShip>(sheetParameterContainer2["rptRow_Start"],sheetParameterContainer2["rptRow_End"], SchemaCollection.RelationShipList,
                        new CellFormatter<RelationShip>(sheetParameterContainer2["PrimaryTableName"],t => t.PrimaryTableName),
                                       new CellFormatter<RelationShip>(sheetParameterContainer2["PrimaryColumnName"],t => t.PrimaryColumnName),
                                       new CellFormatter<RelationShip>(sheetParameterContainer2["RelatedTableName"],t => t.RelatedTableName),
                                       new CellFormatter<RelationShip>(sheetParameterContainer2["RelatedColumnName"],t => t.RelatedColumnName),
                                       new CellFormatter<RelationShip>(sheetParameterContainer2["IsMetadata"], t => t.IsMetadata ? "建模配置" : "手动配置")
                    )
                )
            );
        }
    }
}