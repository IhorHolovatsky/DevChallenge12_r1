using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CDN.CacheServer.Api.Services;
using CDN.Domain.Configuration;
using CDN.Domain.Exceptions;
using CDN.Domain.Models;
using CDN.Domain.Repositories;
using CDN.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CDN.CacheServer.Api.Controllers
{
    public class ObjectController : Controller
    {
        private readonly IFileObjectRepository _fileObjectRepository;
        private readonly IObjectCacheService _objectCacheService;
        private readonly ICdnServerService _cdnServerService;
        private readonly CdnOptions _cdnOptions;

        public ObjectController(IFileObjectRepository fileObjectRepository,
                                IObjectCacheService objectCacheService,
                                ICdnServerService cdnServerService,
                                IOptions<CdnOptions> cdnOptions)
        {
            _fileObjectRepository = fileObjectRepository ?? throw new ArgumentNullException(nameof(fileObjectRepository));
            _objectCacheService = objectCacheService ?? throw new ArgumentNullException(nameof(objectCacheService));
            _cdnServerService = cdnServerService ?? throw new ArgumentNullException(nameof(cdnServerService));
            _cdnOptions = cdnOptions?.Value ?? throw new ArgumentNullException(nameof(_cdnOptions));
        }

        public async Task<IActionResult> LoadObject([FromQuery(Name = "lg")]double longitude,
                                                    [FromQuery(Name = "lt")]double latitude)
        {
            var objectKey = Request.Path.ToString().TrimStart('/');
            
            //Check if object exists at all
            var fileObject = await _fileObjectRepository.GetObjectByKeyAsync(objectKey);
            if (fileObject == null)
            {
                return NotFound();
            }

            var result = await _objectCacheService.GetObjectFromCache(fileObject);

            if (!result.IsSuccess)
            {
                switch (result.Exception)
                {
                    //If no space, just redirect to another nearest Cache server
                    case NoFreeSpaceException _:
                        var server = await _cdnServerService.GetNearestServerAsync(longitude, 
                                                                                   latitude, 
                                                                                   CdnServerRole.CacheServer, 
                                                                                   new List<int> { _cdnOptions.Server.Id});

                        return Redirect($"{server.Host.Trim()}{Request.Path}?lg={longitude}&lt={latitude}");
                    default:
                    //Rethrow all other errors
                        throw result.Exception;
                }
            }

            return File(result.FileStream, "application/octet-stream", objectKey);
        }
    }
}