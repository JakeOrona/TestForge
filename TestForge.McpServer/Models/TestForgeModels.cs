using TestForge.McpServer.Models;

namespace TestForge.McpServer;

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
    public List<JiraComment> Comments { get; init; } = new();
    public Dictionary<string, string> CustomFields { get; init; } = new();
}

/// <summary>
/// Represents a Jira comment with metadata
/// </summary>
public record JiraComment
{
    public string Id { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string AuthorId { get; init; } = string.Empty;
    public DateTime Created { get; init; } = DateTime.MinValue;
    public string Content { get; init; } = string.Empty;
    public string CleanContent { get; init; } = string.Empty;
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

/// <summary>
/// Enhanced test suite containing original and LLM-enhanced test cases
/// </summary>
public class EnhancedTestSuite
{
    public List<TestCase> OriginalTests { get; set; } = new();
    public List<TestCase> EnhancedTests { get; set; } = new();
    public TestEnhancementConfig EnhancementConfig { get; set; } = new();
    public DateTime GenerationTimestamp { get; set; }
    public int TotalTestCount => OriginalTests.Count + EnhancedTests.Count;
    public TestCoverageSummary CoverageSummary { get; set; } = new();
}

/// <summary>
/// Configuration for test enhancement generation
/// </summary>
public class TestEnhancementConfig
{
    public bool IncludeNegativeTests { get; set; } = true;
    public bool IncludeSecurityTests { get; set; } = true;
    public bool IncludeAccessibilityTests { get; set; } = true;
    public bool IncludePerformanceTests { get; set; } = true;
    public bool IncludeBoundaryTests { get; set; } = true;
    public bool IncludeErrorHandlingTests { get; set; } = true;
    public bool IncludeIntegrationTests { get; set; } = true;
    public bool IncludeUserExperienceTests { get; set; } = true;
    public bool IncludeDataValidationTests { get; set; } = true;
    public bool IncludeStateManagementTests { get; set; } = true;
    public TestComplexityLevel MaxComplexity { get; set; } = TestComplexityLevel.High;
}

/// <summary>
/// Summary of test coverage across different categories
/// </summary>
public class TestCoverageSummary
{
    public int PositiveTestCount { get; set; }
    public int NegativeTestCount { get; set; }
    public int SecurityTestCount { get; set; }
    public int AccessibilityTestCount { get; set; }
    public int PerformanceTestCount { get; set; }
    public int BoundaryTestCount { get; set; }
    public int ErrorHandlingTestCount { get; set; }
    public int IntegrationTestCount { get; set; }
    public int UserExperienceTestCount { get; set; }
    public int DataValidationTestCount { get; set; }
    public int StateManagementTestCount { get; set; }
    public decimal CoveragePercentage { get; set; }
    public List<string> IdentifiedGaps { get; set; } = new();
}

/// <summary>
/// Test complexity levels for enhancement generation
/// </summary>
public enum TestComplexityLevel
{
    Basic = 1,
    Intermediate = 2,
    Advanced = 3,
    High = 4,
    Expert = 5
}

/// <summary>
/// Enhanced test case with comprehensive metadata
/// </summary>
public class TestCase
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public TestCategoryType Category { get; set; } = TestCategoryType.Functional;
    public TestType Type { get; set; } = TestType.Positive;
    public List<string> Preconditions { get; set; } = new();
    public List<TestStep> TestSteps { get; set; } = new();
    public List<string> ExpectedResults { get; set; } = new();
    public List<string> TestData { get; set; } = new();
    public double Confidence { get; set; } = 1.0;
    public string Source { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Individual test step with action and expected result
/// </summary>
public class TestStep
{
    public int StepNumber { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ExpectedResult { get; set; } = string.Empty;
    public string TestData { get; set; } = string.Empty;
}

/// <summary>
/// Test category types for comprehensive coverage
/// </summary>
public enum TestCategoryType
{
    Functional,
    Security,
    Performance,
    Accessibility,
    UI,
    Integration,
    DataValidation,
    ErrorHandling,
    UserExperience,
    StateManagement
}

/// <summary>
/// Test types for scenario classification
/// </summary>
public enum TestType
{
    Positive,
    Negative,
    Boundary,
    Edge,
    ErrorHandling,
    Load,
    Stress,
    Compatibility,
    Usability,
    Security
}

/// <summary>
/// Result of test case enhancement operation
/// </summary>
public class TestEnhancementResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Error { get; set; }
    public int OriginalTestCount { get; set; }
    public int EnhancedTestCount { get; set; }
    public int TotalTestCount { get; set; }
    public TestCoverageSummary CoverageSummary { get; set; } = new();
    public string FormattedTestCases { get; set; } = string.Empty;
    public DateTime GenerationTimestamp { get; set; }
    public List<TestCategoryBreakdown> CategoryBreakdown { get; set; } = new();
}

/// <summary>
/// Breakdown of tests by category
/// </summary>
public class TestCategoryBreakdown
{
    public string Category { get; set; } = string.Empty;
    public int TestCount { get; set; }
    public List<string> TestTitles { get; set; } = new();
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Comprehensive test matrix for complete coverage analysis
/// </summary>
public class TestMatrix
{
    public List<TestCategory> Categories { get; set; } = new();
    public List<TestScenario> Scenarios { get; set; } = new();
    public TestCoverageMatrix CoverageMatrix { get; set; } = new();
}

/// <summary>
/// Test coverage matrix mapping features to test types
/// </summary>
public class TestCoverageMatrix
{
    public Dictionary<string, List<string>> FeatureTestMapping { get; set; } = new();
    public Dictionary<string, List<string>> UserRoleTestMapping { get; set; } = new();
    public Dictionary<string, List<string>> ComponentTestMapping { get; set; } = new();
    public List<string> IdentifiedGaps { get; set; } = new();
}

/// <summary>
/// Test category with comprehensive details
/// </summary>
public class TestCategory
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> TestTypes { get; set; } = new();
    public int PotentialTestCount { get; set; }
}

/// <summary>
/// Test scenario with full context
/// </summary>
public class TestScenario
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public List<string> RelatedFeatures { get; set; } = new();
}

/// <summary>
/// Parsed Jira data structure for LLM enhancement
/// </summary>
public class ParsedJiraData
{
    public string TicketId { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public List<string> AcceptanceCriteria { get; set; } = new();
    public Dictionary<string, string> CustomFields { get; set; } = new();
    public List<UIComponent> UIComponents { get; set; } = new();
    public List<BusinessLogic> BusinessLogic { get; set; } = new();
    public double ComplexityScore { get; set; }
}
