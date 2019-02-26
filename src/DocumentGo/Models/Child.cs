using System.Collections.Generic;

namespace DocumentGo.Models
{
    public class Child
    {
        public string Name { get; set; }

        public int Order { get; set; }

        public DrawObjectEnum DrawObjectEnum { get; set; }

        public List<string> Entities { get; set; }
    }
}