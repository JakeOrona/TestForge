using System.Text.Json.Serialization;

namespace ChatDemo.McpServer.Models;

/// <summary>
/// Represents a comprehensive analysis of a Jira ticket optimized for LLM processing
/// </summary>
public class LLMTicketAnalysis
{
    [JsonPropertyName("ticketInfo")]
    public TicketInfo TicketInfo { get; set; } = new();

    [JsonPropertyName("complexityAnalysis")]
    public ComplexityAnalysis ComplexityAnalysis { get; set; } = new();

    [JsonPropertyName("suggestedTestAreas")]
    public SuggestedTestAreas SuggestedTestAreas { get; set; } = new();

    [JsonPropertyName("baselineTestCases")]
    public List<BaselineTestCase> BaselineTestCases { get; set; } = new();

    [JsonPropertyName("llmGuidance")]
    public LLMGuidance LLMGuidance { get; set; } = new();
}

public class TicketInfo
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("acceptanceCriteria")]
    public List<string> AcceptanceCriteria { get; set; } = new();
}

public class ComplexityAnalysis
{
    [JsonPropertyName("uiComponents")]
    public List<UIComponent> UIComponents { get; set; } = new();

    [JsonPropertyName("businessLogic")]
    public List<BusinessLogic> BusinessLogic { get; set; } = new();

    [JsonPropertyName("integrationPoints")]
    public List<IntegrationPoint> IntegrationPoints { get; set; } = new();

    [JsonPropertyName("dataFlow")]
    public List<DataFlow> DataFlow { get; set; } = new();
}

public class UIComponent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("complexity")]
    public string Complexity { get; set; } = string.Empty;

    [JsonPropertyName("testAreas")]
    public List<string> TestAreas { get; set; } = new();

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
}

public class BusinessLogic
{
    [JsonPropertyName("rule")]
    public string Rule { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("testScenarios")]
    public List<string> TestScenarios { get; set; } = new();
}

public class IntegrationPoint
{
    [JsonPropertyName("system")]
    public string System { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("risk")]
    public string Risk { get; set; } = string.Empty;

    [JsonPropertyName("testRequirements")]
    public List<string> TestRequirements { get; set; } = new();
}

public class DataFlow
{
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("destination")]
    public string Destination { get; set; } = string.Empty;

    [JsonPropertyName("dataType")]
    public string DataType { get; set; } = string.Empty;

    [JsonPropertyName("validationRules")]
    public List<string> ValidationRules { get; set; } = new();
}

public class SuggestedTestAreas
{
    [JsonPropertyName("functional")]
    public List<string> Functional { get; set; } = new();

    [JsonPropertyName("ui")]
    public List<string> UI { get; set; } = new();

    [JsonPropertyName("integration")]
    public List<string> Integration { get; set; } = new();

    [JsonPropertyName("performance")]
    public List<string> Performance { get; set; } = new();

    [JsonPropertyName("security")]
    public List<string> Security { get; set; } = new();

    [JsonPropertyName("accessibility")]
    public List<string> Accessibility { get; set; } = new();
}

public class BaselineTestCase
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("steps")]
    public List<string> Steps { get; set; } = new();

    [JsonPropertyName("expectedResult")]
    public string ExpectedResult { get; set; } = string.Empty;

    [JsonPropertyName("testData")]
    public string TestData { get; set; } = string.Empty;

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
}

public class LLMGuidance
{
    [JsonPropertyName("focusAreas")]
    public List<string> FocusAreas { get; set; } = new();

    [JsonPropertyName("suggestedPrompts")]
    public List<string> SuggestedPrompts { get; set; } = new();

    [JsonPropertyName("complexityScore")]
    public double ComplexityScore { get; set; }

    [JsonPropertyName("riskAreas")]
    public List<string> RiskAreas { get; set; } = new();

    [JsonPropertyName("testingStrategy")]
    public string TestingStrategy { get; set; } = string.Empty;
}
