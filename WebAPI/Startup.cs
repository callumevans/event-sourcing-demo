using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            Configuration = configuration;
            ServiceProvider = serviceProvider;
        }

        public IConfiguration Configuration { get; }

        public IServiceProvider ServiceProvider { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            
            services.AddHostedService<EventStoreClientLifecycleManager>();
            
            services.AddSingleton<EventStoreClient>();

            services.Configure<EventStoreOptions>(Configuration.GetSection("EventStore"));
            
            services.AddDbContextPool<ToDoListContext>(options =>
            {
                options.UseSqlite("Data Source=./todolist.db");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            using (var seedingScope = app.ApplicationServices.CreateScope())
            {
                var dbContext = seedingScope.ServiceProvider.GetService<ToDoListContext>();

                dbContext.Database.Migrate();
                dbContext.Database.EnsureCreated();
            }
            
            app.UseMvc();
        }
    }
}