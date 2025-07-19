using System.Text.Json;
using System.Text.RegularExpressions;
using TestForge.McpServer.Models;
using Microsoft.Extensions.Logging;

namespace TestForge.McpServer.Services;

/// <summary>
/// Interface for LLM-enhanced test case generation with comprehensive coverage
/// </summary>
public interface ILLMTestEnhancementService
{
    Task<EnhancedTestSuite> EnhanceTestCases(
        ParsedJiraData parsedXml, 
        List<TestCase> initialTests, 
        TestEnhancementConfig config
    );
    
    Task<TestMatrix> GenerateTestMatrix(ParsedJiraData data);
    List<TestCase> GenerateNegativeTestScenarios(ParsedJiraData data);
    List<TestCase> GenerateSecurityTestScenarios(ParsedJiraData data);
    List<TestCase> GenerateAccessibilityTestScenarios(ParsedJiraData data);
    List<TestCase> GeneratePerformanceTestScenarios(ParsedJiraData data);
    List<TestCase> GenerateBoundaryTestScenarios(ParsedJiraData data);
    List<TestCase> GenerateErrorHandlingTestScenarios(ParsedJiraData data);
    List<TestCase> GenerateIntegrationTestScenarios(ParsedJiraData data);
    List<TestCase> GenerateUserExperienceTestScenarios(ParsedJiraData data);
    List<TestCase> GenerateDataValidationTestScenarios(ParsedJiraData data);
    List<TestCase> GenerateStateManagementTestScenarios(ParsedJiraData data);
    
    // New methods for enhanced capabilities
    TestCategory[] InferTestCategoriesFromSemanticContext(string content);
    List<TestCase> GeneratePerformanceTestsFromDomain(string domain, List<string> contexts);
    List<TestCase> GenerateDataValidationTests(TechnicalArtifacts artifacts);
}

/// <summary>
/// Service for generating comprehensive test cases using LLM enhancement patterns
/// </summary>
public class LLMTestEnhancementService : ILLMTestEnhancementService
{
    private readonly ILogger<LLMTestEnhancementService> _logger;
    private readonly IUiComponentAnalysisService _uiAnalysisService;
    private readonly IBusinessLogicAnalysisService _businessLogicService;

    // Semantic trigger rules for test category inference
    private readonly Dictionary<string[], TestCategory[]> _semanticTriggers = new()
    {
        { new[] {"phoneme", "SSML", "pronunciation", "TTS", "audio"}, new[] {TestCategory.Performance, TestCategory.DataValidation} },
        { new[] {"JSON", "SQL", "UPDATE", "database", "data"}, new[] {TestCategory.DataValidation, TestCategory.FormatCompliance} },
        { new[] {"fallback", "optional", "if", "when", "conditional"}, new[] {TestCategory.ConditionalLogic, TestCategory.EdgeCases} },
        { new[] {"IVR", "variants", "multiple", "flows", "contexts"}, new[] {TestCategory.ScenarioExpansion, TestCategory.IntegrationTests} },
        { new[] {"performance", "latency", "load", "concurrent", "timeout"}, new[] {TestCategory.Performance, TestCategory.BoundaryTests} },
        { new[] {"security", "auth", "permission", "access", "validation"}, new[] {TestCategory.SecurityTests, TestCategory.DataValidation} },
        { new[] {"error", "exception", "failure", "invalid", "malformed"}, new[] {TestCategory.ErrorHandlingTests, TestCategory.NegativeTests} },
        { new[] {"UI", "interface", "accessibility", "user", "experience"}, new[] {TestCategory.AccessibilityTests, TestCategory.UserExperienceTests} }
    };

    public LLMTestEnhancementService(
        ILogger<LLMTestEnhancementService> logger,
        IUiComponentAnalysisService uiAnalysisService,
        IBusinessLogicAnalysisService businessLogicService)
    {
        _logger = logger;
        _uiAnalysisService = uiAnalysisService;
        _businessLogicService = businessLogicService;
    }

    /// <summary>
    /// Enhances initial test cases with comprehensive coverage across all testing categories
    /// </summary>
    public async Task<EnhancedTestSuite> EnhanceTestCases(
        ParsedJiraData parsedXml, 
        List<TestCase> initialTests, 
        TestEnhancementConfig config)
    {
        _logger.LogInformation("Starting comprehensive test case enhancement for {TicketId}", parsedXml.TicketId);

        var enhancedSuite = new EnhancedTestSuite
        {
            OriginalTests = initialTests,
            EnhancementConfig = config,
            GenerationTimestamp = DateTime.UtcNow
        };

        try
        {
            // Generate all test categories in parallel for maximum coverage
            var enhancementTasks = new List<Task<List<TestCase>>>();
            
            if (config.IncludeNegativeTests)
                enhancementTasks.Add(Task.Run(() => GenerateNegativeTestScenarios(parsedXml)));
            
            if (config.IncludeSecurityTests)
                enhancementTasks.Add(Task.Run(() => GenerateSecurityTestScenarios(parsedXml)));
            
            if (config.IncludeAccessibilityTests)
                enhancementTasks.Add(Task.Run(() => GenerateAccessibilityTestScenarios(parsedXml)));
            
            if (config.IncludePerformanceTests)
                enhancementTasks.Add(Task.Run(() => GeneratePerformanceTestScenarios(parsedXml)));
            
            if (config.IncludeBoundaryTests)
                enhancementTasks.Add(Task.Run(() => GenerateBoundaryTestScenarios(parsedXml)));
            
            if (config.IncludeErrorHandlingTests)
                enhancementTasks.Add(Task.Run(() => GenerateErrorHandlingTestScenarios(parsedXml)));
            
            if (config.IncludeIntegrationTests)
                enhancementTasks.Add(Task.Run(() => GenerateIntegrationTestScenarios(parsedXml)));
            
            if (config.IncludeUserExperienceTests)
                enhancementTasks.Add(Task.Run(() => GenerateUserExperienceTestScenarios(parsedXml)));
            
            if (config.IncludeDataValidationTests)
                enhancementTasks.Add(Task.Run(() => GenerateDataValidationTestScenarios(parsedXml)));
            
            if (config.IncludeStateManagementTests)
                enhancementTasks.Add(Task.Run(() => GenerateStateManagementTestScenarios(parsedXml)));

            var enhancementResults = await Task.WhenAll(enhancementTasks);
            enhancedSuite.EnhancedTests = enhancementResults.SelectMany(tests => tests).ToList();

            // Remove duplicates and ensure uniqueness
            enhancedSuite.EnhancedTests = RemoveDuplicateTestCases(enhancedSuite.EnhancedTests);
            
            // Validate and categorize all tests
            ValidateTestCaseCoverage(enhancedSuite, parsedXml);

            _logger.LogInformation("Generated {TotalTests} comprehensive test cases ({OriginalCount} original + {EnhancedCount} enhanced)",
                enhancedSuite.TotalTestCount, initialTests.Count, enhancedSuite.EnhancedTests.Count);

            return enhancedSuite;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during test case enhancement for {TicketId}", parsedXml.TicketId);
            throw;
        }
    }

