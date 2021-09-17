using AspNetCoreRateLimit;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.Extensions;
using CompanyEmployees.Utility;
using Contracts;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NLog;
using Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureCors();
            services.ConfigureIISIntegration();
            services.ConfigureLoggerService();
            services.ConfigureSqlContext(Configuration);
            services.ConfigureRepositoryManager();
            services.ConfigureVersioning();
            services.ConfigureResponseCaching();
            services.ConfigureHttpCacheHeaders();

            services.AddControllers(config =>
            {
                // 是否遵守 Accept 標頭
                config.RespectBrowserAcceptHeader = true;
                // 如果伺服器端不支持HTTP request中Accept的格式值，則回傳HTTP 406 Not Acceptable
                config.ReturnHttpNotAcceptable = true;
                config.CacheProfiles.Add("120SecondsDuration", new CacheProfile { Duration = 120 });
            }).AddNewtonsoftJson()
            .AddXmlDataContractSerializerFormatters()
            .AddCustomCSVFormatter();

            #region Swagger 辨視版本

            services.AddApiVersioning(setup =>
            {
                setup.DefaultApiVersion = new ApiVersion(1, 0);
                setup.AssumeDefaultVersionWhenUnspecified = true;
                setup.ReportApiVersions = true;
            });

            services.AddVersionedApiExplorer(setup =>
            {
                setup.GroupNameFormat = "'v'VVV";
                setup.SubstituteApiVersionInUrl = true;
            });

            #endregion Swagger 辨視版本

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CompanyEmployees", Version = "v1" });
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "CompanyEmployees", Version = "v2" });
            });

            services.AddAutoMapper(typeof(Startup));

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddScoped<ValidationFilterAttribute>();
            services.AddScoped<ValidateCompanyExistsAttribute>();
            services.AddScoped<ValidateEmployeeForCompanyExistsAttribute>();
            services.AddScoped<ValidateMediaTypeAttribute>();
            services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>();
            services.AddScoped<EmployeeLinks>();

            services.AddCustomMediaTypes();

            #region 速率限制和節流 

            services.AddMemoryCache();

            services.ConfigureRateLimitingOptions();
            services.AddHttpContextAccessor();

            #endregion 速率限制和節流
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerManager logger, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint(
                            $"/swagger/{description.GroupName}/swagger.json",
                            description.GroupName.ToUpperInvariant());
                    }
                });
            }
            else
            {
                app.UseHsts();
            }

            app.ConfigureExceptionHandler(logger);
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseCors("CorsPolicy");

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All
            });

            app.UseResponseCaching();
            app.UseHttpCacheHeaders();
            //速率限制和節流
            app.UseIpRateLimiting();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
