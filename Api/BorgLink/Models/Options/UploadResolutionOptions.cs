using BorgLink.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Models.Options
{
    /// <summary>
    /// FOr defining resolutions to save borgs in
    /// </summary>
    public class UploadResolutionOptions
    {
        /// <summary>
        /// THe container name to save resolution of
        /// </summary>
        public ResolutionContainer ResolutionContainer { get; set; }

        /// <summary>
        /// The width of the uploaded image
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of the uploaded image
        /// </summary>
        public int Height { get; set; }
    }
}
