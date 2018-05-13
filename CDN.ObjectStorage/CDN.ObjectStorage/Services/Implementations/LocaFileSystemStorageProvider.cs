using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CDN.Domain.Configuration;
using CDN.Domain.Exceptions;
using CDN.Domain.Models;
using CDN.Domain.Repositories;
using CDN.Domain.Utils;
using CDN.OriginServer.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace CDN.OriginServer.Api.Services.Implementations
{
    public class LocaFileSystemStorageProvider : IObjectStorageProvider
    {
        private readonly IFileObjectRepository _fileObjectRepository;
        private readonly ICdnServerRepository _cdnServerRepository;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly StorageOptions _storageOptions;
        private readonly CdnOptions _cdnOptions;

        private string BaseUploadFolder => $@"{_hostingEnvironment.ContentRootPath}\{_storageOptions.BaseFolder}";

        public LocaFileSystemStorageProvider(IHostingEnvironment hostingEnvironment,
                                             IFileObjectRepository fileObjectRepository,
                                             ICdnServerRepository cdnServerRepository,
                                             IOptions<StorageOptions> storageOptions,
                                             IOptions<CdnOptions> cdnOptions)
        {
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _fileObjectRepository = fileObjectRepository ?? throw new ArgumentNullException(nameof(fileObjectRepository));
            _cdnServerRepository = cdnServerRepository ?? throw new ArgumentNullException(nameof(cdnServerRepository));
            _storageOptions = storageOptions?.Value ?? throw new ArgumentNullException(nameof(storageOptions));
            _cdnOptions = cdnOptions?.Value ?? throw new ArgumentNullException(nameof(cdnOptions));
        }
        
        public Task<IEnumerable<CdnFileObject>> GetObjectsMetaDataAsync()
        {
            return _fileObjectRepository.GetObjectsAsync();
        }

        #region Multipart download

        public async Task<MultiPartObjectData> DownloadObjectPartAsync(string objectKey, int start, long end, int version = 0)
        {
            await ValidateRequest(objectKey, version);
            var model = new MultiPartObjectData
            {
                Stream = Stream.Null
            };

            using (var file = File.OpenRead($@"{BaseUploadFolder}\{objectKey}.{version}"))
            {
                model.FileLength = file.Length;

                if (start > file.Length) return model;
                if (end > file.Length)
                    end = file.Length;

                model.Start = start;
                model.End = end;

                //Seek to 'start' byte
                file.Seek(start, SeekOrigin.Current);

                var memoryStream = new MemoryStream();

                var bytesRead = 0;
                var buffer = new byte[81920];

                while (end > start + bytesRead)
                {
                    var bufferLength = end - (start + bytesRead) > buffer.Length
                                                ? buffer.Length
                                                : (int)end - (start + bytesRead);

                    bytesRead += file.Read(buffer, 0, bufferLength);

                    memoryStream.Write(buffer, 0, bufferLength);
                }

                //Reset position of stream
                memoryStream.Seek(0, SeekOrigin.Begin);

                model.Stream = memoryStream;
                return model;
            }
        }

        #endregion

        #region Multipart upload

        /// <inheritdoc />
        public async Task<CdnFileObject> InitMultiPartUploadAsync(string objectKey, int version = 0)
        {
            var fileObject = await _fileObjectRepository.GetObjectByKeyAsync(objectKey, version);
            if (fileObject != null)
            {
                throw new ObjectKeyAlreadyExistsException($"Object with key '{objectKey}' and version '{version}' already exists.");
            }

            var uploadId = Guid.NewGuid();

            fileObject = new CdnFileObject
            {
                Id = objectKey,
                ServerId = _cdnOptions.Server.Id,
                UploadId = uploadId,
                VersionId = version
            };

            await _fileObjectRepository.AddObjectAsync(fileObject);

            //Create directory for chunks
            Directory.CreateDirectory(GetChunkFolder(objectKey, uploadId.ToString()));

            return fileObject;
        }

        /// <inheritdoc />
        public async Task UploadChunkAsync(string objectKey,
                                           string uploadId,
                                           int partNumber,
                                           Stream stream,
                                           int version = 0)
        {
            await ValidateMultiPartRequest(objectKey, uploadId, version);

            var chunkFileName = $@"{GetChunkFolder(objectKey, uploadId)}\{partNumber}";

            if (File.Exists(chunkFileName))
            {
                throw new ObjectChunkAlreadyExistsException($"Chunk {partNumber} already exists for Object key '{objectKey}'");
            }

            //Save chunk locally
            using (var chunkFileStream = File.Create(chunkFileName))
            {
                stream.CopyStream(chunkFileStream);
            }
        }

        /// <inheritdoc />
        public async Task CompleteMultipartUploadAsync(string objectKey, string uploadId, int version = 0)
        {
            var fileObject = await ValidateMultiPartRequest(objectKey, uploadId, version);

            var chunkFilePaths = Directory.GetFiles(GetChunkFolder(objectKey, uploadId)).ToList();

            var outputObjectPath = $@"{BaseUploadFolder}\{objectKey}.{version}";
            FileUtils.MergeFiles(outputObjectPath, chunkFilePaths);

            //TODO: use parallel merging algorythm
            //var mergedFile = FileUtils.MergeFilesRecursively(chunkFilePaths);
            //File.Move(mergedFile, outputObjectPath);
            
            //Delete chunks folder
            Directory.Delete(GetChunkFolder(objectKey, uploadId), true);

            //Set properties
            fileObject.UploadId = null;
            fileObject.DateUploaded = DateTime.Now;
            fileObject.Size = new FileInfo(outputObjectPath).Length;

            await _fileObjectRepository.UpdateObjectAsync(fileObject);
        }

        /// <inheritdoc />
        public async Task AbortMultipartUploadAsync(string objectKey, string uploadId, int version)
        {
            var fileObject = await ValidateMultiPartRequest(objectKey, uploadId, version);

            //remove all chunks
            Directory.Delete(GetChunkFolder(objectKey, uploadId), true);

            //Remove object from registry
            await _fileObjectRepository.DeleteObjectAsync(fileObject);
        }

        #endregion

        #region Upload/Download

        public async Task<Stream> DownloadObjectAsync(string objectKey, int version)
        {
            var fileObject = await ValidateRequest(objectKey, version);

            fileObject.LastAccess = DateTime.Now;
            await _fileObjectRepository.UpdateObjectAsync(fileObject);

            return File.OpenRead($@"{BaseUploadFolder}\{objectKey}.{version}");
        }

        public async Task<CdnFileObject> UploadObjectAsync(string objectKey, Stream stream, int version)
        {
            var fileObject = await _fileObjectRepository.GetObjectByKeyAsync(objectKey, version);
            if (fileObject != null)
            {
                throw new ObjectKeyAlreadyExistsException($"Object with key '{objectKey}' and version '{version}' already exists.");
            }

            fileObject = new CdnFileObject
            {
                Id = objectKey,
                VersionId = version,
                ServerId = _cdnOptions.Server.Id
            };

            //For reserving object key
            await _fileObjectRepository.AddObjectAsync(fileObject);

            var outputObjectPath = $@"{BaseUploadFolder}\{objectKey}.{version}";
            using (var fileStream = File.OpenWrite(outputObjectPath))
            {
                await stream.CopyToAsync(fileStream);
            }

            fileObject.DateUploaded = DateTime.Now;
            fileObject.Size = new FileInfo(outputObjectPath).Length;

            await _fileObjectRepository.UpdateObjectAsync(fileObject);

            return fileObject;
        }

        #endregion

        #region Private Members

        private async Task<CdnFileObject> ValidateRequest(string objectKey, int version)
        {
            var fileObject = await _fileObjectRepository.GetObjectByKeyAsync(objectKey, version);
            if (fileObject == null)
            {
                throw new ObjectKeyNotFoundException($"Object with key '{objectKey}' and version '{version}' was not found.");
            }

            if (FileUtils.IsValidFileName(objectKey))
            {
                //throw new InvalidObjectKeyException($"Object key '{objectKey}' has invalid characters '{string.Join(" ", Path.GetInvalidFileNameChars())}'.");
            }

            return fileObject;
        }

        private async Task<CdnFileObject> ValidateMultiPartRequest(string objectKey, string uploadId, int version)
        {
            var fileObject = await ValidateRequest(objectKey, version);

            if (!fileObject.UploadId?.ToString().Equals(uploadId) ?? true)
            {
                throw new UploadIdNotFoundException($"UploadId '{uploadId}' is not associated with Object key '{objectKey}'");
            }

            return fileObject;
        }

        private string GetChunkFolder(string objectKey, string uploadId)
        {
            return $@"{BaseUploadFolder}\{uploadId}.{objectKey}";
        }
        #endregion
    }
}