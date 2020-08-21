using Anotis.Models;
using Anotis.Models.Attendance;
using Anotis.Models.Attendance.Shikimori;
using Anotis.Models.BackgroundRefreshing;
using Anotis.Models.Database;
using Anotis.Models.Database.LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Prometheus;
using ShikimoriSharp;
using ShikimoriSharp.Bases;

namespace Anotis
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore(options =>
            {
                options.RequireHttpsPermanent = true; // does not affect api requests
                options.RespectBrowserAcceptHeader = true; // false by default
                //options.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
                //remove these two below, but added so you know where to place them...
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Anotis api", Version = "v1"});
            });
            services.Configure<AnotisConfig>(Configuration.GetSection("AnotisConfig"));
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<AnotisConfig>>().Value);
            services.AddSingleton(it => 
                new ShikimoriClient((ILogger) it.GetService(typeof(ILogger<Startup>)),
                GetSettings(it.GetRequiredService<AnotisConfig>()))
            );
            services.AddSingleton<ShikimoriAttendance>();
            services.AddSingleton<IDatabase, Lite>();
            services.AddSingleton<MangaReceiver>();
            services.AddSingleton<TokenRenewer>();
            services.AddSingleton<UserReceiver>();
            services.AddHostedService<BackgroundNewUpdatesRefresher>();
            services.AddHostedService<BackgroundUserUpdatesRefresher>();
        }

        private static ClientSettings GetSettings(AnotisConfig config)
        {
            return new ClientSettings(config.Shikimori.ClientName, 
                config.Shikimori.ClientId,
                config.Shikimori.ClientSecret, 
                config.Shikimori.RedirectUrl
                );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddFile("logs/log.txt");

            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Anotis api"); });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseHttpMetrics();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });
        }
    }
}