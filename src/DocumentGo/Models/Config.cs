using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DocumentGo.Models
{
    public class Config
    {
        public string WebRoot { get; set; }

        public string ApplicationCode { get; set; }

        public string Output { get; set; }

        public string DotExe { get; set; }

        public List<Module> Modules { get; set; }

        public static Config FromConfigFile()
        {
            if (!File.Exists("ConfigSet.json"))
            {
                //Config option = Config.Default();
                //string jsonContent = JsonConvert.SerializeObject(option);
                //File.WriteAllText("ConfigSet.tpl.json", jsonContent);
                Console.WriteLine("缺少配置文件");
                Console.ReadKey();
                Environment.Exit(0);
            }
            Config config= JsonConvert.DeserializeObject<Config>(File.ReadAllText("ConfigSet.json", System.Text.Encoding.UTF8));

            if (string.IsNullOrEmpty(config.Output) || string.IsNullOrWhiteSpace(config.Output))
            {
                config.Output= Path.Combine(Directory.GetCurrentDirectory() + "\\Output");
            }

            if(!Directory.Exists(config.Output))
            {
                Directory.CreateDirectory(config.Output);
            }

            return config;
        }
    }
}