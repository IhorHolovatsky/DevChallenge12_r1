using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CDN.Domain.Configuration;
using CDN.Domain.Constants;
using CDN.Domain.Models;
using CDN.Domain.Repositories;
using CDN.Domain.Repositories.Implementations;
using CDN.OriginServer.Api.Filters;
using CDN.OriginServer.Api.Services;
using CDN.OriginServer.Api.Services.Implementations;
using CDN.OriginServer.Api.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;

namespace CDN.OriginServer.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostEnvironment { get; }
        public string ApiName { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            HostEnvironment = env;
            Configuration = configuration;
            ApiName = "CDN Object Storage Api";
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(HostEnvironment.ContentRootFileProvider);
            services.AddScoped<IObjectStorageProvider, LocaFileSystemStorageProvider>();
            services.AddScoped<ICdnServerRepository, CdnServerRepository>();
            services.AddScoped<IFileObjectRepository, FileObjectRepository>();
            services.AddScoped<TokenAuthorizeAttribute>();

            services.AddMvc(opt => { opt.Filters.Add<GlobalExceptionFilter>(); })
                    .AddJsonOptions(opt => opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore);

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme()
                {
                    Description = "JExample: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });

                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                    {"Basic", new string[] { }}
                });

                c.SwaggerDoc("v1",
                    new Info
                    {
                        Title = ApiName,
                        Version = "v1"
                    });
                c.OperationFilter<FileUploadOperation>(); //Register File Upload Operation Filter
            });

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

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(opt =>
            {
                opt.SwaggerEndpoint("../swagger/v1/swagger.json", ApiName);
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
