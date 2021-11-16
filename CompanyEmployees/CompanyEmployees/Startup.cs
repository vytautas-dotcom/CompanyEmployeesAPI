using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompanyEmployees.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using LoggerService;
using NLog;
using System.IO;
using Contracts;
using CompanyEmployees.ActionFilters;
using Repository.DataShaping;
using Entities.DataTransferObjects;
using CompanyEmployees.Utility;

namespace CompanyEmployees
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureCors();
            services.ConfigureIISIntegration();
            services.ConfigureLoggerService();
            services.ConfigureSqlConnection(Configuration);
            services.ConfigureRepositoryManager();
            services.AddAutoMapper(typeof(Startup));

            services.AddScoped<ValidationFilterAttribute>();
            services.AddScoped<ValidateCompanyExistsAttribute>();
            services.AddScoped<ValidateEmployeeForCompanyExistsAttribute>();
            services.AddScoped<ValidateMediaTypeAttribute>();

            services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddControllers(config => 
            {
                config.RespectBrowserAcceptHeader = true;
            }).AddNewtonsoftJson()
              .AddXmlDataContractSerializerFormatters()
              .AddCustomCSVFormatter();

            services.AddCustomMediaTypes();
            services.AddScoped<EmployeeLinks>();

            services.ConfigureVersioning();

            services.ConfigureResponseCaching();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerManager logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.ConfigureExceptionHandler(logger);

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors("MyPolicy");
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

            app.UseResponseCaching();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
