using DocumentGo.Import;
using DocumentGo.Models;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Collections.Generic;
using System.IO;

namespace DocumentGo
{
    public class ExcelImport : BaseImport
    {
        private IWorkbook Workbook;

        public ExcelImport(Config config) : base(config)
        {
        }

        public override object Import()
        {
            SchemaCollection schemaCollection = new SchemaCollection();

            var fileName = Path.Combine(Config.Output, "Report.xls");

            Workbook = new HSSFWorkbook(new FileStream(fileName, FileMode.Open, FileAccess.Read));

            schemaCollection.TableList = ReadTableSheet("数据库表格");

            schemaCollection.RelationShipList = ReadRelationShipSheet("主外键关系");

            return schemaCollection;
        }

        private List<RelationShip> ReadRelationShipSheet(string sheetName)
        {
            List<RelationShip> result = new List<RelationShip>();

            ISheet sheet = Workbook.GetSheet(sheetName);

            for (int i = sheet.FirstRowNum + 1; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue; //没有数据的行默认是null　　　　　　　
                ICell cell = row.GetCell(row.FirstCellNum);
                if (cell == null) continue;
                if (string.IsNullOrEmpty(cell.ToString()) || string.IsNullOrWhiteSpace(cell.ToString())) continue;
                RelationShip ship = new RelationShip
                {
                    PrimaryTableName = cell.ToString(),
                    PrimaryColumnName = row.GetCell(row.FirstCellNum + 1).ToString(),
                    RelatedTableName = row.GetCell(row.FirstCellNum + 2).ToString(),
                    RelatedColumnName = row.GetCell(row.FirstCellNum + 3).ToString()
                };

                result.Add(ship);
            }

            return result;
        }

        private List<Table> ReadTableSheet(string sheetName)
        {
            List<Table> result = new List<Table>();

            ISheet sheet = Workbook.GetSheet(sheetName);
            Table table=null;
            for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null)
                {
                    continue;
                }

                ICell cell = row.GetCell(row.FirstCellNum);
                if (cell == null)
                {
                    continue;
                }

                var firstCellVal = cell.ToString();
                if (firstCellVal == "表名")
                {
                    table = new Table
                    {
                        Name = row.GetCell(row.FirstCellNum + 1).ToString(),
                        DisplayName = row.GetCell(row.FirstCellNum + 4).ToString()
                    };

                    result.Add(table);

                    continue;
                }
                else if (firstCellVal == "字段")
                {
                    continue;
                }
                else if (false==string.IsNullOrEmpty(firstCellVal) && false==string.IsNullOrWhiteSpace(firstCellVal))
                {
                    Column column = new Column
                    {
                        Name = firstCellVal,
                        DisplayName = row.GetCell(row.FirstCellNum + 1).ToString(),
                        AttributeType = row.GetCell(row.FirstCellNum + 2).ToString(),
                        DbType = row.GetCell(row.FirstCellNum + 3).ToString(),
                        IsPrimary = row.GetCell(row.FirstCellNum + 4).ToString() == "是",
                        IsNullable = row.GetCell(row.FirstCellNum + 5).ToString() == "是",
                        Remark = row.GetCell(row.FirstCellNum + 6).ToString()
                    };

                    table.Columns.Add(column);
                }
            }

            return result;
        }
    }
}