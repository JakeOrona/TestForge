using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using TestForge.McpServer.Models;
using TestForge.McpServer.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Add MCP server with tools
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// Add configuration for appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false);
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true);

// Register HttpClient for Jira API calls
builder.Services.AddHttpClient();

// Register Jira configuration
builder.Services.Configure<JiraConfig>(builder.Configuration.GetSection("Jira"));

// Register all TestForge services
builder.Services.AddScoped<ILLMTestEnhancementService, LLMTestEnhancementService>();
builder.Services.AddScoped<ITestRailFormattingService, TestRailFormattingService>();
builder.Services.AddScoped<IUiComponentAnalysisService, UiComponentAnalysisServiceWrapper>();
builder.Services.AddScoped<IBusinessLogicAnalysisService, BusinessLogicAnalysisServiceWrapper>();

// Register other existing services (if they have interfaces)
// Note: Many existing services are static, so they don't need DI registration

var app = builder.Build();

// Initialize the TestForge tools with required services
using (var scope = app.Services.CreateScope())
{
    var llmService = scope.ServiceProvider.GetRequiredService<ILLMTestEnhancementService>();
    var testRailService = scope.ServiceProvider.GetRequiredService<ITestRailFormattingService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    TestForge.McpServer.TestForgeTools.Initialize(llmService, testRailService, logger);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.MapMcp("/mcp");

app.Run();
