using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CDN.Domain.Constants;
using CDN.Domain.Models;
using CDN.Domain.Repositories;
using CDN.Domain.Repositories.Implementations;
using CDN.Domain.Services;
using CDN.Domain.Services.Implementations;
using CDN.RouteServer.Api.Services;
using CDN.RouteServer.Api.Services.Implementation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CDN.RouteServer.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddRouting();
            services.AddScoped<ICdnServerRepository, CdnServerRepository>();
            services.AddSingleton<ICdnServerService, CdnServerService>();
            services.AddScoped<ICdnRouter, CdnRouter>();

            var serviceProvider = services.BuildServiceProvider();
            OnStartup(serviceProvider);

            //To force '.' in doubles
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");

            return serviceProvider;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "All resources",
                    "{*url}",
                    new { controller = "Route", action = "ChooseServer" });
            });
        }


        public void OnStartup(IServiceProvider serviceProvider)
        {
            //Get Server settings
            var server = Configuration.GetSection(ConfigurationConstants.SERVER_SECTION_NAME).Get<CdnServer>();
            server.IsOnline = true;

            //Register server in network
            var cdnServerRepository = serviceProvider.GetService<ICdnServerRepository>();
            cdnServerRepository.InsertOrUpdateServer(server);
        }
    }
}
