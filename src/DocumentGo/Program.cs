using McMaster.Extensions.CommandLineUtils;
using System;
using DocumentGo.Models;

namespace DocumentGo
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Config config = Config.FromConfigFile();
            MetadataAnalysis metadataAnalysis = new MetadataAnalysis(config);

            if (args.Length > 0&& args[0]=="Report")
            {
                Console.WriteLine("1=>Dot-Png");
                Extension.Dot2Png(config.DotExe, metadataAnalysis.OutPutPath);
                Console.WriteLine("2=>Report");
                metadataAnalysis.Export(DocType.Rtf);
                Console.WriteLine("已重新生成Report.rtf");
            }
            else
            {
                Console.WriteLine("1=>SimpleXls");
                metadataAnalysis.Export(DocType.SimpleXls);
                Console.WriteLine("2=>Xls");
                metadataAnalysis.Export(DocType.Xls);
                Console.WriteLine("3=>Dot");
                metadataAnalysis.Export(DocType.Dot);
                Console.WriteLine("4=>Png");
                metadataAnalysis.Export(DocType.Png);
                Console.WriteLine("5=>Rtf");
                metadataAnalysis.Export(DocType.Rtf);
                Console.WriteLine("所有文档已输出");
            }

            //CommandLineApplication app = new CommandLineApplication();

            //app.HelpOption("-h|--help");
            //CommandOption optionAll = app.Option("-a|--All", "输出全部", CommandOptionType.NoValue);
            //CommandOption optionDocument = app.Option("-doc|--Documnet", "输出Report.rtf", CommandOptionType.NoValue);

            //app.OnExecute(() =>
            //{
            //    Config config = Config.FromConfigFile();
            //    MetadataAnalysis metadataAnalysis = new MetadataAnalysis(config);

            //    if (args.Length > 0 && (args[0] == optionDocument.ShortName|| args[0] == optionDocument.LongName))
            //    {
            //        Console.WriteLine("1=>Dot-Png");
            //        Extension.Dot2Png(config.DotExe, metadataAnalysis.OutPutPath);
            //        Console.WriteLine("2=>Report");
            //        metadataAnalysis.Export(DocType.Rtf);
            //        Console.WriteLine("已重新生成Report.rtf");
            //    }
            //    else
            //    {
            //        Console.WriteLine("1=>SimpleXls");
            //        metadataAnalysis.Export(DocType.SimpleXls);
            //        Console.WriteLine("2=>Xls");
            //        metadataAnalysis.Export(DocType.Xls);
            //        Console.WriteLine("3=>Dot");
            //        metadataAnalysis.Export(DocType.Dot);
            //        Console.WriteLine("4=>Png");
            //        metadataAnalysis.Export(DocType.Png);
            //        Console.WriteLine("5=>Rtf");
            //        metadataAnalysis.Export(DocType.Rtf);
            //        Console.WriteLine("所有文档已输出");
            //    }
            //});

            //app.Execute(args);
        }
    }
}
