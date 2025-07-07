using ArcCorpBackend.Services;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Serilog;

namespace ArcCorpBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var date = DateTime.Now;
            var logFileName = $"log-{date:yyyy-MM-dd-HH-mm}.txt";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File($"logs/{logFileName}", rollingInterval: RollingInterval.Hour, fileSizeLimitBytes: 104_857_600) // 100MB
                                                                                                                             //.WriteTo.Seq("http://localhost:5341") // optional Seq sink
                .CreateLogger();
            // Retrieve your decrypted JWT key from your ConstantSecretKeyService
            var JWT = ConstantSecretKeyService.Instance.GetJWT();

            var builder = WebApplication.CreateBuilder(args);

            // Add controllers to the DI container.
            builder.Services.AddControllers();

            // Add OpenAPI support.
            builder.Services.AddOpenApi();

            // Configure JWT authentication using your JWT key.
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(JWT)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
                };
            });
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });


            var app = builder.Build();
            app.UseCors("AllowAll");


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            // Enable authentication and authorization middleware
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
