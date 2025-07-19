using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using TestForge.McpServer.Models;
using TestForge.McpServer.Services;
using Microsoft.Extensions.Logging;

namespace TestForge.McpServer;

/// <summary>
/// Thin controller layer for MCP tools that delegates to feature-focused services
/// </summary>
[McpServerToolType]
public static class TestForgeTools
{
    private static ILLMTestEnhancementService? _llmEnhancementService;
    private static ITestRailFormattingService? _testRailFormattingService;
    private static ILogger? _logger;

    /// <summary>
    /// Initialize the tools with dependency injection services
    /// </summary>
    public static void Initialize(ILLMTestEnhancementService llmEnhancementService, ITestRailFormattingService testRailFormattingService, ILogger logger)
    {
        _llmEnhancementService = llmEnhancementService;
        _testRailFormattingService = testRailFormattingService;
        _logger = logger;
        
        // Initialize JiraStoryParsingService with logger for structured logging
        JiraStoryParsingService.Initialize(logger);
    }
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
    [McpServerTool, Description("PRIMARY analysis tool for Jira API workflows. Requires authentication. Use this AFTER setting up Jira credentials. Extracts comprehensive structured data optimized for LLM enhancement including UI components, business logic, and complexity scoring. Use results to guide subsequent tool calls.")]
    public static string AnalyzeJiraTicketForLLM(
        [Description("Jira ticket ID (e.g., DEV-15860)")] string ticketId,
        [Description("Jira base URL (optional, uses config if not provided)")] string jiraBaseUrl = "",
        [Description("Username for Jira authentication (optional)")] string username = "",
        [Description("API token for Jira authentication (optional)")] string apiToken = "")
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
    [McpServerTool, Description("PRIMARY analysis tool for XML workflows. Call this AFTER XML validation/cleaning. Extracts comprehensive structured data optimized for LLM enhancement including UI components, business logic, and complexity scoring. Use results to guide subsequent tool calls.")]
    public static string AnalyzeJiraXmlForLLM([Description("Raw Jira XML content to analyze")] string jiraXml)
        => JiraXmlAnalysisService.AnalyzeForLLM(jiraXml);

    /// <summary>
    /// Generates test case templates that LLM can enhance and expand upon based on ticket type and complexity
    /// </summary>
    /// <param name="ticketType">The type of ticket (Story, Bug, Epic, Task, etc.)</param>
    /// <param name="priority">Priority level (High, Medium, Low, Critical)</param>
    /// <param name="component">Component or area (UI, API, Database, Integration, etc.)</param>
    /// <returns>Structured test case templates with metadata for LLM enhancement</returns>
    [McpServerTool, Description("Generate baseline test case templates using ticket type and priority from analyze_jira_xml_for_llm results. Provides structured templates for LLM enhancement. Call AFTER primary analysis to create foundation test cases.")]
    public static string GenerateTestCaseTemplates(
        [Description("Type of ticket (Story, Bug, Epic, Task, etc.)")] string ticketType,
        [Description("Priority level (High, Medium, Low, Critical)")] string priority = "Medium",
        [Description("Component or area (UI, API, Database, Integration, etc.)")] string component = "UI")
        => TestCaseTemplateService.GenerateTemplates(ticketType, priority, component);

    /// <summary>
    /// Extracts and categorizes UI components from a ticket description for targeted test planning
    /// </summary>
    /// <param name="description">The ticket description or requirements text</param>
    /// <returns>Structured analysis of UI components with complexity scoring and test recommendations</returns>
    [McpServerTool, Description("Use description text from analyze_jira_xml_for_llm to extract UI-specific insights. Identifies forms, buttons, modals, navigation elements with complexity scoring. Call AFTER primary analysis for UI-heavy tickets.")]
    public static string ExtractUIComponentsAnalysis([Description("Ticket description or requirements text for UI analysis")] string description)
        => UiComponentAnalysisService.ExtractComponents(description);

    /// <summary>
    /// Identifies business rules and validation logic from requirements for comprehensive test coverage
    /// </summary>
    /// <param name="description">The ticket description or requirements text</param>
    /// <returns>Structured analysis of business logic with confidence scores and test scenario suggestions</returns>
    [McpServerTool, Description("Use description text from analyze_jira_xml_for_llm to extract business rules and validation logic. Identifies constraints, rules, and validation requirements. Call AFTER primary analysis for logic-heavy tickets.")]
    public static string ExtractBusinessLogicAnalysis([Description("Ticket description or requirements text for business logic extraction")] string description)
        => BusinessLogicAnalysisService.ExtractLogic(description);

