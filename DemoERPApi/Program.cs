using DemoERPApi.Data;
using DemoERPApi.Middleware;
using DemoERPApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ======================================
// DATABASE
// ======================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DemoERPConnection")));


// ======================================
// CONTROLLERS + VALIDATION
// ======================================
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            return new BadRequestObjectResult(context.ModelState);
        };
    });
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ILoggingService, LoggingService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();



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
// JWT CONFIGURATION
// ======================================

var jwtKey =
    builder.Configuration["JwtSettings:Key"]
    ?? "DemoERP_Test_JWT_Key_12345678901234567890";

var jwtIssuer =
    builder.Configuration["JwtSettings:Issuer"]
    ?? "DemoERPApi";


var jwtAudience =
    builder.Configuration["JwtSettings:Audience"]
    ?? "DemoERPApiUsers";

// ======================================
// JWT DEBUG SETTINGS
// ======================================

Console.WriteLine("==============================");
Console.WriteLine("JWT VALIDATION SETTINGS");
Console.WriteLine($"Issuer: {jwtIssuer}");
Console.WriteLine($"Audience: {jwtAudience}");
Console.WriteLine($"Key Length: {jwtKey.Length}");
Console.WriteLine("==============================");
/*
var key =
    _configuration["Jwt:Key"]
    ?? throw new Exception("JWT Key missing");


Console.WriteLine("==============================");
Console.WriteLine("JWT TOKEN GENERATION SETTINGS");
Console.WriteLine($"Issuer: {_configuration["Jwt:Issuer"]}");
Console.WriteLine($"Audience: {_configuration["Jwt:Audience"]}");
Console.WriteLine($"Key Length: {key.Length}");
Console.WriteLine("==============================");
*/
// ======================================
// AUTHENTICATION (ONLY ONE JWT SETUP)
// ======================================



builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                //      IssuerSigningKey =
                //          new SymmetricSecurityKey(
                //             Encoding.UTF8.GetBytes(
                //                builder.Configuration["Jwt:Key"]!
                //            )),


                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),



                ValidateIssuer = true,
                //    ValidIssuer =
                //       builder.Configuration["Jwt:Issuer"],

                ValidateAudience = true,
                //    ValidAudience =
                //         builder.Configuration["Jwt:Audience"],




                ValidIssuer = jwtIssuer,

                ValidAudience = jwtAudience,





                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };


        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.NoResult();

                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                return Task.CompletedTask;
            },


            OnChallenge = context =>
            {
                context.HandleResponse();

                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                return Task.CompletedTask;
            }
        };
    });




/*


builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
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

                RoleClaimType = ClaimTypes.Role,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
            };


        // JWT DEBUGGING
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {

                Console.WriteLine(
                    "==============================");

                Console.WriteLine("TOKEN VALID");

                foreach (var claim in context.Principal.Claims)
                {
                    Console.WriteLine($"{claim.Type} = {claim.Value}");
                }


                Console.WriteLine(
                    "==============================");

                Console.WriteLine(
                    "JWT AUTHENTICATION FAILED");

                Console.WriteLine(
                    context.Exception.Message);

                Console.WriteLine(
                    "==============================");


                return Task.CompletedTask;
            },


            OnChallenge = context =>
            {
                Console.WriteLine(
                    "==============================");

                Console.WriteLine(
                    "JWT CHALLENGE");

                Console.WriteLine(
                    $"Error: {context.Error}");

                Console.WriteLine(
                    $"Description: {context.ErrorDescription}");

                Console.WriteLine(
                    "==============================");


                return Task.CompletedTask;
            }
        };
    });
*/


// ======================================
// AUTHORIZATION
// ======================================

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "CustomerOwner",
        policy =>
        {
            policy.RequireAuthenticatedUser();
        });
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


    context.Database.Migrate();


    DbSeeder.Seed(context);
}



// ======================================
// HTTP PIPELINE
/*
ExceptionMiddleware
        |
LoggingMiddleware
        |
Authentication
        |
Authorization
        |
Controller


Client Request

       |
       v

ExceptionMiddleware
(catch unexpected errors)

       |
       v

LoggingMiddleware
(request + response timing)

       |
       v

Controller

       |
       v

ILogger<CustomerController>

       |
       v

SQL Server
 */
// ======================================

app.UseHttpsRedirection();




// ======================================
// HTTP PIPELINE
// ======================================

// Global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Request correlation ID
app.UseMiddleware<RequestIdMiddleware>();

// Request/Response logging
app.UseMiddleware<LoggingMiddleware>();


app.UseHttpsRedirection();


app.UseAuthentication();

app.UseAuthorization();


app.MapControllers();


app.MapGet(
    "/",
    () => "Demo ERP API is running!");



app.Run();



public partial class Program { }