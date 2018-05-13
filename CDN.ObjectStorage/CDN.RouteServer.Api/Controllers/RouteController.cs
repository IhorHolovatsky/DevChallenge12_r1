using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CDN.RouteServer.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CDN.RouteServer.Api.Controllers
{
    [Produces("application/json")]
    public class RouteController : Controller
    {
        private readonly ICdnRouter _router;
        public RouteController(ICdnRouter router)
        {
            _router = router ?? throw new ArgumentNullException(nameof(router));
        }

        public async Task<IActionResult> ChooseServer([FromQuery(Name = "lg")]double longitude,
                                                      [FromQuery(Name = "lt")]double latitude)
        {
#if DEBUG
            //For local testing, we should pass location via query string (to emulate users)
            var server = await _router.SelectNearestServerAsync(longitude, latitude);
#else
            //Get user location from it's IP address
            var server = await _router.SelectNearestServerAsync(HttpContext.Connection);
#endif

            return Redirect($"{server.Host.Trim()}{Request.Path}?lg={longitude}&lt={latitude}");
        }
    }
}