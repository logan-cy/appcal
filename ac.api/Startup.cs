using System.Collections.Immutable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ac.api.Data;
using ac.api.Interfaces;
using ac.api.Models.DTO;
using ac.api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Serialization;

namespace ac.api
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

            //Default values seems fine for passwords
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 3;
            });
            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.Zero;
            });

            services.AddAuthorization();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Sys:Issuer"],
                    ValidAudience = Configuration["Sys:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["Sys:Key"])),
                    ValidateIssuer = true,
                    ValidateAudience = true
                };
            });
            services.AddControllers();
            var identityBuilder = services.AddIdentityCore<IdentityUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            identityBuilder.AddSignInManager<SignInManager<IdentityUser>>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ac.api", Version = "v1" });
            });

            services.AddSingleton<IEmailService, MailjetService>();
            services.Configure<EmailOptionsDTO>(Configuration.GetSection("Mailjet"));

            services.AddScoped<IUserService, UserService>();

            services.AddCors();

            // Add framework services.
            services.AddMvc(options =>
            {
                options.RespectBrowserAcceptHeader = true;
            })
            //support application/xml
            .AddXmlDataContractSerializerFormatters();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ac.api v1"));

                app.UseCors(builder =>
                {
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                    builder.AllowAnyOrigin();
                });
            }

            app.Use(async (context, next) =>
            {
                var principal = context.User as ClaimsPrincipal;
                var accessToken = principal?.Claims
                  .FirstOrDefault(c => c.Type == "access_token");
                if (accessToken != null)
                {
                    //_logger.LogDebug(accessToken.Value);
                }
                if (context.Request.Path.Value.Contains(";"))
                {
                    var left = context.Request.Path.Value.Split(';');
                    context.Request.Path = new PathString(left[0]);
                }
                await next();
            });

            app.UseCors(builder =>
            {
                builder.WithOrigins("http://localhost:5001");
                builder.AllowAnyMethod();
                builder.AllowAnyOrigin();
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
