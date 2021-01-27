using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using SwaggerUI_AuthorizationCodeFlow.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SwaggerUI_AuthorizationCodeFlow
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

            services
                .Configure<AppSettings>(Configuration)
                .AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerGenOptions>()
                .AddScoped(sp => sp.GetRequiredService<IOptionsSnapshot<AppSettings>>().Value)
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    Configuration.Bind($"{nameof(AppSettings.Security)}:{nameof(AppSettings.Security.Jwt)}", options);
                })
                .Services
                .AddHttpClient()
                .AddSwaggerGen()
                .AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var appSettings = Configuration.Get<AppSettings>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
               
            }

            app
            .UseSwagger()
            .UseSwaggerUI(setup =>
            {
                setup.SwaggerEndpoint($"/swagger/v1/swagger.json", "Version 1.0");
                setup.OAuthClientId(appSettings.Security.Jwt.ClientId);
                setup.OAuthClientSecret(appSettings.Security.Jwt.ClientSecret);
                setup.OAuthAppName("Weather API");
                setup.OAuthScopeSeparator(" ");
                setup.OAuthUsePkce();
            })
            .UseHttpsRedirection()
            .UseRouting()
            .UseAuthentication()
            .UseAuthorization()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
