using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Chat API", Version = "v1" });
    c.EnableAnnotations();
    c.AddServer(new OpenApiServer { Url = "https://localhost:5000/api/chat" });
});

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<Chat.Application.Services.OpenAIChatService>();
builder.Services.AddHttpClient<Chat.Application.Services.RetrievalService>();
builder.Services.AddHttpClient<Chat.Application.Services.SimpleEmbeddingService>();
builder.Services.AddHttpClient<Chat.Application.Services.LegalDocumentService>();

// Redis with fallback
builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
{
    try
    {
        var connectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
        return StackExchange.Redis.ConnectionMultiplexer.Connect(connectionString);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Cannot connect to Redis: {ex.Message}");
        Console.WriteLine("Service will run with fallback mode (no vector search)");
        // Return a mock connection multiplexer
        return null!;
    }
});

// Services
builder.Services.AddScoped<Chat.Application.Services.IChatService, Chat.Application.Services.OpenAIChatService>();
builder.Services.AddScoped<Chat.Application.Services.IRetrievalService, Chat.Application.Services.RetrievalService>();
builder.Services.AddScoped<Chat.Application.Services.IEmbeddingService, Chat.Application.Services.SimpleEmbeddingService>();
builder.Services.AddScoped<Chat.Application.Services.ILegalDocumentService, Chat.Application.Services.LegalDocumentService>();

// Vector Store with fallback
builder.Services.AddScoped<Chat.Application.Services.IVectorStoreService>(sp =>
{
    var redis = sp.GetService<StackExchange.Redis.IConnectionMultiplexer>();
    if (redis != null)
    {
        return new Chat.Application.Services.RedisVectorStoreService(redis, sp.GetRequiredService<Chat.Application.Services.IEmbeddingService>());
    }
    else
    {
        return new Chat.Application.Services.MockVectorStoreService();
    }
});

builder.Services.AddScoped<Chat.Application.Services.IKnowledgeService, Chat.Application.Services.KnowledgeService>();

// CORS policy for frontend
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend", policy =>
	{
		policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Chat API v1");
    });
}
// Use CORS
app.UseCors("AllowFrontend");

app.MapControllers();

app.UseHttpsRedirection();
app.Run();