    /// <summary>
    /// Generates comprehensive test matrix showing all possible test scenarios
    /// </summary>
    public async Task<TestMatrix> GenerateTestMatrix(ParsedJiraData data)
    {
        _logger.LogInformation("Generating comprehensive test matrix for {TicketId}", data.TicketId);

        var matrix = new TestMatrix();
        
        // Define comprehensive test categories
        matrix.Categories = new List<TestCategoryDetails>
        {
            new() { Name = "Functional", Description = "Core functionality testing", TestTypes = new() { "Positive", "Negative", "Boundary" }, PotentialTestCount = 5 },
            new() { Name = "Security", Description = "Security and vulnerability testing", TestTypes = new() { "Authentication", "Authorization", "Input Validation", "XSS", "SQL Injection" }, PotentialTestCount = 8 },
            new() { Name = "Performance", Description = "Load and performance testing", TestTypes = new() { "Load", "Stress", "Volume", "Response Time" }, PotentialTestCount = 4 },
            new() { Name = "Accessibility", Description = "WCAG compliance testing", TestTypes = new() { "Screen Reader", "Keyboard Navigation", "Color Contrast", "ARIA" }, PotentialTestCount = 6 },
            new() { Name = "UI/UX", Description = "User interface and experience testing", TestTypes = new() { "Responsiveness", "Cross-browser", "Mobile", "Usability" }, PotentialTestCount = 7 },
            new() { Name = "Integration", Description = "System integration testing", TestTypes = new() { "API", "Database", "Third-party", "Workflow" }, PotentialTestCount = 5 },
            new() { Name = "Data Validation", Description = "Data integrity and validation testing", TestTypes = new() { "Format", "Range", "Type", "Required Fields" }, PotentialTestCount = 6 },
            new() { Name = "Error Handling", Description = "Error scenarios and recovery testing", TestTypes = new() { "Network Failure", "Timeout", "Invalid Input", "System Error" }, PotentialTestCount = 4 },
            new() { Name = "State Management", Description = "Application state and session testing", TestTypes = new() { "Session", "Cache", "Persistence", "Synchronization" }, PotentialTestCount = 4 },
            new() { Name = "Boundary Testing", Description = "Edge cases and limits testing", TestTypes = new() { "Min/Max Values", "Empty Data", "Special Characters", "Unicode" }, PotentialTestCount = 5 }
        };

        // Generate potential test scenarios based on ticket data
        matrix.Scenarios = GeneratePotentialScenarios(data);
        
        // Create coverage matrix
        matrix.CoverageMatrix = await GenerateCoverageMatrix(data);

        return matrix;
    }

    /// <summary>
    /// Generates negative test scenarios for comprehensive coverage
    /// </summary>
    public List<TestCase> GenerateNegativeTestScenarios(ParsedJiraData data)
    {
        var negativeTests = new List<TestCase>();
        var testId = 1;

        // Generate negative tests for each UI component
        foreach (var component in data.UIComponents)
        {
            switch (component.Type.ToLower())
            {
                case "form":
                case "input":
                    negativeTests.AddRange(GenerateInputNegativeTests(component, data, ref testId));
                    break;
                case "button":
                    negativeTests.AddRange(GenerateButtonNegativeTests(component, data, ref testId));
                    break;
                case "dropdown":
                case "select":
                    negativeTests.AddRange(GenerateDropdownNegativeTests(component, data, ref testId));
                    break;
                case "modal":
                case "dialog":
                    negativeTests.AddRange(GenerateModalNegativeTests(component, data, ref testId));
                    break;
            }
        }

        // Generate negative tests for business logic
        foreach (var logic in data.BusinessLogic)
        {
            negativeTests.AddRange(GenerateBusinessLogicNegativeTests(logic, data, ref testId));
        }

        // Generate general negative scenarios
        negativeTests.AddRange(GenerateGeneralNegativeTests(data, ref testId));

        return negativeTests;
    }

