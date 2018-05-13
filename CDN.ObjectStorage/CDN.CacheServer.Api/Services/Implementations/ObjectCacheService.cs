using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CDN.CacheServer.Api.Models;
using CDN.Domain.Configuration;
using CDN.Domain.Constants;
using CDN.Domain.Exceptions;
using CDN.Domain.Models;
using CDN.Domain.Repositories;
using CDN.Domain.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace CDN.CacheServer.Api.Services.Implementations
{
    public class ObjectCacheService : IObjectCacheService
    {
        private string BaseCacheFolder => $@"{_hostingEnvironment.ContentRootPath}\{_storageOptions.BaseFolder}";

        private readonly ICdnServerService _cdnServerService;
        private readonly IFileObjectRepository _fileObjectRepository;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly StorageOptions _storageOptions;
        private readonly CdnOptions _cdnOptions;

        public ObjectCacheService(ICdnServerService cdnServerService,
                                  IFileObjectRepository fileObjectRepository,
                                  IHostingEnvironment hostingEnvironment,
                                  IConfiguration configuration)
        {
            _cdnServerService = cdnServerService ?? throw new ArgumentNullException(nameof(cdnServerService));
            _fileObjectRepository = fileObjectRepository ?? throw new ArgumentNullException(nameof(fileObjectRepository));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));

            _storageOptions = configuration?.GetSection(ConfigurationConstants.STORAGE_SECTION_NAME).Get<StorageOptions>() ?? throw new ArgumentNullException(nameof(configuration));
            _cdnOptions = configuration?.GetSection(ConfigurationConstants.CDN_SECTION_NAME).Get<CdnOptions>() ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<CachedObjectResult> GetObjectFromCache(CdnFileObject fileObject)
        {
            var fileName = $"{BaseCacheFolder}\\{fileObject.Id}.{fileObject.VersionId}";
            var result = new CachedObjectResult();

            //Get fileObject in current server
            var currentServerfileObject = await _fileObjectRepository.GetObjectByKeyAsync(fileObject.Id, 0, _cdnOptions.Server.Id);
            if (currentServerfileObject != null)
            {
                currentServerfileObject.LastAccess = DateTime.Now;
                await _fileObjectRepository.UpdateObjectAsync(currentServerfileObject);

                result.IsSuccess = true;
                result.FileStream = File.OpenRead(fileName);
                return result;
            }

            //if object was not previously cached  and no space for downloading...
            if (Startup.CdnServer.FreeSpace < fileObject.Size)
            {
                result.IsSuccess = false;
                result.Exception = new NoFreeSpaceException();
                return result;
            }

            //Get nearest origin server
            var server = await _cdnServerService.GetNearestServerAsync(_cdnOptions.Server.Longitude,
                                                                       _cdnOptions.Server.Latitude,
                                                                       CdnServerRole.OriginServer);

            var fileUrl = $@"{server.Host.Trim()}/api/download/{fileObject.Id}/{fileObject.VersionId}";

            //Download file object and save it locally...
            using (var client = new HttpClient())
            {
                //Hard code auth token...
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "49ec97b507cc21806861817f2f0564d4");
                using (var response = await client.GetAsync(fileUrl))
                using (var fileStream = File.Create(fileName))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
            }

            fileObject.ServerId = _cdnOptions.Server.Id;
            fileObject.LastAccess = DateTime.Now;

            await _fileObjectRepository.AddObjectAsync(fileObject);

            result.IsSuccess = true;
            result.FileStream = File.OpenRead(fileName);
            return result;
        }
    }
}