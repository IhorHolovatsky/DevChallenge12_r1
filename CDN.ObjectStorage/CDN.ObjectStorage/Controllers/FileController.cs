using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CDN.Domain.Models;
using CDN.OriginServer.Api.Filters;
using CDN.OriginServer.Api.Models;
using CDN.OriginServer.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace CDN.OriginServer.Api.Controllers
{
    [ServiceFilter(typeof(TokenAuthorizeAttribute))]
    [Route("api/")]
    public class FileController : Controller
    {
        private readonly IObjectStorageProvider _objectStorageProvider;

        public FileController(IObjectStorageProvider objectStorageProvider)
        {
            _objectStorageProvider = objectStorageProvider ?? throw new ArgumentNullException(nameof(objectStorageProvider));
        }

        [HttpGet]
        [Route("download/{objectKey}/{version:int}")]
        public async Task<IActionResult> DownloadObject(string objectKey, int version)
        {
            var stream = await _objectStorageProvider.DownloadObjectAsync(objectKey, version);
            return File(stream, "application/octet-stream", objectKey);
        }

        [HttpPost]
        [Route("upload/{objectKey}/{version:int}")]
        public Task<CdnFileObject> UploadObject(string objectKey, int version, IFormFile uploadedFile)
        {
            return _objectStorageProvider.UploadObjectAsync(objectKey, uploadedFile.OpenReadStream(), version);
        } 

        [HttpGet]
        [Route("files")]
        public Task<IEnumerable<CdnFileObject>> UploadObject()
        {
            return _objectStorageProvider.GetObjectsMetaDataAsync();
        }

    }
}