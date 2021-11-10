using AutoMapper;
using BorgLink.Models;
using BorgLink.Models.Enums;
using BorgLink.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Mapping.Converters
{
    /// <summary>
    /// Convert our borg to OpenSea standard
    /// </summary>
    public class OpenseaBorgConverter : ITypeConverter<Borg, OpenseaBorgViewModel>
    {
        /// <summary>
        /// Convert a borg to what OpenSea expects
        /// </summary>
        /// <param name="source">The source object</param>
        /// <param name="destination">THe destination object to return</param>
        /// <param name="context">The runtime context</param>
        /// <returns>Converted message threads</returns>
        public OpenseaBorgViewModel Convert(Borg source, OpenseaBorgViewModel destination, ResolutionContext context)
        {
            // Setup
            if (source == null)
                return null;
            if (destination == null)
                destination = new OpenseaBorgViewModel();
   
            // Map
            destination.Name = source.Name;
            destination.Image = string.Format(source.Url, ResolutionContainer.Large.ToString().ToLower());
            destination.Attributes = source?.BorgAttributes?
                .Where(x => x.Attribute != null)
                .Where(x => (!x.Attribute.Name?.Contains("blank") ?? false))
                .Select(x => new OpenSeaAttributeViewModel(){ TraitType = $"Layer {x.Attribute.LayerNumber}", Value = x.Attribute.Name })
                .ToList();
            destination.Description = "Cyborgs DAO is made of ~20k androgynous cyborgs that spawn, breed & die on-chain. All borg artwork is stored in the token (no IPFS) and borgs are randomly generated directly on-chain when spawned.";
            destination.External_url = $"https://borgs.app/borgs/{source.BorgId}";

            // Returned the mapping
            return destination;
        }
    }
}
