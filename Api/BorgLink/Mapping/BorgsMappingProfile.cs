using AutoMapper;
using BorgLink.Ethereum;
using BorgLink.Mapping.Converters;
using BorgLink.Models;
using BorgLink.Models.Enums;
using BorgLink.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Mapping
{
    /// <summary>
    /// The mapping profile
    /// </summary>
    public class BorgsMappingProfile : Profile
    {
        /// <summary>
        /// The constructor
        /// </summary>
        public BorgsMappingProfile()
        {
            InitializeMap();
        }

        /// <summary>
        /// Initialze and create all of the mappings of models - view models
        /// </summary>
        private void InitializeMap()
        {
            #region Borgs

            CreateMap<LikeBorgsViewModel, LikeBorgs>();
            CreateMap<LikeBorgs, LikeBorgsViewModel>();

            CreateMap<PlainBorgViewModel, Borg>();
            CreateMap<Borg, PlainBorgViewModel>()
                .ForMember(dest => dest.Id, src => src.MapFrom(x => x.BorgId));

            CreateMap<AttributeCountViewModel, AttributeCount>();
            CreateMap<AttributeCount, AttributeCountViewModel>();

            CreateMap<Borg, BorgViewModel>()
                .ForMember(dest => dest.Id, src => src.MapFrom(x => x.BorgId))
                .ForMember(dest => dest.Url, src => src.MapFrom(x => string.Format(x.Url, ResolutionContainer.Default.ToString().ToLower())));
            CreateMap<BorgViewModel, Borg>();

            CreateMap<GetBorgOutputDTO, BorgViewModel>();
            CreateMap<BorgViewModel, GetBorgOutputDTO>();

            CreateMap<GeneratedBorgEventDTO, Borg>()
                .ForMember(dest => dest.BorgId, src => src.MapFrom(x => x.BorgId));
            CreateMap<Borg, GeneratedBorgEventDTO>();

            CreateMap<BredBorgEventDTO, Borg>()
                .ForMember(dest => dest.BorgId, src => src.MapFrom(x => x.ChildId))
                .ForMember(dest => dest.ParentId1, src => src.MapFrom(x => x.ParentId1))
                .ForMember(dest => dest.ParentId2, src => src.MapFrom(x => x.ParentId2));

            CreateMap<Borg, BredBorgEventDTO>();

            CreateMap<Borg, OpenseaBorgViewModel>()
                .ConvertUsing<OpenseaBorgConverter>();

            #endregion

            #region Attributes

            CreateMap<AttributeViewModel, Models.Attribute>();
            CreateMap<Models.Attribute, AttributeViewModel>();

            #endregion

            #region Borg Attributes

            CreateMap<BorgAttributeViewModel, BorgAttribute>();
            CreateMap<BorgAttribute, BorgAttributeViewModel>();

            #endregion
        }
    }
}
