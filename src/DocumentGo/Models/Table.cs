using System.Collections.Generic;

namespace DocumentGo.Models
{
    public class Table
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public List<Column> Columns { get; set; }
    }
}