using System.Linq;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.CustomOutputFormatters;
using Contracts;
using Entities;
using LoggingService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository;

namespace CompanyEmployees.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policyBuilder =>
                {
                    policyBuilder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }

        public static void ConfigureIisIntegration(this IServiceCollection services)
        {
            services.Configure<IISOptions>(options => { });
        }

        public static void ConfigureLoggerService(this IServiceCollection services)
        {
            services.AddScoped<ILoggerManager, LoggerManager>();
        }

        public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<RepositoryContext>(opts =>
            {
                opts.UseSqlServer(config.GetConnectionString("DefaultConnection"), settings =>
                {
                    settings.EnableRetryOnFailure();
                    settings.CommandTimeout(10);
                    settings.MigrationsAssembly("CompanyEmployees");
                });
            });
        }

        public static void ConfigureRepositoryManager(this IServiceCollection services)
        {
            services.AddScoped<IRepositoryManager, RepositoryManager>();
        }

        public static void ConfigureControllers(this IServiceCollection services)
        {
            services.AddControllers(options =>
                    {
                        options.RespectBrowserAcceptHeader = true;
                        options.ReturnHttpNotAcceptable = true;
                    })
                .AddNewtonsoftJson()
                .AddXmlDataContractSerializerFormatters()
                .AddCustomCsvFormatter();
        }

        public static IMvcBuilder AddCustomCsvFormatter(this IMvcBuilder builder)
        {
            return builder.AddMvcOptions(config =>
            {
                config.OutputFormatters.Add(new CsvCompaniesOutputFormatter());
                config.OutputFormatters.Add(new CsvEmployeesOutputFormatter());
            });
        }

        public static void ConfigureApiBehaviour(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(opt =>
            {
                opt.SuppressModelStateInvalidFilter = true;
            });
        }

        public static void AddCustomActionFilters(this IServiceCollection services)
        {
            services.AddScoped<ValidateModelState>();
            services.AddScoped<ValidateCompanyExistsAttribute>();
            services.AddScoped<ValidateEmployeeExistsAttribute>();
            services.AddScoped<ValidateMediaTypeAttribute>();
        }

        public static void AddCustomMediaTypes(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(config =>
            {
                var newtonsoftJsonOutputFormatter = config
                    .OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>()
                    ?.FirstOrDefault();

                newtonsoftJsonOutputFormatter?.SupportedMediaTypes
                    .Add("application/vnd.codemaze.hateos+json");

                var xmlOutputFormatter = config.OutputFormatters
                    .OfType<XmlDataContractSerializerInputFormatter>()
                    ?.FirstOrDefault();

            });
        }
    }
}
