using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BorgImageReader.Models
{
    public partial class CreateLayerItem : CreateLayerItemBase { }

    [Function("createLayerItems")]
    public class CreateLayerItemBase : FunctionMessage
    {
        [Parameter("uint256", "chance", 1)]
        public BigInteger Chance { get; set; }

        [Parameter("uint8", "layerIndex", 2)]
        public BigInteger LayerIndex { get; set; }

        [Parameter("string", "attributeName", 3)]
        public string AttributeName { get; set; }
    }
}
