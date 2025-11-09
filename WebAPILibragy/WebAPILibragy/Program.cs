using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using WebAPILibragy.Classes;
using WebAPILibragy.DataBase;
using WebAPILibragy.model.custom;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DBConnect>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddLogging();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
// Добавляем версионирование
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = 20;
        options.Window = TimeSpan.FromSeconds(120);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 1;
    });
    rateLimiterOptions.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.Headers.RetryAfter = "1";
        context.HttpContext.Response.Headers["X-Limit-Remaining"] = "20";
        context.HttpContext.Response.Headers["Retry-After"] = "120";
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Too many requests", token);
    };
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Autorization";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("admin"));
    options.AddPolicy("RequireReadingRole", policy =>
        policy.RequireRole("admin", "Reading"));
    options.AddPolicy("RequireBookingRole", policy =>
        policy.RequireRole("admin", "Booking"));
});


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "Система учета информации о книгах и читателей библиотеки. ",
        Version = "Стандарт v1",
        License = new OpenApiLicense
        {
            Name = "Работник: Куренков Алексей",
        }
    });
    // Добавление глобального параметра (опционально)
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "IdempotenceKey"
                }
            },
            new string[] {}
        }
    });

    // Определение схемы безопасности для заголовка
    c.AddSecurityDefinition("IdempotenceKey", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Idempotence Key (GUID)",
        Name = "Idempotence-Key",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});


var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// настраиваем CORS
app.UseCors(builder => builder.WithOrigins("http://localhost:5043")
                             .AllowAnyMethod()
                             .AllowAnyHeader()
                             .WithExposedHeaders("custom-header"));

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthorization();
app.UseAuthentication();   // добавление middleware аутентификации  
app.MapControllers();
app.Use(async (context, next) =>
{
    await next();

    // Проверяем, был ли применен rate limiting
    var endpoint = context.GetEndpoint();
    if (endpoint != null)
    {
        var rateLimitMetadata = endpoint.Metadata.GetMetadata<EnableRateLimitingAttribute>();
    }
});

app.Run();
