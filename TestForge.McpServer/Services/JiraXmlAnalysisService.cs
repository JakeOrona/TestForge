using System.Text.Json;
using System.Text.RegularExpressions;
using TestForge.McpServer.Models;

namespace TestForge.McpServer.Services;

/// <summary>
/// Service responsible for analyzing Jira XML content and providing structured data for LLM enhancement
/// </summary>
public static class JiraXmlAnalysisService
{
    /// <summary>
    /// Analyzes raw Jira XML and returns structured data optimized for LLM enhancement
    /// </summary>
    /// <param name="jiraXml">The raw Jira XML content to analyze</param>
    /// <returns>Comprehensive JSON analysis including UI components, business logic, complexity scoring, and LLM guidance</returns>
    public static string AnalyzeForLLM(string jiraXml)
    {
        try
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(jiraXml))
            {
                return JsonSerializer.Serialize(new { error = "Jira XML content is required" });
            }

            // Step 1: Validate and clean XML
            var validation = JiraXmlValidationService.Validate(jiraXml);
            string cleanXml = jiraXml;
            
            // Parse validation result to check if XML is valid
            var validationObj = JsonSerializer.Deserialize<dynamic>(validation);
            bool isValid = validationObj?.GetProperty("isValid").GetBoolean() ?? false;
            
            if (!isValid)
            {
                var cleanResult = JiraXmlCleaningService.Clean(jiraXml);
                var cleanObj = JsonSerializer.Deserialize<dynamic>(cleanResult);
                if (cleanObj?.GetProperty("success").GetBoolean() == true)
                {
                    cleanXml = cleanObj.GetProperty("cleanedXml").GetString() ?? jiraXml;
                }
                else
                {
                    return JsonSerializer.Serialize(new { 
                        error = "XML validation failed and cleaning was unsuccessful",
                        suggestion = "Try using validate_jira_xml or clean_jira_xml tools first"
                    });
                }
            }

            // Step 2: Parse XML to extract ticket fields
            var parseResult = JiraStoryParsingService.ParseJiraXml(cleanXml);
            
            if (!parseResult.IsSuccess || parseResult.Story == null)
            {
                return JsonSerializer.Serialize(new { 
                    error = $"Failed to parse Jira XML: {parseResult.ErrorMessage}",
                    suggestion = "Try using validate_jira_xml or clean_jira_xml tools first"
                });
            }

            var story = parseResult.Story;
            
            // Step 3: Analyze description for UI components and business logic
            var description = story.Description;
            var uiComponentsAnalysis = AnalyzeUIComponents(description);
            var businessLogicAnalysis = AnalyzeBusinessLogic(description);
            
            // Step 4: Generate complexity scoring
            var complexityScore = CalculateComplexityScore(story, uiComponentsAnalysis, businessLogicAnalysis);
            
            // Step 5: Create structured analysis for LLM processing
            var analysis = new
            {
                ticketInfo = new
                {
                    key = story.IssueKey,
                    summary = story.Summary,
                    description = story.Description,
                    type = story.IssueType,
                    priority = story.Priority,
                    storyPoints = story.StoryPoints,
                    status = story.CustomFields.GetValueOrDefault("status", "Unknown"),
                    acceptanceCriteria = story.AcceptanceCriteria,
                    commentsSummary = new
                    {
                        totalComments = story.Comments.Count,
                        hasComments = story.Comments.Any(),
                        recentActivity = story.Comments.Count > 0 ? story.Comments.OrderByDescending(c => c.Created).Take(3).Select(c => new
                        {
                            author = c.Author,
                            date = c.Created,
                            preview = c.CleanContent.Length > 50 ? c.CleanContent.Substring(0, 50) + "..." : c.CleanContent
                        }).ToArray() : null
                    }
                },
                complexityAnalysis = new
                {
                    overallScore = complexityScore,
                    uiComponents = uiComponentsAnalysis,
                    businessLogic = businessLogicAnalysis,
                    integrationPoints = IdentifyIntegrationPoints(description),
                    testingComplexity = AssessTestingComplexity(story.IssueType, description)
                },
                suggestedTestAreas = new
                {
                    functional = GenerateFunctionalTestAreas(story),
                    ui = ExtractUITestAreas(description),
                    integration = ExtractIntegrationTestAreas(description),
                    security = IdentifySecurityTestAreas(description),
                    performance = IdentifyPerformanceTestAreas(description)
                },
                baselineTestCases = GenerateBaselineTestCases(story),
                commentsAnalysis = AnalyzeComments(story.Comments),
                llmGuidance = new
                {
                    focusAreas = DetermineFocusAreas(story, complexityScore),
                    suggestedPrompts = GenerateLLMPrompts(story),
                    complexityScore = complexityScore,
                    testingStrategy = DetermineTestingStrategy(story.IssueType, complexityScore),
                    confidence = CalculateAnalysisConfidence(story, isValid)
                },
                xmlProcessing = new
                {
                    wasXmlCleaned = !isValid,
                    originalValidation = isValid,
                    parsingSuccess = parseResult.IsSuccess,
                    extractedFields = new
                    {
                        hasDescription = !string.IsNullOrEmpty(story.Description),
                        hasAcceptanceCriteria = story.AcceptanceCriteria.Any(),
                        hasComments = story.Comments.Any(),
                        customFieldsCount = story.CustomFields.Count
                    }
                }
            };
            
