using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CDN.Domain.Models;
using CDN.Domain.Repositories;
using CDN.Domain.Services;
using GeoCoordinatePortable;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace CDN.RouteServer.Api.Services.Implementation
{
    public class CdnRouter : ICdnRouter
    {
        private readonly ICdnServerRepository _cdnServerRepository;
        private readonly ICdnServerService _cdnServerService;
        private readonly IConfiguration _configuration;

        public CdnRouter(ICdnServerRepository cdnServerRepository,
            ICdnServerService cdnServerService,
            IConfiguration configuration)
        {
            _cdnServerRepository = cdnServerRepository ?? throw new ArgumentNullException(nameof(cdnServerRepository));
            _cdnServerService = cdnServerService ?? throw new ArgumentNullException(nameof(cdnServerService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<CdnServer> SelectNearestServerAsync(ConnectionInfo connection)
        {
            var apiUrl = _configuration["IPStack:ApiUrl"];
            var apiKey = _configuration["IPStack:ApiKey"];

            var url = $"{apiUrl}/{connection.RemoteIpAddress}?access_key={apiKey}";

            using (var client = new HttpClient())
            using (var result = await client.GetAsync(url))
            using (var content = result.Content)
            {
                var data = await content.ReadAsStringAsync();
                var json = JObject.Parse(data);

                return await SelectNearestServerAsync(json.GetValue("longitude").Value<double?>() ?? 0,
                                                      json.GetValue("latitude").Value<double?>() ?? 0);
            }
        }

        public Task<CdnServer> SelectNearestServerAsync(double longitude, double latitude)
        {
            return _cdnServerService.GetNearestServerAsync(longitude, latitude, CdnServerRole.CacheServer);
        }
    }
}