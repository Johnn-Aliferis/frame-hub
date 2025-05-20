using DotNetEnv;
using FrameHub.ContextConfiguration;
using FrameHub.Extensions;
using FrameHub.Model.Entities;
using FrameHub.Repository.Implementations;
using FrameHub.Repository.Interfaces;
using FrameHub.Service.Factories;
using FrameHub.Service.Implementations;
using FrameHub.Service.Interfaces;
using FrameHub.Service.Strategies;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (builder.Environment.IsDevelopment())
{
    Env.Load();
}


// Later to be added in extensions :
builder.Services.AddTransient<ILoginService, LoginService>();
builder.Services.AddTransient<IRegistrationService, RegistrationService>();

builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<ISubscriptionPlanRepository, SubscriptionPlanRepository>();


builder.Services.AddScoped<ISsoProviderStrategyFactory, SsoProviderStrategyFactory>();
builder.Services.AddScoped<GoogleSsoProviderStrategy>();
//
// builder.Services.AddIdentityCore<ApplicationUser>()
//     .AddRoles<IdentityRole>()
//     .AddEntityFrameworkStores<AppDbContext>()
//     .AddDefaultTokenProviders();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => { })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Later add in extensions
// builder.Services.AddAuthentication()
//     .AddGoogle(googleOptions =>
//     {
//         googleOptions.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")!;
//         googleOptions.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_SECRET")!;
//     });

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie("External")
    .AddGoogle("google", options =>
    {
        options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")!;
        options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_SECRET")!;
        options.CallbackPath = "/google-sso-callback";
    });


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

app.UseAuthorization();

app.MapControllers();

app.Run();