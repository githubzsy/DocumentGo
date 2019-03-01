using DocumentGo.Models;

namespace DocumentGo
{
    public abstract class BaseExport
    {
        public Config Config { get; }

        public SchemaCollection SchemaCollection { get; }

        protected BaseExport(Config config, SchemaCollection schemaCollection)
        {
            Config = config;
            SchemaCollection = schemaCollection;
        }

        public abstract void Export();
    }
}