using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BorgLink.Ethereum
{
    /// <summary>
    /// For getting borgs parents from contract
    /// </summary>
    public partial class BorgParentsFunction : BorgParentsFunctionBase
    {
        /// <summary>
        /// The borg to get parents for
        /// </summary>
        [Parameter("uint256", "borgId", 1)]
        public int BorgId { get; set; }
    }

    /// <summary>
    /// For getting borgs parents from contract
    /// </summary>
    [Function("getBorgsParents", "tuple")]
    public class BorgParentsFunctionBase : FunctionMessage
    { }

    /// <summary>
    /// For getting a borgs image from the contract
    /// </summary>
    public partial class BorgImageFunction : BorgImageFunctionBase {

        /// <summary>
        /// The borg to get the image for
        /// </summary>
        [Parameter("uint256", "borgId", 1)]
        public int BorgId { get; set; }
    }

    /// <summary>
    /// For getting a borgs image from the contract
    /// </summary>
    [Function("getBorgImage", "string[]")]
    public class BorgImageFunctionBase : FunctionMessage
    { }

    /// <summary>
    /// For getting the borg from the contract
    /// </summary>
    public partial class GetBorgFunction : GetBorgFunctionBase
    {
        /// <summary>
        /// The borg to get
        /// </summary>
        [Parameter("uint256", "borgId", 1)]
        public int BorgId { get; set; }
    }

    /// <summary>
    /// For getting the borg from the contract
    /// </summary>
    [Function("getBorg", "tuple")]
    public class GetBorgFunctionBase : FunctionMessage
    { }

    /// <summary>
    /// For getting the borgs attributes from the contract
    /// </summary>
    public partial class GetBorgsAttributesFunction : GetBorgsAttributesFunctionBase
    {
        /// <summary>
        /// The borg to get attributes for
        /// </summary>
        [Parameter("uint256", "borgId", 1)]
        public int BorgId { get; set; }
    }

    /// <summary>
    /// For getting the borgs attributes from the contract
    /// </summary>
    [Function("getBorgsAttributeNames", "string[]")]
    public class GetBorgsAttributesFunctionBase : FunctionMessage
    { }

    /// <summary>
    /// Event for transferring
    /// </summary>
    public partial class TransferEventDTO : TransferEventDTOBase { }

    /// <summary>
    /// Event for transferring
    /// </summary>
    [Event("Transfer")]
    public class TransferEventDTOBase : IEventDTO
    {
        /// <summary>
        /// Who to transfer from
        /// </summary>
        [Parameter("address", "_from", 1, true)]
        public virtual string From { get; set; }

        /// <summary>
        /// Who to transfer to
        /// </summary>
        [Parameter("address", "_to", 2, true)]
        public virtual string To { get; set; }

        /// <summary>
        /// The value to transfer
        /// </summary>
        [Parameter("uint256", "_value", 3, false)]
        public virtual BigInteger Value { get; set; }
    }

    /// <summary>
    /// For generating a borg
    /// </summary>
    public partial class GeneratedBorgEventDTO : GeneratedBorgEventDTOBase { }

    /// <summary>
    /// Event for generating a borg
    /// </summary>
    [Event("GeneratedBorg")]
    public class GeneratedBorgEventDTOBase : IEventDTO
    {
        /// <summary>
        /// The token id
        /// </summary>
        [Parameter("uint256", "borgId", 1, true)]
        public BigInteger BorgId { get; set; }

        /// <summary>
        /// The creator of the borg (address)
        /// </summary>
        [Parameter("address", "creator", 2, true)]
        public string Creator { get; set; }

        /// <summary>
        /// The time at which the event was fired
        /// </summary>
        [Parameter("uint256", "timestamp", 3, false)]
        public BigInteger Timestamp { get; set; }
    }

    /// <summary>
    /// Event for breeding a borg
    /// </summary>
    public partial class BredBorgEventDTO : BredBorgEventDTOBase { }

    /// <summary>
    /// Event for breeding a borg
    /// </summary>
    [Event("BredBorg")]
    public class BredBorgEventDTOBase : IEventDTO
    {
        /// <summary>
        /// The new borg created
        /// </summary>
        [Parameter("uint256", "childId", 1, true)]
        public BigInteger ChildId { get; set; }

        /// <summary>
        /// THe first parent
        /// </summary>
        [Parameter("uint256", "parentId1", 2, true)]
        public BigInteger ParentId1 { get; set; }

        /// <summary>
        /// The second parent
        /// </summary>
        [Parameter("uint256", "parentId2", 3, true)]
        public BigInteger ParentId2 { get; set; }

        /// <summary>
        /// The owner of the new child
        /// </summary>
        [Parameter("address", "breeder", 4, false)]
        public string Breeder { get; set; }

        /// <summary>
        /// When the borg was bred
        /// </summary>
        [Parameter("uint256", "timestamp", 5, false)]
        public BigInteger Timestamp { get; set; }
    }

    /// <summary>
    /// For getting a borg from the contract
    /// </summary>
    [FunctionOutput]
    public class GetBorgOutputDTO : IFunctionOutputDTO
    {
        /// <summary>
        /// The given name
        /// </summary>
        [Parameter("string", "name", 1)]
        public string Name { get; set; }

        /// <summary>
        /// The borg image
        /// </summary>
        [Parameter("bytes8[]", "image", 2)]
        public List<byte[]> Image { get; set; }

        /// <summary>
        /// The attributes which make up the image
        /// </summary>
        [Parameter("string[]", "attributes", 3)]
        public List<string> Attributes { get; set; }

        /// <summary>
        /// A prent if exists
        /// </summary>
        [Parameter("uint256", "parentId1", 4)]
        public BigInteger ParentId1 { get; set; }

        /// <summary>
        /// A parent if exists
        /// </summary>
        [Parameter("uint256", "parentId2", 5)]
        public BigInteger ParentId2 { get; set; }

        /// <summary>
        /// A child if exists
        /// </summary>
        [Parameter("uint256", "childId", 6)]
        public BigInteger ChildId { get; set; }
    }
}
