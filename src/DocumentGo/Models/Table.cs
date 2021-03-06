﻿using System;
using System.Collections.Generic;

namespace DocumentGo.Models
{
    [Serializable]
    public class Table
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public List<Column> Columns { get; set; } = new List<Column>();
    }
}