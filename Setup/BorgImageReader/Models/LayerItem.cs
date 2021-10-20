using System;
using System.Collections.Generic;
using System.Text;

namespace BorgImageReader
{
    public class LayerItem
    {
        public string Name { get; set; }
        public decimal Chance { get; set; }
        public Histogram Histogram { get; set; }
    }
}
