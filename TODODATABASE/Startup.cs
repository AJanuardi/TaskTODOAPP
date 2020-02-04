using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TODODATABASE.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.IO;
using Serilog;
using Serilog.Events;
using System.Diagnostics;

namespace TODODATABASE
{
    public class Startup
    {
        public IConfiguration Configuration {get;}
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };
            });

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("localhost"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseMiddleware<LoggingMiddleware>();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
     public class LoggingMiddleware
        {
            const string MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            static readonly ILogger Log = Serilog.Log.ForContext<LoggingMiddleware>();

            readonly RequestDelegate _next;

            public LoggingMiddleware(RequestDelegate next)
            {
                if (next == null) throw new ArgumentNullException(nameof(next));
                _next = next;
            }

            public async Task Invoke(HttpContext httpContext)
            {
                if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

                var sw = Stopwatch.StartNew();
                try
                {
                    await _next(httpContext);
                    sw.Stop();

                    var statusCode = httpContext.Response?.StatusCode;
                    var level = statusCode > 499 ? LogEventLevel.Error : LogEventLevel.Information;

                    var log = level == LogEventLevel.Error ? LogForErrorContext(httpContext) : Log;
                    log.Write(level, MessageTemplate, httpContext.Request.Method, httpContext.Request.Path, statusCode, sw.Elapsed.TotalMilliseconds);
                    TextWriter tw = new StreamWriter("Log.txt", true);
                    tw.WriteLine( $"HTTP {httpContext.Request.Method} {httpContext.Request.Path} responded {statusCode} in {sw.Elapsed.TotalMilliseconds} ms");
                    tw.Close(); 
                }
                // Never caught, because `LogException()` returns false.
                catch (Exception ex) when (LogException(httpContext, sw, ex)) { }
            }

            static bool LogException(HttpContext httpContext, Stopwatch sw, Exception ex)
            {
                sw.Stop();

                LogForErrorContext(httpContext)
                    .Error(ex, MessageTemplate, httpContext.Request.Method, httpContext.Request.Path, 500, sw.Elapsed.TotalMilliseconds);
                TextWriter tw = new StreamWriter("Log.txt", true);
                tw.WriteLine( $"HTTP {httpContext.Request.Method} {httpContext.Request.Path} responded {"500"} in {sw.Elapsed.TotalMilliseconds} ms");
                tw.Close();
                return false;
            }

            static ILogger LogForErrorContext(HttpContext httpContext)
            {
                var request = httpContext.Request;

                var result = Log
                    .ForContext("RequestHeaders", request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), destructureObjects: true)
                    .ForContext("RequestHost", request.Host)
                    .ForContext("RequestProtocol", request.Protocol);

                if (request.HasFormContentType)
                    result = result.ForContext("RequestForm", request.Form.ToDictionary(v => v.Key, v => v.Value.ToString()));

                TextWriter tw = new StreamWriter("Log.txt", true);
                tw.WriteLine(result);
                tw.Close(); 
                return result;
            }
        }
}
