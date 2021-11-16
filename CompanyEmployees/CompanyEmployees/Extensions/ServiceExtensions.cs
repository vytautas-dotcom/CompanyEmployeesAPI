using Contracts;
using LoggerService;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities;
using Microsoft.EntityFrameworkCore;
using Repository;
using CompanyEmployees.Formatting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Marvin.Cache.Headers;

namespace CompanyEmployees.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", builder => builder
                                                            .AllowAnyOrigin()
                                                            .AllowAnyMethod()
                                                            .AllowAnyHeader());
            });
        }
        public static void ConfigureIISIntegration(this IServiceCollection services)
        {
            services.Configure<IISOptions>(options => { });
        }
        public static void ConfigureLoggerService(this IServiceCollection services)
        {
            services.AddScoped<ILoggerManager, LoggerManager>();
        }
        public static void ConfigureSqlConnection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<RepositoryContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SqlConnection"), builder =>
                    builder.MigrationsAssembly(nameof(CompanyEmployees))));
        }
        public static void ConfigureRepositoryManager(this IServiceCollection services)
        {
            services.AddScoped<IRepositoryManager, RepositoryManager>();
        }
        public static IMvcBuilder AddCustomCSVFormatter(this IMvcBuilder builder)
        {
            return builder.AddMvcOptions(config => config.OutputFormatters.Add(new CsvOutputFormatter()));
        }
        public static void AddCustomMediaTypes(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(config =>
            {
                var newtonsoftJsonOutputFormatter = config.OutputFormatters
                                                          .OfType<NewtonsoftJsonOutputFormatter>()?
                                                          .FirstOrDefault();
                if (newtonsoftJsonOutputFormatter != null)
                {
                    newtonsoftJsonOutputFormatter.SupportedMediaTypes
                                                 .Add("application/vnd.vt.hateoas+json");
                    newtonsoftJsonOutputFormatter.SupportedMediaTypes
                                                 .Add("application/vnd.vt.apiroot+json");
                }
                var xmlOutputFormatter = config.OutputFormatters
                                               .OfType<XmlDataContractSerializerOutputFormatter>()?
                                               .FirstOrDefault();
                if (xmlOutputFormatter != null)
                {
                    xmlOutputFormatter.SupportedMediaTypes
                                      .Add("application/vnd.vt.hateoas+xml");
                    xmlOutputFormatter.SupportedMediaTypes
                                      .Add("application/vnd.vt.apiroot+xml");
                }
            });
        }
        public static void ConfigureVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ApiVersionReader = new HeaderApiVersionReader("api-version");
            });
        }
        public static void ConfigureResponseCaching(this IServiceCollection services)
            =>
                services.AddResponseCaching();

        public static void ConfigureHttpCacheHeaders(this IServiceCollection services)
            =>
                services.AddHttpCacheHeaders(
                    expirationOptions =>
                    {
                    expirationOptions.MaxAge = 85;
                    expirationOptions.CacheLocation = CacheLocation.Private;
                    },
                    validationOptions =>
                    {
                        validationOptions.MustRevalidate = true;
                    });
    }
}
