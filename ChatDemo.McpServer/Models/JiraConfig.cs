namespace ChatDemo.McpServer.Models;

/// <summary>
/// Configuration settings for Jira API integration
/// </summary>
public class JiraConfig
{
    /// <summary>
    /// The base URL of the Jira instance (e.g., https://yourcompany.atlassian.net)
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// The username/email for Jira authentication
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The API token for Jira authentication
    /// </summary>
    public string ApiToken { get; set; } = string.Empty;

    /// <summary>
    /// Validates that all required configuration values are provided
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(BaseUrl) && 
                          !string.IsNullOrWhiteSpace(Username) && 
                          !string.IsNullOrWhiteSpace(ApiToken);
}
