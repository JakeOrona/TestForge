using System.Text;
using System.Text.Json;
using TestForge.McpServer.Models;

namespace TestForge.McpServer.Services;

/// <summary>
/// Interface for TestRail formatting services
/// </summary>
public interface ITestRailFormattingService
{
    Task<string> FormatEnhancedTestSuite(EnhancedTestSuite testSuite);
    string FormatTestRailOutput(List<TestRailTestCase> testCases, JiraStory story);
    string FormatTestRailCsv(List<TestRailTestCase> testCases, JiraStory story);
    string FormatTestRailJson(List<TestRailTestCase> testCases, JiraStory story);
    string FormatTestRailXml(List<TestRailTestCase> testCases, JiraStory story);
    string FormatTestRailMarkdown(List<TestRailTestCase> testCases, JiraStory story);
    string FormatComprehensiveTestSuite(EnhancedTestSuite testSuite);
}

/// <summary>
/// Service responsible for formatting TestRail test cases into readable output
/// </summary>
public class TestRailFormattingService : ITestRailFormattingService
{
    /// <summary>
    /// Formats an enhanced test suite with comprehensive coverage into TestRail-compatible format
    /// </summary>
    /// <param name="testSuite">The enhanced test suite to format</param>
    /// <returns>Formatted TestRail output string</returns>
    public async Task<string> FormatEnhancedTestSuite(EnhancedTestSuite testSuite)
    {
        var output = new StringBuilder();
        
        // Header section
        output.AppendLine("=== COMPREHENSIVE TEST SUITE GENERATION ===");
        output.AppendLine($"Generated: {testSuite.GenerationTimestamp:yyyy-MM-dd HH:mm:ss}");
        output.AppendLine($"Total Test Cases: {testSuite.TotalTestCount}");
        output.AppendLine($"Original Tests: {testSuite.OriginalTests.Count}");
        output.AppendLine($"Enhanced Tests: {testSuite.EnhancedTests.Count}");
        output.AppendLine($"Coverage: {testSuite.CoverageSummary.CoveragePercentage:F1}%");
        output.AppendLine();

        // Coverage summary
        output.AppendLine("=== COVERAGE SUMMARY ===");
        output.AppendLine($"Positive Tests: {testSuite.CoverageSummary.PositiveTestCount}");
        output.AppendLine($"Negative Tests: {testSuite.CoverageSummary.NegativeTestCount}");
        output.AppendLine($"Security Tests: {testSuite.CoverageSummary.SecurityTestCount}");
        output.AppendLine($"Accessibility Tests: {testSuite.CoverageSummary.AccessibilityTestCount}");
        output.AppendLine($"Performance Tests: {testSuite.CoverageSummary.PerformanceTestCount}");
        output.AppendLine($"Boundary Tests: {testSuite.CoverageSummary.BoundaryTestCount}");
        output.AppendLine($"Error Handling Tests: {testSuite.CoverageSummary.ErrorHandlingTestCount}");
        output.AppendLine($"Integration Tests: {testSuite.CoverageSummary.IntegrationTestCount}");
        output.AppendLine($"User Experience Tests: {testSuite.CoverageSummary.UserExperienceTestCount}");
        output.AppendLine($"Data Validation Tests: {testSuite.CoverageSummary.DataValidationTestCount}");
        output.AppendLine($"State Management Tests: {testSuite.CoverageSummary.StateManagementTestCount}");
        output.AppendLine();

        // Original tests section
        if (testSuite.OriginalTests.Any())
        {
            output.AppendLine("=== ORIGINAL TEST CASES ===");
            output.AppendLine();
            
            foreach (var testCase in testSuite.OriginalTests)
            {
                FormatEnhancedTestCase(output, testCase);
            }
        }

        // Enhanced tests section
        if (testSuite.EnhancedTests.Any())
        {
            output.AppendLine("=== ENHANCED TEST CASES ===");
            output.AppendLine();
            
            // Group by category for better organization
            var groupedTests = testSuite.EnhancedTests.GroupBy(t => t.Category);
            
            foreach (var categoryGroup in groupedTests)
            {
                output.AppendLine($"--- {categoryGroup.Key.ToString().ToUpper()} TESTS ---");
                output.AppendLine();
                
                foreach (var testCase in categoryGroup.OrderBy(t => t.Priority))
                {
                    FormatEnhancedTestCase(output, testCase);
                }
            }
        }

        // Enhancement configuration
        output.AppendLine("=== ENHANCEMENT CONFIGURATION ===");
        output.AppendLine($"Negative Tests: {(testSuite.EnhancementConfig.IncludeNegativeTests ? "✓" : "✗")}");
        output.AppendLine($"Security Tests: {(testSuite.EnhancementConfig.IncludeSecurityTests ? "✓" : "✗")}");
        output.AppendLine($"Accessibility Tests: {(testSuite.EnhancementConfig.IncludeAccessibilityTests ? "✓" : "✗")}");
        output.AppendLine($"Performance Tests: {(testSuite.EnhancementConfig.IncludePerformanceTests ? "✓" : "✗")}");
        output.AppendLine($"Boundary Tests: {(testSuite.EnhancementConfig.IncludeBoundaryTests ? "✓" : "✗")}");
        output.AppendLine($"Error Handling Tests: {(testSuite.EnhancementConfig.IncludeErrorHandlingTests ? "✓" : "✗")}");
        output.AppendLine($"Integration Tests: {(testSuite.EnhancementConfig.IncludeIntegrationTests ? "✓" : "✗")}");
        output.AppendLine($"User Experience Tests: {(testSuite.EnhancementConfig.IncludeUserExperienceTests ? "✓" : "✗")}");
        output.AppendLine($"Data Validation Tests: {(testSuite.EnhancementConfig.IncludeDataValidationTests ? "✓" : "✗")}");
        output.AppendLine($"State Management Tests: {(testSuite.EnhancementConfig.IncludeStateManagementTests ? "✓" : "✗")}");
        output.AppendLine($"Maximum Complexity Level: {testSuite.EnhancementConfig.MaxComplexity}");
        output.AppendLine();

        // Import instructions
        output.AppendLine("=== TESTRAIL IMPORT INSTRUCTIONS ===");
        output.AppendLine("1. Review the comprehensive test coverage above");
        output.AppendLine("2. Select relevant test cases based on project requirements");
        output.AppendLine("3. Copy selected test cases to TestRail");
        output.AppendLine("4. Organize by test category and priority");
        output.AppendLine("5. Execute tests according to risk and priority levels");
        output.AppendLine();

        if (testSuite.CoverageSummary.IdentifiedGaps.Any())
        {
            output.AppendLine("=== IDENTIFIED GAPS ===");
            foreach (var gap in testSuite.CoverageSummary.IdentifiedGaps)
            {
                output.AppendLine($"⚠️  {gap}");
            }
            output.AppendLine();
        }

        output.AppendLine("=== COMPREHENSIVE TEST GENERATION COMPLETE ===");
        output.AppendLine($"Generated {testSuite.TotalTestCount} test cases with {testSuite.CoverageSummary.CoveragePercentage:F1}% coverage");
        
        return output.ToString();
    }

