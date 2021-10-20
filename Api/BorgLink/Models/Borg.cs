using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace BorgLink.Models
{
    /// <summary>
    /// A borg
    /// </summary>
    public class Borg
    {
        /// <summary>
        /// The unique identifier
        /// </summary>
        public int BorgId { get; set; }

        /// <summary>
        /// The url of the borg
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Simple storage for attributes
        /// </summary>
        public List<string> Attributes { get; set; }

        /// <summary>
        /// If the borg has been bred, then this is the first parent
        /// </summary>
        public int? ParentId1 { get; set; }

        /// <summary>
        /// If the borg has been bred, then this is the second parent
        /// </summary>
        public int? ParentId2 { get; set; }

        /// <summary>
        /// A name of the borg (if any) - set by owner
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// If the borg has been bred, then this is the child ref
        /// </summary>
        public int? ChildId { get; set; }

        /// <summary>
        /// The attributes that make up the borg
        /// </summary>
        public List<BorgAttribute> BorgAttributes { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Borg() { }

        /// <summary>
        /// Constructor for easy setup
        /// </summary>
        /// <param name="borgId">The borg</param>
        /// <param name="parentId1">The parent borgs id (1)</param>
        /// <param name="parentId2">The parent borgs id (2)</param>
        public Borg(int borgId, BigInteger? parentId1 = null, BigInteger? parentId2 = null)
        {
            // Set data
            BorgId = borgId;
            ParentId1 = (int?)parentId1;
            ParentId2 = (int?)parentId2;
        }
    }
}
