using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models
{
    public class LikeBorgs
    {
        public List<Attribute> Attributes { get; set; }
        public List<Borg> Borgs {get;set;}
        public int Count { get; set; }
    }
}
