using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentGo.Models;
using iTextSharp.text;
using iTextSharp.text.rtf;
using iTextSharp.text.rtf.style;

namespace DocumentGo
{
    /// <summary>
    /// 导出Rtf文档
    /// </summary>
    public class RtfExport : BaseExport
    {
        #region 字体样式

        private RtfParagraphStyle _heading1;

        private RtfParagraphStyle _heading2;

        private RtfParagraphStyle _heading3;

        private RtfParagraphStyle _tableHead;

        private RtfParagraphStyle _tableCell;

        #endregion

        public RtfExport(Config config, SchemaCollection schemaCollection) : base(config, schemaCollection)
        {
            InitStyle();
        }

        // 导出
        public override void Export()
        {
            // 创建文档
            Document doc = OpenDocument();

            Dictionary<string, Image> imageDict = DrawImageList();

            doc.NewPage();

            Section section;
            Section subSection;

            Chapter cpt = new Chapter(new Paragraph("概述") { Alignment = Element.ALIGN_LEFT, Font = _heading1 }, 1);
            doc.Add(cpt);
            //doc.Add(Chunk.NEWLINE);
            int i = 2;
            foreach (Module module in Config.Modules.OrderBy(m => m.Order).ToList())
            {
                // 一级标题
                cpt = new Chapter(new Paragraph(module.Name) { Alignment = Element.ALIGN_LEFT, Font = _heading1 }, i);
                //cpt.Add(Chunk.NEWLINE);
                foreach (Child child in module.Children.OrderBy(m => m.Order).ToList())
                {
                    // 二级标题
                    section = cpt.AddSection(new Paragraph(child.Name) { Alignment = Element.ALIGN_LEFT, Font = _heading2 }, 2);

                    // 是否需要绘制关系图
                    if (child.DrawObjectEnum == DrawObjectEnum.Image || child.DrawObjectEnum == DrawObjectEnum.All)
                    {
                        if (imageDict.ContainsKey(module.Name + "_" + child.Name))
                        {
                            section.Add(new Phrase(new Chunk(imageDict[module.Name + "_" + child.Name], 0, 0)));
                            section.Add(Chunk.NEWLINE);
                        }                        
                    }

                    // 是否需要绘制表格
                    if (child.DrawObjectEnum == DrawObjectEnum.Table || child.DrawObjectEnum == DrawObjectEnum.All)
                    {
                        List<Models.Table> tables =
                            SchemaCollection.TableList.Where(m => child.Entities.Contains(m.Name)).ToList();

                        foreach (Models.Table entity in tables)
                        {
                            // 三级标题
                            subSection = section.AddSection(new Paragraph($"{entity.DisplayName}[{entity.Name}]", _heading3), 3);
                            subSection.Add(DrawTable(entity));
                            subSection.Add(Chunk.NEWLINE);
                        }
                    }
                }
                doc.Add(cpt);
                i++;
            }

            doc.Close();
        }

        #region Private

        private void InitStyle()
        {
            #region 标题一

            _heading1 = RtfParagraphStyle.STYLE_HEADING_1;
            _heading1.SetAlignment(Element.ALIGN_LEFT);
            _heading1.SetStyle(Font.BOLD);
            _heading1.Size = 18f;

            #endregion

            #region 标题二

            _heading2 = RtfParagraphStyle.STYLE_HEADING_2;
            _heading2.SetAlignment(Element.ALIGN_LEFT);
            _heading2.SetStyle(Font.BOLD);
            _heading2.Size = 16f;

            #endregion

            #region 标题三

            _heading3 = RtfParagraphStyle.STYLE_HEADING_3;
            _heading3.SetAlignment(Element.ALIGN_LEFT);
            _heading3.SetStyle(Font.BOLD);
            _heading3.Size = 14f;

            #endregion

            #region Table表头

            _tableHead = RtfParagraphStyle.STYLE_NORMAL;
            _tableHead.SetAlignment(Element.ALIGN_LEFT);
            _tableHead.SetStyle(Font.BOLD);
            _tableHead.Size = 12f;

            #endregion

            #region Table单元格

            _tableCell = RtfParagraphStyle.STYLE_NORMAL;
            _tableCell.SetAlignment(Element.ALIGN_LEFT);
            _tableCell.SetStyle(Font.NORMAL);
            _tableCell.Size = 10f;

            #endregion
        }

