using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Xml.Linq;
using System.Xml;
using System.Text.Json;

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
    /// Parses Jira story ticket XML and generates test case steps for web UI automation.
    /// </summary>
    /// <param name="jiraXml">The Jira ticket XML content to parse</param>
    /// <returns>Generated test case steps or error message</returns>
    [McpServerTool, Description("Parses Jira story ticket XML and generates test case steps for web UI automation.")]
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

            // Generate formatted output showing extracted data
            var result = FormatExtractedStoryData(parseResult.Story);
            
            // TODO: Next phase - implement test case generation from story data
            return result + "\n\n[Next Phase: Test case generation will be implemented]";
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
            var doc = XDocument.Parse(xmlContent);
            
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
    /// Extracts field value using multiple possible XML element names
    /// </summary>
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

    /// <summary>
    /// Extracts acceptance criteria from various XML structures
    /// </summary>
    private static List<string> ExtractAcceptanceCriteria(XElement element)
    {
        var criteria = new List<string>();
        
        // Try different possible structures for acceptance criteria
        var acceptanceElements = element.Descendants("acceptance-criteria")
                               .Concat(element.Descendants("acceptancecriteria"))
                               .Concat(element.Descendants("criteria"))
                               .Concat(element.Descendants("acceptance"));
        
        foreach (var criteriaElement in acceptanceElements)
        {
            var text = criteriaElement.Value?.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                // Split on common delimiters for multiple criteria
                var splitCriteria = text.Split(new[] { '\n', '\r', ';', '*', '-' }, 
                                             StringSplitOptions.RemoveEmptyEntries)
                                       .Select(c => c.Trim())
                                       .Where(c => !string.IsNullOrWhiteSpace(c));
                
                criteria.AddRange(splitCriteria);
            }
        }
        
        return criteria;
    }

    /// <summary>
    /// Extracts custom fields from Jira XML
    /// </summary>
    private static Dictionary<string, string> ExtractCustomFields(XElement element)
    {
        var customFields = new Dictionary<string, string>();
        
        // Look for custom field patterns
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

    /// <summary>
    /// Formats extracted story data for output
    /// </summary>
    private static string FormatExtractedStoryData(JiraStory story)
    {
        var output = new System.Text.StringBuilder();
        
        output.AppendLine("=== JIRA STORY EXTRACTION RESULTS ===");
        output.AppendLine($"Issue Key: {story.IssueKey}");
        output.AppendLine($"Summary: {story.Summary}");
        output.AppendLine($"Issue Type: {story.IssueType}");
        output.AppendLine($"Priority: {story.Priority}");
        output.AppendLine($"Story Points: {story.StoryPoints}");
        output.AppendLine();
        
        output.AppendLine("Description:");
        output.AppendLine(string.IsNullOrWhiteSpace(story.Description) ? "(No description)" : story.Description);
        output.AppendLine();
        
        if (story.AcceptanceCriteria.Any())
        {
            output.AppendLine("Acceptance Criteria:");
            for (int i = 0; i < story.AcceptanceCriteria.Count; i++)
            {
                output.AppendLine($"  {i + 1}. {story.AcceptanceCriteria[i]}");
            }
            output.AppendLine();
        }
        
        if (story.CustomFields.Any())
        {
            output.AppendLine("Custom Fields:");
            foreach (var field in story.CustomFields)
            {
                output.AppendLine($"  {field.Key}: {field.Value}");
            }
            output.AppendLine();
        }
        
        output.AppendLine("=== EXTRACTION COMPLETE ===");
        
        return output.ToString();
    }
}
