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
