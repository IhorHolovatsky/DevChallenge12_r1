using System.Collections.Generic;
using System.Threading.Tasks;
using CDN.Domain.Models;

namespace CDN.Domain.Repositories
{
    public interface ICdnServerRepository
    {
        CdnServer GetServerById(int serverId);
        void InsertOrUpdateServer(CdnServer server);

        /// <summary>
        /// Get server related to specified role.
        /// If not cpecified, all servers will be returned
        /// </summary>
        Task<IEnumerable<CdnServer>> GetServersAsync(CdnServerRole? serverRole);
    }
}