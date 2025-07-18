using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using TestForge.McpServer.Models;
using TestForge.McpServer.Services;

namespace TestForge.McpServer;

/// <summary>
/// Thin controller layer for MCP tools that delegates to feature-focused services
/// </summary>
[McpServerToolType]
public static class TestForgeTools
{
    /// <summary>
    /// Gets current date and time
    /// </summary>
    /// <returns>Current date and time formatted string</returns>
    [McpServerTool, Description("Gets current date and time.")]
    public static string GetCurrentTime() => $"Current time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

    /// <summary>
    /// Analyzes a Jira ticket and returns structured data optimized for LLM enhancement and intelligent test case generation
    /// </summary>
    /// <param name="ticketId">The Jira ticket ID (e.g., DEV-15860)</param>
    /// <param name="jiraBaseUrl">The Jira base URL (optional, uses config if not provided)</param>
    /// <param name="username">Username for Jira authentication (optional)</param>
    /// <param name="apiToken">API token for Jira authentication (optional)</param>
    /// <returns>Comprehensive JSON analysis including UI components, business logic, complexity scoring, and LLM guidance</returns>
    [McpServerTool, Description("Analyzes a Jira ticket and returns structured data optimized for LLM enhancement. Provides comprehensive analysis of UI components, business logic, integration points, and baseline test cases with confidence scores.")]
    public static string AnalyzeJiraTicketForLLM(
        string ticketId,
        string jiraBaseUrl = "",
        string username = "",
        string apiToken = "")
    {
        try
        {
            // For now, return a simple structured response - TODO: Implement actual Jira API integration
            if (string.IsNullOrWhiteSpace(ticketId))
            {
                return JsonSerializer.Serialize(new { error = "Ticket ID is required" });
            }

            // Create a basic analysis response
            var analysis = new
            {
                ticketInfo = new
                {
                    key = ticketId,
                    summary = "LLM Analysis Tool Ready",
                    description = "This tool provides structured analysis for LLM enhancement",
                    type = "Demo",
                    priority = "Medium",
                    status = "Ready"
                },
                complexityAnalysis = new
                {
                    uiComponents = new[]
                    {
                        new { type = "input", name = "demo_input", complexity = "medium", testAreas = new[] { "validation", "interaction" }, confidence = 0.85 }
                    },
                    businessLogic = new[]
                    {
                        new { rule = "demo_rule", description = "Example business logic", confidence = 0.9, testScenarios = new[] { "valid case", "invalid case" } }
                    }
                },
                suggestedTestAreas = new
                {
                    functional = new[] { "basic functionality", "error handling" },
                    ui = new[] { "user interface", "interaction" },
                    integration = new[] { "API integration", "data flow" }
                },
                baselineTestCases = new[]
                {
                    new { type = "happy_path", title = "Basic functionality test", confidence = 0.95 }
                },
                llmGuidance = new
                {
                    focusAreas = new[] { "UI validation", "business logic" },
                    suggestedPrompts = new[] { "Expand with specific test scenarios", "Add edge case considerations" },
                    complexityScore = 6.5,
                    testingStrategy = "Focus on structured analysis and LLM enhancement"
                }
            };
            
            return JsonSerializer.Serialize(analysis, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = $"Analysis failed: {ex.Message}" });
        }
    }

    /// <summary>
    /// Analyzes raw Jira XML and returns structured data optimized for LLM enhancement
    /// </summary>
    /// <param name="jiraXml">The raw Jira XML content to analyze</param>
    /// <returns>Comprehensive JSON analysis including UI components, business logic, complexity scoring, and LLM guidance</returns>
    [McpServerTool, Description("Analyzes raw Jira XML and returns structured data optimized for LLM enhancement. Provides comprehensive analysis of UI components, business logic, integration points, and baseline test cases with confidence scores.")]
    public static string AnalyzeJiraXmlForLLM(string jiraXml)
        => JiraXmlAnalysisService.AnalyzeForLLM(jiraXml);

    /// <summary>
    /// Generates test case templates that LLM can enhance and expand upon based on ticket type and complexity
    /// </summary>
    /// <param name="ticketType">The type of ticket (Story, Bug, Epic, Task, etc.)</param>
    /// <param name="priority">Priority level (High, Medium, Low, Critical)</param>
    /// <param name="component">Component or area (UI, API, Database, Integration, etc.)</param>
    /// <returns>Structured test case templates with metadata for LLM enhancement</returns>
    [McpServerTool, Description("Generates baseline test case templates that LLM can enhance and expand. Provides structured templates for different scenarios with metadata and guidance for intelligent expansion.")]
    public static string GenerateTestCaseTemplates(
        string ticketType,
        string priority = "Medium",
        string component = "UI")
        => TestCaseTemplateService.GenerateTemplates(ticketType, priority, component);

    /// <summary>
    /// Extracts and categorizes UI components from a ticket description for targeted test planning
    /// </summary>
    /// <param name="description">The ticket description or requirements text</param>
    /// <returns>Structured analysis of UI components with complexity scoring and test recommendations</returns>
    [McpServerTool, Description("Extracts and categorizes UI components from ticket descriptions. Identifies forms, buttons, modals, navigation elements, and provides complexity scoring with test area recommendations.")]
    public static string ExtractUIComponentsAnalysis(string description)
        => UiComponentAnalysisService.ExtractComponents(description);

    /// <summary>
    /// Identifies business rules and validation logic from requirements for comprehensive test coverage
    /// </summary>
    /// <param name="description">The ticket description or requirements text</param>
    /// <returns>Structured analysis of business logic with confidence scores and test scenario suggestions</returns>
    [McpServerTool, Description("Identifies business rules and validation logic from ticket descriptions. Extracts validation rules, business constraints, and provides test scenario recommendations with confidence scoring.")]
    public static string ExtractBusinessLogicAnalysis(string description)
        => BusinessLogicAnalysisService.ExtractLogic(description);

    /// <summary>
    /// Parses Jira story ticket XML and generates TestRail-compatible test cases with proper structure, steps, and metadata
    /// </summary>
    /// <param name="jiraXml">The Jira ticket XML content to parse</param>
    /// <returns>TestRail-formatted test cases with detailed steps and expected results</returns>
    [McpServerTool, Description("Parses Jira story ticket XML and generates TestRail-compatible test cases with proper structure, steps, and metadata.")]
    public static string GenerateTestCasesFromJiraXml(string jiraXml)
        => TestRailGenerationService.GenerateFromXml(jiraXml);

    /// <summary>
    /// Validates and diagnoses XML structure issues before parsing
    /// </summary>
    /// <param name="jiraXml">The Jira XML content to validate</param>
    /// <returns>Detailed validation results and diagnostic information</returns>
    [McpServerTool, Description("Validates Jira XML structure and provides detailed diagnostic information about parsing issues.")]
    public static string ValidateJiraXml(string jiraXml)
        => JiraXmlValidationService.Validate(jiraXml);

    /// <summary>
    /// Cleans raw Jira XML exports to fix common formatting issues, missing closing tags, and malformed content
    /// </summary>
    /// <param name="rawXml">The raw Jira XML content to clean</param>
    /// <returns>Cleaned XML with processing information</returns>
    [McpServerTool, Description("Cleans raw Jira XML exports to fix common formatting issues, missing closing tags, and malformed content. Use this before parsing if you encounter XML errors.")]
    public static string CleanJiraXml(string rawXml)
        => JiraXmlCleaningService.Clean(rawXml);
}
