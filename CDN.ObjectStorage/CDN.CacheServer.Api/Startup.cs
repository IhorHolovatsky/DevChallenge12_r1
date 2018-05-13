using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CDN.CacheServer.Api.Services;
using CDN.CacheServer.Api.Services.Implementations;
using CDN.Domain.Configuration;
using CDN.Domain.Constants;
using CDN.Domain.Models;
using CDN.Domain.Repositories;
using CDN.Domain.Repositories.Implementations;
using CDN.Domain.Services;
using CDN.Domain.Services.Implementations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CDN.CacheServer.Api
{
    public class Startup
    {
        /// <summary>
        /// It's really bad solution, but I want to end this task faster
        /// </summary>
        public static CdnServer CdnServer { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddScoped<ICdnServerRepository, CdnServerRepository>();
            services.AddScoped<IFileObjectRepository, FileObjectRepository>();
            services.AddScoped<IObjectCacheService, ObjectCacheService>();
            services.AddSingleton<IHostedService, CleanUpService>();
            services.AddSingleton<ICdnServerService, CdnServerService>();
            
            //To Enable strongly typed settings
            services.AddOptions();

            //Register options
            services.Configure<StorageOptions>(Configuration.GetSection(ConfigurationConstants.STORAGE_SECTION_NAME));
            services.Configure<CdnOptions>(Configuration.GetSection(ConfigurationConstants.CDN_SECTION_NAME));


            var serviceProvider = services.BuildServiceProvider();
            OnStartup(serviceProvider);

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
                    new { controller = "Object", action = "LoadObject" });
            });
        }

        public void OnStartup(IServiceProvider serviceProvider)
        {
            //Get Server settings
            CdnServer = Configuration.GetSection(ConfigurationConstants.SERVER_SECTION_NAME).Get<CdnServer>();
            CdnServer.IsOnline = true;
            
            //Register server in network
            var cdnServerRepository = serviceProvider.GetService<ICdnServerRepository>();
            cdnServerRepository.InsertOrUpdateServer(CdnServer);
        }
    }
}
