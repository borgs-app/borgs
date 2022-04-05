using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace BorgSpawner
{
    /// <summary>
    /// For getting an image (argument)
    /// </summary>
    public partial class BorgImageFunction : BorgImageFunctionBase {
        [Parameter("uint256", "borgId", 1)]
        public int BorgId { get; set; }
    }

    /// <summary>
    /// For getting an image (response)
    /// </summary>
    [Function("getBorgImage", "string[]")]
    public class BorgImageFunctionBase : FunctionMessage
    { }

}
