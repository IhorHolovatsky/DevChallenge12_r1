using System.Collections.Generic;
using System.Threading.Tasks;
using CDN.Domain.Models;

namespace CDN.Domain.Services
{
    public interface ICdnServerService
    {
        /// <summary>
        /// Get the most nearest server with role to given location
        /// </summary>
        Task<CdnServer> GetNearestServerAsync(double longitude, 
                                              double latitude, 
                                              CdnServerRole serverRole, 
                                              List<int> excludeServers = null);
    }
}