using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Xml.Linq;
using System.Xml;
using System.Text.RegularExpressions;

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
    [McpServerTool, Description("Says hello to the user with a personalized greeting.")]
    public static string SayHello(string name = "World") => $"Hello from ChatDemo MCP Server, {name}! 🎉";

    [McpServerTool, Description("Provides information about this ChatDemo application.")]
    public static string GetAppInfo() => 
        "This is ChatDemo - an AI-powered chat application built with .NET and Angular, featuring Model Context Protocol (MCP) integration for enhanced AI capabilities.";

    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo(string message) => $"Echo from ChatDemo: {message}";

    [McpServerTool, Description("Reverses the given text.")]
    public static string ReverseText(string text) => new string(text.Reverse().ToArray());

    [McpServerTool, Description("Gets current date and time.")]
    public static string GetCurrentTime() => $"Current time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

    [McpServerTool, Description("Generates a random number between min and max values.")]
    public static int GenerateRandomNumber(int min = 1, int max = 100) => new Random().Next(min, max + 1);

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
    /// Parses Jira XML and extracts story fields
    /// </summary>
    /// <param name="xmlContent">Raw Jira XML content</param>
    /// <returns>Parsing result with extracted story data</returns>
    private static JiraParseResult ParseJiraXml(string xmlContent)
    {
        try
        {
            // Clean the XML to handle common Jira export issues
            var cleanedXml = CleanJiraXml(xmlContent);
            var doc = XDocument.Parse(cleanedXml);
            
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
                ErrorMessage = $"Invalid XML format: {xmlEx.Message}" 
            };
        }
        catch (Exception ex)
        {
            return new JiraParseResult 
            { 
                IsSuccess = false, 
                ErrorMessage = $"XML parsing failed: {ex.Message}" 
            };
        }
    }

    /// <summary>
    /// Cleans raw Jira XML to handle common export issues like duplicate attributes and malformed content
    /// </summary>
    /// <param name="rawXml">Raw Jira XML content</param>
    /// <returns>Cleaned XML content ready for parsing</returns>
    private static string CleanJiraXml(string rawXml)
    {
        if (string.IsNullOrWhiteSpace(rawXml))
            return rawXml;

        // Fix duplicate rel attributes - keep the last one
        rawXml = Regex.Replace(rawXml, 
            @"rel=""[^""]*""\s+([^>]*?)rel=""([^""]*?)""", 
            @"rel=""$2"" $1", 
            RegexOptions.IgnoreCase);
        
        // Fix duplicate class attributes - keep the last one
        rawXml = Regex.Replace(rawXml, 
            @"class=""[^""]*""\s+([^>]*?)class=""([^""]*?)""", 
            @"class=""$2"" $1", 
            RegexOptions.IgnoreCase);
        
        // Fix duplicate data-account-id attributes - keep the last one
        rawXml = Regex.Replace(rawXml, 
            @"data-account-id=""[^""]*""\s+([^>]*?)data-account-id=""([^""]*?)""", 
            @"data-account-id=""$2"" $1", 
            RegexOptions.IgnoreCase);
        
        // Fix duplicate accountid attributes - keep the last one
        rawXml = Regex.Replace(rawXml, 
            @"accountid=""[^""]*""\s+([^>]*?)accountid=""([^""]*?)""", 
            @"accountid=""$2"" $1", 
            RegexOptions.IgnoreCase);
        
        // Remove empty CDATA sections
        rawXml = Regex.Replace(rawXml, 
            @"<!\[CDATA\[\s*\]\]>", 
            "", 
            RegexOptions.IgnoreCase);
        
        // Remove invalid XML characters (control characters except tab, newline, carriage return)
        rawXml = Regex.Replace(rawXml, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");
        
        // Fix malformed HTML entities in descriptions
        rawXml = Regex.Replace(rawXml, @"&(?!(?:amp|lt|gt|quot|apos|#\d+|#x[0-9a-fA-F]+);)", "&amp;");
        
        return rawXml;
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
}
