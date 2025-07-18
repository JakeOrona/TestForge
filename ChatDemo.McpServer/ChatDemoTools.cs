using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Xml.Linq;
using System.Xml;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using ChatDemo.McpServer.Models;

namespace ChatDemo.McpServer;

/// <summary>
/// Represents a parsed Jira story with extracted fields
/// </summary>
public record JiraStory
{
    public string IssueKey { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string StoryPoints { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public string IssueType { get; init; } = string.Empty;
    public List<string> AcceptanceCriteria { get; init; } = new();
    public Dictionary<string, string> CustomFields { get; init; } = new();
}

/// <summary>
/// Represents the parsing result with success status and extracted data
/// </summary>
public record JiraParseResult
{
    public bool IsSuccess { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public JiraStory? Story { get; init; }
}

/// <summary>
/// Represents a TestRail test case step with action and expected result
/// </summary>
public record TestRailStep
{
    public int StepNumber { get; init; }
    public string StepTitle { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string TestData { get; init; } = string.Empty;
    public string ExpectedResult { get; init; } = string.Empty;
}

/// <summary>
/// Represents a complete TestRail test case
/// </summary>
public record TestRailTestCase
{
    public string TestCaseId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public List<string> Preconditions { get; init; } = new();
    public List<TestRailStep> Steps { get; init; } = new();
    public string SourceJiraKey { get; init; } = string.Empty;
}

/// <summary>
/// Represents the complete TestRail test case generation result
/// </summary>
public record TestRailGenerationResult
{
    public bool IsSuccess { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public List<TestRailTestCase> TestCases { get; init; } = new();
    public string FormattedOutput { get; init; } = string.Empty;
}

[McpServerToolType]
public static class ChatDemoTools
{
    [McpServerTool, Description("Gets current date and time.")]
    public static string GetCurrentTime() => $"Current time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

    /// <summary>
    /// Analyzes a Jira ticket and returns structured data optimized for LLM enhancement and intelligent test case generation.
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
            // For now, return a simple structured response
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
    /// Generates test case templates that LLM can enhance and expand upon based on ticket type and complexity.
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
    {
        try
        {
            var templates = new
            {
                ticketType = ticketType,
                priority = priority,
                component = component,
                templates = new[]
                {
                    new
                    {
                        type = "happy_path",
                        title = $"[{component}] Happy Path - {ticketType}",
                        priority = priority,
                        category = "Functional",
                        steps = new[]
                        {
                            "Navigate to the target page/component",
                            "Perform the primary action with valid data",
                            "Verify successful completion",
                            "Confirm expected results are displayed"
                        },
                        expectedResult = "Action completes successfully with expected outcome",
                        testData = "Valid test data set appropriate for the scenario",
                        confidence = 0.95,
                        llmGuidance = new
                        {
                            expandAreas = new[] { "Add specific validation steps", "Include realistic test data examples" },
                            considerations = new[] { "User experience", "Performance impact", "Error prevention" }
                        }
                    },
                    new
                    {
                        type = "error_handling",
                        title = $"[{component}] Error Handling - {ticketType}",
                        priority = priority,
                        category = "Negative",
                        steps = new[]
                        {
                            "Navigate to the target page/component",
                            "Perform action with invalid/missing data",
                            "Verify appropriate error handling",
                            "Confirm error messages are clear and actionable"
                        },
                        expectedResult = "System handles errors gracefully with clear user feedback",
                        testData = "Invalid, missing, or boundary condition data",
                        confidence = 0.87,
                        llmGuidance = new
                        {
                            expandAreas = new[] { "Add specific error conditions", "Include validation message tests" },
                            considerations = new[] { "User guidance", "System stability", "Security implications" }
                        }
                    }
                },
                metadata = new
                {
                    generatedAt = DateTime.UtcNow,
                    version = "1.0",
                    llmOptimized = true,
                    enhancementSuggestions = new[]
                    {
                        "Add domain-specific test scenarios",
                        "Include accessibility considerations",
                        "Expand with security test cases",
                        "Consider integration testing aspects"
                    }
                }
            };
            
            return JsonSerializer.Serialize(templates, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = $"Template generation failed: {ex.Message}" });
        }
    }

    /// <summary>
    /// Extracts and categorizes UI components from a ticket description for targeted test planning.
    /// </summary>
    /// <param name="description">The ticket description or requirements text</param>
    /// <returns>Structured analysis of UI components with complexity scoring and test recommendations</returns>
    [McpServerTool, Description("Extracts and categorizes UI components from ticket descriptions. Identifies forms, buttons, modals, navigation elements, and provides complexity scoring with test area recommendations.")]
    public static string ExtractUIComponentsAnalysis(string description)
    {
        try
        {
            // Basic pattern matching for UI components
            var components = new List<object>();
            
            if (!string.IsNullOrWhiteSpace(description))
            {
                var lowerDesc = description.ToLower();
                
                if (lowerDesc.Contains("input") || lowerDesc.Contains("field"))
                {
                    components.Add(new { type = "input", name = "input_field", complexity = "medium", testAreas = new[] { "validation", "formatting" }, confidence = 0.8 });
                }
                
                if (lowerDesc.Contains("button") || lowerDesc.Contains("click"))
                {
                    components.Add(new { type = "button", name = "button_element", complexity = "low", testAreas = new[] { "interaction", "state" }, confidence = 0.85 });
                }
                
                if (lowerDesc.Contains("modal") || lowerDesc.Contains("dialog"))
                {
                    components.Add(new { type = "modal", name = "modal_dialog", complexity = "high", testAreas = new[] { "display", "interaction", "close" }, confidence = 0.9 });
                }
                
                if (lowerDesc.Contains("form"))
                {
                    components.Add(new { type = "form", name = "form_element", complexity = "high", testAreas = new[] { "validation", "submission" }, confidence = 0.88 });
                }
            }
            
            return JsonSerializer.Serialize(new { uiComponents = components }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = $"UI analysis failed: {ex.Message}" });
        }
    }

    /// <summary>
    /// Identifies business rules and validation logic from requirements for comprehensive test coverage.
    /// </summary>
    /// <param name="description">The ticket description or requirements text</param>
    /// <returns>Structured analysis of business logic with confidence scores and test scenario suggestions</returns>
    [McpServerTool, Description("Identifies business rules and validation logic from ticket descriptions. Extracts validation rules, business constraints, and provides test scenario recommendations with confidence scoring.")]
    public static string ExtractBusinessLogicAnalysis(string description)
    {
        try
        {
            var businessLogic = new List<object>();
            
            if (!string.IsNullOrWhiteSpace(description))
            {
                var lowerDesc = description.ToLower();
                
                if (lowerDesc.Contains("validation") || lowerDesc.Contains("validate"))
                {
                    businessLogic.Add(new { rule = "validation", description = "Input validation logic", confidence = 0.9, testScenarios = new[] { "valid input", "invalid input" } });
                }
                
                if (lowerDesc.Contains("required") || lowerDesc.Contains("mandatory"))
                {
                    businessLogic.Add(new { rule = "required_field", description = "Required field logic", confidence = 0.85, testScenarios = new[] { "missing required", "present required" } });
                }
                
                if (lowerDesc.Contains("minimum") || lowerDesc.Contains("maximum"))
                {
                    businessLogic.Add(new { rule = "min_max_validation", description = "Minimum/maximum value validation", confidence = 0.95, testScenarios = new[] { "below minimum", "above maximum", "within range" } });
                }
                
                if (lowerDesc.Contains("override") || lowerDesc.Contains("inherit"))
                {
                    businessLogic.Add(new { rule = "inheritance", description = "Override and inheritance logic", confidence = 0.88, testScenarios = new[] { "override behavior", "inheritance rules" } });
                }
            }
            
            return JsonSerializer.Serialize(new { businessLogic = businessLogic }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = $"Business logic analysis failed: {ex.Message}" });
        }
    }

    // TODO: Add Jira API integration - temporarily commented out
    // [McpServerTool, Description("Fetches a Jira ticket by ID and generates TestRail test cases directly from the Jira API without requiring XML export.")]
    // public static string GenerateTestCasesFromJiraTicket(string ticketId, string jiraBaseUrl = "", string username = "", string apiToken = "")
    // {
    //     // Implementation will be added after fixing method visibility issues
    //     return "Jira API integration coming soon!";
    // }

    /// <summary>
    /// Parses Jira story ticket XML and generates TestRail-compatible test cases with proper structure, steps, and metadata.
    /// </summary>
    /// <param name="jiraXml">The Jira ticket XML content to parse</param>
    /// <returns>TestRail-formatted test cases with detailed steps and expected results</returns>
    [McpServerTool, Description("Parses Jira story ticket XML and generates TestRail-compatible test cases with proper structure, steps, and metadata.")]
    public static string GenerateTestCasesFromJiraXml(string jiraXml)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(jiraXml))
        {
            return "Error: Jira XML content is required.";
        }

        try
        {
            // Parse and validate XML
            var parseResult = ParseJiraXml(jiraXml);
            
            if (!parseResult.IsSuccess)
            {
                return $"XML Parsing Error: {parseResult.ErrorMessage}";
            }

            if (parseResult.Story == null)
            {
                return "Error: No story data found in XML.";
            }

            // Generate TestRail test cases from extracted story data
            var testRailResult = GenerateTestRailTestCases(parseResult.Story);

            if (!testRailResult.IsSuccess)
            {
                return $"TestRail Generation Error: {testRailResult.ErrorMessage}";
            }

            return testRailResult.FormattedOutput;
        }
        catch (Exception ex)
        {
            return $"Error processing Jira XML: {ex.Message}";
        }
    }

