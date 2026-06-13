using LoopLearn.API.Services.Auth;
using LoopLearn.API.Services.Courses;
using LoopLearn.API.Services.Enroll;
using LoopLearn.API.Services.Shared;
using LoopLearn.DataAccess.Data;
using LoopLearn.DataAccess.Implementation;
using LoopLearn.Entities.Helpers.Models;
using LoopLearn.Entities.Interfaces;
using LoopLearn.Entities.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System.Text;
using System.Text.Json.Serialization;

namespace LoopLearn.API
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── Database ──────────────────────────────────────────────────────
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                ));

            // ── Repositories & Services ───────────────────────────────────────
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<AuthService, AuthService>();
            builder.Services.AddScoped<EnrollmentService, EnrollmentService>();
			builder.Services.AddScoped<CourseUpdateService, CourseUpdateService>();
            builder.Services.AddScoped<CourseValidationService, CourseValidationService>();
            builder.Services.AddScoped<ImageService,ImageService>();


			// ── Identity ──────────────────────────────────────────────────────
			builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // ── Configuration ─────────────────────────────────────────────────
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var stripeSettings = builder.Configuration.GetSection("Stripe");

            builder.Services.Configure<Jwt>(jwtSettings);
            builder.Services.Configure<StripeSettings>(stripeSettings);

            // Set Stripe API key once at startup — not on every controller instantiation
            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            // ── Authentication ────────────────────────────────────────────────
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
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
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // ── Logging ───────────────────────────────────────────────────────
            builder.Services.AddLogging(config =>
            {
                config.AddConsole();
                config.AddDebug();
            });

            // ── CORS ──────────────────────────────────────────────────────────
            builder.Services.AddCors(options =>
            {
                // Development: allow all origins for easy local testing
                options.AddPolicy("AllowAll", policy =>
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

                // Production: restrict to your actual frontend domain
                //options.AddPolicy("AllowFrontend", policy =>
                //    policy.WithOrigins(
                //              builder.Configuration["AllowedOrigins"]
                //                  ?? "https://yourdomain.com"
                //          )
                //          .AllowAnyMethod()
                //          .AllowAnyHeader());
            });

            // ── Controllers (one registration, one place) ─────────────────────
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            builder.Services.AddSwaggerGen();
            builder.Services.AddMemoryCache();

            // ── Build ─────────────────────────────────────────────────────────
            var app = builder.Build();

            // ── Stripe Webhook: preserve raw body before any middleware reads it
            // Without this, the stream is consumed before EventUtility.ConstructEvent,
            // causing signature verification to fail.
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/api/payment/webhook"))
                    context.Request.EnableBuffering();
                await next();
            });

            // ── Seed Database ─────────────────────────────────────────────────
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
                var logger = services.GetRequiredService<ILogger<Program>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

                try
                {
                    await context.SeedDatabaseAsync(logger, roleManager, userManager);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            // ── Middleware Pipeline ───────────────────────────────────────────
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseCors("AllowAll");
            }
            else
            {
                app.UseCors("AllowFrontend");
            }

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}