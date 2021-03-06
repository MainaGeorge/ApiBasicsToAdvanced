using System;
using System.Collections.Generic;
using System.IO;
using AspNetCoreRateLimit;
using CompanyEmployees.Extensions;
using Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NLog;

namespace CompanyEmployees
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            LogManager.LoadConfiguration(Path.Combine(Directory.GetCurrentDirectory(), "nlog.config"));
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.ConfigureControllers();
            services.ConfigureApiBehaviour();
            services.ConfigureCors();
            services.AddCustomActionFilters();
            services.ConfigureIisIntegration();
            services.ConfigureLoggerService();
            services.ConfigureSqlContext(Configuration);
            services.AddCustomServicesToTheDependencyInjectionContainer();
            services.AddAutoMapper(typeof(Startup));
            services.AddCustomMediaTypes();
            services.ConfigureApiVersioning();
            services.ConfigureResponseCaching();
            services.ConfigureHttpCacheHeaders();
            services.AddMemoryCache();
            services.ConfigureRateLimitingOptions();
            services.AddHttpContextAccessor();
            services.AddAuthentication();
            services.ConfigureIdentity();
            services.ConfigureJWT(Configuration);
            services.ConfigureSwagger();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerManager logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CompanyEmployee v1"));
            }
            app.ConfigureExceptionHandler(logger);
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseResponseCaching();
            app.UseHttpCacheHeaders();
            app.UseIpRateLimiting();
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All });
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
