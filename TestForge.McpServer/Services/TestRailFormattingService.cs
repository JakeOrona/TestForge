using System.Text;
using TestForge.McpServer.Models;

namespace TestForge.McpServer.Services;

/// <summary>
/// Service responsible for formatting TestRail test cases into readable output
/// </summary>
public static class TestRailFormattingService
{
    /// <summary>
    /// Formats TestRail test cases into a structured output format
    /// </summary>
    /// <param name="testCases">List of TestRail test cases</param>
    /// <param name="story">The source Jira story</param>
    /// <returns>Formatted TestRail output string</returns>
    public static string FormatTestRailOutput(List<TestRailTestCase> testCases, JiraStory story)
    {
        var output = new StringBuilder();
        
        // Header section
        output.AppendLine("=== TESTRAIL TEST CASE GENERATION ===");
        output.AppendLine($"Source: {story.IssueKey} - {story.Summary}");
        output.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        output.AppendLine($"Test Cases Generated: {testCases.Count}");
        output.AppendLine();

        // Story information section
        output.AppendLine("=== SOURCE STORY INFORMATION ===");
        output.AppendLine($"Issue Key: {story.IssueKey}");
        output.AppendLine($"Summary: {story.Summary}");
        output.AppendLine($"Issue Type: {story.IssueType}");
        output.AppendLine($"Priority: {story.Priority}");
        if (!string.IsNullOrWhiteSpace(story.StoryPoints))
        {
            output.AppendLine($"Story Points: {story.StoryPoints}");
        }
        output.AppendLine();

        if (!string.IsNullOrWhiteSpace(story.Description))
        {
            output.AppendLine("Description:");
            output.AppendLine(WrapText(story.Description, 80));
            output.AppendLine();
        }

        if (story.AcceptanceCriteria.Any())
        {
            output.AppendLine("Acceptance Criteria:");
            for (int i = 0; i < story.AcceptanceCriteria.Count; i++)
            {
                output.AppendLine($"  {i + 1}. {story.AcceptanceCriteria[i]}");
            }
            output.AppendLine();
        }

        // Test cases section
        output.AppendLine("=== GENERATED TEST CASES ===");
        output.AppendLine();

        foreach (var testCase in testCases)
        {
            FormatTestCase(output, testCase);
        }

        // Summary section
        output.AppendLine("=== GENERATION SUMMARY ===");
        output.AppendLine($"Total Test Cases: {testCases.Count}");
        output.AppendLine($"Test Case Types: {string.Join(", ", testCases.Select(tc => tc.Type).Distinct())}");
        output.AppendLine($"Test Categories: {string.Join(", ", testCases.Select(tc => tc.Category).Distinct())}");
        output.AppendLine($"Priority Distribution: {GetPriorityDistribution(testCases)}");
        output.AppendLine();

        // Import instructions
        output.AppendLine("=== TESTRAIL IMPORT INSTRUCTIONS ===");
        output.AppendLine("1. Copy the test case content from above");
        output.AppendLine("2. Log into your TestRail instance");
        output.AppendLine("3. Navigate to the appropriate test suite");
        output.AppendLine("4. Create new test cases using the Add Test Case feature");
        output.AppendLine("5. Paste the formatted content into the respective fields");
        output.AppendLine("6. Review and adjust test cases as needed");
        output.AppendLine("7. Execute test cases during testing phase");
        output.AppendLine();

        output.AppendLine("=== TESTRAIL GENERATION COMPLETE ===");
        output.AppendLine("Note: Review and customize these test cases before importing into TestRail");
        
        return output.ToString();
    }

    /// <summary>
    /// Formats a single test case into the output
    /// </summary>
    /// <param name="output">The StringBuilder to append to</param>
    /// <param name="testCase">The test case to format</param>
    private static void FormatTestCase(StringBuilder output, TestRailTestCase testCase)
    {
        output.AppendLine($"Test Case ID: {testCase.TestCaseId}");
        output.AppendLine($"Title: {testCase.Title}");
        output.AppendLine($"Priority: {testCase.Priority}");
        output.AppendLine($"Category: {testCase.Category}");
        output.AppendLine($"Type: {testCase.Type}");
        output.AppendLine($"Source: {testCase.SourceJiraKey}");
        output.AppendLine();

        if (testCase.Preconditions.Any())
        {
            output.AppendLine("Preconditions:");
            foreach (var precondition in testCase.Preconditions)
            {
                output.AppendLine($"  • {precondition}");
            }
            output.AppendLine();
        }

        if (testCase.Steps.Any())
        {
            output.AppendLine("Test Steps:");
            foreach (var step in testCase.Steps)
            {
                output.AppendLine($"  Step {step.StepNumber}: {step.StepTitle}");
                output.AppendLine($"    Action: {step.Action}");
                if (!string.IsNullOrWhiteSpace(step.TestData))
                {
                    output.AppendLine($"    Test Data: {step.TestData}");
                }
                output.AppendLine($"    Expected Result: {step.ExpectedResult}");
                output.AppendLine();
            }
        }
        
        output.AppendLine("---");
        output.AppendLine();
    }

