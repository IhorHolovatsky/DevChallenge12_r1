using System.IO;
using System.Threading.Tasks;
using CDN.CacheServer.Api.Models;
using CDN.Domain.Models;

namespace CDN.CacheServer.Api.Services
{
    public interface IObjectCacheService
    {
        Task<CachedObjectResult> GetObjectFromCache(CdnFileObject fileObject);
    }
}