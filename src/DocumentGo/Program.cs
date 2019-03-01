using DocumentGo.Import;
using DocumentGo.Models;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Threading;

namespace DocumentGo
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "DocumentGo --By Zhaobb";
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;

            // 读取配置文件
            Config config = Config.FromConfigFile();
            int choice = 0;
            while (true)
            {
                ChoiceTips(choice);

                choice = Prompt.GetInt("你选择的操作是：",
                promptColor: ConsoleColor.White,
                promptBgColor: ConsoleColor.Black);

                switch (choice)
                {
                    case 1: Choice1(config); break;
                    case 2: Choice2(config); break;
                    case 3: Choice3(config); break;
                    case 4: Choice4(config); break;
                    case 5: Choice5(config); break;
                    case 6: Environment.Exit(0); break;
                    default: break;
                }
                Thread.Sleep(2000);
                Console.Clear();
            }
        }

        private static void Choice5(Config config)
        {
            SchemaCollection SchemaCollection = InitSchemaCollection(config);
            BaseExport export = new RtfExport(config, SchemaCollection);
            export.Export();
        }

        private static void Choice4(Config config)
        {
            DotUtil.Exec(config.DotExe, config.Output);
        }

        private static void Choice3(Config config)
        {
            SchemaCollection SchemaCollection = InitSchemaCollection(config);
            // 导出Dot文件
            BaseExport export = new DotExport(config, SchemaCollection);
            export.Export();
        }

        private static void Choice2(Config config)
        {
            SchemaCollection SchemaCollection = InitSchemaCollection(config);
            // 导出Xls
            BaseExport export = new ExcelExport(config, SchemaCollection);
            export.Export();
        }

        private static void Choice1(Config config)
        {
            InitSchemaCollection(config);
        }

        private static SchemaCollection InitSchemaCollection(Config config)
        {
            SchemaCollection SchemaCollection;

            string excelName = Path.Combine(config.Output, "Report.xls");
            string dataFile = Path.Combine(config.Output, "Metadata.dat");

            if (File.Exists(excelName))
            {
                BaseImport import = new ExcelImport(config);

                SchemaCollection = (SchemaCollection)import.Import();
            }
            else if (File.Exists(dataFile))
            {
                byte[] bytes = dataFile.ToBytes();

                SchemaCollection = bytes.ToObject<SchemaCollection>();
            }
            else
            {
                // 读取元数据文件
                MetadataImport import = new MetadataImport(config);

                SchemaCollection = (SchemaCollection)import.Import();

                // 保持文件
                SchemaCollection.ToBytes().ToFile(dataFile);
            }

            return SchemaCollection;
        }

        private static void ChoiceTips(int choice)
        {
            Console.WriteLine("请选择操作:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\t{(choice == 1 ? "*" : " ")}1.读取元数据");
            Console.WriteLine($"\t{(choice == 2 ? "*" : " ")}2.导出Excel(可调整输出的Excel)");
            Console.WriteLine($"\t{(choice == 3 ? "*" : " ")}3.生成Dot(可调整输出的Dot)");
            Console.WriteLine($"\t{(choice == 4 ? "*" : " ")}4.绘制ER图");
            Console.WriteLine($"\t{(choice == 5 ? "*" : " ")}5.生成Rtf");
            Console.WriteLine($"\t 6.退出");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
