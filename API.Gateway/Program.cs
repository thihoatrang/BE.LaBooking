using API.Gateway.Services;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;


var builder = WebApplication.CreateBuilder(args);

// Load cấu hình Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add Ocelot
builder.Services.AddOcelot();

// Add Service Discovery
builder.Services.AddScoped<IServiceDiscoveryService, ServiceDiscoveryService>();

// Add Cross-Service Saga
builder.Services.AddHttpClient<CrossServiceSagaService>();
builder.Services.AddScoped<CrossServiceSagaService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    
    options.AddPolicy("AllowMobile",
        policy =>
        {
            policy.WithOrigins(
                    "http://localhost:5173", // Frontend
                    "capacitor://localhost", // Capacitor apps
                    "ionic://localhost",     // Ionic apps
                    "http://localhost",      // Local development
                    "https://localhost"      // HTTPS local development
                   )
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Gateway", Version = "v1" });
});

// Thêm Swagger từ các microservice (dùng Ocelot.Swagger)
//options.EnableAnnotations();

// Add Controllers
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway v1");

        // Thêm Swagger từ các microservice (qua proxy)
        c.SwaggerEndpoint("/swagger/users/v1/swagger.json", "Users API v1");
        c.SwaggerEndpoint("/swagger/lawyers/v1/swagger.json", "Lawyers API v1");
        c.SwaggerEndpoint("/swagger/appointments/v1/swagger.json", "Appointments API v1");
        c.SwaggerEndpoint("/swagger/chat/v1/swagger.json", "Chat API v1");
        //c.SwaggerEndpoint("/swagger/payments/swagger.json", "Payments API");
    });
}

//// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}


app.UseCors("AllowMobile");



// Map controllers
app.MapControllers();

// Use Ocelot
await app.UseOcelot();

app.Run();
