using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CDN.Domain.Models;

namespace CDN.Domain.Repositories
{
    public interface IFileObjectRepository
    {
        /// <summary>
        /// Get all objects in system, if time provided get obects with lastAccess time less than provided
        /// </summary>
        Task<IEnumerable<CdnFileObject>> GetObjectsAsync(int? serverId = null, DateTime? lastAccessTime = null);

        /// <summary>
        /// Get object by main parameters
        /// </summary>
        /// <param name="objectKey">The object key. Required</param>
        /// <param name="version">The version of object. Optional</param>
        /// <param name="serverId">Server where object located.</param>
        /// <returns></returns>
        Task<CdnFileObject> GetObjectByKeyAsync(string objectKey, 
                                                int? version = null, 
                                                int? serverId = null);

        Task AddObjectAsync(CdnFileObject fileObject);

        Task<CdnFileObject> UpdateObjectAsync(CdnFileObject fileObject);

        Task DeleteObjectAsync(CdnFileObject fileObject);
    }
}