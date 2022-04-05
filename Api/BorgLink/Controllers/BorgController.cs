using AutoMapper;
using Bazinga.AspNetCore.Authentication.Basic;
using BorgLink.Models;
using BorgLink.Models.Enums;
using BorgLink.Models.Options;
using BorgLink.Models.Paging;
using BorgLink.Models.ViewModels;
using BorgLink.Services;
using BorgLink.Services.Ethereum;
using BorgLink.Utils;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Controllers
{
    /// <summary>
    /// Borgs interface
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class BorgController : BaseController
    {
        private readonly ILogger<BorgController> _logger;
        private readonly BorgService _borgService;
        private readonly BorgTokenService _borgTokenService;
        private readonly IMapper _mapper;
        private readonly TwitterService _twitterService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="borgService"></param>
        /// <param name="borgTokenService"></param>
        /// <param name="cacheService"></param>
        /// <param name="logger"></param>
        /// <param name="mapper"></param>
        public BorgController(BorgService borgService, BorgTokenService borgTokenService, MemoryCacheService cacheService,
            ILogger<BorgController> logger, IMapper mapper, TwitterService twitterService)
            : base (cacheService)
        {
            _twitterService = twitterService;
            _mapper = mapper;
            _logger = logger;
            _borgTokenService = borgTokenService;
            _borgService = borgService;
        }

        /// <summary>
        /// Private: Test
        /// will not duplicate inserts
        /// </summary>
        /// <param name="id">The borg</param>
        /// <returns>The url of the borg</returns>
        /*[HttpGet]
        [Authorize(AuthenticationSchemes = BasicAuthenticationDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("contract/{id}")]
        public async Task<ActionResult<string>> GetBorgFromContract(int id)
        {
            // Save
            var borg = await _borgService.GetContractBorgAsync(id);
            if (borg == null)
                return UnprocessableEntity("Borg doesn't exist");

            return Ok(borg);
        }
        */

        /// <summary>
        /// Private: Used to generate/regenerate a generated borg (supply obj.Object.BorgId). This 
        /// will not duplicate inserts
        /// </summary>
        /// <param name="id">The borg</param>
        /// <returns>The url of the borg</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = BasicAuthenticationDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("{id}")]
        public async Task<ActionResult<string>> SaveBorg(int id)
        {
            // Save
            var borg = new Borg(id);

            // Queue job
            _borgService.SaveBorg(borg, true);

            return Ok();
        }

        /// <summary>
        /// Public: Gets all borgs
        /// </summary>
        /// <param name="parentId">The parent to filter by (optional)</param>
        /// <param name="childId">The child to filter by (optional)</param>
        /// <param name="attributes">The attributes to filter by (optional)</param>
        /// <param name="condition">The condition to filter by (optional)</param>
        /// <param name="pageNumber">The page to retrieve</param>
        /// <param name="perPage">The number to display per page</param>
        /// <returns>Borgs</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("~/borgs")]
        public async Task<ActionResult<PagedResult<BorgViewModel>>> GetAllBorgsAsync(int? id, int? parentId, int? childId, string attributeStr = null, Condition condition = Condition.Both, 
            uint pageNumber = 0, uint perPage = 36)
        {
            var missingBorgs = await _borgService.GetMissingBorgIdsAsync();
            // Check for page limit
            if (perPage > 1000)
                return BadRequest("Per page limit is 1000");

            // Join for cache key
            var attributes = string.IsNullOrEmpty(attributeStr) ? null : attributeStr.Split(',').ToList();

            // Try to get image from cache, if not then get new one from storage
            var cachedItem = GetCachedItem<PagedResult<BorgViewModel>>($"pagedborgs_parent_{parentId}_child_{childId}_attributes_{attributeStr}_condition_{condition}_page_{pageNumber}_perPage_{perPage}", () =>
            {
                // Get the borgs
                var borgs = _borgService.GetPagedBorgs(id, parentId, childId, attributes, condition, new Page(pageNumber, perPage));

                // Build the result to cache
                return new PagedResult<BorgViewModel>()
                {
                    Results = _mapper.Map<List<BorgViewModel>>(borgs.Results),
                    TotalResults = borgs.TotalResults,
                    Page = borgs.Page,
                };
            }, 30);

            // Map and return
            return Ok(cachedItem);
        }

        /// <summary>
        /// Public: Gets all borgs attributes and how many times they have been used)
        /// </summary>
        /// <param name="condition">The condition of the borgs with the attributes - optional - default alive</param>
        /// <returns>Borgs attributes</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("~/borgs/attributes")]
       public async Task<ActionResult<List<AttributeCountViewModel>>> GetAllBorgAttributeCountsAsync(Condition condition)
        {
            // Try to get image from cache, if not then get new one from storage
            var cachedItem = GetCachedItem<List<AttributeCountViewModel>>($"borg_attributes{condition}", () =>
            {
                // Get counts
                var attributeCounts = _borgService.GetUsedAttributeCounts(condition);
                  
                // Return mapped counts
                return _mapper.Map<List<AttributeCountViewModel>>(attributeCounts);
            }, 30);

            // Map and return
            return Ok(cachedItem);
        }

        /// <summary>
        /// Public: Gets all borgs attributes and how many times they have been used)
        /// </summary>
        /// <param name="condition">The condition of the borgs with the attributes - optional - default alive</param>
        /// <returns>Borgs attributes</returns>
        /*[HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("~/borgs/like")]
        public async Task<ActionResult<List<LikeBorgsViewModel>>> GetLikeBorgsAsync(Condition condition, uint pageNumber = 0, uint perPage = 36)
        {
            // Try to get image from cache, if not then get new one from storage
            var cachedItem = GetCachedItem<List<LikeBorgsViewModel>>($"like_borgs{condition}", () =>
            {
                // Get counts
                var likeBorgs = _borgService.GetLikeBorgs(condition, new Page(pageNumber, perPage));

                // Return mapped counts
                return _mapper.Map<List<LikeBorgsViewModel>>(likeBorgs);
            }, 30);

            // Map and return
            return Ok(cachedItem);
        }*/

        /// <summary>
        /// Public: Gets the rarity of a borg
        /// </summary>
        /// <param name="id">The borg to check rarity of</param>
        /// <returns>Borgs</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("~/borgs/rarity/{id}")]
        public async Task<ActionResult<decimal>> GetRarityOfBorgAsync(int id)
        {
            // Try to get image from cache, if not then get new one from storage
            var cachedItem = GetCachedItem<DecimalResult>($"rarity_{id}", () =>
            {
                // Get rarity
                return new DecimalResult(_borgService.GetRarityAsync(id));
            }, 30);

            // Map and return
            return Ok(cachedItem.Result);
        }

        /// <summary>
        /// Public: Gets a borg by id
        /// </summary>
        /// <param name="id">The borg</param>
        /// <returns>Borgs</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("~/borg/{id}")]
        public async Task<ActionResult<BorgViewModel>> GetBorgByIdAsync(int id)
        {
            // Try to get image from cache, if not then get new one from storage
            var cachedItem = GetCachedItem<BorgViewModel>($"borg_{id}", () =>
            {
                // Get borg
                var borg = _borgService.GetFullBorgFromDatabaseById(id);

                // Map and return
                return _mapper.Map<BorgViewModel>(borg);
            }, 20);

            // Map and return
            return Ok(cachedItem);
        }

        /// <summary>
        /// Public: Gets a borg by id to be displayed on OpenSea
        /// </summary>
        /// <param name="id">The borg</param>
        /// <returns>Borgs</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("~/opensea/borg/{id}")]
        public async Task<ActionResult<OpenseaBorgViewModel>> GetOpenseaBorgByIdAsync(int id)
        {
            // Try to get image from cache, if not then get new one from storage
            var cachedItem = GetCachedItem<OpenseaBorgViewModel>($"openseaborg_{id}", () =>
            {
                // Get borg
                var borg = _borgService.GetFullBorgFromDatabaseById(id);

                // Map and return
                return _mapper.Map<OpenseaBorgViewModel>(borg);
            }, 30);

            // Map and return
            return Ok(cachedItem);
        }
    }
}
