using System;
using DocumentGo.Models;

namespace DocumentGo
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            // 读取配置文件
            Config config = Config.FromConfigFile();

            // 读取元数据文件
            MetadataReader reader = new MetadataReader(config.WebRoot,config.ApplicationCode);

            //// 导出Dot文件
            //ExportBase export = new ExportDot(config, reader.SchemaCollection);
            //export.Export();

            //// Dot->Png
            //DotUtil.Exec(config.DotExe, export.Output);

            // 导出Xls
            ExportBase export = new ExportXls(config, reader.SchemaCollection);
            export.Export();

            //// 导出Rtf
            //export = new ExportRtf(config, reader.SchemaCollection);
            //export.Export();
        }
    }
}