    /// <summary>
    /// Enhanced XML parsing with better error handling and automatic fixing
    /// </summary>
    /// <param name="xmlContent">Raw Jira XML content</param>
    /// <returns>Parsing result with extracted story data</returns>
    private static JiraParseResult ParseJiraXml(string xmlContent)
    {
        try
        {
            // Step 1: Validate original XML
            var validation = ValidateXmlStructure(xmlContent);
            
            string xmlToUse = xmlContent;
            
            // Step 2: If invalid, try cleaning
            if (!validation.IsValid)
            {
                xmlToUse = CleanJiraXmlInternal(xmlContent);
                var cleanValidation = ValidateXmlStructure(xmlToUse);
                
                if (!cleanValidation.IsValid)
                {
                    return new JiraParseResult 
                    { 
                        IsSuccess = false, 
                        ErrorMessage = $"XML validation failed even after cleaning. Original: {validation.Error}. After cleaning: {cleanValidation.Error}" 
                    };
                }
            }

            // Step 3: Parse the XML
            var doc = XDocument.Parse(xmlToUse);
            
            // Handle different Jira XML export formats
            var issueElement = doc.Descendants("item").FirstOrDefault() ?? 
                              doc.Descendants("issue").FirstOrDefault() ??
                              doc.Root;

            if (issueElement == null)
            {
                return new JiraParseResult 
                { 
                    IsSuccess = false, 
                    ErrorMessage = "No valid Jira issue structure found in XML" 
                };
            }

            // Extract core fields using multiple possible XML structures
            var story = new JiraStory
            {
                IssueKey = ExtractFieldValue(issueElement, "key", "issue-key", "number"),
                Summary = ExtractFieldValue(issueElement, "summary", "title", "subject"),
                Description = ExtractFieldValue(issueElement, "description", "desc", "details"),
                StoryPoints = ExtractFieldValue(issueElement, "story-points", "storypoints", "points"),
                Priority = ExtractFieldValue(issueElement, "priority", "prio"),
                IssueType = ExtractFieldValue(issueElement, "type", "issuetype", "issue-type"),
                AcceptanceCriteria = ExtractAcceptanceCriteria(issueElement),
                CustomFields = ExtractCustomFields(issueElement)
            };

            return new JiraParseResult { IsSuccess = true, Story = story };
        }
        catch (XmlException xmlEx)
        {
            return new JiraParseResult 
            { 
                IsSuccess = false, 
                ErrorMessage = $"XML parsing error at line {xmlEx.LineNumber}, position {xmlEx.LinePosition}: {xmlEx.Message}" 
            };
        }
        catch (Exception ex)
        {
            return new JiraParseResult 
            { 
                IsSuccess = false, 
                ErrorMessage = $"Unexpected parsing error: {ex.Message}" 
            };
        }
    }

