using DotNetEnv;
using FrameHub.ContextConfiguration;
using FrameHub.Extensions;
using FrameHub.Repository.Implementations;
using FrameHub.Repository.Interfaces;
using FrameHub.Service.Factories;
using FrameHub.Service.Implementations;
using FrameHub.Service.Interfaces;
using FrameHub.Service.Strategies;
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
builder.Services.AddTransient<ILoginStrategyFactory, LoginStrategyFactory>();
builder.Services.AddTransient<IRegistrationStrategyFactory, RegistrationStrategyFactory>();
builder.Services.AddTransient<DefaultLoginStrategy>();
builder.Services.AddTransient<DefaultRegistrationStrategy>();
builder.Services.AddTransient<GoogleLoginStrategy>();
builder.Services.AddTransient<GoogleRegistrationStrategy>();

builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<ISubscriptionPlanRepository, SubscriptionPlanRepository>();

builder.Services.AddIdentityCore<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


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