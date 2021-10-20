using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.Paging
{
    /// <summary>
    /// A page of results
    /// </summary>
    /// <typeparam name="T">THe type of results the page returns</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// The current page being returned
        /// </summary>
        public Page Page { get; set; }

        //public SortOrder SortOrder { get; set; }

        /// <summary>
        /// The total number of results being returned
        /// </summary>
        public int TotalResults { get; set; }

        /// <summary>
        /// The page of results
        /// </summary>
        public List<T> Results { get; set; }
    }
}
