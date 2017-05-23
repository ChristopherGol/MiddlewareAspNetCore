using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace MiddlewareAspNetCore
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ValidatePersonMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidatePersonMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            int age;

            // Validate 'age' query parameter. 
            if(int.TryParse(httpContext.Request.Query["age"], out age) == false || int.Parse(httpContext.Request.Query["age"]) <= 0 || int.Parse(httpContext.Request.Query["age"]) > 150)
            {
                httpContext.Response.WriteAsync("Wrong age format or incorrect age!");
            }

            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ValidatePersonMiddlewareExtensions
    {
        public static IApplicationBuilder UseValidatePersonMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ValidatePersonMiddleware>();
        }
    }
}
