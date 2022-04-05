using Azure.Storage.Blobs.Models;
using BorgLink.Models.Enums;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BorgLink.Services.Storage.Interfaces
{
    /// <summary>
    /// To manage BLOBs
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Gets a file stream
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <param name="containerName">The container</param>
        /// <returns>A file stream</returns>
        Task<Stream> GetFileStream(string containerName, string fileName);

        /// <summary>
        /// Gets container name for file type
        /// </summary>
        /// <param name="resolutionContainer">The size of file to store</param>
        /// <returns>Container name</returns>
        public string GetContainerName(ResolutionContainer? resolutionContainer);

        /// <summary>
        /// Get SasUri
        /// </summary>
        /// <param name="type">The container client</param>
        /// <param name="resolutionContainer">The file size</param>
        /// <param name="storedPolicyName">Policy</param>
        /// <returns>Uri</returns>
        public Uri GetServiceSasUriForContainer(ResolutionContainer resolutionContainer, string storedPolicyName = null);

        /// <summary>
        /// Delete a blob from storage
        /// </summary>
        /// <param name="etag">The name of the file (etag)</param>
        /// <returns></returns>
        Task<bool> DeleteBlobAsync(string etag, ResolutionContainer resolutionContainer);

        /// <summary>
        /// Upload a blob to the store
        /// </summary>
        /// <param name="stream">The stream to upload</param>
        /// <param name="name">The fileName to store</param>
        /// <param name="type">The type of data the blob is</param>
        /// <param name="resolutionContainer">The resolution folder image is being saved to (sub folder)</param>
        /// <returns>The upload information</returns>
        Task<BlobContentInfo> UploadBlobAsync(MemoryStream stream, string name, ResolutionContainer resolutionContainer);

        /// <summary>
        /// Get borg
        /// </summary>
        /// <param name="borgId">The borg to get</param>
        /// <param name="container">The container the borg lives in</param>
        /// <returns>Borg</returns>
        Task<bool> DoesBorgExist(int borgId, ResolutionContainer container);
    }
}
