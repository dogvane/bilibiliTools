using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BaiduFanyi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Swashbuckle.AspNetCore.Swagger;

namespace 弹幕合并
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "chsarptools",
                    Version = "v1",
                    Description = "chsarptools API ",
                });

                //处理复杂名称
                c.CustomSchemaIds((type) => type.FullName);
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

#if DEBUG
            app.UseSwagger(c => {
                //设置json路径
                c.RouteTemplate = "docs/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(c => {
                //访问swagger UI的路由，如http://localhost:端口/docs
                c.RoutePrefix = "docs";
                c.SwaggerEndpoint("/docs/v1/swagger.json", "Swagger测试V1");
                //更改UI样式
                c.InjectStylesheet("/swagger-ui/custom.css");
                //引入UI变更js
                c.InjectOnCompleteJavaScript("/swagger-ui/custom.js");
            });
#endif
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            // app.UseMvc();

            app.UseBaiduFanyi(Configuration);
        }
    }
}