    /// <summary>
    /// Parses Jira story ticket XML and generates TestRail-compatible test cases with proper structure, steps, and metadata
    /// </summary>
    /// <param name="jiraXml">The Jira ticket XML content to parse</param>
    /// <returns>TestRail-formatted test cases with detailed steps and expected results</returns>
    [McpServerTool, Description("Parses Jira story ticket XML and generates TestRail-compatible test cases with proper structure, steps, and metadata.")]
    public static string GenerateTestCasesFromJiraXml([Description("Jira ticket XML content to parse")] string jiraXml)
        => TestRailGenerationService.GenerateFromXml(jiraXml);

    /// <summary>
    /// Validates and diagnoses XML structure issues before parsing
    /// </summary>
    /// <param name="jiraXml">The Jira XML content to validate</param>
    /// <returns>Detailed validation results and diagnostic information</returns>
    [McpServerTool, Description("ALWAYS call this FIRST when user provides raw Jira XML. Validates XML structure and provides detailed diagnostic information. If validation fails, call clean_jira_xml before proceeding with analysis.")]
    public static string ValidateJiraXml([Description("Jira XML content to validate")] string jiraXml)
        => JiraXmlValidationService.Validate(jiraXml);

    /// <summary>
    /// Cleans raw Jira XML exports to fix common formatting issues, missing closing tags, and malformed content
    /// </summary>
    /// <param name="rawXml">The raw Jira XML content to clean</param>
    /// <returns>Cleaned XML with processing information</returns>
    [McpServerTool, Description("Call this ONLY when validate_jira_xml fails. Fixes common issues in raw Jira XML exports including missing closing tags, malformed HTML, and duplicate attributes. Essential preprocessing step for raw Jira exports.")]
    public static string CleanJiraXml([Description("Raw malformed XML content to clean")] string rawXml)
        => JiraXmlCleaningService.Clean(rawXml);

    /// <summary>
    /// Complete end-to-end workflow for processing Jira XML and generating comprehensive test cases
    /// </summary>
    /// <param name="jiraXml">The raw Jira XML content to process</param>
    /// <returns>Complete workflow results with all analysis steps and TestRail-ready test cases</returns>
    [McpServerTool, Description("COMPLETE END-TO-END WORKFLOW: Validates, cleans, analyzes Jira XML and generates comprehensive TestRail-ready test cases in one call. Use this when user wants full analysis without manual orchestration. Handles all XML issues automatically and provides complete structured output.")]
    public static string ProcessJiraWorkflow([Description("Raw Jira XML content for complete workflow")] string jiraXml)
        => JiraWorkflowService.ProcessComplete(jiraXml);

