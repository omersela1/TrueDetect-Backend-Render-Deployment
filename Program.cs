using TrueDetectWebAPI.Extensions;
using TrueDetectWebAPI.Interfaces;
using TrueDetectWebAPI.Services;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure global request size limit (e.g., 10 MB)
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:7890")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TrueDetect API",
        Version = "v1"
    });
    
    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers();  // Register controllers
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddHttpClient("local", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["LocalApi:Endpoint"]);
});

// Register application services
builder.Services.AddApplicationServices();

builder.Services.AddKeycloakAuthentication(builder.Configuration);


var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

Console.WriteLine($"Listening on port: {port}");

var app = builder.Build();

var checker = app.Services
    .GetServices<IHostedService>()
    .OfType<ScheduleChecker>()
    .First();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();



checker.OnRetrainingTimeReached += async (_, _) =>
{
    var client = app.Services.GetRequiredService<IHttpClientFactory>()
                             .CreateClient("local");

    // 2.1 fire retraining pipeline
    var retrainResp = await client.PostAsync("/api/SendRetrainingData", null);
    if (!retrainResp.IsSuccessStatusCode)
    {
        Console.Error.WriteLine($"Retrain failed: {retrainResp.StatusCode}");
        return;
    }

    // 2.2 schedule the next run 24 hrs from now
    var nextTime = DateTime.UtcNow.AddMinutes(15)
                      .ToString("o"); // e.g. "2025-05-27T15:05:00.0000000Z"
    var schedResp = await client.PostAsync($"/api/SetRetrainingTime/{Uri.EscapeDataString(nextTime)}",
        null
    );
    if (!schedResp.IsSuccessStatusCode)
        Console.Error.WriteLine($"Reschedule failed: {schedResp.StatusCode}");
};

app.Run();
