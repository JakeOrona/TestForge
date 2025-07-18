using System.Text.Json;
using System.Text.RegularExpressions;
using TestForge.McpServer.Models;

namespace TestForge.McpServer.Services;

/// <summary>
/// Service responsible for generating TestRail-compatible test cases from Jira story data
/// </summary>
public static class TestRailGenerationService
{
    /// <summary>
    /// Generates TestRail-compatible test cases from Jira XML
    /// </summary>
    /// <param name="jiraXml">The Jira ticket XML content to parse</param>
    /// <returns>TestRail-formatted test cases with detailed steps and expected results</returns>
    public static string GenerateFromXml(string jiraXml)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(jiraXml))
        {
            return "Error: Jira XML content is required.";
        }

        try
        {
            // Parse and validate XML
            var parseResult = JiraStoryParsingService.ParseJiraXml(jiraXml);
            
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
            var formattedOutput = TestRailFormattingService.FormatTestRailOutput(testCases, story);

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
    /// <param name="story">The Jira story data</param>
    /// <returns>Main test case or null if cannot generate</returns>
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
    /// <param name="story">The Jira story data</param>
    /// <returns>List of acceptance criteria test cases</returns>
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
    /// <param name="story">The Jira story data</param>
    /// <returns>List of edge case test cases</returns>
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
    /// <param name="story">The Jira story data</param>
    /// <returns>List of test steps</returns>
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
    /// <param name="criteria">The acceptance criteria text</param>
    /// <returns>List of test steps</returns>
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
    /// Determines the category of the story based on content
    /// </summary>
    /// <param name="story">The Jira story data</param>
    /// <returns>Category string</returns>
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

    /// <summary>
    /// Extracts feature code from summary
    /// </summary>
    /// <param name="summary">The story summary</param>
    /// <returns>Feature code string</returns>
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

    /// <summary>
    /// Generates test case ID
    /// </summary>
    /// <param name="featureCode">The feature code</param>
    /// <param name="number">The test case number</param>
    /// <returns>Test case ID</returns>
    private static string GenerateTestCaseId(string featureCode, int number)
    {
        return $"TC_{featureCode}_{number:D3}";
    }

    /// <summary>
    /// Maps Jira priority to TestRail priority
    /// </summary>
    /// <param name="jiraPriority">The Jira priority</param>
    /// <returns>TestRail priority</returns>
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

    /// <summary>
    /// Generates preconditions for the test case
    /// </summary>
    /// <param name="story">The Jira story data</param>
    /// <returns>List of preconditions</returns>
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

    /// <summary>
    /// Extracts actions from text using regex patterns
    /// </summary>
    /// <param name="text">The text to extract actions from</param>
    /// <returns>List of actions</returns>
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

    /// <summary>
    /// Generates default actions based on story category
    /// </summary>
    /// <param name="story">The Jira story data</param>
    /// <returns>List of default actions</returns>
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

    /// <summary>
    /// Generates step title from action
    /// </summary>
    /// <param name="action">The action text</param>
    /// <returns>Step title</returns>
    private static string GenerateStepTitle(string action)
    {
        return action.Split(' ').Take(3).Aggregate((a, b) => $"{a} {b}").ToTitleCase();
    }

    /// <summary>
    /// Generates detailed action description
    /// </summary>
    /// <param name="action">The action text</param>
    /// <param name="story">The Jira story data</param>
    /// <returns>Detailed action description</returns>
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

    /// <summary>
    /// Generates test data for the action
    /// </summary>
    /// <param name="action">The action text</param>
    /// <param name="story">The Jira story data</param>
    /// <returns>Test data description</returns>
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

    /// <summary>
    /// Generates expected result for the action
    /// </summary>
    /// <param name="action">The action text</param>
    /// <param name="story">The Jira story data</param>
    /// <returns>Expected result description</returns>
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

    /// <summary>
    /// Parses Given/When/Then patterns from criteria text
    /// </summary>
    /// <param name="criteria">The criteria text</param>
    /// <returns>List of test rail steps</returns>
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

    /// <summary>
    /// Converts Given/When/Then description to action
    /// </summary>
    /// <param name="description">The description text</param>
    /// <returns>Action text</returns>
    private static string ConvertGwtToAction(string description)
    {
        return description.Replace("I am", "Navigate to")
                         .Replace("I enter", "Enter")
                         .Replace("I click", "Click")
                         .Replace("I fill", "Fill")
                         .Replace("I should", "Verify that");
    }

    /// <summary>
    /// Extracts test data from description
    /// </summary>
    /// <param name="description">The description text</param>
    /// <returns>Test data</returns>
    private static string ExtractTestDataFromDescription(string description)
    {
        var dataMatch = Regex.Match(description, @"with\s+([^,\n]+)");
        if (dataMatch.Success)
        {
            return dataMatch.Groups[1].Value.Trim();
        }
        
        return "";
    }

    /// <summary>
    /// Converts Given/When/Then description to expected result
    /// </summary>
    /// <param name="description">The description text</param>
    /// <param name="stepType">The step type</param>
    /// <returns>Expected result text</returns>
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

    /// <summary>
    /// Generates invalid credentials test case
    /// </summary>
    /// <param name="story">The Jira story data</param>
    /// <param name="featureCode">The feature code</param>
    /// <param name="index">The test case index</param>
    /// <returns>Invalid credentials test case</returns>
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

    /// <summary>
    /// Generates form validation test case
    /// </summary>
    /// <param name="story">The Jira story data</param>
    /// <param name="featureCode">The feature code</param>
    /// <param name="index">The test case index</param>
    /// <returns>Form validation test case</returns>
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

    /// <summary>
    /// Generates empty fields test case
    /// </summary>
    /// <param name="story">The Jira story data</param>
    /// <param name="featureCode">The feature code</param>
    /// <param name="index">The test case index</param>
    /// <returns>Empty fields test case</returns>
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

/// <summary>
/// Extension method for string formatting
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts string to title case
    /// </summary>
    /// <param name="input">The input string</param>
    /// <returns>Title case string</returns>
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
