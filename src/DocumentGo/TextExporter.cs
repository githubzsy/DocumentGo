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
    public class TextExporter
    {

        private readonly Config _config;

        private readonly MetadataAnalysis _analysis;

        private RtfParagraphStyle _heading1;

        private RtfParagraphStyle _heading2;

        private RtfParagraphStyle _heading3;

        private RtfParagraphStyle _tableHead;

        private RtfParagraphStyle _tableCell;

        List<MetadataEntity> _entities;

        public TextExporter(MetadataAnalysis analysis)
        {
            _config = analysis.Config;

            _analysis = analysis;

            InitStyle();
        }

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

        public void Export()
        {
            // 创建文档
            var doc = OpenDocument();

            var imageDict = DrawImageList();

            doc.NewPage();

            Section section;
            Section subSection;

            var cpt = new Chapter(new Paragraph("概述") { Alignment = Element.ALIGN_LEFT, Font = _heading1 }, 1);
            doc.Add(cpt);
            //doc.Add(Chunk.NEWLINE);
            var i = 2;
            foreach (var module in _config.Modules.OrderBy(m => m.Order).ToList())
            {
                // 一级标题
                cpt = new Chapter(new Paragraph(module.Name) { Alignment = Element.ALIGN_LEFT, Font = _heading1 }, i);
                //cpt.Add(Chunk.NEWLINE);
                foreach (var child in module.Children.OrderBy(m => m.Order).ToList())
                {
                    // 二级标题
                    section= cpt.AddSection(new Paragraph(child.Name) { Alignment = Element.ALIGN_LEFT, Font = _heading2 }, 2);

                    // 是否需要绘制关系图
                    if (child.DrawObjectEnum == DrawObjectEnum.Image|| child.DrawObjectEnum == DrawObjectEnum.All)
                    {
                        section.Add(new Phrase(new Chunk(imageDict[module.Name + "_" + child.Name],0,0)));
                        section.Add(Chunk.NEWLINE);
                    }

                    // 是否需要绘制表格
                    if (child.DrawObjectEnum == DrawObjectEnum.Table || child.DrawObjectEnum == DrawObjectEnum.All)
                    {
                        _entities =
                            _analysis.MetadataEntityList.Where(m => child.Entities.Contains(m.Name)).ToList();

                        foreach (var entity in _entities)
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


        private Document OpenDocument()
        {
            // 创建文档
            var doc = new Document(PageSize.A4, 60, 60, 60, 60);
            // 写文档实例
            RtfWriter2.GetInstance(doc, new FileStream(Path.Combine(_analysis.OutPutPath, "Report.rtf"), FileMode.Create, FileAccess.Write)); ;
            // 打开文档
            doc.Open();

            return doc;
        }

        private Dictionary<string,Image> DrawImageList()
        {
            var dict=new Dictionary<string, Image>();

            var names = new List<string>();

            foreach (var module in _config.Modules)
            {
                names.AddRange(module.Children.Where(m => m.DrawObjectEnum != DrawObjectEnum.Table)
                    .Select(m => module.Name+"_"+m.Name).ToList());
            }

            foreach (var name in names)
            {
                var imgName = Path.Combine(_analysis.OutPutPath, name + ".png");

                if (File.Exists(imgName))
                {
                    var image = Image.GetInstance(new Uri(imgName));
                    image.ScalePercent(20f);
                    image.Alignment = Image.TEXTWRAP | Element.ALIGN_CENTER;

                    dict.Add(name, image);
                }

                
            }

            return dict;
        }
        

        private iTextSharp.text.Table DrawTable(MetadataEntity entity)
        {
            var t = new iTextSharp.text.Table(5, entity.Attributes.Count)
            {
                AutoFillEmptyCells = true,
                CellsFitPage = true,
                Cellpadding = 3,
                DefaultVerticalAlignment = Element.ALIGN_MIDDLE,
                BorderWidth = 1.0f,
                Width = 100f
            };

            t.SetWidths(new int[] {1, 4, 3, 3, 4});

            AddRow(t, "数据表中文名称", entity.DisplayName);
            AddRow(t, "数据表英文名称", entity.Name);
            AddRow(t, "功能简述", string.Empty);

            t.AddCell(BuildHeaderCell("序号"));
            t.AddCell(BuildHeaderCell("字段中文名"));
            t.AddCell(BuildHeaderCell("字段英文名"));
            t.AddCell(BuildHeaderCell("数据类型"));
            t.AddCell(BuildHeaderCell("枚举&说明"));

            for (var j = 0; j < entity.Attributes.Count; j++)
            {
                var attr = entity.Attributes[j];

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
            var phrase1 = new Phrase(title1, _tableCell);
            var phrase2 = new Phrase(title2, _tableCell);

            var cell1 = new Cell(phrase1)
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT,
                Colspan = 2
            };

            table.AddCell(cell1);

            var cell2 = new Cell(phrase2)
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT,
                Colspan = 3
            };

            table.AddCell(cell2);
        }

        private Cell BuildHeaderCell(string title)
        {
            var phrase = new Phrase(title, _tableHead);
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
            var phrase = new Phrase(text, _tableCell);

            return new Cell(phrase)
            {
                
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
        }
    }
}