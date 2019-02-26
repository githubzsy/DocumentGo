using System.Collections.Generic;

namespace DocumentGo.Models
{
    public class Module
    {
        public string Name { get; set; }

        public int Order { get; set; }

        public List<Child> Children { get; set; }

    }
}