        private Document OpenDocument()
        {
            // 创建文档
            Document doc = new Document(PageSize.A4, 60, 60, 60, 60);
            // 写文档实例
            RtfWriter2.GetInstance(doc, new FileStream(Path.Combine(Config.Output, "Report.rtf"), FileMode.Create, FileAccess.Write)); ;
            // 打开文档
            doc.Open();

            return doc;
        }

        private Dictionary<string, Image> DrawImageList()
        {
            Dictionary<string, Image> dict = new Dictionary<string, Image>();

            List<string> names = new List<string>();

            foreach (Module module in Config.Modules)
            {
                names.AddRange(module.Children.Where(m => m.DrawObjectEnum != DrawObjectEnum.Table)
                    .Select(m => module.Name + "_" + m.Name).ToList());
            }

            foreach (string name in names)
            {
                string imgName = Path.Combine(Config.Output, name + ".png");

                if (File.Exists(imgName))
                {
                    Image image = Image.GetInstance(new Uri(imgName));
                    image.ScalePercent(20f);
                    image.Alignment = Image.TEXTWRAP | Element.ALIGN_CENTER;

                    dict.Add(name, image);
                }


            }

            return dict;
        }


        private iTextSharp.text.Table DrawTable(Models.Table entity)
        {
            iTextSharp.text.Table t = new iTextSharp.text.Table(5, entity.Columns.Count)
            {
                AutoFillEmptyCells = true,
                CellsFitPage = true,
                Cellpadding = 3,
                DefaultVerticalAlignment = Element.ALIGN_MIDDLE,
                BorderWidth = 1.0f,
                Width = 100f
            };

            t.SetWidths(new int[] { 1, 4, 3, 3, 4 });

            AddRow(t, "数据表中文名称", entity.DisplayName);
            AddRow(t, "数据表英文名称", entity.Name);
            AddRow(t, "功能简述", string.Empty);

            t.AddCell(BuildHeaderCell("序号"));
            t.AddCell(BuildHeaderCell("字段中文名"));
            t.AddCell(BuildHeaderCell("字段英文名"));
            t.AddCell(BuildHeaderCell("数据类型"));
            t.AddCell(BuildHeaderCell("枚举&说明"));

            for (int j = 0; j < entity.Columns.Count; j++)
            {
                Column attr = entity.Columns[j];

                t.AddCell(BuildCell((j + 1).ToString()));
                t.AddCell(BuildCell(attr.DisplayName));
                t.AddCell(BuildCell(attr.Name));
                t.AddCell(BuildCell(attr.DbType));
                t.AddCell(BuildCell(attr.Remark));
            }

            return t;
        }

        private void AddRow(iTextSharp.text.Table table, string title1, string title2)
        {
            Phrase phrase1 = new Phrase(title1, _tableCell);
            Phrase phrase2 = new Phrase(title2, _tableCell);

            Cell cell1 = new Cell(phrase1)
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT,
                Colspan = 2
            };

            table.AddCell(cell1);

            Cell cell2 = new Cell(phrase2)
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT,
                Colspan = 3
            };

            table.AddCell(cell2);
        }

        private Cell BuildHeaderCell(string title)
        {
            Phrase phrase = new Phrase(title, _tableHead);
            return new Cell(phrase)
            {
                Header = true,
                BackgroundColor = Color.LIGHT_GRAY,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
        }

        private Cell BuildCell(string text)
        {
            Phrase phrase = new Phrase(text, _tableCell);

            return new Cell(phrase)
            {

                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
        }

        #endregion
    }
}