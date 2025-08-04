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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Services via Extensions
builder.Services.AddApplicationServices(builder.Configuration);

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