using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.Paging
{
    /// <summary>
    /// For returning pages of results
    /// </summary>
    public class Page
    {
        /// <summary>
        /// The current page number
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// How many items to display per page
        /// </summary>
        public int PerPage { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pageNumber">The current page number</param>
        /// <param name="perPage">How many items to display per page</param>
        public Page(uint pageNumber, uint perPage)
        {
            PageNumber = (int)pageNumber;
            PerPage = (int)perPage;
        }
    }
}
