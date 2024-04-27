using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Admin
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class AuditoriaMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditoriaMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            string controller = httpContext.Request.RouteValues["controller"] as string;
            string action = httpContext.Request.RouteValues["action"] as string;
            //var ipaddress = httpContext.Connection.RemoteIpAddress;
            var brownse = httpContext.Request.Headers["user-agent"];
            //if(httpContext.Request.Method != HttpMethod.Get.Method)

            await _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class AuditoriaMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuditoriaMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuditoriaMiddleware>();
        }
    }
}
