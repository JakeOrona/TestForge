namespace TestForge.McpServer.Models;

/// <summary>
/// Metadata extracted from Jira analysis for workflow processing
/// </summary>
public class WorkflowMetadata
{
    public string TicketType { get; set; } = "Story";
    public string Priority { get; set; } = "Medium";
    public string Component { get; set; } = "UI";
    public string Description { get; set; } = "";
    public string TicketKey { get; set; } = "";
    public string Summary { get; set; } = "";
}

/// <summary>
/// Workflow step result with status and output
/// </summary>
public class WorkflowStep
{
    public string Result { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Complete workflow result structure
/// </summary>
public class WorkflowResult
{
    public string WorkflowVersion { get; set; } = "1.0";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, WorkflowStep> Steps { get; set; } = new();
    public WorkflowSummary Summary { get; set; } = new();
}

/// <summary>
/// Workflow processing summary
/// </summary>
public class WorkflowSummary
{
    public int TotalSteps { get; set; }
    public int SuccessfulSteps { get; set; }
    public bool XmlCleaned { get; set; }
    public string TicketType { get; set; } = "";
    public string Priority { get; set; } = "";
    public string Component { get; set; } = "";
    public DateTime ProcessingTime { get; set; } = DateTime.UtcNow;
    public string Recommendation { get; set; } = "";
}
