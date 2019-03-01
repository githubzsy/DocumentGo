using McMaster.Extensions.CommandLineUtils;
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

            ExportBase export = new ExportDot(config, reader.SchemaCollection);
            export.Export();

            DotUtil.Exec(config.DotExe, export.Output);


            //MetadataAnalysis metadataAnalysis = new MetadataAnalysis(config);

            //if (args.Length > 0&& args[0]=="Report")
            //{
            //    Console.WriteLine("1=>Dot-Png");
            //    Extension.Dot2Png(config.DotExe, metadataAnalysis.OutPutPath);
            //    Console.WriteLine("2=>Report");
            //    metadataAnalysis.Export(DocType.Rtf);
            //    Console.WriteLine("已重新生成Report.rtf");
            //}
            //else
            //{
            //    Console.WriteLine("1=>SimpleXls");
            //    metadataAnalysis.Export(DocType.SimpleXls);
            //    Console.WriteLine("2=>Xls");
            //    metadataAnalysis.Export(DocType.Xls);
            //    Console.WriteLine("3=>Dot");
            //    metadataAnalysis.Export(DocType.Dot);
            //    Console.WriteLine("4=>Png");
            //    metadataAnalysis.Export(DocType.Png);
            //    Console.WriteLine("5=>Rtf");
            //    metadataAnalysis.Export(DocType.Rtf);
            //    Console.WriteLine("所有文档已输出");
            //}
        }
    }
}
