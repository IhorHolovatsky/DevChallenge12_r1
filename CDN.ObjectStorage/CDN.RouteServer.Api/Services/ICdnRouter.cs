using System.Threading.Tasks;
using CDN.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace CDN.RouteServer.Api.Services
{
    public interface ICdnRouter
    {
        Task<CdnServer> SelectNearestServerAsync(ConnectionInfo connection);
        Task<CdnServer> SelectNearestServerAsync(double longitude, double latitude);
    }
}