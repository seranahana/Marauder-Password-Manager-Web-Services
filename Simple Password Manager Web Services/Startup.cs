using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SimplePM.WebAPI.Library;
using SimplePM.WebAPI.Library.Repositories;
using SimplePM.WebAPI.Library.Repositories.Models;
using SimplePM.WebAPI.Library.Processing;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.SwaggerUI;
using StackExchange.Redis;

namespace SimplePM.WebAPI
{
    public class Startup
    {
        private readonly IWebHostEnvironment _hostEnv;
        public Startup(IConfiguration configuration, IWebHostEnvironment hostEnv)
        {
            Configuration = configuration;
            _hostEnv = hostEnv;
            CreateRSAParameters();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services, Serilog.ILogger logger)
        {
            string databasePath = System.IO.Path.Combine("..", "Assets.cs");
            services.AddControllers(options => options.SuppressAsyncSuffixInActionNames = false);
            services.AddDbContext<AssetsContext>(options => options.UseSqlServer(Configuration.GetConnectionString("SqlServer")));
            //services.AddStackExchangeRedisCache(options =>
            //{
            //    options.Configuration = Configuration.GetConnectionString("Redis");
            //    options.InstanceName = "SimplePasswordManagerWebAPI_";
            //});
            services.AddDataProtection();
            services.AddHttpClient(name: "SimplePasswordManager_Service", 
                configureClient: options =>
                {
                    options.BaseAddress = new Uri("https://localhost:5001/");
                    options.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json", 1.0));
                });
            services.AddScoped<IEntryRepository, EntryRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IAccountProcessor, AccountProcessor>();
            services.AddScoped<IEntriesProcessor, EntriesProcessor>();
            services.AddSingleton(logger);
            services.AddSingleton<IConnectionMultiplexer>(cm => ConnectionMultiplexer.Connect(Configuration.GetConnectionString("Redis")));
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Simple_Password_Manager_Web_API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Simple_Password_Manager_Web_API v1");
                    c.SupportedSubmitMethods(new[] { SubmitMethod.Get, SubmitMethod.Post, SubmitMethod.Put, SubmitMethod.Delete, SubmitMethod.Patch });
                });
            }
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static void CreateRSAParameters()
        {
            var rsa = System.Security.Cryptography.RSA.Create();
            Program.PublicKey = rsa.ToXmlStringExt(false);
            Program.PrivateKey = rsa.ToXmlStringExt(true);
        }
    }
}
