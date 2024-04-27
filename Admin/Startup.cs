using System;
using System.IO;
using System.Linq;
using System.Text;
using Admin.Models;
using Algolia.Search.Clients;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Lizelaser0310.Utilities;
using Microsoft.Extensions.FileProviders;
using Nest;
using Newtonsoft.Json;
using Serilog;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using Serilog.Events;
using System.Collections.Generic;

namespace Admin
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Constants = new Constants();
        }

        public IConfiguration Configuration { get; }
        private Constants Constants { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Configuration.GetSection("Constants").Bind(Constants);
            
            var tokenKey = Encoding.ASCII.GetBytes(Configuration.GetValue<string>("TokenKey"));
            var encryptionKey = Convert.FromBase64String(Configuration.GetValue<string>("EncrytionKey"));
            var elasticKey = Configuration.GetValue<string>("ElasticKey");
            var hashIdsKey = Configuration.GetValue<string>("HashIdsKey");
            var captchaKey = Configuration.GetValue<string>("CaptchaKey");
            var smtpMail = Configuration.GetValue<string>("SmtpMail");
            var smtpPassword = Configuration.GetValue<string>("SmtpPassword");

            var emailCredentials = new EmailCredentials()
            {
                Email = smtpMail,
                Password = smtpPassword,
                Host = "smtp.zoho.com",
                Port = 465,
                UseSSL = true
            };
            var keys = new Keys(encryptionKey,tokenKey,elasticKey,hashIdsKey,captchaKey);
            
            var elasticNode = new Uri(Constants.ElasticUrl);
            var elasticSettings = new ConnectionSettings(elasticNode).BasicAuthentication("elastic",keys.ElasticKey);
            var elasticClient = new ElasticClient(elasticSettings);
            var authCache = new Dictionary<int, HashSet<string>>();

            services.AddSingleton<IConstants>(Constants);
            services.AddSingleton<IKeys>(keys);
            services.AddSingleton(emailCredentials);
            services.AddSingleton(elasticClient);
            services.AddSingleton(authCache);

            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(jwt =>
            {
                jwt.RequireHttpsMetadata = false;
                jwt.SaveToken = true;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(tokenKey),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            services.AddMvc();
            services.AddDbContext<HISCOMContext>(db => db.UseSqlServer(Configuration.GetConnectionString("connectionDB")));
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // app.UseHttpsRedirection();

            try
            {
                //var baseDirectory = context.Parametro.SingleOrDefault(p=>p.Llave==Constants.BaseDirectory);
                var storage = Constants.Storage ?? env.ContentRootPath;
                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider(storage)
                });
            }
            catch
            {
                app.UseStaticFiles();
            }

            app.UseSerilogRequestLogging();
            
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors(cors => cors.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(origin => true).AllowCredentials());
            app.UsePermisoMiddleware();
            app.UseAuditoriaMiddleware();


            app.UseEndpoints(end =>
            {
                end.MapControllers();
            });
        }
    }
}
