using BorgLink.Ethereum;
using BorgLink.Extensions;
using BorgLink.Mapping;
using BorgLink.Models.Options;
using BorgLink.Repositories;
using BorgLink.Services;
using BorgLink.Services.Ethereum;
using BorgLink.Services.Middleware;
using BorgLink.Services.Platform;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using BorgLink.Services.Storage.Interfaces;
using BorgLink.Services.Storage;
using BorgLink.Utils;
using BorgLink.Context.Contexts;
using Newtonsoft.Json;
using Bazinga.AspNetCore.Authentication.Basic;

namespace BorgLink
{
    /// <summary>
    /// The startup class for the web application
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// The applications configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The configuration for the application</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">THe services to configure</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Setup controller
            services.AddControllers()
                .ConfigureApiBehaviorOptions(options => {
                    options.ClientErrorMapping[404].Link = "https://httpstatuses.com/404";
                })
                .AddNewtonsoftJson(opts =>
                {
                    opts.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                    opts.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                })
                .AddJsonOptions(opt => { opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

            // Add basic authentication for me to update api when failure
            services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
                .AddBasicAuthentication(credentials => Task.FromResult(credentials.username == Configuration.GetValue<string>("BasicAuthenticationOptions:UserName") &&
                credentials.password == Configuration.GetValue<string>("BasicAuthenticationOptions:Password")));

            // Setup auto-mapper
            services.AddAutoMapper(typeof(BorgsMappingProfile).Assembly);

            // Options
            services.Configure<BorgTokenServiceOptions>(options => Configuration.GetSection("BorgTokenServiceOptions").Bind(options));
            services.Configure<BorgServiceOptions>(options => Configuration.GetSection("BorgServiceOptions").Bind(options));
            services.Configure<TwitterServiceOptions>(options => Configuration.GetSection("TwitterServiceOptions").Bind(options));
            services.Configure<BorgEventListenerOptions>(options => Configuration.GetSection("BorgEventListenerOptions").Bind(options));
            services.Configure<BorgEventSyncOptions>(options => Configuration.GetSection("BorgEventSyncOptions").Bind(options));
            services.Configure<StorageOptions>(options => Configuration.GetSection("StorageOptions").Bind(options));
            services.Configure<WebhookServiceOptions>(options => Configuration.GetSection("WebhookServiceOptions").Bind(options));
            services.Configure<UploadResolutionOptions>(options => Configuration.GetSection("UploadResolutionOptions").Bind(options));

            //FileUtility.GenerateAzureAppSettings();

            // Add http client
            services.AddHttpClient();

            // Added in memory cache 
            services.AddMemoryCache();

            // Added runtime service collection accessor from context 
            services.AddHttpContextAccessor();

            // For rate limiting
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimit"));
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            // Register webhook service
            services.AddHttpClient("WebhookService", (HttpClient client) => { client.BaseAddress = new Uri(Configuration.GetValue<string>("WebhookServiceOptions:Endpoint")); });

            // DI
            services.AddTransient<BorgRepository>();
            services.AddTransient<AttributeRepository>();
            services.AddTransient<BorgAttributeRepository>();

            // Add services
            services.AddTransient<IStorageService, StorageService>();
            services.AddTransient<MemoryCacheService>();
            services.AddTransient<BorgTokenService>();
            services.AddTransient<TwitterService>();
            services.AddTransient<BorgService>();
            services.AddTransient<WebhookService>(s => new WebhookService(s.GetService<IHttpClientFactory>().CreateClient("WebhookService")));

            // Hosted services
            services.AddHostedService<BorgEventListenerService<GeneratedBorgEventDTO>>();
            services.AddHostedService<BorgEventListenerService<BredBorgEventDTO>>();
            services.AddHostedService<BorgEventSyncService>();

            // Setup database contexts
            services.SetContext<BorgContext>(Configuration.GetValue<string>("AppConnectionString"));

            // Add swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BorgLink", Version = "v1" });
            });

        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application to configure</param>
        /// <param name="env">The runtime environment</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Use swagger
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BorgLink v1"));

            // Rate limit ips
            app.UseIpRateLimiting();

            // Use route[]
            app.UseRouting();

            // Use authorization
            app.UseAuthorization();

            // Use authentication
            app.UseAuthentication();

            // Use error hanling middleware
            app.UseMiddleware<ErrorHandlingMiddleware>();

            // Stop options making real call
            app.UseMiddleware<CircumventRequestForOptionsMiddleware>();

            // Map endpoints to controller names
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