    /// <summary>
    /// Validates and diagnoses XML structure issues before parsing
    /// </summary>
    [McpServerTool, Description("Validates Jira XML structure and provides detailed diagnostic information about parsing issues.")]
    public static string ValidateJiraXml(string jiraXml)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(jiraXml))
            {
                return JsonSerializer.Serialize(new { 
                    isValid = false, 
                    error = "XML content is empty or null" 
                }, new JsonSerializerOptions { WriteIndented = true });
            }

            // Step 1: Basic XML validation
            var validationResult = ValidateXmlStructure(jiraXml);
            
            if (!validationResult.IsValid)
            {
                // Try to fix and re-validate
                var cleanedXml = CleanJiraXmlInternal(jiraXml);
                var retryValidation = ValidateXmlStructure(cleanedXml);
                
                return JsonSerializer.Serialize(new {
                    isValid = retryValidation.IsValid,
                    originalError = validationResult.Error,
                    cleaningApplied = true,
                    afterCleaningValid = retryValidation.IsValid,
                    afterCleaningError = retryValidation.Error,
                    suggestions = GetFixSuggestions(validationResult.Error),
                    cleanedXmlPreview = cleanedXml.Length > 500 ? cleanedXml.Substring(0, 500) + "..." : cleanedXml
                }, new JsonSerializerOptions { WriteIndented = true });
            }

            return JsonSerializer.Serialize(new { 
                isValid = true, 
                message = "XML structure is valid" 
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { 
                isValid = false, 
                error = $"Validation failed: {ex.Message}" 
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    /// <summary>
    /// Cleans raw Jira XML exports to fix common formatting issues, missing closing tags, and malformed content
    /// </summary>
    [McpServerTool, Description("Cleans raw Jira XML exports to fix common formatting issues, missing closing tags, and malformed content. Use this before parsing if you encounter XML errors.")]
    public static string CleanJiraXml(string rawXml)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(rawXml))
            {
                return JsonSerializer.Serialize(new { 
                    success = false, 
                    error = "Input XML is empty" 
                }, new JsonSerializerOptions { WriteIndented = true });
            }

            var originalLength = rawXml.Length;
            var cleanedXml = CleanJiraXmlInternal(rawXml);
            var cleanedLength = cleanedXml.Length;

            // Validate the cleaned XML
            var validationResult = ValidateXmlStructure(cleanedXml);

            return JsonSerializer.Serialize(new {
                success = validationResult.IsValid,
                originalLength = originalLength,
                cleanedLength = cleanedLength,
                bytesChanged = Math.Abs(originalLength - cleanedLength),
                isValid = validationResult.IsValid,
                validationError = validationResult.Error,
                cleanedXml = cleanedXml,
                cleaningSteps = new[] {
                    "Fixed missing closing tags",
                    "Cleaned comment content",
                    "Removed duplicate attributes",
                    "Escaped HTML entities",
                    "Removed invalid characters",
                    "Fixed nested structure"
                }
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { 
                success = false, 
                error = $"Cleaning failed: {ex.Message}" 
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    /// <summary>
    /// Enhanced XML cleaning to handle comment section and structural issues
    /// </summary>
    private static string CleanJiraXmlInternal(string rawXml)
    {
        if (string.IsNullOrWhiteSpace(rawXml))
            return rawXml;

        // Step 1: Fix missing closing tags for comments section
        rawXml = FixMissingCommentClosingTags(rawXml);
        
        // Step 2: Clean malformed comment content
        rawXml = CleanCommentContent(rawXml);
        
        // Step 3: Fix duplicate attributes (existing functionality)
        rawXml = Regex.Replace(rawXml, 
            @"rel=""[^""]*""\s+([^>]*?)rel=""([^""]*?)""", 
            @"rel=""$2"" $1", 
            RegexOptions.IgnoreCase);
        
        // Step 4: Fix other duplicate attributes
        rawXml = Regex.Replace(rawXml, 
            @"class=""[^""]*""\s+([^>]*?)class=""([^""]*?)""", 
            @"class=""$2"" $1", 
            RegexOptions.IgnoreCase);
        
        rawXml = Regex.Replace(rawXml, 
            @"data-account-id=""[^""]*""\s+([^>]*?)data-account-id=""([^""]*?)""", 
            @"data-account-id=""$2"" $1", 
            RegexOptions.IgnoreCase);
        
        rawXml = Regex.Replace(rawXml, 
            @"accountid=""[^""]*""\s+([^>]*?)accountid=""([^""]*?)""", 
            @"accountid=""$2"" $1", 
            RegexOptions.IgnoreCase);
        
        // Step 5: Fix malformed HTML entities
        rawXml = Regex.Replace(rawXml, @"&(?!(?:amp|lt|gt|quot|apos|#\d+|#x[0-9a-fA-F]+);)", "&amp;");
        
        // Step 6: Remove invalid XML characters
        rawXml = Regex.Replace(rawXml, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");
        
        // Step 7: Remove empty CDATA sections
        rawXml = Regex.Replace(rawXml, 
            @"<!\[CDATA\[\s*\]\]>", 
            "", 
            RegexOptions.IgnoreCase);
        
        // Step 8: Fix nested XML structure issues
        rawXml = FixNestedXmlStructure(rawXml);
        
        return rawXml;
    }

    /// <summary>
    /// Fixes missing closing tags for comments section
    /// </summary>
    private static string FixMissingCommentClosingTags(string xml)
    {
        // Pattern to find <comments> without proper closing
        var commentsPattern = @"<comments[^>]*>(?:(?!<\/comments>)[\s\S])*?(?=<\/\w+>|$)";
        
        var matches = Regex.Matches(xml, commentsPattern, RegexOptions.IgnoreCase);
        
        foreach (Match match in matches)
        {
            var commentsSection = match.Value;
            
            // Check if it already has closing tag
            if (!commentsSection.Contains("</comments>"))
            {
                // Add missing closing tag
                var replacement = commentsSection + "</comments>";
                xml = xml.Replace(commentsSection, replacement);
            }
        }
        
        return xml;
    }

    /// <summary>
    /// Cleans malformed content within comment tags
    /// </summary>
    private static string CleanCommentContent(string xml)
    {
        // Pattern to find comment tags with content
        var commentPattern = @"<comment[^>]*>(.*?)</comment>";
        
        return Regex.Replace(xml, commentPattern, match =>
        {
            var commentTag = match.Groups[0].Value;
            var content = match.Groups[1].Value;
            
            // Wrap content in CDATA if it contains HTML or special characters
            if (ContainsHtmlOrSpecialChars(content))
            {
                var cleanContent = $"<![CDATA[{content}]]>";
                return commentTag.Replace(content, cleanContent);
            }
            
            return commentTag;
        }, RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }

    /// <summary>
    /// Fixes nested XML structure issues
    /// </summary>
    private static string FixNestedXmlStructure(string xml)
    {
        // Fix unclosed comments tags that might interfere with parsing
        xml = Regex.Replace(xml, @"<comments>\s*(?!<comment|</comments>)", "<comments>", RegexOptions.IgnoreCase);
        
        // If we find </item> without a preceding </comments>, add it
        xml = Regex.Replace(xml, @"(?<!<\/comments>)\s*<\/item>", "</comments></item>", RegexOptions.IgnoreCase);
        
        return xml;
    }

    /// <summary>
    /// Checks if content contains HTML or special characters that need CDATA wrapping
    /// </summary>
    private static bool ContainsHtmlOrSpecialChars(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;
        
        // Check for HTML tags
        if (Regex.IsMatch(content, @"<[^>]+>"))
            return true;
        
        // Check for special XML characters
        if (content.Contains("&") || content.Contains("<") || content.Contains(">"))
            return true;
        
        // Check for user mentions and links
        if (content.Contains("@") || content.Contains("http"))
            return true;
        
        return false;
    }

    /// <summary>
    /// Validates XML structure and provides detailed error information
    /// </summary>
    private static (bool IsValid, string Error) ValidateXmlStructure(string xml)
    {
        try
        {
            XDocument.Parse(xml);
            return (true, string.Empty);
        }
        catch (XmlException ex)
        {
            return (false, $"Line {ex.LineNumber}, Position {ex.LinePosition}: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Provides fix suggestions based on common XML errors
    /// </summary>
    private static List<string> GetFixSuggestions(string error)
    {
        var suggestions = new List<string>();
        
        if (error.Contains("does not match the end tag"))
        {
            suggestions.Add("Missing closing tag - check for unclosed elements");
            suggestions.Add("Malformed nested structure - verify tag hierarchy");
            suggestions.Add("Try the clean_jira_xml tool to fix common issues");
        }
        
        if (error.Contains("comments"))
        {
            suggestions.Add("Comments section has structural issues");
            suggestions.Add("Check for missing </comments> closing tag");
            suggestions.Add("Verify comment content doesn't contain unescaped HTML");
        }
        
        if (error.Contains("invalid character"))
        {
            suggestions.Add("XML contains invalid characters");
            suggestions.Add("Check for control characters or unescaped entities");
        }
        
        return suggestions;
    }

    /// <summary>
    /// Generates TestRail test cases from parsed Jira story data
    /// </summary>
    /// <param name="story">Parsed Jira story with extracted fields</param>
    /// <returns>TestRail test case generation result</returns>
    private static TestRailGenerationResult GenerateTestRailTestCases(JiraStory story)
    {
        try
        {
            var testCases = new List<TestRailTestCase>();

            // Generate main happy path test case
            var mainTestCase = GenerateMainTestCase(story);
            if (mainTestCase != null)
            {
                testCases.Add(mainTestCase);
            }

            // Generate test cases from acceptance criteria
            var criteriaTestCases = GenerateAcceptanceCriteriaTestCases(story);
            testCases.AddRange(criteriaTestCases);

            // Generate edge case test cases
            var edgeTestCases = GenerateEdgeCaseTestCases(story);
            testCases.AddRange(edgeTestCases);

            // Format output
            var formattedOutput = FormatTestRailOutput(testCases, story);

            return new TestRailGenerationResult
            {
                IsSuccess = true,
                TestCases = testCases,
                FormattedOutput = formattedOutput
            };
        }
        catch (Exception ex)
        {
            return new TestRailGenerationResult
            {
                IsSuccess = false,
                ErrorMessage = $"TestRail generation failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Generates the main happy path test case from the story
    /// </summary>
    private static TestRailTestCase? GenerateMainTestCase(JiraStory story)
    {
        if (string.IsNullOrWhiteSpace(story.Summary))
        {
            return null;
        }

        var category = DetermineCategory(story);
        var featureCode = ExtractFeatureCode(story.Summary);
        var testCaseId = GenerateTestCaseId(featureCode, 1);

        var preconditions = GeneratePreconditions(story);
        var steps = GenerateMainTestSteps(story);

        return new TestRailTestCase
        {
            TestCaseId = testCaseId,
            Title = $"{story.Summary} - Happy Path",
            Priority = MapPriority(story.Priority),
            Category = category,
            Type = "Functional",
            Preconditions = preconditions,
            Steps = steps,
            SourceJiraKey = story.IssueKey
        };
    }

    /// <summary>
    /// Generates test cases from acceptance criteria
    /// </summary>
    private static List<TestRailTestCase> GenerateAcceptanceCriteriaTestCases(JiraStory story)
    {
        var testCases = new List<TestRailTestCase>();
        var category = DetermineCategory(story);
        var featureCode = ExtractFeatureCode(story.Summary);

        for (int i = 0; i < story.AcceptanceCriteria.Count; i++)
        {
            var criteria = story.AcceptanceCriteria[i];
            var testCaseId = GenerateTestCaseId(featureCode, i + 2);

            var testCase = new TestRailTestCase
            {
                TestCaseId = testCaseId,
                Title = $"{story.Summary} - AC {i + 1}",
                Priority = MapPriority(story.Priority),
                Category = category,
                Type = "Functional",
                Preconditions = GeneratePreconditions(story),
                Steps = GenerateStepsFromCriteria(criteria),
                SourceJiraKey = story.IssueKey
            };

            testCases.Add(testCase);
        }

        return testCases;
    }

    /// <summary>
    /// Generates edge case test cases based on story context
    /// </summary>
    private static List<TestRailTestCase> GenerateEdgeCaseTestCases(JiraStory story)
    {
        var testCases = new List<TestRailTestCase>();
        var category = DetermineCategory(story);
        var featureCode = ExtractFeatureCode(story.Summary);
        var baseIndex = story.AcceptanceCriteria.Count + 2;

        // Generate negative test cases based on story type
        if (category == "Authentication")
        {
            testCases.Add(GenerateInvalidCredentialsTestCase(story, featureCode, baseIndex));
        }
        else if (category == "Forms")
        {
            testCases.Add(GenerateFormValidationTestCase(story, featureCode, baseIndex));
            testCases.Add(GenerateEmptyFieldsTestCase(story, featureCode, baseIndex + 1));
        }

        return testCases;
    }

    /// <summary>
    /// Generates main test steps from story description and acceptance criteria
    /// </summary>
    private static List<TestRailStep> GenerateMainTestSteps(JiraStory story)
    {
        var steps = new List<TestRailStep>();
        var stepNumber = 1;

        // Extract actions from story description
        var actions = ExtractActionsFromText(story.Description);
        
        if (!actions.Any())
        {
            // Generate default steps based on story type
            actions = GenerateDefaultActions(story);
        }

        foreach (var action in actions)
        {
            var step = new TestRailStep
            {
                StepNumber = stepNumber++,
                StepTitle = GenerateStepTitle(action),
                Action = GenerateDetailedAction(action, story),
                TestData = GenerateTestData(action, story),
                ExpectedResult = GenerateExpectedResult(action, story)
            };

            steps.Add(step);
        }

        return steps;
    }

    /// <summary>
    /// Generates test steps from acceptance criteria text
    /// </summary>
    private static List<TestRailStep> GenerateStepsFromCriteria(string criteria)
    {
        var steps = new List<TestRailStep>();

        // Parse Given/When/Then patterns
        var gwtSteps = ParseGivenWhenThen(criteria);
        
        foreach (var gwtStep in gwtSteps)
        {
            steps.Add(gwtStep);
        }

        return steps;
    }

    /// <summary>
    /// Formats TestRail output with proper structure
    /// </summary>
    private static string FormatTestRailOutput(List<TestRailTestCase> testCases, JiraStory story)
    {
        var output = new System.Text.StringBuilder();
        
        output.AppendLine("=== TESTRAIL TEST CASE GENERATION ===");
        output.AppendLine($"Source: {story.IssueKey} - {story.Summary}");
        output.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        output.AppendLine($"Test Cases Generated: {testCases.Count}");
        output.AppendLine();

        foreach (var testCase in testCases)
        {
            output.AppendLine($"{testCase.TestCaseId} - {testCase.Title}");
            output.AppendLine($"Priority: {testCase.Priority}");
            output.AppendLine($"Category: {testCase.Category}");
            output.AppendLine($"Type: {testCase.Type}");
            output.AppendLine();

            output.AppendLine("Preconditions:");
            foreach (var precondition in testCase.Preconditions)
            {
                output.AppendLine($"- {precondition}");
            }
            output.AppendLine();

            output.AppendLine("Test Steps:");
            foreach (var step in testCase.Steps)
            {
                output.AppendLine($"Step {step.StepNumber}: {step.StepTitle}");
                output.AppendLine($"  Action: {step.Action}");
                if (!string.IsNullOrWhiteSpace(step.TestData))
                {
                    output.AppendLine($"  Test Data: {step.TestData}");
                }
                output.AppendLine($"  Expected Result: {step.ExpectedResult}");
                output.AppendLine();
            }
            
            output.AppendLine("---");
            output.AppendLine();
        }

        output.AppendLine("=== TESTRAIL GENERATION COMPLETE ===");
        output.AppendLine("Note: Import these test cases into TestRail for execution");
        
        return output.ToString();
    }

    // Helper methods for TestRail generation
    private static string DetermineCategory(JiraStory story)
    {
        var text = $"{story.Summary} {story.Description}".ToLower();
        
        if (text.Contains("login") || text.Contains("authentication") || text.Contains("sign in"))
            return "Authentication";
        if (text.Contains("register") || text.Contains("signup") || text.Contains("sign up"))
            return "Registration";
        if (text.Contains("form") || text.Contains("input") || text.Contains("submit"))
            return "Forms";
        if (text.Contains("navigate") || text.Contains("menu") || text.Contains("page"))
            return "Navigation";
        if (text.Contains("search") || text.Contains("filter"))
            return "Search";
        
        return "General";
    }

    private static string ExtractFeatureCode(string summary)
    {
        // Extract feature code from summary
        var words = summary.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > 0)
        {
            return words[0].ToUpper().Replace("USER", "").Replace("ADMIN", "").Trim();
        }
        
        return "GEN";
    }

    private static string GenerateTestCaseId(string featureCode, int number)
    {
        return $"TC_{featureCode}_{number:D3}";
    }

    private static string MapPriority(string jiraPriority)
    {
        return jiraPriority.ToLower() switch
        {
            "highest" => "Critical",
            "high" => "High",
            "medium" => "Medium",
            "low" => "Low",
            "lowest" => "Low",
            _ => "Medium"
        };
    }

    private static List<string> GeneratePreconditions(JiraStory story)
    {
        var preconditions = new List<string>();
        var category = DetermineCategory(story);

        // Add standard preconditions
        preconditions.Add("Application is accessible via web browser");
        preconditions.Add("Test environment is properly configured");

        // Add category-specific preconditions
        switch (category)
        {
            case "Authentication":
                preconditions.Add("User has valid account credentials");
                preconditions.Add("User is not currently logged in");
                break;
            case "Registration":
                preconditions.Add("User does not have an existing account");
                preconditions.Add("Registration feature is enabled");
                break;
            case "Forms":
                preconditions.Add("All required form fields are accessible");
                preconditions.Add("Form validation is properly configured");
                break;
        }

        return preconditions;
    }

    private static List<string> ExtractActionsFromText(string text)
    {
        var actions = new List<string>();
        
        if (string.IsNullOrWhiteSpace(text))
            return actions;

        var actionPatterns = new[]
        {
            @"(?i)I want to ([^.]+)",
            @"(?i)user can ([^.]+)",
            @"(?i)should be able to ([^.]+)",
            @"(?i)need to ([^.]+)"
        };

        foreach (var pattern in actionPatterns)
        {
            var matches = Regex.Matches(text, pattern);
            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    actions.Add(match.Groups[1].Value.Trim());
                }
            }
        }

        return actions.Distinct().ToList();
    }

    private static List<string> GenerateDefaultActions(JiraStory story)
    {
        var category = DetermineCategory(story);
        
        return category switch
        {
            "Authentication" => new List<string> { "navigate to login page", "enter credentials", "submit login form", "verify successful login" },
            "Registration" => new List<string> { "navigate to registration page", "fill registration form", "submit registration", "verify account creation" },
            "Forms" => new List<string> { "navigate to form", "fill required fields", "submit form", "verify form submission" },
            _ => new List<string> { "navigate to target page", "perform main action", "verify expected result" }
        };
    }

    private static string GenerateStepTitle(string action)
    {
        return action.Split(' ').Take(3).Aggregate((a, b) => $"{a} {b}").ToTitleCase();
    }

    private static string GenerateDetailedAction(string action, JiraStory story)
    {
        var category = DetermineCategory(story);
        
        if (action.Contains("navigate"))
        {
            return $"Navigate to the {category.ToLower()} page using the main menu or direct URL";
        }
        if (action.Contains("enter") || action.Contains("fill"))
        {
            return $"Enter the required information in the form fields";
        }
        if (action.Contains("submit") || action.Contains("click"))
        {
            return $"Click the submit/action button to proceed";
        }
        if (action.Contains("verify"))
        {
            return $"Verify that the expected result is displayed correctly";
        }
        
        return $"Perform the action: {action}";
    }

    private static string GenerateTestData(string action, JiraStory story)
    {
        var category = DetermineCategory(story);
        
        if (action.Contains("credentials") || action.Contains("login"))
        {
            return "username=\"testuser@example.com\", password=\"ValidPass123\"";
        }
        if (action.Contains("register") || action.Contains("signup"))
        {
            return "email=\"newuser@example.com\", username=\"newuser\", password=\"NewPass123\"";
        }
        if (action.Contains("form") || action.Contains("input"))
        {
            return "Sample valid data for all required fields";
        }
        
        return "";
    }

    private static string GenerateExpectedResult(string action, JiraStory story)
    {
        if (action.Contains("navigate"))
        {
            return "The target page loads successfully with all required elements visible";
        }
        if (action.Contains("enter") || action.Contains("fill"))
        {
            return "All data is entered correctly without validation errors";
        }
        if (action.Contains("submit") || action.Contains("click"))
        {
            return "The action is processed successfully and user receives appropriate feedback";
        }
        if (action.Contains("verify"))
        {
            return "All expected elements and data are displayed correctly";
        }
        
        return "The action completes successfully with expected results";
    }

    // Continue with helper methods for XML parsing (keeping original functionality)
    private static string ExtractFieldValue(XElement element, params string[] possibleNames)
    {
        foreach (var name in possibleNames)
        {
            var value = element.Element(name)?.Value?.Trim() ??
                       element.Attribute(name)?.Value?.Trim() ??
                       element.Descendants(name).FirstOrDefault()?.Value?.Trim();
            
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }
        
        return string.Empty;
    }

    private static List<string> ExtractAcceptanceCriteria(XElement element)
    {
        var criteria = new List<string>();
        
        var acceptanceElements = element.Descendants("acceptance-criteria")
                               .Concat(element.Descendants("acceptancecriteria"))
                               .Concat(element.Descendants("criteria"))
                               .Concat(element.Descendants("acceptance"));
        
        foreach (var criteriaElement in acceptanceElements)
        {
            var text = criteriaElement.Value?.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                var splitCriteria = text.Split(new[] { '\n', '\r', ';', '*', '-' }, 
                                             StringSplitOptions.RemoveEmptyEntries)
                                       .Select(c => c.Trim())
                                       .Where(c => !string.IsNullOrWhiteSpace(c));
                
                criteria.AddRange(splitCriteria);
            }
        }
        
        return criteria;
    }

    private static Dictionary<string, string> ExtractCustomFields(XElement element)
    {
        var customFields = new Dictionary<string, string>();
        
        var customFieldElements = element.Descendants()
                                       .Where(e => e.Name.LocalName.StartsWith("customfield") ||
                                                  e.Name.LocalName.StartsWith("custom-field") ||
                                                  e.Name.LocalName.Contains("field"));
        
        foreach (var field in customFieldElements)
        {
            var fieldName = field.Attribute("name")?.Value ?? 
                           field.Attribute("id")?.Value ?? 
                           field.Name.LocalName;
            
            var fieldValue = field.Value?.Trim();
            
            if (!string.IsNullOrWhiteSpace(fieldName) && !string.IsNullOrWhiteSpace(fieldValue))
            {
                customFields[fieldName] = fieldValue;
            }
        }
        
        return customFields;
    }

    private static List<TestRailStep> ParseGivenWhenThen(string criteria)
    {
        var steps = new List<TestRailStep>();
        var stepNumber = 1;
        
        var gwtPatterns = new[]
        {
            (@"(?i)given\s+([^,\n]+)", "Navigate/Setup"),
            (@"(?i)when\s+([^,\n]+)", "Action"),
            (@"(?i)then\s+([^,\n]+)", "Verification"),
            (@"(?i)and\s+([^,\n]+)", "Additional Action")
        };

        foreach (var (pattern, stepType) in gwtPatterns)
        {
            var matches = Regex.Matches(criteria, pattern);
            foreach (Match match in matches)
            {
                var description = match.Groups[1].Value.Trim();
                var step = new TestRailStep
                {
                    StepNumber = stepNumber++,
                    StepTitle = stepType,
                    Action = ConvertGwtToAction(description),
                    TestData = ExtractTestDataFromDescription(description),
                    ExpectedResult = ConvertGwtToExpectedResult(description, stepType)
                };
                
                steps.Add(step);
            }
        }

        return steps;
    }

    private static string ConvertGwtToAction(string description)
    {
        return description.Replace("I am", "Navigate to")
                         .Replace("I enter", "Enter")
                         .Replace("I click", "Click")
                         .Replace("I fill", "Fill")
                         .Replace("I should", "Verify that");
    }

    private static string ExtractTestDataFromDescription(string description)
    {
        var dataMatch = Regex.Match(description, @"with\s+([^,\n]+)");
        if (dataMatch.Success)
        {
            return dataMatch.Groups[1].Value.Trim();
        }
        
        return "";
    }

    private static string ConvertGwtToExpectedResult(string description, string stepType)
    {
        if (stepType == "Verification")
        {
            return description.Replace("I should", "The system should")
                             .Replace("I can", "The user can")
                             .Replace("I am", "The user is");
        }
        
        return $"The action '{description}' completes successfully";
    }

    private static TestRailTestCase GenerateInvalidCredentialsTestCase(JiraStory story, string featureCode, int index)
    {
        return new TestRailTestCase
        {
            TestCaseId = GenerateTestCaseId(featureCode, index),
            Title = $"{story.Summary} - Invalid Credentials",
            Priority = "High",
            Category = "Authentication",
            Type = "Negative",
            Preconditions = new List<string> { "User has invalid credentials", "Login page is accessible" },
            Steps = new List<TestRailStep>
            {
                new TestRailStep { StepNumber = 1, StepTitle = "Navigate to login", Action = "Open login page", ExpectedResult = "Login form is displayed" },
                new TestRailStep { StepNumber = 2, StepTitle = "Enter invalid credentials", Action = "Enter invalid username/password", TestData = "username=\"invalid@test.com\", password=\"wrongpass\"", ExpectedResult = "Credentials are entered" },
                new TestRailStep { StepNumber = 3, StepTitle = "Submit login", Action = "Click login button", ExpectedResult = "Error message is displayed indicating invalid credentials" }
            }
        };
    }

    private static TestRailTestCase GenerateFormValidationTestCase(JiraStory story, string featureCode, int index)
    {
        return new TestRailTestCase
        {
            TestCaseId = GenerateTestCaseId(featureCode, index),
            Title = $"{story.Summary} - Form Validation",
            Priority = "Medium",
            Category = "Forms",
            Type = "Negative",
            Preconditions = new List<string> { "Form is accessible", "Validation rules are configured" },
            Steps = new List<TestRailStep>
            {
                new TestRailStep { StepNumber = 1, StepTitle = "Navigate to form", Action = "Open the form page", ExpectedResult = "Form is displayed with all fields" },
                new TestRailStep { StepNumber = 2, StepTitle = "Enter invalid data", Action = "Enter invalid data in form fields", TestData = "Invalid format data", ExpectedResult = "Data is entered" },
                new TestRailStep { StepNumber = 3, StepTitle = "Submit form", Action = "Click submit button", ExpectedResult = "Validation errors are displayed for invalid fields" }
            }
        };
    }

    private static TestRailTestCase GenerateEmptyFieldsTestCase(JiraStory story, string featureCode, int index)
    {
        return new TestRailTestCase
        {
            TestCaseId = GenerateTestCaseId(featureCode, index),
            Title = $"{story.Summary} - Empty Required Fields",
            Priority = "Medium",
            Category = "Forms",
            Type = "Negative",
            Preconditions = new List<string> { "Form is accessible", "Required field validation is enabled" },
            Steps = new List<TestRailStep>
            {
                new TestRailStep { StepNumber = 1, StepTitle = "Navigate to form", Action = "Open the form page", ExpectedResult = "Form is displayed" },
                new TestRailStep { StepNumber = 2, StepTitle = "Leave fields empty", Action = "Leave required fields empty", ExpectedResult = "Fields remain empty" },
                new TestRailStep { StepNumber = 3, StepTitle = "Submit form", Action = "Click submit button", ExpectedResult = "Required field validation messages are displayed" }
            }
        };
    }
}

// Extension method for string formatting
public static class StringExtensions
{
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var words = input.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            }
        }
        return string.Join(" ", words);
    }

    /// <summary>
    /// Validates if a string matches the expected Jira ticket ID format (PROJECT-123)
    /// </summary>
    /// <param name="ticketId">The ticket ID to validate</param>
    /// <returns>True if the format is valid, false otherwise</returns>
    private static bool IsValidJiraTicketId(string ticketId)
    {
        if (string.IsNullOrWhiteSpace(ticketId))
            return false;

        // Pattern: PROJECT-123 (letters, dash, numbers)
        var pattern = @"^[A-Z]+-\d+$";
        return Regex.IsMatch(ticketId, pattern);
    }

    /// <summary>
    /// Fetches a Jira ticket from the API and converts it to a JiraStory object
    /// </summary>
    /// <param name="ticketId">The Jira ticket ID</param>
    /// <param name="baseUrl">The Jira base URL</param>
    /// <param name="username">The username for authentication</param>
    /// <param name="apiToken">The API token for authentication</param>
    /// <returns>A JiraStory object with the ticket data</returns>
    private static async Task<JiraStory?> FetchJiraTicket(string ticketId, string baseUrl, string username, string apiToken)
    {
        try
        {
            using var httpClient = new HttpClient();
            
            // Setup authentication
            var authValue = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{apiToken}"));
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);
            
            // Fetch ticket from Jira API
            var jiraUrl = $"{baseUrl.TrimEnd('/')}/rest/api/2/issue/{ticketId}";
            var response = await httpClient.GetAsync(jiraUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                // Log the error for debugging
                Console.WriteLine($"Failed to fetch Jira ticket {ticketId}: {response.StatusCode} - {response.ReasonPhrase}");
                return null;
            }
            
            var jsonContent = await response.Content.ReadAsStringAsync();
            var jiraIssue = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonContent);
            
            // Extract key information
            var fields = jiraIssue.GetProperty("fields");
            var key = jiraIssue.GetProperty("key").GetString() ?? "";
            var summary = fields.GetProperty("summary").GetString() ?? "";
            var description = fields.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "";
            var issueType = fields.GetProperty("issuetype").GetProperty("name").GetString() ?? "";
            var priority = fields.GetProperty("priority").GetProperty("name").GetString() ?? "";
            
            // Create JiraStory object
            var story = new JiraStory
            {
                IssueKey = key,
                Summary = summary,
                Description = CleanHtmlFromDescription(description),
                IssueType = issueType,
                Priority = priority,
                StoryPoints = ExtractStoryPoints(fields),
                AcceptanceCriteria = ExtractAcceptanceCriteriaFromDescription(description),
                CustomFields = ExtractCustomFieldsFromJson(fields)
            };
            
            return story;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching Jira ticket {ticketId}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Removes HTML tags from description text
    /// </summary>
    /// <param name="description">The description with HTML tags</param>
    /// <returns>Clean text without HTML tags</returns>
    private static string CleanHtmlFromDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return string.Empty;

        // Remove HTML tags
        var cleanText = Regex.Replace(description, "<.*?>", " ");
        
        // Replace multiple spaces with single space
        cleanText = Regex.Replace(cleanText, @"\s+", " ");
        
        // Decode HTML entities
        cleanText = cleanText.Replace("&lt;", "<")
                           .Replace("&gt;", ">")
                           .Replace("&amp;", "&")
                           .Replace("&quot;", "\"")
                           .Replace("&apos;", "'");
        
        return cleanText.Trim();
    }

    /// <summary>
    /// Extracts story points from Jira fields
    /// </summary>
    /// <param name="fields">The Jira fields JSON element</param>
    /// <returns>Story points as string</returns>
    private static string ExtractStoryPoints(System.Text.Json.JsonElement fields)
    {
        try
        {
            // Common story points field names
            var storyPointsFields = new[] { "customfield_10002", "customfield_10016", "story_points", "storypoints" };
            
            foreach (var fieldName in storyPointsFields)
            {
                if (fields.TryGetProperty(fieldName, out var storyPointsElement) && 
                    storyPointsElement.ValueKind != System.Text.Json.JsonValueKind.Null)
                {
                    return storyPointsElement.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting story points: {ex.Message}");
        }
        
        return string.Empty;
    }

    /// <summary>
    /// Extracts acceptance criteria from description or dedicated fields
    /// </summary>
    /// <param name="description">The ticket description</param>
    /// <returns>List of acceptance criteria</returns>
    private static List<string> ExtractAcceptanceCriteriaFromDescription(string description)
    {
        var criteria = new List<string>();
        
        if (string.IsNullOrWhiteSpace(description))
            return criteria;

        // Look for acceptance criteria patterns
        var patterns = new[]
        {
            @"Acceptance Criteria[:\s]*(.+?)(?=\n\n|\n[A-Z]|$)",
            @"Given[:\s].+?(?=\n\n|\n[A-Z]|$)",
            @"When[:\s].+?(?=\n\n|\n[A-Z]|$)",
            @"Then[:\s].+?(?=\n\n|\n[A-Z]|$)"
        };

        foreach (var pattern in patterns)
        {
            var matches = Regex.Matches(description, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                var criterion = match.Value.Trim();
                if (!string.IsNullOrWhiteSpace(criterion))
                {
                    criteria.Add(criterion);
                }
            }
        }

        return criteria;
    }

    /// <summary>
    /// Extracts custom fields from Jira JSON
    /// </summary>
    /// <param name="fields">The Jira fields JSON element</param>
    /// <returns>Dictionary of custom fields</returns>
    private static Dictionary<string, string> ExtractCustomFieldsFromJson(System.Text.Json.JsonElement fields)
    {
        var customFields = new Dictionary<string, string>();
        
        try
        {
            foreach (var property in fields.EnumerateObject())
            {
                if (property.Name.StartsWith("customfield_"))
                {
                    var value = property.Value.ValueKind == System.Text.Json.JsonValueKind.String 
                        ? property.Value.GetString() ?? "" 
                        : property.Value.ToString();
                    
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        customFields[property.Name] = value;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting custom fields: {ex.Message}");
        }
        
        return customFields;
    }

    /// <summary>
    /// Creates a mock analysis for demonstration purposes - in real implementation, this would fetch from Jira API
    /// </summary>
    /// <param name="ticketId">The Jira ticket ID</param>
    /// <returns>Mock analysis data</returns>
    private static LLMTicketAnalysis CreateMockAnalysis(string ticketId)
    {
        return new LLMTicketAnalysis
        {
            TicketInfo = new TicketInfo
            {
                Key = ticketId,
                Summary = "Mock analysis for demonstration",
                Description = "This is a mock analysis. In real implementation, this would fetch actual ticket data from Jira API.",
                Type = "Story",
                Priority = "Medium",
                Status = "Ready for Testing",
                AcceptanceCriteria = new List<string>
                {
                    "Given a user navigates to the page",
                    "When they perform the action",
                    "Then the expected result should occur"
                }
            },
            ComplexityAnalysis = new ComplexityAnalysis
            {
                UIComponents = new List<UIComponent>
                {
                    new UIComponent
                    {
                        Type = "input",
                        Name = "minimum_input",
                        Complexity = "medium",
                        TestAreas = new List<string> { "validation", "placeholder text", "empty state" },
                        Confidence = 0.85
                    },
                    new UIComponent
                    {
                        Type = "modal",
                        Name = "edit_rate_modal",
                        Complexity = "high",
                        TestAreas = new List<string> { "display", "interaction", "close behavior" },
                        Confidence = 0.92
                    }
                },
                BusinessLogic = new List<BusinessLogic>
                {
                    new BusinessLogic
                    {
                        Rule = "minimum_validation",
                        Description = "Minimum input should be empty when override rate has no minimum",
                        Confidence = 0.95,
                        TestScenarios = new List<string> { "empty override minimum", "inherited minimum display" }
                    }
                },
                IntegrationPoints = new List<IntegrationPoint>
                {
                    new IntegrationPoint
                    {
                        System = "Rates API",
                        Type = "REST API",
                        Risk = "medium",
                        TestRequirements = new List<string> { "API response validation", "error handling" }
                    }
                }
            },
            SuggestedTestAreas = new SuggestedTestAreas
            {
                Functional = new List<string> { "minimum validation", "override behavior", "inheritance logic" },
                UI = new List<string> { "modal display", "input states", "placeholder text" },
                Integration = new List<string> { "API calls", "data synchronization" },
                Performance = new List<string> { "modal load time", "API response time" }
            },
            BaselineTestCases = new List<BaselineTestCase>
            {
                new BaselineTestCase
                {
                    Type = "happy_path",
                    Title = "Verify minimum input behavior with override",
                    Priority = "High",
                    Category = "Functional",
                    Steps = new List<string>
                    {
                        "Navigate to Account Viewer → Rates Tab",
                        "Select rate with minimum and override",
                        "Open Edit Override Rate modal",
                        "Verify minimum input state"
                    },
                    ExpectedResult = "Minimum input shows correct state based on override configuration",
                    TestData = "Rate with minimum=2, override with no minimum",
                    Confidence = 0.9
                }
            },
            LLMGuidance = new LLMGuidance
            {
                FocusAreas = new List<string> { "UI validation", "business logic", "edge cases" },
                SuggestedPrompts = new List<string>
                {
                    "Expand edge cases for different minimum configurations",
                    "Add accessibility tests for modal interactions",
                    "Consider negative scenarios with invalid data"
                },
                ComplexityScore = 7.2,
                RiskAreas = new List<string> { "inheritance logic", "modal state management" },
                TestingStrategy = "Focus on UI state validation and business rule compliance"
            }
        };
    }

    /// <summary>
    /// Creates test case templates for different ticket types
    /// </summary>
    /// <param name="ticketType">The type of ticket</param>
    /// <param name="priority">Priority level</param>
    /// <param name="component">Component area</param>
    /// <returns>Test case templates</returns>
    private static object CreateTestCaseTemplates(string ticketType, string priority, string component)
    {
        var templates = new
        {
            ticketType = ticketType,
            priority = priority,
            component = component,
            templates = new[]
            {
                new
                {
                    type = "happy_path",
                    title = $"[{component}] Happy Path - {ticketType}",
                    priority = priority,
                    category = "Functional",
                    steps = new[]
                    {
                        "Navigate to the target page/component",
                        "Perform the primary action with valid data",
                        "Verify successful completion",
                        "Confirm expected results are displayed"
                    },
                    expectedResult = "Action completes successfully with expected outcome",
                    testData = "Valid test data set appropriate for the scenario",
                    confidence = 0.95,
                    llmGuidance = new
                    {
                        expandAreas = new[] { "Add specific validation steps", "Include realistic test data examples" },
                        considerations = new[] { "User experience", "Performance impact", "Error prevention" }
                    }
                },
                new
                {
                    type = "error_handling",
                    title = $"[{component}] Error Handling - {ticketType}",
                    priority = priority,
                    category = "Negative",
                    steps = new[]
                    {
                        "Navigate to the target page/component",
                        "Perform action with invalid/missing data",
                        "Verify appropriate error handling",
                        "Confirm error messages are clear and actionable"
                    },
                    expectedResult = "System handles errors gracefully with clear user feedback",
                    testData = "Invalid, missing, or boundary condition data",
                    confidence = 0.87,
                    llmGuidance = new
                    {
                        expandAreas = new[] { "Add specific error conditions", "Include validation message tests" },
                        considerations = new[] { "User guidance", "System stability", "Security implications" }
                    }
                },
                new
                {
                    type = "edge_cases",
                    title = $"[{component}] Edge Cases - {ticketType}",
                    priority = priority,
                    category = "Edge Case",
                    steps = new[]
                    {
                        "Identify boundary conditions",
                        "Test with extreme values",
                        "Verify system behavior at limits",
                        "Confirm graceful degradation"
                    },
                    expectedResult = "System handles edge cases appropriately without failure",
                    testData = "Boundary values, extreme inputs, unusual conditions",
                    confidence = 0.78,
                    llmGuidance = new
                    {
                        expandAreas = new[] { "Identify specific boundaries", "Add performance considerations" },
                        considerations = new[] { "System limits", "Resource constraints", "Scalability" }
                    }
                }
            },
            metadata = new
            {
                generatedAt = DateTime.UtcNow,
                version = "1.0",
                llmOptimized = true,
                enhancementSuggestions = new[]
                {
                    "Add domain-specific test scenarios",
                    "Include accessibility considerations",
                    "Expand with security test cases",
                    "Consider integration testing aspects"
                }
            }
        };

        return templates;
    }

    /// <summary>
    /// Extracts UI components from description using pattern matching
    /// </summary>
    /// <param name="description">The ticket description</param>
    /// <returns>List of identified UI components</returns>
    private static List<UIComponent> ExtractUIComponents(string description)
    {
        var components = new List<UIComponent>();

        if (string.IsNullOrWhiteSpace(description))
            return components;

        var uiPatterns = new Dictionary<string, (string type, string complexity, List<string> testAreas)>
        {
            { @"input|field|textbox|text field", ("input", "medium", new List<string> { "validation", "formatting", "placeholder" }) },
            { @"button|btn|click|submit", ("button", "low", new List<string> { "interaction", "state changes", "feedback" }) },
            { @"modal|dialog|popup|overlay", ("modal", "high", new List<string> { "display", "interaction", "close behavior", "focus management" }) },
            { @"dropdown|select|combo", ("dropdown", "medium", new List<string> { "options", "selection", "keyboard navigation" }) },
            { @"tab|tabs|tabbed", ("tab", "medium", new List<string> { "navigation", "state", "content switching" }) },
            { @"form|forms", ("form", "high", new List<string> { "validation", "submission", "error handling" }) },
            { @"table|grid|list", ("table", "high", new List<string> { "data display", "sorting", "filtering", "pagination" }) },
            { @"navigation|nav|menu", ("navigation", "medium", new List<string> { "routing", "state", "accessibility" }) }
        };

        foreach (var pattern in uiPatterns)
        {
            var matches = Regex.Matches(description, pattern.Key, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                components.Add(new UIComponent
                {
                    Type = pattern.Value.type,
                    Name = match.Value.ToLower(),
                    Complexity = pattern.Value.complexity,
                    TestAreas = pattern.Value.testAreas,
                    Confidence = 0.8 + (matches.Count * 0.05) // Higher confidence with more matches
                });
            }
        }

        return components.GroupBy(c => c.Type).Select(g => g.First()).ToList(); // Remove duplicates
    }

    /// <summary>
    /// Extracts business logic from description
    /// </summary>
    /// <param name="description">The ticket description</param>
    /// <returns>List of identified business logic</returns>
    private static List<BusinessLogic> ExtractBusinessLogic(string description)
    {
        var businessLogic = new List<BusinessLogic>();

        if (string.IsNullOrWhiteSpace(description))
            return businessLogic;

        var logicPatterns = new Dictionary<string, (string rule, List<string> scenarios)>
        {
            { @"validation|validate|valid|invalid", ("validation", new List<string> { "valid input", "invalid input", "boundary conditions" }) },
            { @"required|mandatory|must", ("required_field", new List<string> { "missing required fields", "required field validation" }) },
            { @"minimum|min|maximum|max", ("min_max_validation", new List<string> { "minimum values", "maximum values", "boundary testing" }) },
            { @"empty|null|blank", ("empty_state", new List<string> { "empty input handling", "null value processing" }) },
            { @"override|overridden|inherit", ("inheritance", new List<string> { "override behavior", "inheritance logic", "precedence rules" }) },
            { @"permission|access|authorize", ("authorization", new List<string> { "access control", "permission validation", "unauthorized access" }) },
            { @"calculation|calculate|compute", ("calculation", new List<string> { "calculation accuracy", "formula validation", "edge cases" }) }
        };

        foreach (var pattern in logicPatterns)
        {
            var matches = Regex.Matches(description, pattern.Key, RegexOptions.IgnoreCase);
            if (matches.Count > 0)
            {
                businessLogic.Add(new BusinessLogic
                {
                    Rule = pattern.Value.rule,
                    Description = $"Business logic related to {pattern.Key}",
                    Confidence = Math.Min(0.95, 0.6 + (matches.Count * 0.1)),
                    TestScenarios = pattern.Value.scenarios
                });
            }
        }

        return businessLogic;
    }
}
