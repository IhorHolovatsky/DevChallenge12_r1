using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CDN.Domain.Models;
using CDN.OriginServer.Api.Models;

namespace CDN.OriginServer.Api.Services
{
    public interface IObjectStorageProvider
    {
        Task<IEnumerable<CdnFileObject>> GetObjectsMetaDataAsync();
        
        Task<Stream> DownloadObjectAsync(string objectKey, int version = 0);

        Task<CdnFileObject> UploadObjectAsync(string objectKey,
                                              Stream stream,
                                              int version = 0);

        Task<MultiPartObjectData> DownloadObjectPartAsync(string objectKey, int start, long end, int version = 0);

        #region Multipart upload

        /// <summary>
        /// Start multipart upload for given objectKey,
        /// should throw error if objectKey already exists
        /// </summary>
        /// <returns>ObjectModel with uploadId</returns>
        Task<CdnFileObject> InitMultiPartUploadAsync(string objectKey, int version = 0);

        /// <summary>
        /// Upload part of file
        /// </summary>

        Task UploadChunkAsync(string objectKey,
                              string uploadId,
                              int partNumber,
                              Stream stream,
                              int version = 0);

        /// <summary>
        /// Complete multipart upload of file.
        /// Merge all chunks and enable access for file
        /// </summary>
        Task CompleteMultipartUploadAsync(string objectKey, string uploadId, int version = 0);

        /// <summary>
        /// Abort multipart upload, delete all uploaded chunks, and free up objectKey
        /// </summary>
        Task AbortMultipartUploadAsync(string objectKey, string uploadId, int version = 0);

        #endregion
    }
}