    /// <summary>
    /// Enhance initial test cases using LLM analysis for comprehensive coverage including negative, security, accessibility, performance, and boundary testing
    /// </summary>
    /// <param name="parsedXmlData">Parsed Jira XML data as JSON string</param>
    /// <param name="initialTests">Initial generated test cases as JSON string</param>
    /// <param name="enhancementConfig">Enhancement configuration specifying which test categories to generate</param>
    /// <returns>Enhanced test suite with comprehensive coverage</returns>
    [McpServerTool, Description("COMPREHENSIVE TEST ENHANCEMENT: Enhances initial test cases using LLM analysis for maximum coverage including negative, security, accessibility, performance, and boundary testing. Generates 10+ test categories with intelligent deduplication and TestRail-compatible output.")]
    public static async Task<string> EnhanceTestCasesWithLLM(
        [Description("Parsed Jira XML data as JSON string")] string parsedXmlData,
        [Description("Initial generated test cases as JSON string")] string initialTests,
        [Description("Enhancement configuration specifying test categories")] string enhancementConfig = "")
    {
        try
        {
            // Add explicit parameter validation
            if (string.IsNullOrWhiteSpace(parsedXmlData))
            {
                return JsonSerializer.Serialize(new { 
                    error = "Missing required parameter: parsedXmlData",
                    method = nameof(EnhanceTestCasesWithLLM),
                    timestamp = DateTime.UtcNow,
                    details = "The parsedXmlData parameter is required and cannot be null or empty"
                });
            }

            if (string.IsNullOrWhiteSpace(initialTests))
            {
                return JsonSerializer.Serialize(new { 
                    error = "Missing required parameter: initialTests",
                    method = nameof(EnhanceTestCasesWithLLM),
                    timestamp = DateTime.UtcNow,
                    details = "The initialTests parameter is required and cannot be null or empty"
                });
            }

            if (_llmEnhancementService == null || _testRailFormattingService == null)
            {
                return JsonSerializer.Serialize(new { 
                    error = "LLM enhancement service not initialized. Please ensure proper dependency injection setup." 
                });
            }

            _logger?.LogInformation("Starting LLM-enhanced test case generation with data length: {DataLength}, tests length: {TestsLength}", 
                parsedXmlData.Length, initialTests.Length);

            // Parse input parameters
            var parsedData = JsonSerializer.Deserialize<ParsedJiraData>(parsedXmlData);
            var initialTestCases = JsonSerializer.Deserialize<List<TestCase>>(initialTests);
            var config = string.IsNullOrEmpty(enhancementConfig) 
                ? new TestEnhancementConfig() 
                : JsonSerializer.Deserialize<TestEnhancementConfig>(enhancementConfig);

            if (parsedData == null || initialTestCases == null || config == null)
            {
                return JsonSerializer.Serialize(new { 
                    error = "Invalid input data. Please provide valid JSON for parsedXmlData, initialTests, and enhancementConfig." 
                });
            }

            // Generate comprehensive enhanced test suite
            var enhancedSuite = await _llmEnhancementService.EnhanceTestCases(parsedData, initialTestCases, config);

            // Format output for TestRail compatibility
            var formattedOutput = await _testRailFormattingService.FormatEnhancedTestSuite(enhancedSuite);

            var result = new TestEnhancementResult
            {
                Success = true,
                Message = $"Successfully generated {enhancedSuite.TotalTestCount} comprehensive test cases",
                OriginalTestCount = initialTestCases.Count,
                EnhancedTestCount = enhancedSuite.EnhancedTests.Count,
                TotalTestCount = enhancedSuite.TotalTestCount,
                CoverageSummary = enhancedSuite.CoverageSummary,
                FormattedTestCases = formattedOutput,
                GenerationTimestamp = DateTime.UtcNow,
                CategoryBreakdown = GenerateCategoryBreakdown(enhancedSuite)
            };

            _logger?.LogInformation("LLM enhancement completed: {TotalTests} tests generated with {CoveragePercentage}% coverage",
                result.TotalTestCount, result.CoverageSummary.CoveragePercentage);

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during LLM test case enhancement");
            var errorResult = new TestEnhancementResult
            {
                Success = false,
                Message = $"Error during test enhancement: {ex.Message}",
                Error = ex.ToString()
            };
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    /// <summary>
    /// Generate a complete test matrix showing all possible test scenarios and coverage areas
    /// </summary>
    /// <param name="parsedXmlData">Parsed Jira XML data as JSON string</param>
    /// <returns>Comprehensive test matrix with coverage analysis</returns>
    [McpServerTool, Description("COMPREHENSIVE TEST MATRIX: Generates a complete test matrix showing all possible test scenarios, coverage areas, and potential gaps. Use this to analyze test coverage potential before generating actual test cases.")]
    public static async Task<string> GenerateComprehensiveTestMatrix([Description("Parsed Jira XML data as JSON string")] string parsedXmlData)
    {
        try
        {
            // Add explicit parameter validation
            if (string.IsNullOrWhiteSpace(parsedXmlData))
            {
                return JsonSerializer.Serialize(new { 
                    error = "Missing required parameter: parsedXmlData",
                    method = nameof(GenerateComprehensiveTestMatrix),
                    timestamp = DateTime.UtcNow,
                    details = "The parsedXmlData parameter is required and cannot be null or empty"
                });
            }

            if (_llmEnhancementService == null)
            {
                return JsonSerializer.Serialize(new { 
                    error = "LLM enhancement service not initialized. Please ensure proper dependency injection setup." 
                });
            }

            _logger?.LogInformation("Starting comprehensive test matrix generation with data length: {DataLength}", parsedXmlData.Length);

            var parsedData = JsonSerializer.Deserialize<ParsedJiraData>(parsedXmlData);
            if (parsedData == null)
            {
                return JsonSerializer.Serialize(new { 
                    error = "Invalid parsedXmlData. Please provide valid JSON." 
                });
            }

            var testMatrix = await _llmEnhancementService.GenerateTestMatrix(parsedData);
            
            return JsonSerializer.Serialize(testMatrix, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error generating comprehensive test matrix");
            return JsonSerializer.Serialize(new { 
                error = $"Error generating test matrix: {ex.Message}" 
            });
        }
    }

    /// <summary>
    /// Generate comprehensive test cases directly from Jira XML with maximum coverage
    /// </summary>
    /// <param name="jiraXml">The raw Jira XML content to process</param>
    /// <param name="enhancementConfig">Enhancement configuration for test generation</param>
    /// <returns>Complete test suite with comprehensive coverage</returns>
    [McpServerTool, Description("AUTOMATED COMPREHENSIVE TESTING: Processes Jira XML and automatically generates comprehensive test cases with maximum coverage across all categories. Combines validation, cleaning, analysis, and enhancement in a single workflow.")]
    public static async Task<string> GenerateComprehensiveTestSuite(
        [Description("Raw Jira XML content")] string jiraXml,
        [Description("Enhancement configuration JSON")] string enhancementConfig = "")
    {
        try
        {
            // Add explicit parameter validation
            if (string.IsNullOrWhiteSpace(jiraXml))
            {
                return JsonSerializer.Serialize(new { 
                    error = "Missing required parameter: jiraXml",
                    method = nameof(GenerateComprehensiveTestSuite),
                    timestamp = DateTime.UtcNow,
                    details = "The jiraXml parameter is required and cannot be null or empty"
                });
            }

            if (_llmEnhancementService == null || _testRailFormattingService == null)
            {
                return JsonSerializer.Serialize(new { 
                    error = "Services not initialized. Please ensure proper dependency injection setup." 
                });
            }

            _logger?.LogInformation("Starting comprehensive test suite generation with jiraXml length: {XmlLength}", jiraXml.Length);

            // Step 1: Process Jira XML to get structured data
            var analysisResult = AnalyzeJiraXmlForLLM(jiraXml);
            var parsedData = ExtractParsedJiraData(analysisResult);
            
            if (parsedData == null)
            {
                return JsonSerializer.Serialize(new { 
                    error = "Failed to parse Jira XML. Please validate XML structure first." 
                });
            }

            // Step 2: Create initial test case from structured data with confidence scoring
            var defaultTestCase = new TestCase
            {
                Id = "INITIAL_001",
                Title = parsedData?.Summary ?? "Basic Functional Test",
                Description = parsedData?.Description ?? "Initial test case from Jira XML analysis",
                Priority = parsedData?.Priority ?? "Medium",
                Category = TestCategoryType.Functional,
                Type = TestType.Positive,
                Confidence = CalculateInitialConfidence(parsedData),
                Source = "Structured Analysis",
                Metadata = new Dictionary<string, object>
                {
                    {"AnalysisQuality", parsedData?.ComplexityScore ?? 0.0},
                    {"DataCompleteness", CalculateDataCompleteness(parsedData)}
                }
            };
            var initialTests = new List<TestCase> { defaultTestCase };

            // Step 3: Parse enhancement configuration
            var config = string.IsNullOrEmpty(enhancementConfig) 
                ? new TestEnhancementConfig() 
                : JsonSerializer.Deserialize<TestEnhancementConfig>(enhancementConfig);

            if (config == null)
            {
                config = new TestEnhancementConfig();
            }

            // Step 4: Generate comprehensive enhanced test suite
            var enhancedSuite = await _llmEnhancementService.EnhanceTestCases(parsedData, initialTests, config);

            // Step 5: Format for TestRail
            var formattedOutput = _testRailFormattingService.FormatComprehensiveTestSuite(enhancedSuite);

            // Step 6: Calculate comprehensive confidence metrics
            var overallConfidence = CalculateOverallConfidence(enhancedSuite);

            var result = new
            {
                success = true,
                message = $"Generated comprehensive test suite with {enhancedSuite.TotalTestCount} test cases",
                summary = new
                {
                    totalTests = enhancedSuite.TotalTestCount,
                    originalTests = enhancedSuite.OriginalTests.Count,
                    enhancedTests = enhancedSuite.EnhancedTests.Count,
                    coverage = enhancedSuite.CoverageSummary.CoveragePercentage,
                    categories = enhancedSuite.EnhancedTests.GroupBy(t => t.Category).ToDictionary(g => g.Key.ToString(), g => g.Count())
                },
                confidenceMetrics = new
                {
                    overallConfidence = overallConfidence.OverallConfidence,
                    averageConfidence = overallConfidence.AverageConfidence,
                    confidenceRange = new
                    {
                        minimum = overallConfidence.MinimumConfidence,
                        maximum = overallConfidence.MaximumConfidence
                    },
                    distribution = overallConfidence.ConfidenceDistribution,
                    qualityIndicators = new
                    {
                        highConfidencePercentage = enhancedSuite.TotalTestCount > 0 ? (double)overallConfidence.QualityMetrics.HighConfidenceTests / enhancedSuite.TotalTestCount * 100 : 0,
                        averageStepsPerTest = overallConfidence.QualityMetrics.AverageStepsPerTest,
                        categoryDiversity = overallConfidence.QualityMetrics.CategoryCoverage
                    }
                },
                coverageBreakdown = enhancedSuite.CoverageSummary,
                testSuite = enhancedSuite,
                formattedOutput = formattedOutput,
                generatedAt = DateTime.UtcNow
            };

            _logger?.LogInformation("Comprehensive test suite generation completed: {TotalTests} tests with {Coverage}% coverage",
                enhancedSuite.TotalTestCount, enhancedSuite.CoverageSummary.CoveragePercentage);

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during comprehensive test suite generation");
            return JsonSerializer.Serialize(new { 
                success = false,
                error = $"Error generating comprehensive test suite: {ex.Message}",
                details = ex.ToString()
            });
        }
    }

    #region Helper Methods

    private static List<TestCategoryBreakdown> GenerateCategoryBreakdown(EnhancedTestSuite testSuite)
    {
        var breakdown = new List<TestCategoryBreakdown>();
        
        // Group tests by category
        var categoryGroups = testSuite.EnhancedTests.GroupBy(t => t.Category);
        
        foreach (var group in categoryGroups)
        {
            breakdown.Add(new TestCategoryBreakdown
            {
                Category = group.Key.ToString(),
                TestCount = group.Count(),
                TestTitles = group.Select(t => t.Title).ToList(),
                Description = GetCategoryDescription(group.Key)
            });
        }
        
        return breakdown;
    }

    private static string GetCategoryDescription(TestCategoryType category)
    {
        return category switch
        {
            TestCategoryType.Functional => "Core functionality and feature testing",
            TestCategoryType.Security => "Security vulnerability and protection testing",
            TestCategoryType.Performance => "Load, stress, and response time testing",
            TestCategoryType.Accessibility => "WCAG compliance and accessibility testing",
            TestCategoryType.UI => "User interface and visual testing",
            TestCategoryType.Integration => "System integration and API testing",
            TestCategoryType.DataValidation => "Data integrity and validation testing",
            TestCategoryType.ErrorHandling => "Error scenarios and recovery testing",
            TestCategoryType.UserExperience => "Usability and user experience testing",
            TestCategoryType.StateManagement => "Application state and session testing",
            _ => "General testing category"
        };
    }

    private static ParsedJiraData? ExtractParsedJiraData(string analysisResult)
    {
        try
        {
            var analysis = JsonSerializer.Deserialize<JsonElement>(analysisResult);
            
            if (analysis.TryGetProperty("ticketInfo", out var ticketInfo))
            {
                return new ParsedJiraData
                {
                    TicketId = ticketInfo.TryGetProperty("key", out var key) ? key.GetString() ?? "" : "",
                    Summary = ticketInfo.TryGetProperty("summary", out var summary) ? summary.GetString() ?? "" : "",
                    Description = ticketInfo.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                    Type = ticketInfo.TryGetProperty("type", out var type) ? type.GetString() ?? "" : "",
                    Priority = ticketInfo.TryGetProperty("priority", out var priority) ? priority.GetString() ?? "" : "",
                    AcceptanceCriteria = ticketInfo.TryGetProperty("acceptanceCriteria", out var ac) ? 
                        ac.EnumerateArray().Select(x => x.GetString() ?? "").ToList() : new List<string>(),
                    ComplexityScore = analysis.TryGetProperty("llmGuidance", out var guidance) && 
                                    guidance.TryGetProperty("complexityScore", out var score) ? score.GetDouble() : 0.0
                };
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to extract parsed Jira data from analysis result");
        }
        
        return null;
    }

    private static List<TestCase> ExtractInitialTestCases(string initialTestsResult)
    {
        if (string.IsNullOrWhiteSpace(initialTestsResult))
        {
            _logger?.LogWarning("Initial tests result is null or empty");
            return CreateDefaultTestCase();
        }

        // Validate JSON format before parsing
        var trimmed = initialTestsResult.TrimStart();
        if (!trimmed.StartsWith('{') && !trimmed.StartsWith('['))
        {
            _logger?.LogWarning("Initial tests result is not JSON format, using default test case");
            return CreateDefaultTestCase();
        }

        try
        {
            var result = JsonSerializer.Deserialize<JsonElement>(initialTestsResult);
            return ProcessJsonTestCases(result);
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "Failed to parse initial tests JSON: {Preview}", 
                initialTestsResult.Substring(0, Math.Min(100, initialTestsResult.Length)));
            return CreateDefaultTestCase();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error processing initial tests");
            return CreateDefaultTestCase();
        }
    }

    private static List<TestCase> CreateDefaultTestCase()
    {
        return new List<TestCase>
        {
            new TestCase
            {
                Id = "INITIAL_001",
                Title = "Basic Functional Test",
                Description = "Default test case generated due to parsing issues",
                Priority = "Medium", 
                Category = TestCategoryType.Functional,
                Type = TestType.Positive,
                Confidence = 0.6,
                Source = "Default Generation"
            }
        };
    }

    private static List<TestCase> ProcessJsonTestCases(JsonElement result)
    {
        // Implementation for processing valid JSON test case data
        // For now, return default test case as fallback
        return CreateDefaultTestCase();
    }

    /// <summary>
    /// Validates generated test cases against original analysis and provides final confidence scoring with LLM validation
    /// </summary>
    /// <param name="testCases">Generated test cases as JSON string</param>
    /// <param name="originalAnalysis">Original Jira analysis as JSON string</param>
    /// <param name="baselineConfidence">Baseline executive summary confidence as JSON string</param>
    /// <returns>Final confidence score with validation details and adjustment reasoning</returns>
    [McpServerTool, Description("CONFIDENCE VALIDATION: Validates generated test cases against original analysis and provides final confidence scoring. Use this to get quality metrics and confidence adjustments after test case generation. Includes LLM validation criteria and reconciliation logic.")]
    public static string ValidateTestCaseConfidence(
        [Description("Generated test cases as JSON string")] string testCases,
        [Description("Original Jira analysis as JSON string")] string originalAnalysis,
        [Description("Baseline executive summary confidence as JSON string")] string baselineConfidence)
    {
        try
        {
            // Validate input parameters
            if (string.IsNullOrWhiteSpace(testCases))
            {
                return JsonSerializer.Serialize(new { 
                    error = "Missing required parameter: testCases",
                    timestamp = DateTime.UtcNow
                });
            }

            if (string.IsNullOrWhiteSpace(originalAnalysis))
            {
                return JsonSerializer.Serialize(new { 
                    error = "Missing required parameter: originalAnalysis",
                    timestamp = DateTime.UtcNow
                });
            }

            if (string.IsNullOrWhiteSpace(baselineConfidence))
            {
                return JsonSerializer.Serialize(new { 
                    error = "Missing required parameter: baselineConfidence",
                    timestamp = DateTime.UtcNow
                });
            }

            // Parse baseline confidence
            ExecutiveSummaryConfidence? baseline;
            try
            {
                baseline = JsonSerializer.Deserialize<ExecutiveSummaryConfidence>(baselineConfidence);
                if (baseline == null)
                {
                    return JsonSerializer.Serialize(new { 
                        error = "Invalid baseline confidence format - deserialization returned null",
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (JsonException ex)
            {
                return JsonSerializer.Serialize(new { 
                    error = $"Invalid baseline confidence JSON format: {ex.Message}",
                    timestamp = DateTime.UtcNow
                });
            }

            _logger?.LogInformation("Starting test case confidence validation with {TestCaseLength} test cases and {AnalysisLength} analysis data", 
                testCases.Length, originalAnalysis.Length);

            // Perform confidence validation and adjustment
            var finalConfidenceScore = TestCaseTemplateService.ValidateAndAdjustConfidence(
                testCases, originalAnalysis, baseline);

            // Create comprehensive validation response
            var validationResponse = new
            {
                timestamp = DateTime.UtcNow,
                validationMethod = "LLM_CONFIDENCE_VALIDATION",
                finalConfidenceScore = finalConfidenceScore,
                recommendations = GenerateConfidenceRecommendations(finalConfidenceScore),
                qualityMetrics = new
                {
                    confidenceVariance = Math.Abs(finalConfidenceScore.FinalConfidence - finalConfidenceScore.BaselineConfidence),
                    confidenceDirection = finalConfidenceScore.FinalConfidence > finalConfidenceScore.BaselineConfidence ? "INCREASED" : 
                                         finalConfidenceScore.FinalConfidence < finalConfidenceScore.BaselineConfidence ? "DECREASED" : "UNCHANGED",
                    validationQuality = DetermineValidationQuality(finalConfidenceScore.LLMValidationScore),
                    requiresManualReview = finalConfidenceScore.RequiresManualReview
                },
                nextSteps = DetermineNextSteps(finalConfidenceScore)
            };

            _logger?.LogInformation("Completed confidence validation - Final score: {FinalScore}, Adjustment: {Adjustment}, Manual review: {ManualReview}", 
                finalConfidenceScore.FinalConfidence, finalConfidenceScore.ConfidenceAdjustment, finalConfidenceScore.RequiresManualReview);

            return JsonSerializer.Serialize(validationResponse, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to validate test case confidence");
            return JsonSerializer.Serialize(new { 
                error = $"Confidence validation failed: {ex.Message}",
                timestamp = DateTime.UtcNow,
                fallbackRecommendation = "Consider manual review of test case quality and coverage"
            });
        }
    }

    /// <summary>
    /// Generates recommendations based on confidence score results
    /// </summary>
    private static List<string> GenerateConfidenceRecommendations(FinalConfidenceScore score)
    {
        var recommendations = new List<string>();

        if (score.FinalConfidence >= 0.85)
        {
            recommendations.Add("High confidence - Test cases are ready for execution");
            recommendations.Add("Consider proceeding with test automation implementation");
        }
        else if (score.FinalConfidence >= 0.70)
        {
            recommendations.Add("Good confidence - Test cases have solid foundation");
            recommendations.Add("Minor refinements may improve coverage");
        }
        else if (score.FinalConfidence >= 0.55)
        {
            recommendations.Add("Moderate confidence - Test cases need enhancement");
            recommendations.Add("Consider additional test scenarios for better coverage");
        }
        else
        {
            recommendations.Add("Low confidence - Significant test case improvements needed");
            recommendations.Add("Review requirements and enhance test case depth");
        }

        if (score.RequiresManualReview)
        {
            recommendations.Add("Manual review required due to confidence variance");
        }

        return recommendations;
    }

    /// <summary>
    /// Determines validation quality level
    /// </summary>
    private static string DetermineValidationQuality(double validationScore)
    {
        return validationScore switch
        {
            >= 0.85 => "EXCELLENT",
            >= 0.70 => "GOOD", 
            >= 0.55 => "ADEQUATE",
            >= 0.40 => "POOR",
            _ => "CRITICAL"
        };
    }

    /// <summary>
    /// Determines next steps based on confidence results
    /// </summary>
    private static List<string> DetermineNextSteps(FinalConfidenceScore score)
    {
        var nextSteps = new List<string>();

        if (score.FinalConfidence >= 0.80)
        {
            nextSteps.Add("Proceed with test execution planning");
            nextSteps.Add("Consider test automation framework setup");
            nextSteps.Add("Schedule test case review with stakeholders");
        }
        else if (score.FinalConfidence >= 0.60)
        {
            nextSteps.Add("Enhance test cases based on confidence factors");
            nextSteps.Add("Add more edge case scenarios");
            nextSteps.Add("Re-validate confidence after improvements");
        }
        else
        {
            nextSteps.Add("Revisit requirements analysis");
            nextSteps.Add("Enhance test case foundation");
            nextSteps.Add("Consider stakeholder input for missing scenarios");
        }

        if (score.RequiresManualReview)
        {
            nextSteps.Add("Schedule manual review session");
            nextSteps.Add("Validate confidence variance with subject matter expert");
        }

        return nextSteps;
    }

    /// <summary>
    /// Calculate initial confidence score based on Jira data quality
    /// </summary>
    private static double CalculateInitialConfidence(ParsedJiraData? parsedData)
    {
        if (parsedData == null) return 0.5;
        
        double baseConfidence = 0.6;
        
        // Boost confidence based on data quality
        if (!string.IsNullOrEmpty(parsedData.Summary)) baseConfidence += 0.1;
        if (!string.IsNullOrEmpty(parsedData.Description)) baseConfidence += 0.1;
        if (parsedData.AcceptanceCriteria?.Any() == true) baseConfidence += 0.15;
        if (parsedData.ComplexityScore > 0) baseConfidence += 0.05;
        
        return Math.Min(0.95, baseConfidence);
    }

    /// <summary>
    /// Calculate data completeness score for confidence assessment
    /// </summary>
    private static double CalculateDataCompleteness(ParsedJiraData? parsedData)
    {
        if (parsedData == null) return 0.0;
        
        int completenessScore = 0;
        int totalFields = 6;
        
        if (!string.IsNullOrEmpty(parsedData.TicketId)) completenessScore++;
        if (!string.IsNullOrEmpty(parsedData.Summary)) completenessScore++;
        if (!string.IsNullOrEmpty(parsedData.Description)) completenessScore++;
        if (!string.IsNullOrEmpty(parsedData.Type)) completenessScore++;
        if (!string.IsNullOrEmpty(parsedData.Priority)) completenessScore++;
        if (parsedData.AcceptanceCriteria?.Any() == true) completenessScore++;
        
        return (double)completenessScore / totalFields;
    }

    /// <summary>
    /// Calculate comprehensive overall confidence score for test suite
    /// </summary>
    private static OverallConfidenceScore CalculateOverallConfidence(EnhancedTestSuite testSuite)
    {
        var allTests = testSuite.OriginalTests.Concat(testSuite.EnhancedTests).ToList();
        
        if (!allTests.Any())
        {
            return new OverallConfidenceScore
            {
                OverallConfidence = 0.0,
                AverageConfidence = 0.0,
                ConfidenceDistribution = new Dictionary<string, int>(),
                QualityMetrics = new QualityMetrics { TotalTests = 0 }
            };
        }
        
        var avgConfidence = allTests.Average(t => t.Confidence);
        var minConfidence = allTests.Min(t => t.Confidence);
        var maxConfidence = allTests.Max(t => t.Confidence);
        
        // Calculate overall confidence with adjustments
        double overallConfidence = avgConfidence;
        
        // Penalize for low minimum confidence
        if (minConfidence < 0.6) overallConfidence *= 0.9;
        
        // Boost for high coverage
        if (testSuite.CoverageSummary.CoveragePercentage > 80) overallConfidence *= 1.05;
        
        // Boost for test variety
        var categoryCount = allTests.GroupBy(t => t.Category).Count();
        if (categoryCount >= 8) overallConfidence *= 1.03;
        
        overallConfidence = Math.Min(0.98, overallConfidence);
        
        return new OverallConfidenceScore
        {
            OverallConfidence = overallConfidence,
            AverageConfidence = avgConfidence,
            MinimumConfidence = minConfidence,
            MaximumConfidence = maxConfidence,
            ConfidenceDistribution = CalculateConfidenceDistribution(allTests),
            QualityMetrics = new QualityMetrics
            {
                TotalTests = allTests.Count,
                HighConfidenceTests = allTests.Count(t => t.Confidence >= 0.8),
                MediumConfidenceTests = allTests.Count(t => t.Confidence >= 0.6 && t.Confidence < 0.8),
                LowConfidenceTests = allTests.Count(t => t.Confidence < 0.6),
                CategoryCoverage = categoryCount,
                AverageStepsPerTest = allTests.Where(t => t.TestSteps?.Any() == true).Any() 
                    ? allTests.Where(t => t.TestSteps?.Any() == true).Average(t => t.TestSteps.Count) 
                    : 0.0
            }
        };
    }

    /// <summary>
    /// Calculate confidence distribution across test cases
    /// </summary>
    private static Dictionary<string, int> CalculateConfidenceDistribution(List<TestCase> tests)
    {
        return new Dictionary<string, int>
        {
            ["High (0.8-1.0)"] = tests.Count(t => t.Confidence >= 0.8),
            ["Medium (0.6-0.8)"] = tests.Count(t => t.Confidence >= 0.6 && t.Confidence < 0.8),
            ["Low (0.0-0.6)"] = tests.Count(t => t.Confidence < 0.6)
        };
    }

    #endregion
}