    /// <summary>
    /// Formats a comprehensive test suite into a structured format
    /// </summary>
    /// <param name="testSuite">The enhanced test suite to format</param>
    /// <returns>Formatted comprehensive test suite string</returns>
    public string FormatComprehensiveTestSuite(EnhancedTestSuite testSuite)
    {
        var output = new StringBuilder();
        
        // Generate detailed test case documentation
        output.AppendLine("# Comprehensive Test Suite");
        output.AppendLine();
        output.AppendLine("## Overview");
        output.AppendLine($"- **Total Test Cases:** {testSuite.TotalTestCount}");
        output.AppendLine($"- **Coverage:** {testSuite.CoverageSummary.CoveragePercentage:F1}%");
        output.AppendLine($"- **Generated:** {testSuite.GenerationTimestamp:yyyy-MM-dd HH:mm:ss}");
        output.AppendLine();

        // Test categories breakdown
        output.AppendLine("## Test Categories");
        output.AppendLine("| Category | Test Count | Description |");
        output.AppendLine("|----------|------------|-------------|");
        output.AppendLine($"| Functional | {testSuite.CoverageSummary.PositiveTestCount} | Core functionality testing |");
        output.AppendLine($"| Negative | {testSuite.CoverageSummary.NegativeTestCount} | Error condition testing |");
        output.AppendLine($"| Security | {testSuite.CoverageSummary.SecurityTestCount} | Security vulnerability testing |");
        output.AppendLine($"| Accessibility | {testSuite.CoverageSummary.AccessibilityTestCount} | WCAG compliance testing |");
        output.AppendLine($"| Performance | {testSuite.CoverageSummary.PerformanceTestCount} | Load and performance testing |");
        output.AppendLine($"| Boundary | {testSuite.CoverageSummary.BoundaryTestCount} | Edge case testing |");
        output.AppendLine($"| Error Handling | {testSuite.CoverageSummary.ErrorHandlingTestCount} | Error scenario testing |");
        output.AppendLine($"| Integration | {testSuite.CoverageSummary.IntegrationTestCount} | System integration testing |");
        output.AppendLine($"| User Experience | {testSuite.CoverageSummary.UserExperienceTestCount} | UI/UX testing |");
        output.AppendLine($"| Data Validation | {testSuite.CoverageSummary.DataValidationTestCount} | Data integrity testing |");
        output.AppendLine($"| State Management | {testSuite.CoverageSummary.StateManagementTestCount} | Application state testing |");
        output.AppendLine();

        // All test cases in markdown format
        foreach (var testCase in testSuite.OriginalTests.Concat(testSuite.EnhancedTests))
        {
            output.AppendLine($"## Test Case: {testCase.Title}");
            output.AppendLine($"**Test ID:** {testCase.Id}");
            output.AppendLine($"**Priority:** {testCase.Priority}");
            output.AppendLine($"**Type:** {testCase.Type}");
            output.AppendLine($"**Category:** {testCase.Category}");
            output.AppendLine($"**Source:** {testCase.Source}");
            output.AppendLine($"**Confidence:** {testCase.Confidence:F2}");
            output.AppendLine();

            if (!string.IsNullOrEmpty(testCase.Description))
            {
                output.AppendLine($"**Description:** {testCase.Description}");
                output.AppendLine();
            }

            if (testCase.Preconditions.Any())
            {
                output.AppendLine("**Preconditions:**");
                foreach (var precondition in testCase.Preconditions)
                {
                    output.AppendLine($"- {precondition}");
                }
                output.AppendLine();
            }

            if (testCase.TestSteps.Any())
            {
                output.AppendLine("**Test Steps:**");
                foreach (var step in testCase.TestSteps)
                {
                    output.AppendLine($"{step.StepNumber}. {step.Action}");
                    if (!string.IsNullOrEmpty(step.TestData))
                    {
                        output.AppendLine($"   - **Test Data:** {step.TestData}");
                    }
                    output.AppendLine($"   - **Expected Result:** {step.ExpectedResult}");
                    output.AppendLine();
                }
            }

            if (testCase.ExpectedResults.Any())
            {
                output.AppendLine("**Expected Results:**");
                foreach (var result in testCase.ExpectedResults)
                {
                    output.AppendLine($"- {result}");
                }
                output.AppendLine();
            }

            if (testCase.TestData.Any())
            {
                output.AppendLine("**Test Data:**");
                foreach (var data in testCase.TestData)
                {
                    output.AppendLine($"- {data}");
                }
                output.AppendLine();
            }

            output.AppendLine("---");
            output.AppendLine();
        }

        return output.ToString();
    }

