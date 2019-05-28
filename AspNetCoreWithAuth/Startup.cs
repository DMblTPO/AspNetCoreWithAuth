using AspNetCoreWithAuth.Models;
using AspNetCoreWithAuth.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AspNetCoreWithAuth
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
            services.AddCors();

            // add db context
            var con = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<UsersContext>(options => options.UseSqlServer(con));

            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection(nameof(AuthSettings));
            services.Configure<AuthSettings>(appSettingsSection);

            // configure jwt authentication
            var authSettings = appSettingsSection.Get<AuthSettings>();

            services.AddSingleton(authSettings);

            services.AddAuthentication(
                    option =>
                    {
                        option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // JwtBearer
                        option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        option.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    })
                .AddJwtBearer(
                    options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.SaveToken = true;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = authSettings.Issuer,

                            ValidateAudience = true,
                            ValidAudience = authSettings.Audience,

                            ValidateLifetime = true,

                            IssuerSigningKey = authSettings.GetSymmetricSecurityKey(),
                            ValidateIssuerSigningKey = true
                        };
                    });
 
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
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
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();

            app.UseMvc(
                routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");
                });
        }
    }
}
