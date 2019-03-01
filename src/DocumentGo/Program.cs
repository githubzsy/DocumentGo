using DocumentGo.Import;
using DocumentGo.Models;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;

namespace DocumentGo
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "DocumentGo --By Zhaobb";

            // 读取配置文件
            Config config = Config.FromConfigFile();

            while (true)
            {
                int choice = Prompt.GetInt("请选择操作:\n\n1. 读取元数据\n2. 导出Excel(可调整输出的Excel)\n3. 导出Dot(可调整输出的Dot)\n4. 生成Png\n5. 生成Rtf\n\n6. 退出\n",
                promptColor: ConsoleColor.Green,
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
            }
        }

        private static void Choice5(Config config)
        {
            SchemaCollection SchemaCollection = InitSchemaCollection(config);
            BaseExport export = new RtfExport(config, SchemaCollection);
            export.Export();
            Console.WriteLine("***OK 生成Rtf");
        }

        private static void Choice4(Config config)
        {
            DotUtil.Exec(config.DotExe, config.Output);
            Console.WriteLine("***OK 生成Png");
        }

        private static void Choice3(Config config)
        {
            SchemaCollection SchemaCollection = InitSchemaCollection(config);
            // 导出Dot文件
            BaseExport export = new DotExport(config, SchemaCollection);
            export.Export();

            Console.WriteLine("***OK 导出Dot(可调整输出的Dot)");
        }

        private static void Choice2(Config config)
        {
            SchemaCollection SchemaCollection = InitSchemaCollection(config);
            // 导出Xls
            BaseExport export = new ExcelExport(config, SchemaCollection);
            export.Export();
            Console.WriteLine("***OK 导出Excel(可调整输出的Excel)");
        }

        private static void Choice1(Config config)
        {
            InitSchemaCollection(config);
            Console.WriteLine("***OK 读取元数据");
        }

        private static SchemaCollection InitSchemaCollection(Config config)
        {
            SchemaCollection SchemaCollection;

            var excelName= Path.Combine(config.Output, "Report.xls");
            var dataFile = Path.Combine(config.Output, "Metadata.dat");

            if (File.Exists(excelName))
            {
                BaseImport import = new ExcelImport(config);

                SchemaCollection = (SchemaCollection)import.Import();
            }else if (File.Exists(dataFile))
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
    }
}