    /// <summary>
    /// Formats TestRail output in CSV format for easy import
    /// </summary>
    /// <param name="testCases">List of TestRail test cases</param>
    /// <param name="story">The source Jira story</param>
    /// <returns>CSV formatted string</returns>
    public static string FormatTestRailCsv(List<TestRailTestCase> testCases, JiraStory story)
    {
        var csv = new StringBuilder();
        
        // CSV Header
        csv.AppendLine("Test Case ID,Title,Priority,Category,Type,Source,Preconditions,Steps,Expected Results");
        
        foreach (var testCase in testCases)
        {
            var preconditions = string.Join("; ", testCase.Preconditions);
            var steps = string.Join("; ", testCase.Steps.Select(s => $"Step {s.StepNumber}: {s.Action}"));
            var expectedResults = string.Join("; ", testCase.Steps.Select(s => s.ExpectedResult));
            
            csv.AppendLine($"\"{testCase.TestCaseId}\",\"{testCase.Title}\",\"{testCase.Priority}\",\"{testCase.Category}\",\"{testCase.Type}\",\"{testCase.SourceJiraKey}\",\"{preconditions}\",\"{steps}\",\"{expectedResults}\"");
        }
        
        return csv.ToString();
    }

    /// <summary>
    /// Formats TestRail output in JSON format
    /// </summary>
    /// <param name="testCases">List of TestRail test cases</param>
    /// <param name="story">The source Jira story</param>
    /// <returns>JSON formatted string</returns>
    public static string FormatTestRailJson(List<TestRailTestCase> testCases, JiraStory story)
    {
        var result = new
        {
            metadata = new
            {
                sourceStory = story.IssueKey,
                summary = story.Summary,
                generatedAt = DateTime.Now,
                testCaseCount = testCases.Count
            },
            testCases = testCases.Select(tc => new
            {
                id = tc.TestCaseId,
                title = tc.Title,
                priority = tc.Priority,
                category = tc.Category,
                type = tc.Type,
                source = tc.SourceJiraKey,
                preconditions = tc.Preconditions,
                steps = tc.Steps.Select(s => new
                {
                    stepNumber = s.StepNumber,
                    title = s.StepTitle,
                    action = s.Action,
                    testData = s.TestData,
                    expectedResult = s.ExpectedResult
                })
            })
        };
        
        return System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Formats TestRail output in XML format
    /// </summary>
    /// <param name="testCases">List of TestRail test cases</param>
    /// <param name="story">The source Jira story</param>
    /// <returns>XML formatted string</returns>
    public static string FormatTestRailXml(List<TestRailTestCase> testCases, JiraStory story)
    {
        var xml = new StringBuilder();
        
        xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        xml.AppendLine("<TestRailExport>");
        xml.AppendLine($"  <Metadata>");
        xml.AppendLine($"    <SourceStory>{story.IssueKey}</SourceStory>");
        xml.AppendLine($"    <Summary>{EscapeXml(story.Summary)}</Summary>");
        xml.AppendLine($"    <GeneratedAt>{DateTime.Now:yyyy-MM-dd HH:mm:ss}</GeneratedAt>");
        xml.AppendLine($"    <TestCaseCount>{testCases.Count}</TestCaseCount>");
        xml.AppendLine($"  </Metadata>");
        xml.AppendLine("  <TestCases>");
        
        foreach (var testCase in testCases)
        {
            xml.AppendLine($"    <TestCase>");
            xml.AppendLine($"      <ID>{testCase.TestCaseId}</ID>");
            xml.AppendLine($"      <Title>{EscapeXml(testCase.Title)}</Title>");
            xml.AppendLine($"      <Priority>{testCase.Priority}</Priority>");
            xml.AppendLine($"      <Category>{testCase.Category}</Category>");
            xml.AppendLine($"      <Type>{testCase.Type}</Type>");
            xml.AppendLine($"      <Source>{testCase.SourceJiraKey}</Source>");
            xml.AppendLine($"      <Preconditions>");
            
            foreach (var precondition in testCase.Preconditions)
            {
                xml.AppendLine($"        <Precondition>{EscapeXml(precondition)}</Precondition>");
            }
            
            xml.AppendLine($"      </Preconditions>");
            xml.AppendLine($"      <Steps>");
            
            foreach (var step in testCase.Steps)
            {
                xml.AppendLine($"        <Step>");
                xml.AppendLine($"          <Number>{step.StepNumber}</Number>");
                xml.AppendLine($"          <Title>{EscapeXml(step.StepTitle)}</Title>");
                xml.AppendLine($"          <Action>{EscapeXml(step.Action)}</Action>");
                xml.AppendLine($"          <TestData>{EscapeXml(step.TestData)}</TestData>");
                xml.AppendLine($"          <ExpectedResult>{EscapeXml(step.ExpectedResult)}</ExpectedResult>");
                xml.AppendLine($"        </Step>");
            }
            
            xml.AppendLine($"      </Steps>");
            xml.AppendLine($"    </TestCase>");
        }
        
        xml.AppendLine("  </TestCases>");
        xml.AppendLine("</TestRailExport>");
        
        return xml.ToString();
    }

    /// <summary>
    /// Formats TestRail output in Markdown format
    /// </summary>
    /// <param name="testCases">List of TestRail test cases</param>
    /// <param name="story">The source Jira story</param>
    /// <returns>Markdown formatted string</returns>
    public static string FormatTestRailMarkdown(List<TestRailTestCase> testCases, JiraStory story)
    {
        var md = new StringBuilder();
        
        md.AppendLine("# TestRail Test Case Generation");
        md.AppendLine();
        md.AppendLine($"**Source:** {story.IssueKey} - {story.Summary}");
        md.AppendLine($"**Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        md.AppendLine($"**Test Cases Generated:** {testCases.Count}");
        md.AppendLine();
        
        md.AppendLine("## Source Story Information");
        md.AppendLine();
        md.AppendLine($"- **Issue Key:** {story.IssueKey}");
        md.AppendLine($"- **Summary:** {story.Summary}");
        md.AppendLine($"- **Issue Type:** {story.IssueType}");
        md.AppendLine($"- **Priority:** {story.Priority}");
        if (!string.IsNullOrWhiteSpace(story.StoryPoints))
        {
            md.AppendLine($"- **Story Points:** {story.StoryPoints}");
        }
        md.AppendLine();
        
        if (!string.IsNullOrWhiteSpace(story.Description))
        {
            md.AppendLine("### Description");
            md.AppendLine(story.Description);
            md.AppendLine();
        }
        
        if (story.AcceptanceCriteria.Any())
        {
            md.AppendLine("### Acceptance Criteria");
            for (int i = 0; i < story.AcceptanceCriteria.Count; i++)
            {
                md.AppendLine($"{i + 1}. {story.AcceptanceCriteria[i]}");
            }
            md.AppendLine();
        }
        
        md.AppendLine("## Generated Test Cases");
        md.AppendLine();
        
        foreach (var testCase in testCases)
        {
            md.AppendLine($"### {testCase.TestCaseId} - {testCase.Title}");
            md.AppendLine();
            md.AppendLine($"- **Priority:** {testCase.Priority}");
            md.AppendLine($"- **Category:** {testCase.Category}");
            md.AppendLine($"- **Type:** {testCase.Type}");
            md.AppendLine($"- **Source:** {testCase.SourceJiraKey}");
            md.AppendLine();
            
            if (testCase.Preconditions.Any())
            {
                md.AppendLine("#### Preconditions");
                foreach (var precondition in testCase.Preconditions)
                {
                    md.AppendLine($"- {precondition}");
                }
                md.AppendLine();
            }
            
            if (testCase.Steps.Any())
            {
                md.AppendLine("#### Test Steps");
                md.AppendLine();
                md.AppendLine("| Step | Action | Test Data | Expected Result |");
                md.AppendLine("|------|--------|-----------|-----------------|");
                
                foreach (var step in testCase.Steps)
                {
                    md.AppendLine($"| {step.StepNumber} | {step.Action} | {step.TestData} | {step.ExpectedResult} |");
                }
                md.AppendLine();
            }
            
            md.AppendLine("---");
            md.AppendLine();
        }
        
        return md.ToString();
    }

    /// <summary>
    /// Wraps text to a specified width
    /// </summary>
    /// <param name="text">The text to wrap</param>
    /// <param name="width">The maximum width</param>
    /// <returns>Wrapped text</returns>
    private static string WrapText(string text, int width)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= width)
            return text;

        var words = text.Split(' ');
        var lines = new List<string>();
        var currentLine = "";

        foreach (var word in words)
        {
            if (currentLine.Length + word.Length + 1 <= width)
            {
                currentLine += (currentLine.Length > 0 ? " " : "") + word;
            }
            else
            {
                if (currentLine.Length > 0)
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
                else
                {
                    lines.Add(word);
                }
            }
        }

        if (currentLine.Length > 0)
            lines.Add(currentLine);

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Gets priority distribution summary
    /// </summary>
    /// <param name="testCases">List of test cases</param>
    /// <returns>Priority distribution string</returns>
    private static string GetPriorityDistribution(List<TestRailTestCase> testCases)
    {
        var distribution = testCases.GroupBy(tc => tc.Priority)
                                   .Select(g => $"{g.Key}: {g.Count()}")
                                   .ToArray();
        return string.Join(", ", distribution);
    }

    /// <summary>
    /// Escapes XML special characters
    /// </summary>
    /// <param name="text">The text to escape</param>
    /// <returns>Escaped text</returns>
    private static string EscapeXml(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return text.Replace("&", "&amp;")
                  .Replace("<", "&lt;")
                  .Replace(">", "&gt;")
                  .Replace("\"", "&quot;")
                  .Replace("'", "&apos;");
    }
}
