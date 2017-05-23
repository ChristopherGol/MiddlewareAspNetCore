using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace MiddlewareAspNetCore
{
    public class OwnPipeline
    {
        public void Configure(IApplicationBuilder app)
        {
            // Create middleware object. 
            app.UseValidatePersonMiddleware();
        }
    }
}
