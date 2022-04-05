using Azure.Storage.Blobs.Models;
using BorgLink.Ethereum;
using BorgLink.Models;
using BorgLink.Models.Enums;
using BorgLink.Models.Options;
using BorgLink.Models.Paging;
using BorgLink.Repositories;
using BorgLink.Services.Ethereum;
using BorgLink.Services.Platform;
using BorgLink.Services.Storage.Interfaces;
using BorgLink.Utils;
using Hangfire;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BorgLink.Services
{
    /// <summary>
    /// Service for all things borg
    /// </summary>
    public class BorgService
    {
        private readonly BorgServiceOptions _options;
        private readonly BorgTokenService _borgTokenService;
        private readonly AttributeRepository _attributeRepository;
        private readonly BorgAttributeRepository _borgAttributeRepository;
        private readonly IStorageService _storageService;
        private readonly WebhookService _webhookService;
        private readonly TwitterService _twitterService;
        private readonly BorgRepository _borgRepository;
        private readonly JobRepository _jobRepository;
        private readonly TwitterServiceOptions _twitterOptions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="borgTokenService">The token service to talk to the blockchain</param>
        /// <param name="storageService">The storage service to get/set images</param>
        /// <param name="borgRepository">The borg database repository</param>
        /// <param name="borgAttributeRepository">The borg attributes database repository</param>
        /// <param name="attributeRepository">The attribute database repository</param>
        /// <param name="options">The options/settings</param>
        public BorgService(BorgTokenService borgTokenService, IStorageService storageService, BorgRepository borgRepository, WebhookService webhookService, JobRepository jobRepository,
            TwitterService twitterService, BorgAttributeRepository borgAttributeRepository, AttributeRepository attributeRepository, IOptions<BorgServiceOptions> options,
            IOptions<TwitterServiceOptions> twitterOptions)
        {
            _twitterOptions = twitterOptions.Value;
            _jobRepository = jobRepository;
            _twitterService = twitterService;
            _borgRepository = borgRepository;
            _borgTokenService = borgTokenService;
            _storageService = storageService;
            _attributeRepository = attributeRepository;
            _borgAttributeRepository = borgAttributeRepository;
            _webhookService = webhookService;

            _options = options.Value;
        }

        /// <summary>
        /// Gets a basic borg from the database (no relationships)
        /// </summary>
        /// <param name="borgId">The borg to get</param>
        /// <returns>The borg requested (if exists)</returns>
        public Borg GetBorgFromDatabaseById(int borgId)
        {
            return _borgRepository.GetAll()
                .FirstOrDefault(x => x.BorgId == borgId);
        }

        /// <summary>
        /// Gets the FULL borg from the database (ALL relationships)
        /// </summary>
        /// <param name="borgId">The borg to get</param>
        /// <returns>The borg requested (if exists)</returns>
        public Borg GetFullBorgFromDatabaseById(int borgId)
        {
            return _borgRepository.GetAllWithAttributes()
                .FirstOrDefault(x => x.BorgId == borgId);
        }

        /// <summary>
        /// Save a borg to the databse
        /// </summary>
        /// <param name="borg">The borg to save</param>
        /// <returns>The saved borg</returns>
        public async Task<Borg> SaveBorgInDatabaseAsync(Borg borg)
        {
            // Add borg
            _borgRepository.Add(borg);

            // Save
            var saved = await _borgRepository.EnsureSaveChangesAsync();
            if (saved)
                return borg;

            return null;
        }

        /// <summary>
        /// Save a borg in db
        /// </summary>
        /// <param name="borg">THe borg to save</param>
        /// <param name="triggerWebhooks">If webhooks are to be triggered with creation</param>
        /// <returns>The url of the uploaded borg</returns>
        public bool SaveBorg(Borg borg, bool triggerWebhooks)
        {
            // If not then save borg
            BackgroundJob.Enqueue(() => SaveBorgInDatabaseAndStorageAsync(borg, triggerWebhooks));
            return true;
        }

        /// <summary>
        /// Save a borg in db
        /// </summary>
        /// <param name="borg">THe borg to save</param>
        /// <param name="triggerWebhooks">If webhooks are to be triggered with creation</param>
        /// <returns>The url of the uploaded borg</returns>
        public async Task<Borg> SaveBorgInDatabaseAndStorageAsync(Borg borg, bool triggerWebhooks)
        {
            // Check for existing
            var existingBorg = GetBorgFromDatabaseById(borg.BorgId);

            // Get borg
            if (existingBorg == null)
            {
                // Get borg from contract
                var importedBorg = await ImportBorgAsync(borg.BorgId);

                // Save in database
                if (importedBorg != null)
                {
                    // Save
                    var savedBorg = await SaveBorgInDatabaseAsync(importedBorg);

                    // Link attributes
                    await AddBorgAttributesAsync(importedBorg.BorgId, importedBorg.Attributes);

                    // Check if we need to update parents (for children)
                    if (importedBorg.ParentId1.HasValue && importedBorg.ParentId2.HasValue)
                        await UpdateParentsChildrenAsync(importedBorg.ParentId1.Value, importedBorg.ParentId2.Value, borg.BorgId);

                    // Post to twitter
                    var url = $"{savedBorg.Url.Replace("{0}", "large")}";
                    await _twitterService.PostAsync($"#Borg {importedBorg.BorgId} has been spawned at {DateTime.UtcNow} UTC https://borgs.app/borgs/{importedBorg.BorgId}", url);

                    // Prompt site rebuild
                    if (triggerWebhooks)
                        await _webhookService.PropegateAsync();

                    return savedBorg;
                }
            }

            return existingBorg;
        }

        /// <summary>
        /// Link attributes to a borg
        /// </summary>
        /// <param name="borgId">The borg to add attributes to</param>
        /// <param name="attributes">The attributes to add to borg</param>
        /// <returns>The operation success</returns>
        public async Task<bool> AddBorgAttributesAsync(int borgId, List<string> attributes)
        {
            // Get matching attributes from db
            var dbAttributes = _attributeRepository.GetAll()
                .Where(x => attributes.Contains(x.Name))
                .ToList();

            // Add attributes to borg
            await SaveBorgAttributesInDatabaseAsync(borgId, dbAttributes.OrderBy(x => x.LayerNumber)
                .Select(x => x.Id)
                .ToList());

            // If we got here with no errors, then true
            return true;
        }

        /// <summary>
        /// Get the total used attribute counts
        /// </summary>
        /// <returns>How many times each attribute has been used (that exists)</returns>
        public List<AttributeCount> GetUsedAttributeCounts(Condition condition)
        {
            return  _attributeRepository.GetAttributeCounts(condition)
                .ToList();
        }

        /// <summary>
        /// Get a borgs total used attribute counts
        /// </summary>
        /// <returns>How many times each attribute has been used (that exists)</returns>
        public List<AttributeCount> GetUsedAttributeCounts(int borgId)
        {
            // Get all of the borgs attributes
            var attributeIds = _borgAttributeRepository.GetAll()
                .Where(x => x.BorgId == borgId)
                .Select(x => x.AttributeId)
                .ToList();

            // Get all used counts for the borgs attributes only
            return _borgAttributeRepository.GetAllWithAttribute()
                .Where(x => attributeIds.Contains(x.AttributeId))
                .GroupBy(x => x.Attribute.Name)
                .Select(x => new AttributeCount() { Name = x.Key, Count = x.Count() })
                .ToList();
        }

        /// <summary>
        /// Saves a link between the attributes and a borg in the database
        /// </summary>
        /// <param name="borgId">The borg to save attributes for</param>
        /// <param name="attributeIds">The attributes to save for the borg</param>
        /// <returns>The saved links</returns>
        private async Task<List<BorgAttribute>> SaveBorgAttributesInDatabaseAsync(int borgId, List<int> attributeIds)
        {
            // Link to borg now
            var attributeLinks = new List<BorgAttribute>();
            for (var i = 0; i < attributeIds.Count(); i++)
            {
                // Create link
                attributeLinks.Add(new BorgAttribute() { AttributeId = attributeIds[i], BorgId = borgId });
            }

            // Insert
            _borgAttributeRepository.AddRange(attributeLinks);
            if (await _borgAttributeRepository.EnsureSaveChangesAsync())
                return attributeLinks;

            return null;
        }

        /// <summary>
        /// This updates the parents/children or a borg
        /// </summary>
        /// <param name="parentAId">The first parent</param>
        /// <param name="parentBId">The second parent</param>
        /// <param name="childId">The child</param>
        /// <returns>If the update was successful or not</returns>
        public async Task<bool> UpdateParentsChildrenAsync(int parentAId, int parentBId, int childId)
        {
            // Get parents
            var parentA = GetBorgFromDatabaseById(parentAId);
            var parentB = GetBorgFromDatabaseById(parentBId);

            // Set children
            if (parentA != null)
            {
                parentA.ChildId = childId;
                _borgRepository.Update(parentA);
            }

            if (parentB != null)
            {
                parentB.ChildId = childId;
                _borgRepository.Update(parentB);
            }

            return await _borgRepository.EnsureSaveChangesAsync();
        }

        public async Task<GetBorgOutputDTO> GetContractBorgAsync(int borgId)
        {
            // Get a borgs image
            var blockChainBorg = await _borgTokenService.GetBorgAsync(borgId);

            // Check that there is something to upload
            if (blockChainBorg?.Attributes?.All(x => string.IsNullOrEmpty(x)) ?? true)
                return null;

            // Convert to bitmap
            var borgImage = ImageUtils.ConvertBorgToBitmap(blockChainBorg.Image);

            // Resize
            borgImage = ImageUtils.ResizeBitmap(borgImage, 24, 24);

            return blockChainBorg;
        }

        /// <summary>
        /// Imports a borg from the blockchain and uploads to storage
        /// </summary>
        /// <param name="borgId">The borg to save image from</param>
        /// <returns></returns>
        public async Task<Borg> ImportBorgAsync(int borgId)
        {
            // Define Borg
            var borg = new Borg(borgId);

            // Get a borgs image
            var blockChainBorg = await _borgTokenService.GetBorgAsync(borgId);

            // Check that there is something to upload
            if (blockChainBorg?.Attributes?.All(x => string.IsNullOrEmpty(x)) ?? true)
                return null;

            // Upload 
            if (!await DoesBorgExistInStorageAsync(borgId, ResolutionContainer.Default))
                await UploadBorgToStorage(borgId, blockChainBorg.Image, ResolutionContainer.Default, 24, 24, 0);

            if (!await DoesBorgExistInStorageAsync(borgId, ResolutionContainer.Medium))
                await UploadBorgToStorage(borgId, blockChainBorg.Image, ResolutionContainer.Medium, 600, 600, 14);

            if (!await DoesBorgExistInStorageAsync(borgId, ResolutionContainer.Large))
                await UploadBorgToStorage(borgId, blockChainBorg.Image, ResolutionContainer.Large, 1400, 1400, 30);

            // Copy the rest of data across
            borg.Attributes = blockChainBorg.Attributes;
            borg.ParentId1 = blockChainBorg.ParentId1 > 0 ? (int?)blockChainBorg.ParentId1 : null;
            borg.ParentId2 = blockChainBorg.ParentId2 > 0 ? (int?)blockChainBorg.ParentId2 : null;
            borg.ChildId = blockChainBorg.ChildId > 0 ? (int?)blockChainBorg.ChildId : null;

            // Set url
            borg.Url = $"{_options.BaseStorageUrl}/{_storageService.GetContainerName(null)}/{borgId}.png";

            return borg;
        }

        public async Task<bool> DoesBorgExistInStorageAsync(int borgId, ResolutionContainer container)
        {
            return await _storageService.DoesBorgExist(borgId, container);
        }

        private async Task UploadBorgToStorage(int borgId, List<byte[]> image, ResolutionContainer resolutionContainer, int width, int height, int blank)
        {
            // Convert to bitmap
            var borgImage = ImageUtils.ConvertBorgToBitmap(image);

            // Resize
            borgImage = ImageUtils.ResizeBitmap(borgImage, width, height);

            // Crop
            borgImage = ImageUtils.Crop(borgImage, new Rectangle(new Point(0, 0), new Size(width - blank, height - blank)));

            // Open stream to image
            using (var stream = new MemoryStream())
            {
                borgImage.Save(stream, ImageFormat.Png);

                // Save image and get url
                var info = await _storageService.UploadBlobAsync(stream, $"{borgId}.png", resolutionContainer);
            }
        }

        /// <summary>
        /// Get the rarity of a borg
        /// </summary>
        /// <param name="id">The borg to check rarity of</param>
        /// <returns>The rarity of the borg</returns>
        public decimal GetRarityAsync(int id)
        {
            // Get borg
            var borg = GetBorgFromDatabaseById(id);
            if (borg == null)
                return -1;

            // Get total borgs (alive)
            var totalBorgs = GetTotalBorgs(Condition.Alive);

            // Define rarity
            var rarities = new List<decimal>();

            // Get the total used counts
            var attributeCounts = GetUsedAttributeCounts(id);

            // Find the rarity for each attribute
            foreach (var attribute in attributeCounts)
            {
                // Find the probability for the one attribute (how much its used)
                var probability = (decimal)attribute.Count / (decimal)totalBorgs;

                // Save it
                rarities.Add(probability);
            }

            // Return the average
            return rarities.Sum(x => x) / attributeCounts.Count();
        }

        /// <summary>
        /// Counts total borgs based on condition
        /// </summary>
        /// <param name="condition">The condition of the borgs</param>
        /// <returns>How many borgs of a certain condition exist</returns>
        public decimal GetTotalBorgs(Condition condition)
        {
            // Get all borgs
            var borgs = _borgRepository.GetAll();

            // Filter
            if (condition == Condition.Alive)
                borgs = borgs.Where(x => x.ChildId == null);
            if (condition == Condition.Dead)
                borgs = borgs.Where(x => x.ChildId != null);

            // Count
            return borgs.Count();
        }

        /// <summary>
        /// Get Borgs paged
        /// </summary>
        /// <param name="parentId">The parent to search by</param>
        /// <param name="childId">The child to search by</param>
        /// <param name="attributes">Any attributes to search by</param>
        /// <param name="condition">The current condition of the borg to search by</param>
        /// <param name="page">The page to get</param>
        /// <returns>Paged Borgs</returns>
        public PagedResult<Borg> GetPagedBorgs(int? id, int? parentId, int? childId, List<string> attributes, Condition? condition, Page page)
        {
            // Get the borgs
            var borgs = GetBorgs(parentId, childId, attributes, condition);

            // Filter by id if needed
            if (id.HasValue)
                borgs = borgs.Where(x => x.BorgId == id);

            // Page - default newest first
            var pagedBorgs = borgs.OrderByDescending(x => x.BorgId)
                .Skip(page.PageNumber * page.PerPage)
                .Take(page.PerPage)
                .ToList();

            // Return
            return new PagedResult<Borg>()
            {
                Page = page,
                Results = pagedBorgs,
                TotalResults = borgs.Count()
            };
        }

        /// <summary>
        /// Gets borgs from database
        /// </summary>
        /// <param name="parentId">ParentA or ParentB id</param>
        /// <param name="childId">The child</param>
        /// <param name="attributes">Any attributes to filter by</param>
        /// <param name="condition">The current condition of the borg to search by</param>
        /// <returns>A list of borgs</returns>
        public IQueryable<Borg> GetBorgs(int? parentId, int? childId, List<string> attributes, Condition? condition)
        {
            // Get borgs
            var borgs = _borgRepository.GetAllWithAttributes();

            // Filter by parent
            if (childId.HasValue)
                borgs = borgs.Where(x => x.ParentId1 == parentId || x.ParentId2 == parentId);

            // Filter by child
            if (childId.HasValue)
                borgs = borgs.Where(x => x.ChildId == childId);

            // Filter by attributes
            if ((attributes?.Any() ?? false))
            {
                // Get all matching attribute ids
                var dbAttributeIds = _attributeRepository.GetAll()
                    .Where(x => attributes.Contains(x.Name))
                    .Select(x => x.Id)
                    .ToList();

                // Get all borgs with these attributes
                var borgIds = _borgAttributeRepository.GetAll()
                    .Where(x => dbAttributeIds.Contains(x.AttributeId))
                    .Select(x => x.BorgId)
                    .Distinct()
                    .ToList();

                // Filter by the selected ids
                borgs = borgs.Where(x => borgIds.Contains(x.BorgId));
            }

            // Filter by condition
            if (condition.HasValue && condition == Condition.Alive)
                borgs = borgs.Where(x => x.ChildId == null);
            else if (condition.HasValue && condition == Condition.Dead)
                borgs = borgs.Where(x => x.ChildId != null);

            // Return the query
            return borgs;
        }

        /// <summary>
        /// Get all Borgs not in sequence
        /// </summary>
        /// <returns>Unsequenced Borgs (therefore missing)</returns>
        public async Task<List<int>> GetMissingBorgIdsAsync()
        {
            // Get all the borgs
            var borgs = _borgRepository.GetAll()
                .OrderBy(x => x.BorgId).ToList();

            // Define missing borgs
            var missingBorgs = new List<int>();

            // Define previous value
            var previousValue = 0;

            // Check all borgs 
            for (int i = 0; i < borgs.Count(); i++)
            {
                // Nothing to compare to until > 0
                if (i > 0)
                {
                    // Check is in sequence
                    var value = borgs[i].BorgId - previousValue;
                    if (value > 1)
                    {
                        for (int j = 0; j < value; j++)
                        {
                            // Add to missing Borgs
                            missingBorgs.Add(previousValue + j);
                        }
                    }
                }

                // Set the previous value
                previousValue = borgs[i].BorgId;
            }

            // Now for any above
            var newestId = await _borgTokenService.GetTotalGeneratedCountAsync();

            // Get the latest id from the imported
            var latestId = borgs.LastOrDefault()?.BorgId;
            if(latestId != null)
            {
                for (int i = latestId.Value + 1; i <= newestId; i++)
                {
                    missingBorgs.Add(i);
                }
            }

            // Return missing Borgs
            return missingBorgs;
        }
    }
}
