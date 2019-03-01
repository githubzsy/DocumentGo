using System.IO;
using DocumentGo.Models;

namespace DocumentGo
{
    public abstract class ExportBase
    {
        public Config Config { get; }

        public string Output { get; }

        public SchemaCollection SchemaCollection { get; }

        protected ExportBase(Config config, SchemaCollection schemaCollection)
        {
            Config = config;
            SchemaCollection = schemaCollection;

            Output = Path.Combine(Directory.GetCurrentDirectory() + "\\Output");

            if (!Directory.Exists(Output))
            {
                Directory.CreateDirectory(Output);
            }
        }

        public abstract void Export();
    }
}