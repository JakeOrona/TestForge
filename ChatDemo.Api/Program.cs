using ChatDemo.Api.Services;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add HttpClient for communicating with MCP server
builder.Services.AddHttpClient();

// Configure ChatGPT/OpenAI client
builder.Services.AddSingleton<IChatClient>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    
    // Get OpenAI configuration from appsettings
    var apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey is required");
    var modelName = configuration["OpenAI:ModelName"] ?? "gpt-4o-mini";
    
    // Check if we have the required configuration
    if (string.IsNullOrEmpty(apiKey))
    {
        throw new InvalidOperationException("OpenAI API Key is not configured");
    }

    // Create OpenAI ChatClient and convert to IChatClient using AsIChatClient() extension
    return new OpenAI.Chat.ChatClient(modelName, apiKey).AsIChatClient();
});

// Register the ChatbotAIService
builder.Services.AddScoped<ChatbotAIService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200", "http://localhost:4201")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Use CORS
app.UseCors("AllowAngularApp");

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