    /// <summary>
    /// Formats a single enhanced test case into the output
    /// </summary>
    /// <param name="output">The StringBuilder to append to</param>
    /// <param name="testCase">The enhanced test case to format</param>
    private static void FormatEnhancedTestCase(StringBuilder output, TestCase testCase)
    {
        output.AppendLine($"Test Case ID: {testCase.Id}");
        output.AppendLine($"Title: {testCase.Title}");
        output.AppendLine($"Priority: {testCase.Priority}");
        output.AppendLine($"Category: {testCase.Category}");
        output.AppendLine($"Type: {testCase.Type}");
        output.AppendLine($"Source: {testCase.Source}");
        output.AppendLine($"Confidence: {testCase.Confidence:F2}");
        output.AppendLine();

        if (!string.IsNullOrEmpty(testCase.Description))
        {
            output.AppendLine($"Description: {testCase.Description}");
            output.AppendLine();
        }

        if (testCase.Preconditions.Any())
        {
            output.AppendLine("Preconditions:");
            foreach (var precondition in testCase.Preconditions)
            {
                output.AppendLine($"  • {precondition}");
            }
            output.AppendLine();
        }

        if (testCase.TestSteps.Any())
        {
            output.AppendLine("Test Steps:");
            foreach (var step in testCase.TestSteps)
            {
                output.AppendLine($"  Step {step.StepNumber}: {step.Action}");
                if (!string.IsNullOrWhiteSpace(step.TestData))
                {
                    output.AppendLine($"    Test Data: {step.TestData}");
                }
                output.AppendLine($"    Expected Result: {step.ExpectedResult}");
                output.AppendLine();
            }
        }

        if (testCase.ExpectedResults.Any())
        {
            output.AppendLine("Expected Results:");
            foreach (var result in testCase.ExpectedResults)
            {
                output.AppendLine($"  • {result}");
            }
            output.AppendLine();
        }

        if (testCase.TestData.Any())
        {
            output.AppendLine("Test Data:");
            foreach (var data in testCase.TestData)
            {
                output.AppendLine($"  • {data}");
            }
            output.AppendLine();
        }
        
        output.AppendLine("---");
        output.AppendLine();
    }

    /// <summary>
    /// Formats TestRail test cases into a structured output format
    /// </summary>
    /// <param name="testCases">List of TestRail test cases</param>
    /// <param name="story">The source Jira story</param>
    /// <returns>Formatted TestRail output string</returns>
    public string FormatTestRailOutput(List<TestRailTestCase> testCases, JiraStory story)
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
    public string FormatTestRailCsv(List<TestRailTestCase> testCases, JiraStory story)
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
    public string FormatTestRailJson(List<TestRailTestCase> testCases, JiraStory story)
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
        
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Formats TestRail output in XML format
    /// </summary>
    /// <param name="testCases">List of TestRail test cases</param>
    /// <param name="story">The source Jira story</param>
    /// <returns>XML formatted string</returns>
    public string FormatTestRailXml(List<TestRailTestCase> testCases, JiraStory story)
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
    public string FormatTestRailMarkdown(List<TestRailTestCase> testCases, JiraStory story)
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

    #region Static Compatibility Methods

    /// <summary>
    /// Static method for formatting TestRail output (backward compatibility)
    /// </summary>
    public static string FormatTestRailOutputStatic(List<TestRailTestCase> testCases, JiraStory story)
    {
        var service = new TestRailFormattingService();
        return service.FormatTestRailOutput(testCases, story);
    }

    #endregion
}
