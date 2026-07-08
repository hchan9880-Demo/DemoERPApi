using DemoERPApi.Data;
using DemoERPApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;


var builder = WebApplication.CreateBuilder(args);


// ======================================
// DATABASE
// ======================================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DemoERPConnection")));


// ======================================
// SERVICES
// ======================================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<PasswordService>();


// ======================================
// CORS
// ======================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// ======================================
// AUTHORIZATION
// ======================================

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CustomerOwner",
        policy =>
        {
            policy.RequireAuthenticatedUser();
        });
});


// ======================================
// JWT AUTHENTICATION
// ======================================

var jwtKey = "ThisIsATestJwtKey123456789012345";

var jwtIssuer = "DemoERPApi";

var jwtAudience = "DemoERPApiUsers";


builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)

    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,


                ValidIssuer = jwtIssuer,

                ValidAudience = jwtAudience,


                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
            };
    });


// ======================================
// SWAGGER JWT SUPPORT
// ======================================

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description =
                "Enter JWT token as: Bearer {token}",

            Name = "Authorization",

            In = ParameterLocation.Header,

            Type = SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT"
        });


    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,

                            Id = "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        });
});


var app = builder.Build();


// ======================================
// DEVELOPMENT TOOLS
// ======================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}


// ======================================
// DATABASE MIGRATION + SEED
// ======================================

using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider
             .GetRequiredService<AppDbContext>();


    // Apply migrations

    context.Database.Migrate();


    // Insert test/default data

    DbSeeder.Seed(context);
}


// ======================================
// HTTP PIPELINE
// ======================================

app.UseHttpsRedirection();


app.UseCors("AllowAll");


app.UseAuthentication();


app.UseAuthorization();


app.MapControllers();


app.MapGet("/",
    () => "Demo ERP API is running!");


app.Run();



public partial class Program
{
}