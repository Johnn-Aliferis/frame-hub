using System.Net;
using System.Text;
using Amazon.S3;
using DotNetEnv;
using FrameHub.Modules.Auth.Application.Services;
using FrameHub.Modules.Auth.Domain.Entities;
using FrameHub.Modules.Auth.Infrastructure.Configuration;
using FrameHub.Modules.Auth.Infrastructure.Repository;
using FrameHub.Modules.Auth.Infrastructure.Service;
using FrameHub.Modules.Media.Application.Service;
using FrameHub.Modules.Media.Infrastructure.Repository;
using FrameHub.Modules.Media.Infrastructure.Service;
using FrameHub.Modules.Shared.Application.Exception;
using FrameHub.Modules.Shared.Application.Interface;
using FrameHub.Modules.Shared.Infrastructure.Persistence;
using FrameHub.Modules.Shared.Infrastructure.Repository;
using FrameHub.Modules.Subscriptions.Application.Service;
using FrameHub.Modules.Subscriptions.Infrastructure.Messaging.RabbitMQ;
using FrameHub.Modules.Subscriptions.Infrastructure.Repository;
using FrameHub.Modules.Subscriptions.Infrastructure.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Stripe;

namespace FrameHub.Modules.Shared.Extensions;

public static class ServiceExtensions
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Load Environment Variables
        Env.Load();
        
        // Core Services
        services.AddTransient<ILoginService, LoginService>();
        services.AddTransient<IJwtService, JwtService>();
        services.AddTransient<IRegistrationService, RegistrationService>();
        services.AddTransient<IPaymentSubscriptionService, PaymentSubscriptionService>();
        services.AddTransient<IStripeService, StripeService>();
        services.AddTransient<IStripeConsumerService, StripeConsumerService>();
        services.AddTransient<IPhotoRepository, PhotoRepository>();
        services.AddTransient<IMediaService, MediaService>();
        services.AddTransient<IUploadProvider, AmazonS3Provider>();
        services.AddTransient<IUnitOfWork, UnitOfWork>();
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        services.AddTransient<IWebhookEventRepository, WebhookEventRepository>();
        services.AddTransient<CustomerService>();
        services.AddTransient<SubscriptionService>();
        services.AddTransient<PaymentMethodService>();
        services.AddTransient<InvoiceService>();
        
        services.AddScoped<ISsoService, SsoService>();
        services.AddIdentity<ApplicationUser, IdentityRole>(options => { })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // AWS
        services.AddDefaultAWSOptions(configuration.GetAWSOptions());
        services.AddAWSService<IAmazonS3>();
        
        
        services.Configure<JwtSettingsOptions>(configuration.GetSection("JwtSettings"));
        
        var jwtSettings = configuration
            .GetSection("JwtSettings")
            .Get<JwtSettingsOptions>();
        
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
        
        if (string.IsNullOrEmpty(jwtSecret))
        {
            throw new GeneralException("JWT_SECRET environment variable is missing.", HttpStatusCode.InternalServerError);
        }
        if (jwtSettings is null)
        {
            throw new GeneralException("jwtSettings config is missing.", HttpStatusCode.InternalServerError);
        }
        
        // Authentication setup
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        
            })
            .AddCookie(options =>
            {
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            })
            .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,
        
                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,
        
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
        
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                    };
                })
            .AddGoogle("google",options =>
            {
                options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")!;
                options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_SECRET")!;
            
                // Isolate Google's cookies
                options.CorrelationCookie.SameSite = SameSiteMode.None;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.CorrelationCookie.Path = "/";
            });

        // For multiple providers : 
        // builder.Services.AddTransient<AzureBlobUploadProvider>();
        // builder.Services.AddTransient<IUploadProvider>(sp =>
        // {
        //     var config = sp.GetRequiredService<IConfiguration>();
        //     var provider = config["UploadProvider"];
        //
        //     return provider switch
        //     {
        //         "Azure" => sp.GetRequiredService<AzureBlobUploadProvider>(),
        //         _ => throw new InvalidOperationException("Unknown or unsupported upload provider")
        //     };
        // });
        
        // Stripe Payments setup 
        StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");

        // Broker 
        services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
        services.AddHostedService<StripeWebhookConsumer>();
    }
}