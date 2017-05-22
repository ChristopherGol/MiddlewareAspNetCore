using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using MiddlewareAspNetCore.Model;

namespace MiddlewareAspNetCore
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // Add response compression service.
            services.AddResponseCompression();

            // Add response cache service.
            services.AddResponseCaching();

            // Add session service with specified options.
            services.AddSession(options=> {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.CookieHttpOnly = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // Run own middleware for all requests.
            app.UseSecurityMiddleware();

            var logger = loggerFactory.CreateLogger("MiddlewareAspNetCore");

            // It handles all requests and terminates the pipeline.
            app.Run(async context =>
            {
                await context.Response.WriteAsync("Hi!");
            });

            // It allows sequenced the multiple requests.
            app.Use(async (context, next) =>
            {
                // Log information to debug console.
                logger.LogInformation(context.Request.Path.ToString());

                // Next delegate in the pipeline. 
                await next.Invoke();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                // Catch exceptions thrown in the middleware.
                app.UseExceptionHandler("/Home/Error");
            }

            // Use session mechanism.
            app.UseSession();

            // Return static files (no compress).
            app.UseStaticFiles();

            // Authenticate requests before access to secure resources.
            app.UseIdentity();

            // It allows to compress response. 
            app.UseResponseCompression();

            // Requests execution with default route: {controller=Home}/{action=Index}
            app.UseMvcWithDefaultRoute();

            // 'Map' allows to match of the given request path and execute specified code for example url: http://localhost:xxxx/map. 
            app.Map("/map", Map);

            // Set cookie after open link http://localhost:xxxx/cookie. 
            app.Map("/cookie", CookieMiddleware);

            // Set cache after open link http://localhost:xxxx/cache. 
            app.Map("/cache", CacheMiddleware);

            // Set addintional header after open url contains 'secret' key for example: http://localhost:xxxx/secureinfo?secret=Top. 
            app.MapWhen(context => context.Request.Query.ContainsKey("secret"), TopSecret);

            // 'Map' supports nesting request path for example: http://localhost:xxxx/map4/map5
            app.Map("/map4/map5", MapNesting);

            // Get query string and display. 
            app.Map("/querystring", GetQueryString);

            // Validate query string from url e.g. http://localhost:xxxx/getperson?name=A&surname=B&age=25
            app.Map("/getperson", ChangeQueryString);
            
        }

        private static void Map(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                // Write specified text for response.
                await context.Response.WriteAsync("Map middleware.");
            });
        }

        private static void CookieMiddleware(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                // Create cookie options (Expire date, HttpOnly flag, Secure flag). 
                CookieOptions options = new CookieOptions();
                options.Expires = new DateTimeOffset(2017, 06, 19, 17, 17, 0, TimeSpan.Zero);
                options.HttpOnly = true;
                options.Secure = false;

                // Create cookie. 
                context.Response.Cookies.Append("middleware", "cookie", options);

                await context.Response.WriteAsync("Added cookie.");
            });
        }

        private static void CacheMiddleware(IApplicationBuilder app)
        {
            app.UseResponseCaching();
            app.Run(async context =>
            {
                // Set options for cache. 
                context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue()
                {
                    MaxAge = TimeSpan.FromSeconds(60),
                    Public = true,
                };

                await context.Response.WriteAsync("Cached time" + DateTime.UtcNow);
            });
        }

        private static void MapNesting(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                await context.Response.WriteAsync("Requests nesting");
            });
        }

        private static void GetQueryString(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                // Get query request. 
                string name = context.Request.Query["name"];
                string surname = context.Request.Query["surname"];
                int age = int.Parse(context.Request.Query["age"]);

                string response = string.Format("Name: {0}, Surname: {1}, Age: {2}", name, surname, age);
                await context.Response.WriteAsync(response);

            });
        }

        private static void ChangeQueryString(IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                string name = context.Request.Query["name"];
                string surname = context.Request.Query["surname"];
                int age = int.Parse(context.Request.Query["age"]);

                if (age < 100)
                {
                    await next.Invoke();
                }
                else
                {
                    await context.Response.WriteAsync("Wrong age!");
                }
            });

            app.Run(async context =>
            {
                await context.Response.WriteAsync(context.Request.Query["name"] + " " + context.Request.Query["surname"] + " " + context.Request.Query["age"]);
            });

        }

        private static void TopSecret(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                string typeInformation = context.Request.Query["secret"];

                // Add additional header to response. 
                context.Response.Headers.Add("Type-Information", typeInformation);
            });

        }

    }
}