    /// <summary>
    /// Generates security test scenarios including OWASP Top 10
    /// </summary>
    public List<TestCase> GenerateSecurityTestScenarios(ParsedJiraData data)
    {
        var securityTests = new List<TestCase>();
        var testId = 1;

        // Input validation security tests
        securityTests.Add(new TestCase
        {
            Id = $"SEC_{testId++:D3}",
            Title = "SQL Injection Prevention Test",
            Description = "Verify system prevents SQL injection attacks through input validation",
            Priority = "High",
            Category = TestCategoryType.Security,
            Type = TestType.Security,
            Preconditions = new() { "Application is running", "Input fields are accessible" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = "Navigate to input form", ExpectedResult = "Form loads successfully" },
                new() { StepNumber = 2, Action = "Enter SQL injection payload: ' OR '1'='1", ExpectedResult = "Input is sanitized or rejected" },
                new() { StepNumber = 3, Action = "Submit form", ExpectedResult = "No database error occurs, input is safely handled" }
            },
            ExpectedResults = new() { "No SQL injection is successful", "Error handling is secure" },
            Confidence = 0.9,
            Source = "Security Analysis"
        });

        // XSS prevention tests
        securityTests.Add(new TestCase
        {
            Id = $"SEC_{testId++:D3}",
            Title = "Cross-Site Scripting (XSS) Prevention Test",
            Description = "Verify system prevents XSS attacks through proper input sanitization",
            Priority = "High",
            Category = TestCategoryType.Security,
            Type = TestType.Security,
            Preconditions = new() { "Application is running", "Input fields are accessible" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = "Navigate to input form", ExpectedResult = "Form loads successfully" },
                new() { StepNumber = 2, Action = "Enter XSS payload: <script>alert('XSS')</script>", ExpectedResult = "Input is sanitized" },
                new() { StepNumber = 3, Action = "Submit and view output", ExpectedResult = "Script is not executed, content is safely displayed" }
            },
            ExpectedResults = new() { "XSS payload is neutralized", "No script execution occurs" },
            Confidence = 0.9,
            Source = "Security Analysis"
        });

        // Authentication and authorization tests
        if (data.Description.Contains("login", StringComparison.OrdinalIgnoreCase) || 
            data.Description.Contains("auth", StringComparison.OrdinalIgnoreCase))
        {
            securityTests.AddRange(GenerateAuthenticationSecurityTests(data, ref testId));
        }

        // Session management tests
        securityTests.AddRange(GenerateSessionSecurityTests(data, ref testId));

        return securityTests;
    }

    /// <summary>
    /// Generates accessibility test scenarios for WCAG compliance
    /// </summary>
    public List<TestCase> GenerateAccessibilityTestScenarios(ParsedJiraData data)
    {
        var accessibilityTests = new List<TestCase>();
        var testId = 1;

        // Screen reader compatibility tests
        accessibilityTests.Add(new TestCase
        {
            Id = $"A11Y_{testId++:D3}",
            Title = "Screen Reader Navigation Test",
            Description = "Verify all interactive elements are accessible via screen reader",
            Priority = "Medium",
            Category = TestCategoryType.Accessibility,
            Type = TestType.Positive,
            Preconditions = new() { "Screen reader software is installed", "Application is running" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = "Enable screen reader", ExpectedResult = "Screen reader starts successfully" },
                new() { StepNumber = 2, Action = "Navigate through all interactive elements", ExpectedResult = "All elements are announced correctly" },
                new() { StepNumber = 3, Action = "Verify ARIA labels and roles", ExpectedResult = "Semantic information is provided" }
            },
            ExpectedResults = new() { "All UI elements are accessible", "Proper ARIA implementation" },
            Confidence = 0.85,
            Source = "Accessibility Analysis"
        });

        // Keyboard navigation tests
        accessibilityTests.Add(new TestCase
        {
            Id = $"A11Y_{testId++:D3}",
            Title = "Keyboard Navigation Test",
            Description = "Verify all functionality is accessible via keyboard only",
            Priority = "Medium",
            Category = TestCategoryType.Accessibility,
            Type = TestType.Positive,
            Preconditions = new() { "Application is running", "No mouse/pointer device used" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = "Use Tab key to navigate", ExpectedResult = "Focus moves logically through elements" },
                new() { StepNumber = 2, Action = "Use Enter/Space to activate elements", ExpectedResult = "Actions are triggered correctly" },
                new() { StepNumber = 3, Action = "Use Escape key for dialogs", ExpectedResult = "Modal dialogs close properly" }
            },
            ExpectedResults = new() { "Full keyboard accessibility", "Logical focus management" },
            Confidence = 0.9,
            Source = "Accessibility Analysis"
        });

        // Color contrast tests
        accessibilityTests.Add(new TestCase
        {
            Id = $"A11Y_{testId++:D3}",
            Title = "Color Contrast Compliance Test",
            Description = "Verify color contrast meets WCAG AA standards",
            Priority = "Medium",
            Category = TestCategoryType.Accessibility,
            Type = TestType.Positive,
            Preconditions = new() { "Color contrast analyzer tool available", "Application is running" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = "Analyze text/background color combinations", ExpectedResult = "Tool measures contrast ratios" },
                new() { StepNumber = 2, Action = "Verify minimum 4.5:1 ratio for normal text", ExpectedResult = "Contrast meets WCAG AA requirements" },
                new() { StepNumber = 3, Action = "Verify minimum 3:1 ratio for large text", ExpectedResult = "Large text contrast is adequate" }
            },
            ExpectedResults = new() { "All text meets contrast requirements", "No accessibility barriers" },
            Confidence = 0.8,
            Source = "Accessibility Analysis"
        });

        return accessibilityTests;
    }

    /// <summary>
    /// Generates performance test scenarios
    /// </summary>
    public List<TestCase> GeneratePerformanceTestScenarios(ParsedJiraData data)
    {
        var performanceTests = new List<TestCase>();
        var testId = 1;

        // Load testing
        performanceTests.Add(new TestCase
        {
            Id = $"PERF_{testId++:D3}",
            Title = "Load Testing - Normal User Load",
            Description = "Verify system performance under normal expected load",
            Priority = "Medium",
            Category = TestCategoryType.Performance,
            Type = TestType.Load,
            Preconditions = new() { "Load testing environment setup", "Performance monitoring tools ready" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = "Configure load test for 100 concurrent users", ExpectedResult = "Load test setup complete" },
                new() { StepNumber = 2, Action = "Execute load test for 10 minutes", ExpectedResult = "System maintains functionality" },
                new() { StepNumber = 3, Action = "Monitor response times and resource usage", ExpectedResult = "Metrics are within acceptable ranges" }
            },
            ExpectedResults = new() { "Response time < 3 seconds", "System remains stable", "No memory leaks" },
            Confidence = 0.8,
            Source = "Performance Analysis"
        });

        // Response time tests
        performanceTests.Add(new TestCase
        {
            Id = $"PERF_{testId++:D3}",
            Title = "Response Time Validation",
            Description = "Verify system response times meet performance requirements",
            Priority = "High",
            Category = TestCategoryType.Performance,
            Type = TestType.Positive,
            Preconditions = new() { "Application is running", "Network conditions are stable" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = "Perform typical user actions", ExpectedResult = "Actions complete successfully" },
                new() { StepNumber = 2, Action = "Measure response times", ExpectedResult = "Times are recorded accurately" },
                new() { StepNumber = 3, Action = "Verify against performance criteria", ExpectedResult = "All actions meet time requirements" }
            },
            ExpectedResults = new() { "Page load time < 2 seconds", "API response time < 500ms" },
            Confidence = 0.9,
            Source = "Performance Analysis"
        });

        return performanceTests;
    }

    /// <summary>
    /// Generates boundary test scenarios
    /// </summary>
    public List<TestCase> GenerateBoundaryTestScenarios(ParsedJiraData data)
    {
        var boundaryTests = new List<TestCase>();
        var testId = 1;

        // Generate boundary tests for each UI component
        foreach (var component in data.UIComponents)
        {
            if (component.Type.ToLower() == "input" || component.Type.ToLower() == "form")
            {
                boundaryTests.Add(new TestCase
                {
                    Id = $"BND_{testId++:D3}",
                    Title = $"Boundary Test - {component.Name} Maximum Length",
                    Description = $"Verify {component.Name} handles maximum input length correctly",
                    Priority = "Medium",
                    Category = TestCategoryType.Functional,
                    Type = TestType.Boundary,
                    Preconditions = new() { "Application is running", $"{component.Name} is accessible" },
                    TestSteps = new()
                    {
                        new() { StepNumber = 1, Action = $"Navigate to {component.Name}", ExpectedResult = "Field is accessible" },
                        new() { StepNumber = 2, Action = "Enter maximum allowed characters", ExpectedResult = "Input is accepted" },
                        new() { StepNumber = 3, Action = "Submit form", ExpectedResult = "Data is processed correctly" }
                    },
                    ExpectedResults = new() { "Maximum length is handled properly", "No truncation issues" },
                    Confidence = 0.85,
                    Source = "Boundary Analysis"
                });

                boundaryTests.Add(new TestCase
                {
                    Id = $"BND_{testId++:D3}",
                    Title = $"Boundary Test - {component.Name} Minimum Length",
                    Description = $"Verify {component.Name} handles minimum input requirements",
                    Priority = "Medium",
                    Category = TestCategoryType.Functional,
                    Type = TestType.Boundary,
                    Preconditions = new() { "Application is running", $"{component.Name} is accessible" },
                    TestSteps = new()
                    {
                        new() { StepNumber = 1, Action = $"Navigate to {component.Name}", ExpectedResult = "Field is accessible" },
                        new() { StepNumber = 2, Action = "Enter minimum required characters", ExpectedResult = "Input is accepted" },
                        new() { StepNumber = 3, Action = "Submit form", ExpectedResult = "Validation passes" }
                    },
                    ExpectedResults = new() { "Minimum length validation works", "Appropriate error messages" },
                    Confidence = 0.85,
                    Source = "Boundary Analysis"
                });
            }
        }

        return boundaryTests;
    }

    /// <summary>
    /// Generates error handling test scenarios
    /// </summary>
    public List<TestCase> GenerateErrorHandlingTestScenarios(ParsedJiraData data)
    {
        var errorTests = new List<TestCase>();
        var testId = 1;

        // Network error handling
        errorTests.Add(new TestCase
        {
            Id = $"ERR_{testId++:D3}",
            Title = "Network Failure Handling",
            Description = "Verify system handles network failures gracefully",
            Priority = "High",
            Category = TestCategoryType.ErrorHandling,
            Type = TestType.ErrorHandling,
            Preconditions = new() { "Application is running", "Network connectivity can be controlled" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = "Simulate network disconnection", ExpectedResult = "Network is offline" },
                new() { StepNumber = 2, Action = "Attempt to perform network-dependent action", ExpectedResult = "Error is handled gracefully" },
                new() { StepNumber = 3, Action = "Restore network connection", ExpectedResult = "System recovers automatically" }
            },
            ExpectedResults = new() { "User-friendly error message", "System remains stable", "Automatic recovery" },
            Confidence = 0.8,
            Source = "Error Handling Analysis"
        });

        // Timeout handling
        errorTests.Add(new TestCase
        {
            Id = $"ERR_{testId++:D3}",
            Title = "Request Timeout Handling",
            Description = "Verify system handles request timeouts appropriately",
            Priority = "Medium",
            Category = TestCategoryType.ErrorHandling,
            Type = TestType.ErrorHandling,
            Preconditions = new() { "Application is running", "Request timeout can be simulated" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = "Initiate long-running request", ExpectedResult = "Request starts" },
                new() { StepNumber = 2, Action = "Wait for timeout period", ExpectedResult = "Timeout occurs" },
                new() { StepNumber = 3, Action = "Verify timeout handling", ExpectedResult = "User is notified appropriately" }
            },
            ExpectedResults = new() { "Timeout is handled gracefully", "User can retry", "No system crash" },
            Confidence = 0.85,
            Source = "Error Handling Analysis"
        });

        return errorTests;
    }

    /// <summary>
    /// Generates integration test scenarios
    /// </summary>
    public List<TestCase> GenerateIntegrationTestScenarios(ParsedJiraData data)
    {
        var integrationTests = new List<TestCase>();
        var testId = 1;

        // Database integration tests
        integrationTests.Add(new TestCase
        {
            Id = $"INT_{testId++:D3}",
            Title = "Database Integration Test",
            Description = "Verify data persistence and retrieval functionality",
            Priority = "High",
            Category = TestCategoryType.Integration,
            Type = TestType.Positive,
            Preconditions = new() { "Database is accessible", "Application is running" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = "Create new data record", ExpectedResult = "Record is created successfully" },
                new() { StepNumber = 2, Action = "Retrieve data record", ExpectedResult = "Data is returned correctly" },
                new() { StepNumber = 3, Action = "Update data record", ExpectedResult = "Changes are persisted" },
                new() { StepNumber = 4, Action = "Delete data record", ExpectedResult = "Record is removed" }
            },
            ExpectedResults = new() { "CRUD operations work correctly", "Data integrity is maintained" },
            Confidence = 0.9,
            Source = "Integration Analysis"
        });

        // API integration tests
        integrationTests.Add(new TestCase
        {
            Id = $"INT_{testId++:D3}",
            Title = "API Integration Test",
            Description = "Verify API endpoints integration and data flow",
            Priority = "High",
            Category = TestCategoryType.Integration,
            Type = TestType.Positive,
            Preconditions = new() { "API services are running", "Authentication is configured" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = "Send API request with valid data", ExpectedResult = "Request is processed" },
                new() { StepNumber = 2, Action = "Verify response format", ExpectedResult = "Response matches expected schema" },
                new() { StepNumber = 3, Action = "Validate response data", ExpectedResult = "Data is accurate and complete" }
            },
            ExpectedResults = new() { "API integration works correctly", "Data exchange is successful" },
            Confidence = 0.9,
            Source = "Integration Analysis"
        });

        return integrationTests;
    }

    /// <summary>
    /// Generates user experience test scenarios
    /// </summary>
    public List<TestCase> GenerateUserExperienceTestScenarios(ParsedJiraData data)
    {
        var uxTests = new List<TestCase>();
        var testId = 1;

        // Cross-browser compatibility
        uxTests.Add(new TestCase
        {
            Id = $"UX_{testId++:D3}",
            Title = "Cross-Browser Compatibility Test",
            Description = "Verify functionality works across different browsers",
            Priority = "Medium",
            Category = TestCategoryType.UserExperience,
            Type = TestType.Compatibility,
            Preconditions = new() { "Multiple browsers installed", "Application is accessible" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = "Test in Chrome browser", ExpectedResult = "All features work correctly" },
                new() { StepNumber = 2, Action = "Test in Firefox browser", ExpectedResult = "All features work correctly" },
                new() { StepNumber = 3, Action = "Test in Safari browser", ExpectedResult = "All features work correctly" },
                new() { StepNumber = 4, Action = "Test in Edge browser", ExpectedResult = "All features work correctly" }
            },
            ExpectedResults = new() { "Consistent behavior across browsers", "No browser-specific issues" },
            Confidence = 0.8,
            Source = "UX Analysis"
        });

        // Mobile responsiveness
        uxTests.Add(new TestCase
        {
            Id = $"UX_{testId++:D3}",
            Title = "Mobile Responsiveness Test",
            Description = "Verify application works correctly on mobile devices",
            Priority = "High",
            Category = TestCategoryType.UserExperience,
            Type = TestType.Compatibility,
            Preconditions = new() { "Mobile devices or simulators available", "Application is accessible" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = "Open application on mobile device", ExpectedResult = "Layout adapts to screen size" },
                new() { StepNumber = 2, Action = "Test touch interactions", ExpectedResult = "Touch gestures work correctly" },
                new() { StepNumber = 3, Action = "Verify content readability", ExpectedResult = "Text is readable without zooming" }
            },
            ExpectedResults = new() { "Mobile-friendly interface", "Optimal user experience" },
            Confidence = 0.85,
            Source = "UX Analysis"
        });

        return uxTests;
    }

    /// <summary>
    /// Generates data validation test scenarios
    /// </summary>
    public List<TestCase> GenerateDataValidationTestScenarios(ParsedJiraData data)
    {
        var dataTests = new List<TestCase>();
        var testId = 1;

        // Format validation tests
        foreach (var component in data.UIComponents.Where(c => c.Type.ToLower() == "input"))
        {
            dataTests.Add(new TestCase
            {
                Id = $"DATA_{testId++:D3}",
                Title = $"Data Format Validation - {component.Name}",
                Description = $"Verify {component.Name} validates data format correctly",
                Priority = "Medium",
                Category = TestCategoryType.DataValidation,
                Type = TestType.Positive,
                Preconditions = new() { "Application is running", $"{component.Name} is accessible" },
                TestSteps = new()
                {
                    new() { StepNumber = 1, Action = "Enter valid format data", ExpectedResult = "Input is accepted" },
                    new() { StepNumber = 2, Action = "Enter invalid format data", ExpectedResult = "Validation error is shown" },
                    new() { StepNumber = 3, Action = "Correct the input", ExpectedResult = "Error is cleared" }
                },
                ExpectedResults = new() { "Format validation works correctly", "Clear error messages" },
                Confidence = 0.85,
                Source = "Data Validation Analysis"
            });
        }

        return dataTests;
    }

    /// <summary>
    /// Generates state management test scenarios
    /// </summary>
    public List<TestCase> GenerateStateManagementTestScenarios(ParsedJiraData data)
    {
        var stateTests = new List<TestCase>();
        var testId = 1;

        // Session management tests
        stateTests.Add(new TestCase
        {
            Id = $"STATE_{testId++:D3}",
            Title = "Session State Management Test",
            Description = "Verify user session state is maintained correctly",
            Priority = "High",
            Category = TestCategoryType.StateManagement,
            Type = TestType.Positive,
            Preconditions = new() { "User authentication is available", "Application is running" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = "Login to application", ExpectedResult = "User session is established" },
                new() { StepNumber = 2, Action = "Perform actions that modify state", ExpectedResult = "State changes are tracked" },
                new() { StepNumber = 3, Action = "Refresh page or navigate", ExpectedResult = "State is preserved" }
            },
            ExpectedResults = new() { "Session state is maintained", "User experience is consistent" },
            Confidence = 0.9,
            Source = "State Management Analysis"
        });

        return stateTests;
    }

    #region Helper Methods

    private List<TestCase> RemoveDuplicateTestCases(List<TestCase> testCases)
    {
        var uniqueTests = new List<TestCase>();
        var seenTitles = new HashSet<string>();

        foreach (var testCase in testCases)
        {
            var key = $"{testCase.Title}_{testCase.Category}_{testCase.Type}";
            if (!seenTitles.Contains(key))
            {
                seenTitles.Add(key);
                uniqueTests.Add(testCase);
            }
        }

        return uniqueTests;
    }

    private void ValidateTestCaseCoverage(EnhancedTestSuite suite, ParsedJiraData data)
    {
        var summary = suite.CoverageSummary;
        
        // Count tests by category
        summary.PositiveTestCount = suite.EnhancedTests.Count(t => t.Type == TestType.Positive);
        summary.NegativeTestCount = suite.EnhancedTests.Count(t => t.Type == TestType.Negative);
        summary.SecurityTestCount = suite.EnhancedTests.Count(t => t.Category == TestCategoryType.Security);
        summary.AccessibilityTestCount = suite.EnhancedTests.Count(t => t.Category == TestCategoryType.Accessibility);
        summary.PerformanceTestCount = suite.EnhancedTests.Count(t => t.Category == TestCategoryType.Performance);
        summary.BoundaryTestCount = suite.EnhancedTests.Count(t => t.Type == TestType.Boundary);
        summary.ErrorHandlingTestCount = suite.EnhancedTests.Count(t => t.Category == TestCategoryType.ErrorHandling);
        summary.IntegrationTestCount = suite.EnhancedTests.Count(t => t.Category == TestCategoryType.Integration);
        summary.UserExperienceTestCount = suite.EnhancedTests.Count(t => t.Category == TestCategoryType.UserExperience);
        summary.DataValidationTestCount = suite.EnhancedTests.Count(t => t.Category == TestCategoryType.DataValidation);
        summary.StateManagementTestCount = suite.EnhancedTests.Count(t => t.Category == TestCategoryType.StateManagement);

        // Calculate coverage percentage
        var totalPossibleTests = 50; // Estimated based on complexity
        summary.CoveragePercentage = Math.Min(100m, (suite.TotalTestCount / (decimal)totalPossibleTests) * 100);

        _logger.LogInformation("Test coverage validation completed: {Coverage}%", summary.CoveragePercentage);
    }

    private List<TestCase> GenerateInputNegativeTests(UIComponent component, ParsedJiraData data, ref int testId)
    {
        var tests = new List<TestCase>();

        tests.Add(new TestCase
        {
            Id = $"NEG_{testId++:D3}",
            Title = $"Negative Test - {component.Name} Empty Input",
            Description = $"Verify {component.Name} handles empty input correctly",
            Priority = "Medium",
            Category = TestCategoryType.Functional,
            Type = TestType.Negative,
            Preconditions = new() { "Application is running", $"{component.Name} is accessible" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = $"Navigate to {component.Name}", ExpectedResult = "Field is accessible" },
                new() { StepNumber = 2, Action = "Leave field empty", ExpectedResult = "Field remains empty" },
                new() { StepNumber = 3, Action = "Submit form", ExpectedResult = "Validation error is shown" }
            },
            ExpectedResults = new() { "Required field validation works", "Error message is clear" },
            Confidence = 0.9,
            Source = "Negative Testing"
        });

        return tests;
    }

    private List<TestCase> GenerateButtonNegativeTests(UIComponent component, ParsedJiraData data, ref int testId)
    {
        var tests = new List<TestCase>();

        tests.Add(new TestCase
        {
            Id = $"NEG_{testId++:D3}",
            Title = $"Negative Test - {component.Name} Multiple Clicks",
            Description = $"Verify {component.Name} handles rapid multiple clicks correctly",
            Priority = "Medium",
            Category = TestCategoryType.Functional,
            Type = TestType.Negative,
            Preconditions = new() { "Application is running", $"{component.Name} is accessible" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = $"Navigate to {component.Name}", ExpectedResult = "Button is accessible" },
                new() { StepNumber = 2, Action = "Click button multiple times rapidly", ExpectedResult = "Button handles multiple clicks" },
                new() { StepNumber = 3, Action = "Verify system state", ExpectedResult = "Action is not duplicated" }
            },
            ExpectedResults = new() { "No duplicate actions", "Button is properly disabled during processing" },
            Confidence = 0.85,
            Source = "Negative Testing"
        });

        return tests;
    }

    private List<TestCase> GenerateDropdownNegativeTests(UIComponent component, ParsedJiraData data, ref int testId)
    {
        var tests = new List<TestCase>();

        tests.Add(new TestCase
        {
            Id = $"NEG_{testId++:D3}",
            Title = $"Negative Test - {component.Name} No Selection",
            Description = $"Verify {component.Name} handles no selection correctly",
            Priority = "Medium",
            Category = TestCategoryType.Functional,
            Type = TestType.Negative,
            Preconditions = new() { "Application is running", $"{component.Name} is accessible" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = $"Navigate to {component.Name}", ExpectedResult = "Dropdown is accessible" },
                new() { StepNumber = 2, Action = "Do not select any option", ExpectedResult = "No option is selected" },
                new() { StepNumber = 3, Action = "Submit form", ExpectedResult = "Validation error is shown if required" }
            },
            ExpectedResults = new() { "Required validation works", "Default selection behavior is correct" },
            Confidence = 0.85,
            Source = "Negative Testing"
        });

        return tests;
    }

    private List<TestCase> GenerateModalNegativeTests(UIComponent component, ParsedJiraData data, ref int testId)
    {
        var tests = new List<TestCase>();

        tests.Add(new TestCase
        {
            Id = $"NEG_{testId++:D3}",
            Title = $"Negative Test - {component.Name} Close Without Save",
            Description = $"Verify {component.Name} handles close without save correctly",
            Priority = "Medium",
            Category = TestCategoryType.Functional,
            Type = TestType.Negative,
            Preconditions = new() { "Application is running", $"{component.Name} can be opened" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = $"Open {component.Name}", ExpectedResult = "Modal opens successfully" },
                new() { StepNumber = 2, Action = "Make changes to form", ExpectedResult = "Changes are made" },
                new() { StepNumber = 3, Action = "Close modal without saving", ExpectedResult = "Confirmation dialog appears" }
            },
            ExpectedResults = new() { "Unsaved changes warning", "Data is not lost unexpectedly" },
            Confidence = 0.8,
            Source = "Negative Testing"
        });

        return tests;
    }

    private List<TestCase> GenerateBusinessLogicNegativeTests(BusinessLogic logic, ParsedJiraData data, ref int testId)
    {
        var tests = new List<TestCase>();

        tests.Add(new TestCase
        {
            Id = $"NEG_{testId++:D3}",
            Title = $"Negative Test - {logic.Rule} Violation",
            Description = $"Verify system handles {logic.Rule} violations correctly",
            Priority = "High",
            Category = TestCategoryType.Functional,
            Type = TestType.Negative,
            Preconditions = new() { "Application is running", "Business rule is active" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = "Attempt to violate business rule", ExpectedResult = "Violation is detected" },
                new() { StepNumber = 2, Action = "Verify error handling", ExpectedResult = "Error message is displayed" },
                new() { StepNumber = 3, Action = "Correct the violation", ExpectedResult = "Error is cleared" }
            },
            ExpectedResults = new() { "Business rule is enforced", "Clear error messaging" },
            Confidence = 0.9,
            Source = "Business Logic Testing"
        });

        return tests;
    }

    private List<TestCase> GenerateGeneralNegativeTests(ParsedJiraData data, ref int testId)
    {
        var tests = new List<TestCase>();

        tests.Add(new TestCase
        {
            Id = $"NEG_{testId++:D3}",
            Title = "Negative Test - Invalid Data Types",
            Description = "Verify system handles invalid data types correctly",
            Priority = "Medium",
            Category = TestCategoryType.Functional,
            Type = TestType.Negative,
            Preconditions = new() { "Application is running", "Input fields are accessible" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = "Enter text in numeric field", ExpectedResult = "Invalid input is detected" },
                new() { StepNumber = 2, Action = "Enter numbers in text field", ExpectedResult = "Input is handled appropriately" },
                new() { StepNumber = 3, Action = "Submit form", ExpectedResult = "Validation errors are shown" }
            },
            ExpectedResults = new() { "Data type validation works", "User-friendly error messages" },
            Confidence = 0.85,
            Source = "General Negative Testing"
        });

        return tests;
    }

    private List<TestCase> GenerateAuthenticationSecurityTests(ParsedJiraData data, ref int testId)
    {
        var tests = new List<TestCase>();

        tests.Add(new TestCase
        {
            Id = $"SEC_{testId++:D3}",
            Title = "Security Test - Brute Force Protection",
            Description = "Verify system protects against brute force attacks",
            Priority = "High",
            Category = TestCategoryType.Security,
            Type = TestType.Security,
            Preconditions = new() { "Login functionality is available", "Account lockout is configured" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = "Attempt login with wrong password multiple times", ExpectedResult = "Failed attempts are tracked" },
                new() { StepNumber = 2, Action = "Exceed maximum failed attempts", ExpectedResult = "Account is locked" },
                new() { StepNumber = 3, Action = "Verify lockout duration", ExpectedResult = "Account remains locked for specified time" }
            },
            ExpectedResults = new() { "Brute force protection is active", "Account lockout works correctly" },
            Confidence = 0.9,
            Source = "Security Testing"
        });

        return tests;
    }

    private List<TestCase> GenerateSessionSecurityTests(ParsedJiraData data, ref int testId)
    {
        var tests = new List<TestCase>();

        tests.Add(new TestCase
        {
            Id = $"SEC_{testId++:D3}",
            Title = "Security Test - Session Timeout",
            Description = "Verify user sessions timeout after inactivity",
            Priority = "Medium",
            Category = TestCategoryType.Security,
            Type = TestType.Security,
            Preconditions = new() { "User is logged in", "Session timeout is configured" },
            TestSteps = new()
            {
                new() { StepNumber = 1, Action = "Login to application", ExpectedResult = "User session is established" },
                new() { StepNumber = 2, Action = "Remain inactive for timeout period", ExpectedResult = "Session expires" },
                new() { StepNumber = 3, Action = "Attempt to access protected resource", ExpectedResult = "User is redirected to login" }
            },
            ExpectedResults = new() { "Session timeout works correctly", "Sensitive data is protected" },
            Confidence = 0.85,
            Source = "Security Testing"
        });

        return tests;
    }

    private List<TestScenario> GeneratePotentialScenarios(ParsedJiraData data)
    {
        var scenarios = new List<TestScenario>();
        var scenarioId = 1;

        // Generate scenarios for each UI component
        foreach (var component in data.UIComponents)
        {
            scenarios.Add(new TestScenario
            {
                Id = $"SCENARIO_{scenarioId++:D3}",
                Name = $"{component.Name} Interaction Testing",
                Category = "UI",
                Priority = "Medium",
                RelatedFeatures = new() { component.Name, "User Interface" }
            });
        }

        // Generate scenarios for each business logic
        foreach (var logic in data.BusinessLogic)
        {
            scenarios.Add(new TestScenario
            {
                Id = $"SCENARIO_{scenarioId++:D3}",
                Name = $"{logic.Rule} Validation Testing",
                Category = "Business Logic",
                Priority = "High",
                RelatedFeatures = new() { logic.Rule, "Validation" }
            });
        }

        return scenarios;
    }

    private async Task<TestCoverageMatrix> GenerateCoverageMatrix(ParsedJiraData data)
    {
        var matrix = new TestCoverageMatrix();

        // Feature to test type mapping
        foreach (var component in data.UIComponents)
        {
            matrix.FeatureTestMapping[component.Name] = new List<string>
            {
                "Functional", "UI", "Accessibility", "Security", "Performance"
            };
        }

        // User role mapping (derived from description)
        var userRoles = ExtractUserRoles(data.Description);
        foreach (var role in userRoles)
        {
            matrix.UserRoleTestMapping[role] = new List<string>
            {
                "Authentication", "Authorization", "Functional", "Security"
            };
        }

        // Component mapping
        foreach (var component in data.UIComponents)
        {
            matrix.ComponentTestMapping[component.Name] = new List<string>
            {
                "Unit", "Integration", "UI", "Accessibility"
            };
        }

        return matrix;
    }

    private List<string> ExtractUserRoles(string description)
    {
        var roles = new List<string>();
        var rolePatterns = new[]
        {
            @"as an? (.+?),",
            @"as an? (.+?) i",
            @"user", @"admin", @"customer", @"manager", @"employee"
        };

        foreach (var pattern in rolePatterns)
        {
            var matches = Regex.Matches(description, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    roles.Add(match.Groups[1].Value.Trim());
                }
                else
                {
                    roles.Add(match.Value.Trim());
                }
            }
        }

        return roles.Distinct().ToList();
    }

    #endregion

    #region Enhanced Semantic Analysis Methods

    /// <summary>
    /// Infers test categories from semantic context using keyword-to-category mapping
    /// </summary>
    /// <param name="content">The content to analyze</param>
    /// <returns>Array of inferred test categories</returns>
    public TestCategory[] InferTestCategoriesFromSemanticContext(string content)
    {
        var inferredCategories = new HashSet<TestCategory>();
        var lowercaseContent = content.ToLower();

        foreach (var trigger in _semanticTriggers)
        {
            var keywords = trigger.Key;
            var categories = trigger.Value;

            // Check if any of the keywords are present in the content
            if (keywords.Any(keyword => lowercaseContent.Contains(keyword.ToLower())))
            {
                foreach (var category in categories)
                {
                    inferredCategories.Add(category);
                }
            }
        }

        // Always include basic categories if none were inferred
        if (!inferredCategories.Any())
        {
            inferredCategories.Add(TestCategory.DataValidation);
            inferredCategories.Add(TestCategory.UserExperienceTests);
        }

        return inferredCategories.ToArray();
    }

    /// <summary>
    /// Generates performance tests based on domain context (simplified version)
    /// </summary>
    /// <param name="domain">The domain context</param>
    /// <param name="contexts">List of specific contexts</param>
    /// <returns>List of basic test cases</returns>
    public List<TestCase> GeneratePerformanceTestsFromDomain(string domain, List<string> contexts)
    {
        var performanceTests = new List<TestCase>();
        
        // Generate basic performance test
        performanceTests.Add(new TestCase
        {
            Title = $"Performance Test for {domain}",
            Description = $"Basic performance test for {domain} domain",
            Priority = "Medium",
            Category = TestCategoryType.Performance,
            Type = TestType.Positive,
            TestSteps = new List<TestStep>
            {
                new() { StepNumber = 1, Action = "Execute performance test", ExpectedResult = "Performance within limits" }
            }
        });

        return performanceTests;
    }

    /// <summary>
    /// Generates data validation tests based on technical artifacts (simplified version)
    /// </summary>
    /// <param name="artifacts">Technical artifacts</param>
    /// <returns>List of validation test cases</returns>
    public List<TestCase> GenerateDataValidationTests(TechnicalArtifacts artifacts)
    {
        var validationTests = new List<TestCase>();

        // Generate basic validation test
        validationTests.Add(new TestCase
        {
            Title = "Data Validation Test",
            Description = "Basic data validation test",
            Priority = "High",
            Category = TestCategoryType.DataValidation,
            Type = TestType.Positive,
            TestSteps = new List<TestStep>
            {
                new() { StepNumber = 1, Action = "Validate data format", ExpectedResult = "Data validation passes" }
            }
        });

        return validationTests;
    }

    #endregion
}
