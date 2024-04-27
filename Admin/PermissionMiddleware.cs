using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Admin.Controllers;

namespace Admin
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class PermisoMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Dictionary<int, HashSet<string>> _authCache;

        public PermisoMiddleware(RequestDelegate next, Dictionary<int, HashSet<string>> authCache)
        {
            _next = next;
            _authCache = authCache;
        }

        public async Task InvokeAsync(HttpContext httpContext, HISCOMContext context)
        {
            try
            {
                var controller = httpContext.Request.RouteValues["controller"] as string;
                var action = httpContext.Request.RouteValues["action"] as string;
                
                if (controller == null || action==null)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }
                if (controller == "Public")
                {
                    await _next(httpContext);
                    return;
                }

                int userId;
                var userExists = int.TryParse(httpContext.User.Identity?.Name,out userId);
                if (!userExists)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
                if (!_authCache.ContainsKey(userId))
                {
                    var permisoUsuario = await (from ur in context.UsuarioRol
                                                join rp in context.RolPermiso on ur.RolId equals rp.RolId
                                                join p in context.Permiso on rp.PermisoId equals p.Id
                                                where ur.UsuarioId.Equals(userId)
                                                select new { p.Accion, p.Ruta })
                                            .Distinct().ToListAsync();
                    
                    _authCache.Remove(userId);
                    _authCache.Add(userId, new HashSet<string>());
                    
                    foreach (var item in permisoUsuario) {
                        if (item.Ruta != null)
                        {
                            _authCache[userId].Add(item.Ruta);
                        }

                        if (item.Accion != null)
                        {
                            foreach (var accion in item.Accion.Split(","))
                            {
                                _authCache[userId].Add(accion.Trim());
                            }
                        }
                    }
                }

                if (controller == "Common" || controller == "Marking" || controller == "Payroll")
                {
                    await _next(httpContext);
                    return;
                }
                
                if (_authCache[userId].Contains(action))
                {
                    await _next(httpContext);
                    return;
                }

                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;                
            }
            catch (Exception e)
            {
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await httpContext.Response.WriteAsync(e.Message);
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class PermisoMiddlewareExtensions
    {
        public static IApplicationBuilder UsePermisoMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PermisoMiddleware>();
        }
    }
}
