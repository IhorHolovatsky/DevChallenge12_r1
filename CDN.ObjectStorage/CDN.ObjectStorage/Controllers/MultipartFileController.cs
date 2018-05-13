using System;
using System.Linq;
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
    [Route("api/multipart")]
    public class MultipartFileController : Controller
    {
        private readonly IObjectStorageProvider _objectStorage;

        public MultipartFileController(IObjectStorageProvider objectStorage)
        {
            _objectStorage = objectStorage ?? throw new ArgumentNullException(nameof(objectStorage));
        }

        [HttpGet]
        [Route("{objectKey}/{version:int}")]
        public async Task<IActionResult> DownloadChunk(string objectKey, int version, int start, int end)
        {
            var model = await _objectStorage.DownloadObjectPartAsync(objectKey, start, end, version);

            //Content-Range: bytes 0-15/225
            Response.GetTypedHeaders().ContentRange = new ContentRangeHeaderValue(model.Start, model.End, model.FileLength);
            Response.ContentLength = model.Stream.Length;

            return File(model.Stream, "application/octet-stream", $"{start}-{end}.{objectKey}");
        }


        [HttpPost]
        [Route("{objectKey}/start")]
        public Task<CdnFileObject> StartMultipartUpload(string objectKey)
        {
            return _objectStorage.InitMultiPartUploadAsync(objectKey);
        }

        [HttpPut]
        [Route("{objectKey}/{version:int}/{uploadId}")]
        public Task UploadPart(string objectKey, 
                               string uploadId,
                               int partNumber,
                               IFormFile uploadedFile,
                               int version)
        {

            return _objectStorage.UploadChunkAsync(objectKey, uploadId, partNumber, uploadedFile.OpenReadStream(), version);
        }

        [HttpPost]
        [Route("{objectKey}/{version:int}/{uploadId}")]
        public Task EndMultipartUpload(string objectKey, string uploadId, int version)
        {
            return _objectStorage.CompleteMultipartUploadAsync(objectKey, uploadId, version);
        }

        [HttpDelete]
        [Route("{objectKey}/{version:int}/{uploadId}")]
        public Task AbortMultipartUpload(string objectKey, string uploadId, int version)
        {
            return _objectStorage.AbortMultipartUploadAsync(objectKey, uploadId, version);
        }
    }
}