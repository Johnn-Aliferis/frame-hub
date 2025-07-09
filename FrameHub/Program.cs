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
using FrameHub.Modules.Shared.Extensions;
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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Env.Load();

// Later to be added in extensions :
builder.Services.AddTransient<ILoginService, LoginService>();
builder.Services.AddTransient<IJwtService, JwtService>();
builder.Services.AddTransient<IRegistrationService, RegistrationService>();
builder.Services.AddTransient<IPaymentSubscriptionService, PaymentSubscriptionService>();
builder.Services.AddTransient<IStripeService, StripeService>();
builder.Services.AddTransient<IStripeConsumerService, StripeConsumerService>();
builder.Services.AddTransient<IPhotoRepository, PhotoRepository>();
builder.Services.AddTransient<IMediaService, MediaService>();
builder.Services.AddTransient<IUploadProvider, AmazonS3Provider>();

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();

builder.Services.AddTransient<CustomerService>();
builder.Services.AddTransient<SubscriptionService>();
builder.Services.AddTransient<PaymentMethodService>();
builder.Services.AddTransient<InvoiceService>();

builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
builder.Services.AddTransient<IWebhookEventRepository, WebhookEventRepository>();

builder.Services.AddScoped<ISsoService, SsoService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => { })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


builder.Services.Configure<JwtSettingsOptions>(builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration
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
builder.Services.AddAuthentication(options =>
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
        // options.SignInScheme = "External";
    });

// TODO  : For multiple providers below -- can be used for factory pattern as well. 
//builder.Services.AddTransient<AzureBlobUploadProvider>();
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
builder.Services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
builder.Services.AddHostedService<StripeWebhookConsumer>();

// Security 
builder.Services.AddDataProtection(); 

// For Database Extension
builder.Services.AddDatabaseServices();

// Exception Handlers Extension
builder.Services.AddExceptionHandlers();

// Automapper Extension
builder.Services.AddCustomAutoMapper();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Middleware for global exception handling.
app.UseCustomMiddlewares();

app.UseHttpsRedirection();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.None,
    Secure = CookieSecurePolicy.Always
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }