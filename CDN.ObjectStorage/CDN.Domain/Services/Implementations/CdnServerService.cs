using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CDN.Domain.Exceptions;
using CDN.Domain.Models;
using CDN.Domain.Repositories;
using GeoCoordinatePortable;

namespace CDN.Domain.Services.Implementations
{
    public class CdnServerService : ICdnServerService
    {
        private readonly ICdnServerRepository _cdnServerRepository;

        public CdnServerService(ICdnServerRepository cdnServerRepository)
        {
            _cdnServerRepository = cdnServerRepository ?? throw new ArgumentNullException(nameof(cdnServerRepository));
        }

        /// <inheritdoc />
        public async Task<CdnServer> GetNearestServerAsync(double longitude, 
                                                           double latitude, 
                                                           CdnServerRole serverRole, 
                                                           List<int> excludeServers = null)
        {
            var clientCoordinate = new GeoCoordinate(latitude, longitude);

            var servers = (await _cdnServerRepository.GetServersAsync(serverRole))
                .Where(s => excludeServers == null || !excludeServers.Contains(s.Id))
                .ToList();

            if (!servers?.Any() ?? true)
                throw new NoSuitableServerExcetion($"No found any servers in role '{serverRole}'");

            //Calculate distances and return nearest server
            return servers.Select(s => new
                                  {
                                      Server = s,
                                      Distance = new GeoCoordinate(s.Latitude, s.Longitude).GetDistanceTo(clientCoordinate)
                                  })
                          .OrderBy(s => s.Distance)
                          .Select(s => s.Server)
                          .First();
        }
    }
}