            return JsonSerializer.Serialize(analysis, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { 
                error = $"Analysis failed: {ex.Message}",
                suggestion = "Ensure XML is valid Jira export format. Try validate_jira_xml tool first."
            });
        }
    }

    /// <summary>
    /// Analyzes UI components in the description text
    /// </summary>
    /// <param name="description">The description text to analyze</param>
    /// <returns>UI components analysis result</returns>
    private static object AnalyzeUIComponents(string description)
    {
        var components = new List<object>();
        
        if (!string.IsNullOrWhiteSpace(description))
        {
            var lowerDesc = description.ToLower();
            
            if (lowerDesc.Contains("input") || lowerDesc.Contains("field"))
            {
                components.Add(new { type = "input", name = "input_field", complexity = "medium", testAreas = new[] { "validation", "formatting" }, confidence = 0.8 });
            }
            
            if (lowerDesc.Contains("button") || lowerDesc.Contains("click"))
            {
                components.Add(new { type = "button", name = "button_element", complexity = "low", testAreas = new[] { "interaction", "state" }, confidence = 0.85 });
            }
            
            if (lowerDesc.Contains("modal") || lowerDesc.Contains("dialog"))
            {
                components.Add(new { type = "modal", name = "modal_dialog", complexity = "high", testAreas = new[] { "display", "interaction", "close" }, confidence = 0.9 });
            }
            
            if (lowerDesc.Contains("form"))
            {
                components.Add(new { type = "form", name = "form_element", complexity = "high", testAreas = new[] { "validation", "submission" }, confidence = 0.88 });
            }
        }
        
        return components;
    }

    /// <summary>
    /// Analyzes business logic in the description text
    /// </summary>
    /// <param name="description">The description text to analyze</param>
    /// <returns>Business logic analysis result</returns>
    private static object AnalyzeBusinessLogic(string description)
    {
        var businessLogic = new List<object>();
        
        if (!string.IsNullOrWhiteSpace(description))
        {
            var lowerDesc = description.ToLower();
            
            if (lowerDesc.Contains("validation") || lowerDesc.Contains("validate"))
            {
                businessLogic.Add(new { rule = "validation", description = "Input validation logic", confidence = 0.9, testScenarios = new[] { "valid input", "invalid input" } });
            }
            
            if (lowerDesc.Contains("required") || lowerDesc.Contains("mandatory"))
            {
                businessLogic.Add(new { rule = "required_field", description = "Required field logic", confidence = 0.85, testScenarios = new[] { "missing required", "present required" } });
            }
            
            if (lowerDesc.Contains("minimum") || lowerDesc.Contains("maximum"))
            {
                businessLogic.Add(new { rule = "min_max_validation", description = "Minimum/maximum value validation", confidence = 0.95, testScenarios = new[] { "below minimum", "above maximum", "within range" } });
            }
            
            if (lowerDesc.Contains("override") || lowerDesc.Contains("inherit"))
            {
                businessLogic.Add(new { rule = "inheritance", description = "Override and inheritance logic", confidence = 0.88, testScenarios = new[] { "override behavior", "inheritance rules" } });
            }
        }
        
        return businessLogic;
    }

    /// <summary>
    /// Calculates complexity score based on story data and analysis
    /// </summary>
    /// <param name="story">The Jira story data</param>
    /// <param name="uiComponents">UI components analysis</param>
    /// <param name="businessLogic">Business logic analysis</param>
    /// <returns>Complexity score (1-10)</returns>
    private static double CalculateComplexityScore(JiraStory story, object uiComponents, object businessLogic)
    {
        double score = 3.0; // Base score
        
        // Story points influence
        if (int.TryParse(story.StoryPoints, out int points))
        {
            score += points * 0.5;
        }
        
        // Description complexity
        if (!string.IsNullOrEmpty(story.Description))
        {
            score += Math.Min(story.Description.Length / 500.0, 2.0);
        }
        
        // Acceptance criteria count
        score += story.AcceptanceCriteria.Count * 0.3;
        
        // Cap at 10
        return Math.Min(score, 10.0);
    }

    /// <summary>
    /// Identifies integration points in the description
    /// </summary>
    /// <param name="description">The description text to analyze</param>
    /// <returns>Integration points analysis</returns>
    private static string[] IdentifyIntegrationPoints(string description)
    {
        var integrationPoints = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(description))
        {
            var lowerDesc = description.ToLower();
            
            if (lowerDesc.Contains("api") || lowerDesc.Contains("service"))
                integrationPoints.Add("External API integration");
            
            if (lowerDesc.Contains("database") || lowerDesc.Contains("db"))
                integrationPoints.Add("Database integration");
            
            if (lowerDesc.Contains("auth") || lowerDesc.Contains("login"))
                integrationPoints.Add("Authentication service");
        }
        
        return integrationPoints.ToArray();
    }

    /// <summary>
    /// Assesses testing complexity based on issue type and description
    /// </summary>
    /// <param name="issueType">The issue type</param>
    /// <param name="description">The description text</param>
    /// <returns>Testing complexity assessment</returns>
    private static string AssessTestingComplexity(string issueType, string description)
    {
        var complexity = issueType?.ToLower() switch
        {
            "epic" => "High - requires comprehensive testing across multiple features",
            "story" => "Medium - focused feature testing with acceptance criteria validation",
            "bug" => "Low-Medium - regression testing with specific scenario focus",
            "task" => "Low - task-specific validation",
            _ => "Medium - standard functional testing"
        };
        
        return complexity;
    }

    /// <summary>
    /// Generates functional test areas based on story content
    /// </summary>
    /// <param name="story">The Jira story data</param>
    /// <returns>Functional test areas</returns>
    private static string[] GenerateFunctionalTestAreas(JiraStory story)
    {
        var testAreas = new List<string> { "Basic functionality", "Error handling" };
        
        if (story.AcceptanceCriteria.Any())
            testAreas.Add("Acceptance criteria validation");
        
        if (!string.IsNullOrEmpty(story.Description))
            testAreas.Add("Requirements verification");
        
        return testAreas.ToArray();
    }

    /// <summary>
    /// Extracts UI test areas from description
    /// </summary>
    /// <param name="description">The description text</param>
    /// <returns>UI test areas</returns>
    private static string[] ExtractUITestAreas(string description)
    {
        var testAreas = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(description))
        {
            var lowerDesc = description.ToLower();
            
            if (lowerDesc.Contains("form") || lowerDesc.Contains("input"))
                testAreas.Add("Form validation");
            
            if (lowerDesc.Contains("button") || lowerDesc.Contains("click"))
                testAreas.Add("Button interactions");
            
            if (lowerDesc.Contains("modal") || lowerDesc.Contains("dialog"))
                testAreas.Add("Modal behavior");
        }
        
        return testAreas.ToArray();
    }

    /// <summary>
    /// Extracts integration test areas from description
    /// </summary>
    /// <param name="description">The description text</param>
    /// <returns>Integration test areas</returns>
    private static string[] ExtractIntegrationTestAreas(string description)
    {
        var testAreas = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(description))
        {
            var lowerDesc = description.ToLower();
            
            if (lowerDesc.Contains("api"))
                testAreas.Add("API integration");
            
            if (lowerDesc.Contains("database"))
                testAreas.Add("Database integration");
            
            if (lowerDesc.Contains("service"))
                testAreas.Add("Service integration");
        }
        
        return testAreas.ToArray();
    }

    /// <summary>
    /// Identifies security test areas from description
    /// </summary>
    /// <param name="description">The description text</param>
    /// <returns>Security test areas</returns>
    private static string[] IdentifySecurityTestAreas(string description)
    {
        var testAreas = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(description))
        {
            var lowerDesc = description.ToLower();
            
            if (lowerDesc.Contains("auth") || lowerDesc.Contains("login"))
                testAreas.Add("Authentication security");
            
            if (lowerDesc.Contains("permission") || lowerDesc.Contains("access"))
                testAreas.Add("Authorization testing");
            
            if (lowerDesc.Contains("data") || lowerDesc.Contains("input"))
                testAreas.Add("Input validation security");
        }
        
        return testAreas.ToArray();
    }

    /// <summary>
    /// Identifies performance test areas from description
    /// </summary>
    /// <param name="description">The description text</param>
    /// <returns>Performance test areas</returns>
    private static string[] IdentifyPerformanceTestAreas(string description)
    {
        var testAreas = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(description))
        {
            var lowerDesc = description.ToLower();
            
            if (lowerDesc.Contains("load") || lowerDesc.Contains("performance"))
                testAreas.Add("Load testing");
            
            if (lowerDesc.Contains("response") || lowerDesc.Contains("speed"))
                testAreas.Add("Response time testing");
            
            if (lowerDesc.Contains("concurrent") || lowerDesc.Contains("multiple"))
                testAreas.Add("Concurrency testing");
        }
        
        return testAreas.ToArray();
    }

    /// <summary>
    /// Generates baseline test cases from story data
    /// </summary>
    /// <param name="story">The Jira story data</param>
    /// <returns>Baseline test cases</returns>
    private static object[] GenerateBaselineTestCases(JiraStory story)
    {
        var testCases = new List<object>
        {
            new { type = "happy_path", title = "Basic functionality test", confidence = 0.95 },
            new { type = "error_handling", title = "Error scenario test", confidence = 0.87 }
        };
        
        if (story.AcceptanceCriteria.Any())
        {
            testCases.Add(new { type = "acceptance_criteria", title = "Acceptance criteria validation", confidence = 0.92 });
        }
        
        return testCases.ToArray();
    }

    /// <summary>
    /// Determines focus areas based on story and complexity
    /// </summary>
    /// <param name="story">The Jira story data</param>
    /// <param name="complexityScore">The complexity score</param>
    /// <returns>Focus areas</returns>
    private static string[] DetermineFocusAreas(JiraStory story, double complexityScore)
    {
        var focusAreas = new List<string> { "Functional testing" };
        
        if (complexityScore >= 7.0)
            focusAreas.Add("Integration testing");
        
        if (story.AcceptanceCriteria.Any())
            focusAreas.Add("Acceptance criteria validation");
        
        if (!string.IsNullOrEmpty(story.Description))
            focusAreas.Add("Requirements verification");
        
        return focusAreas.ToArray();
    }

    /// <summary>
    /// Generates LLM prompts based on story data
    /// </summary>
    /// <param name="story">The Jira story data</param>
    /// <returns>LLM prompts</returns>
    private static string[] GenerateLLMPrompts(JiraStory story)
    {
        var prompts = new List<string>
        {
            "Generate comprehensive test scenarios covering positive and negative cases",
            "Create detailed test steps with expected results"
        };
        
        if (story.AcceptanceCriteria.Any())
            prompts.Add("Ensure all acceptance criteria are thoroughly tested");
            
        if (!string.IsNullOrEmpty(story.Description))
            prompts.Add("Extract implicit requirements from the description for additional test coverage");
            
        return prompts.ToArray();
    }

    /// <summary>
    /// Determines testing strategy based on issue type and complexity
    /// </summary>
    /// <param name="issueType">The issue type</param>
    /// <param name="complexityScore">The complexity score</param>
    /// <returns>Testing strategy</returns>
    private static string DetermineTestingStrategy(string issueType, double complexityScore)
    {
        var strategy = issueType?.ToLower() switch
        {
            "epic" => "Comprehensive testing with multiple test phases",
            "story" => "Feature-focused testing with user scenario validation",
            "bug" => "Regression testing with root cause analysis",
            "task" => "Task-specific validation with minimal regression",
            _ => "Standard functional testing approach"
        };

        if (complexityScore >= 7.0)
            strategy += " with emphasis on integration and edge case testing";
            
        return strategy;
    }

    /// <summary>
    /// Calculates analysis confidence based on story completeness and XML quality
    /// </summary>
    /// <param name="story">The Jira story data</param>
    /// <param name="xmlWasValid">Whether the XML was valid</param>
    /// <returns>Analysis confidence score</returns>
    private static double CalculateAnalysisConfidence(JiraStory story, bool xmlWasValid)
    {
        double confidence = 0.5; // Base confidence
        
        // XML quality
        confidence += xmlWasValid ? 0.2 : 0.1;
        
        // Story completeness
        if (!string.IsNullOrEmpty(story.Summary))
            confidence += 0.1;
            
        if (!string.IsNullOrEmpty(story.Description))
            confidence += 0.1;
            
        if (story.AcceptanceCriteria.Any())
            confidence += 0.1;
            
        return Math.Round(confidence, 2);
    }

    /// <summary>
    /// Analyzes comments for additional context and insights
    /// </summary>
    /// <param name="comments">List of Jira comments</param>
    /// <returns>Comment analysis object</returns>
    private static object AnalyzeComments(List<JiraComment> comments)
    {
        if (!comments.Any()) return new { hasComments = false, summary = "No comments found" };
        
        var recentComments = comments.Where(c => c.Created > DateTime.Now.AddDays(-30)).ToList();
        var uniqueAuthors = comments.Select(c => c.Author).Distinct().ToList();
        
        // Extract key insights from comments
        var keyInsights = ExtractCommentInsights(comments);
        var stakeholderConcerns = ExtractStakeholderConcerns(comments);
        var clarifications = ExtractClarifications(comments);
        
        return new
        {
            hasComments = true,
            totalComments = comments.Count,
            recentComments = recentComments.Count,
            uniqueAuthors = uniqueAuthors.Count,
            dateRange = new
            {
                earliest = comments.Min(c => c.Created),
                latest = comments.Max(c => c.Created)
            },
            keyInsights = keyInsights,
            stakeholderConcerns = stakeholderConcerns,
            clarifications = clarifications,
            recentActivity = recentComments.Take(5).Select(c => new
            {
                author = c.Author,
                date = c.Created,
                content = c.CleanContent.Length > 100 ? c.CleanContent.Substring(0, 100) + "..." : c.CleanContent
            }).ToArray()
        };
    }

    /// <summary>
    /// Extracts key insights from comment text
    /// </summary>
    /// <param name="comments">List of comments to analyze</param>
    /// <returns>Array of key insights</returns>
    private static string[] ExtractCommentInsights(List<JiraComment> comments)
    {
        var insights = new List<string>();
        var allText = string.Join(" ", comments.Select(c => c.CleanContent.ToLower()));
        
        // Look for common patterns in comments
        if (allText.Contains("requirement") || allText.Contains("spec"))
            insights.Add("Requirements clarification discussed");
        
        if (allText.Contains("ui") || allText.Contains("interface") || allText.Contains("design"))
            insights.Add("UI/UX considerations mentioned");
        
        if (allText.Contains("performance") || allText.Contains("speed") || allText.Contains("slow"))
            insights.Add("Performance concerns raised");
        
        if (allText.Contains("security") || allText.Contains("auth") || allText.Contains("permission"))
            insights.Add("Security aspects discussed");
        
        if (allText.Contains("integration") || allText.Contains("api") || allText.Contains("service"))
            insights.Add("Integration points identified");
        
        if (allText.Contains("test") || allText.Contains("qa") || allText.Contains("verify"))
            insights.Add("Testing considerations mentioned");
        
        return insights.ToArray();
    }

    /// <summary>
    /// Extracts stakeholder concerns from comments
    /// </summary>
    /// <param name="comments">List of comments to analyze</param>
    /// <returns>Array of stakeholder concerns</returns>
    private static string[] ExtractStakeholderConcerns(List<JiraComment> comments)
    {
        var concerns = new List<string>();
        
        foreach (var comment in comments)
        {
            var content = comment.CleanContent.ToLower();
            
            if (content.Contains("concern") || content.Contains("worry") || content.Contains("issue"))
                concerns.Add($"{comment.Author}: {comment.CleanContent.Substring(0, Math.Min(100, comment.CleanContent.Length))}...");
            
            if (content.Contains("should") || content.Contains("must") || content.Contains("need"))
                concerns.Add($"{comment.Author}: {comment.CleanContent.Substring(0, Math.Min(100, comment.CleanContent.Length))}...");
        }
        
        return concerns.Take(5).ToArray();
    }

    /// <summary>
    /// Extracts clarifications and decisions from comments
    /// </summary>
    /// <param name="comments">List of comments to analyze</param>
    /// <returns>Array of clarifications</returns>
    private static string[] ExtractClarifications(List<JiraComment> comments)
    {
        var clarifications = new List<string>();
        
        foreach (var comment in comments)
        {
            var content = comment.CleanContent.ToLower();
            
            if (content.Contains("clarification") || content.Contains("decision") || content.Contains("agreed"))
                clarifications.Add($"{comment.Author}: {comment.CleanContent.Substring(0, Math.Min(100, comment.CleanContent.Length))}...");
            
            if (content.Contains("let's") || content.Contains("we should") || content.Contains("decided"))
                clarifications.Add($"{comment.Author}: {comment.CleanContent.Substring(0, Math.Min(100, comment.CleanContent.Length))}...");
        }
        
        return clarifications.Take(5).ToArray();
    }
}
