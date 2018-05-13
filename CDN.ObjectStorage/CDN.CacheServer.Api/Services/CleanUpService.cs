using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CDN.Domain.Configuration;
using CDN.Domain.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace CDN.CacheServer.Api.Services
{
    public class CleanUpService : BackGroundServiceBase
    {
        private string BaseCacheFolder => $@"{_hostingEnvironment.ContentRootPath}\{_storageOptions.BaseFolder}";

        private readonly IFileObjectRepository _fileObjectRepository;
        private readonly ICdnServerRepository _cdnServerRepository;
        private readonly StorageOptions _storageOptions;
        private readonly CdnOptions _cdnOptions;
        private readonly IHostingEnvironment _hostingEnvironment;

        public CleanUpService(IFileObjectRepository fileObjectRepository,
            ICdnServerRepository cdnServerRepository,
            IOptions<StorageOptions> storageOptions,
            IOptions<CdnOptions> cdnOptions,
            IHostingEnvironment hostingEnvironment)
        {
            _fileObjectRepository = fileObjectRepository ?? throw new ArgumentNullException(nameof(fileObjectRepository));
            _cdnServerRepository = cdnServerRepository ?? throw new ArgumentNullException(nameof(cdnServerRepository));
            _cdnOptions = cdnOptions?.Value ?? throw new ArgumentNullException(nameof(cdnOptions));
            _storageOptions = storageOptions?.Value ?? throw new ArgumentNullException(nameof(storageOptions));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var driveLetter = Path.GetPathRoot(_hostingEnvironment.ContentRootPath);
                var currentDrive = DriveInfo.GetDrives().First(d => d.Name.Equals(driveLetter));

                var freeSpaceInPercents = 100 * (double)currentDrive.AvailableFreeSpace / currentDrive.TotalSize;

                if (freeSpaceInPercents < _storageOptions.CleanUpWhenAvailMemoryLessThanPercents)
                {
                    await CleanUpOldObjects();
                }

                //Update server free space
                Startup.CdnServer.FreeSpace = currentDrive.AvailableFreeSpace;
                _cdnServerRepository.InsertOrUpdateServer(Startup.CdnServer);

                await Task.Delay(_storageOptions.CleanUpCheckInterval * 1000, stoppingToken);
            }
        }

        private async Task CleanUpOldObjects()
        {
            //Select object which are not popular
            var oldObjects = await _fileObjectRepository.GetObjectsAsync(_cdnOptions.Server.Id,
                                                                         DateTime.Now.AddDays(-_storageOptions.CleanUpObjectAgeInDays));

            var tasks = new List<Task>();
            
            foreach (var o in oldObjects)
            {
                tasks.Add(Task.Factory.StartNew(async () =>
                {
                    await _fileObjectRepository.DeleteObjectAsync(o);
                    File.Delete($@"{BaseCacheFolder}\{o.Id}");
                }));    
            }


            //Delete objects in parralel
            Task.WaitAll(tasks.ToArray());
        }
    }
}