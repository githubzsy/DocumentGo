using DocumentGo.Models;

namespace DocumentGo.Import
{
    public abstract class BaseImport
    {
        public Config Config { get; }

        protected BaseImport(Config config)
        {
            Config = config;
        }

        public abstract object Import();
    }
}
