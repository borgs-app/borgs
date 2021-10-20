using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using BorgLink.Models.Enums;
using BorgLink.Models.Options;
using BorgLink.Services.Storage.Interfaces;
using BorgLink.Utils;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorgLink.Services.Storage
{
    /// <summary>
    /// For communication/fileshare between borgs and Azure storage
    /// </summary>
    public class StorageService : IStorageService
    {
        private readonly StorageOptions _storageOptions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="storageOptions"></param>
        public StorageService(IOptions<StorageOptions> storageOptions)
        {
            _storageOptions = storageOptions.Value;
        }

        /// <summary>
        /// Upload a blob to the store
        /// </summary>
        /// <param name="blobName">The name to save file as</param>
        /// <param name="blob">The blob to store</param>
        /// <param name="type">The type of data the blob is</param>
        /// <returns>THe upload information</returns>
        public async Task<BlobContentInfo> UploadBlobAsync(string blobName, string blob, AssetType type, ResolutionContainer resolutionContainer)
        {
            BlobContentInfo info = null;

            // Get memory stream
            using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(blob)))
            {
                // Get connection to client
                var client = GetBlobConnectionClient(type, resolutionContainer);

                // Upload the stream
                info = await client.UploadBlobAsync(blobName, ms);
            }

            // Return the upload info
            return info;
        }

        /// <summary>
        /// Get a file stream from Azure storage. Careful as this returns a raw stream unmanaged
        /// </summary>
        /// <param name="fileName">The file to retrieve</param>
        /// <param name="containerName">The container</param>
        /// <returns>A file stream</returns>
        public async Task<Stream> GetFileStream(string containerName, string fileName)
        {
            // Build cloud client
            var blobServiceClient = GetCloudStorageClient();

            // Create the stream to return
            var memoryStream = new MemoryStream();

            // Create the container and return a container client object
            var container = blobServiceClient.GetContainerReference(containerName);
            var containerExists = await container.ExistsAsync();
            if (containerExists)
            {
                // Get container
                var blobReference = container.GetBlobReference(fileName);

                // Copy to memory stream
                var exists = await blobReference.ExistsAsync();
                if (exists)
                    await blobReference.DownloadToStreamAsync(memoryStream);
            }

            // Return stream
            memoryStream.Position = 0;
            return memoryStream;
        }

        /// <summary>
        /// Upload a blob to the store
        /// </summary>
        /// <param name="stream">The stream to upload</param>
        /// <param name="etag">The file hash</param>
        /// <param name="type">The type of data the blob is</param>
        /// <param name="resolutionContainer">The resolution folder image is being saved to (sub folder)</param>
        /// <returns>The upload information</returns>
        public async Task<BlobContentInfo> UploadBlobAsync(MemoryStream stream, string etag, AssetType type, ResolutionContainer resolutionContainer)
        {
            // Get memory stream
            using (stream)
            {
                // Reset to start of stream
                stream.Position = 0;

                // Get connection to client
                var client = GetBlobConnectionClient(type, resolutionContainer);

                // Upload the stream
                return await client.UploadBlobAsync(etag, stream);
            }
        }

        /// <summary>
        /// Delete a blob from storage
        /// </summary>
        /// <param name="etag">The name of the file (etag)</param>
        /// <param name="assetType"></param>
        /// <returns></returns>
        public async Task<bool> DeleteBlobAsync(string etag, AssetType assetType, ResolutionContainer resolutionContainer)
        {
            // Get connection to client
            var client = GetBlobConnectionClient(assetType, resolutionContainer);

            // Delete
            return await client.DeleteBlobIfExistsAsync(etag);
        }

        /// <summary>
        /// Get Sas uri
        /// </summary>
        /// <param name="type">The container client</param>
        /// <param name="storedPolicyName">Policy</param>
        /// <returns>Uri</returns>
        public Uri GetServiceSasUriForContainer(AssetType type, ResolutionContainer resolutionContainer, string storedPolicyName = null)
        {
            var containerClient = GetSharedBlobConnectionClient(type, resolutionContainer);

            // Check whether this BlobContainerClient object has been authorized with Shared Key.
            if (containerClient.CanGenerateSasUri)
            {
                // Create a SAS token that's valid for one hour.
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = containerClient.Name,
                    Resource = "c"
                };

                if (storedPolicyName == null)
                {
                    var sasExpiry = _storageOptions.SasExpiry.ParseFrequencyConfig(TimeUnit.Hours);

                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(sasExpiry);
                    sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);
                }
                else
                {
                    sasBuilder.Identifier = storedPolicyName;
                }

                // Generate a SaS url
                return containerClient.GenerateSasUri(sasBuilder);
            }
            else
            { 
                return null;
            }
        }

        #region Helpers

             /// <summary>
             /// Gets a cloud storage client
             /// </summary>
             /// <returns>A cloud service client</returns>
        private BlobContainerClient GetSharedBlobConnectionClient(AssetType type, ResolutionContainer resolutionContainer)
        {
            // Get the access key
            var storageKey = _storageOptions.AccessKeys.FirstOrDefault();

            // Create credentials
            var credentials = new StorageSharedKeyCredential(_storageOptions.Name, storageKey.Key);

            // Build uri
            var uri = $"{_storageOptions.Url}/{GetContainerName(resolutionContainer)}";

            // Return new client
            return new BlobContainerClient(new Uri(uri), credentials);
        }

        /// <summary>
        /// Gets a cloud storage client
        /// </summary>
        /// <returns>A cloud service client</returns>
        private CloudBlobClient GetCloudStorageClient()
        {
            // Get the access key
            var storageKey = _storageOptions.AccessKeys.FirstOrDefault();

            // Create credentials
            var credentials = new StorageCredentials(_storageOptions.Name, storageKey.Key);

            // Create connection
            CloudStorageAccount account = new CloudStorageAccount(credentials, false);

            // Return new client
            return new CloudBlobClient(new Uri(_storageOptions.Url), account.Credentials);
        }

        /// <summary>
        /// Gets a blob storage connection
        /// </summary>
        /// <param name="type">The file type to upload/download</param>
        /// <param name="resolutionContainer">The resolution folder image is being saved to (sub folder)</param>
        /// <returns></returns>
        private BlobContainerClient GetBlobConnectionClient(AssetType type, ResolutionContainer resolutionContainer)
        {
            // Get the access key
            var key = _storageOptions.AccessKeys.FirstOrDefault();

            // Create a BlobServiceClient object which will be used to create a container client
            var blobServiceClient = new BlobServiceClient(key.ConnectionString);

            // Get the upload container
            var container = GetContainerName(resolutionContainer);

            // Create the container and return a container client object
            return blobServiceClient.GetBlobContainerClient(container);
        }

        /// <summary>
        /// Gets container name for file type
        /// </summary>
        /// <param name="resolutionContainer">The size of file to store</param>
        /// <returns>Container name</returns>
        public string GetContainerName(ResolutionContainer? resolutionContainer)
        {
            // Define mode to run in
            var type = _storageOptions.TestMode ? AssetType.Test : AssetType.Live;

            // Define the place holder
            var placeHolder = "{0}";

            // Get container for resolution
            return $"{type}-{resolutionContainer?.ToString() ?? placeHolder}".ToLower();
        }

        #endregion
    }
}
