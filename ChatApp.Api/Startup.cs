using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using ChatApp.DataService.Data;
using ChatApp.DataService.IConfiguration;
using ChatApp.Authentication.Configuration;

namespace ChatApp.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JwtConfiguration>(Configuration.GetSection("JwtConfiguration"));
            services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite(Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("ChatApp.Api")));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddCors();
            services.AddControllers();

            // Versioning
            services.AddApiVersioning(options => {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = ApiVersion.Default;
            });

            // Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(jwt => {
                var key = Encoding.UTF8.GetBytes(Configuration["JwtConfiguration:Secret"]);

                jwt.SaveToken = true;
                jwt.TokenValidationParameters = new TokenValidationParameters {
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false, // TODO Update
                    ValidateAudience = false, // TODO Update
                    RequireExpirationTime = false, // TODO Update
                    ValidateLifetime = true,
                };

            });

            services.AddDefaultIdentity<IdentityUser>(options
                    => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<AppDbContext>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) { app.UseDeveloperExceptionPage(); }

            //app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(builder => builder
                    .WithOrigins("http://localhost:5000") //.AllowAnyOrigin
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            app.UseAuthentication();
            app.UseAuthorization();

            //app.UseMiddleware<JwtMiddleware>();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
