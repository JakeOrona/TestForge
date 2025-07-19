using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using HtmlAgilityPack;
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
                executiveSummary = CalculateExecutiveSummaryConfidence(story, isValid, uiComponentsAnalysis, businessLogicAnalysis, complexityScore),
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
    /// Calculates comprehensive executive summary confidence with weighted factors
    /// </summary>
    /// <param name="story">The Jira story data</param>
    /// <param name="xmlWasValid">Whether the XML was valid</param>
    /// <param name="uiComponents">UI components analysis</param>
    /// <param name="businessLogic">Business logic analysis</param>
    /// <param name="complexityScore">Complexity score</param>
    /// <returns>Executive summary confidence object</returns>
    private static ExecutiveSummaryConfidence CalculateExecutiveSummaryConfidence(
        JiraStory story, 
        bool xmlWasValid, 
        object uiComponents, 
        object businessLogic, 
        double complexityScore)
    {
        // Calculate individual confidence factors with weights
        var breakdown = new ConfidenceBreakdown
        {
            XmlQuality = CalculateXmlQualityConfidence(xmlWasValid, story),
            StoryCompleteness = CalculateStoryCompletenessConfidence(story),
            AnalysisDepth = CalculateAnalysisDepthConfidence(uiComponents, businessLogic),
            DataQuality = CalculateDataQualityConfidence(story),
            IntegrationComplexity = CalculateIntegrationComplexityConfidence(story.Description),
            TestCoverageReadiness = CalculateTestCoverageReadinessConfidence(story, complexityScore)
        };

        // Weighted calculation based on specified percentages
        var overallConfidence = 
            (breakdown.XmlQuality * 0.20) +           // XML Quality (20%)
            (breakdown.StoryCompleteness * 0.25) +    // Story Completeness (25%)
            (breakdown.AnalysisDepth * 0.20) +        // Analysis Depth (20%)
            (breakdown.DataQuality * 0.15) +          // Data Quality (15%)
            (breakdown.IntegrationComplexity * 0.10) + // Integration Complexity (10%)
            (breakdown.TestCoverageReadiness * 0.10);  // Test Coverage Readiness (10%)

        // Ensure confidence is within bounds
        overallConfidence = Math.Max(0.0, Math.Min(1.0, overallConfidence));

        var confidenceLevel = DetermineConfidenceLevel(overallConfidence);
        var factors = AnalyzeConfidenceFactors(breakdown, story);
        var recommendedAction = DetermineRecommendedAction(confidenceLevel, factors);

        return new ExecutiveSummaryConfidence
        {
            OverallConfidence = Math.Round(overallConfidence, 3),
            ConfidenceLevel = confidenceLevel,
            ConfidenceBreakdown = breakdown,
            ConfidenceFactors = factors,
            RecommendedAction = recommendedAction
        };
    }

    /// <summary>
    /// Calculates XML quality confidence factor
    /// </summary>
    private static double CalculateXmlQualityConfidence(bool xmlWasValid, JiraStory story)
    {
        double confidence = xmlWasValid ? 0.8 : 0.4; // Base score for XML validity

        // Boost confidence if key fields were successfully extracted
        if (!string.IsNullOrEmpty(story.IssueKey)) confidence += 0.1;
        if (!string.IsNullOrEmpty(story.Summary)) confidence += 0.05;
        if (!string.IsNullOrEmpty(story.IssueType)) confidence += 0.05;

        return Math.Min(1.0, confidence);
    }

    /// <summary>
    /// Calculates story completeness confidence factor
    /// </summary>
    private static double CalculateStoryCompletenessConfidence(JiraStory story)
    {
        double confidence = 0.0;

        // Core story elements
        if (!string.IsNullOrEmpty(story.Summary)) confidence += 0.25;
        if (!string.IsNullOrEmpty(story.Description)) confidence += 0.35;
        if (story.AcceptanceCriteria.Any()) confidence += 0.30;
        if (!string.IsNullOrEmpty(story.Priority)) confidence += 0.10;

        return Math.Min(1.0, confidence);
    }

    /// <summary>
    /// Calculates analysis depth confidence factor
    /// </summary>
    private static double CalculateAnalysisDepthConfidence(object uiComponents, object businessLogic)
    {
        double confidence = 0.5; // Base confidence

        // UI components analysis depth
        var uiList = uiComponents as IEnumerable<object>;
        if (uiList != null && uiList.Any())
        {
            confidence += Math.Min(0.3, uiList.Count() * 0.1);
        }

        // Business logic analysis depth
        var businessList = businessLogic as IEnumerable<object>;
        if (businessList != null && businessList.Any())
        {
            confidence += Math.Min(0.2, businessList.Count() * 0.05);
        }

        return Math.Min(1.0, confidence);
    }

    /// <summary>
    /// Calculates data quality confidence factor
    /// </summary>
    private static double CalculateDataQualityConfidence(JiraStory story)
    {
        double confidence = 0.3; // Base confidence

        // Story points validity
        if (int.TryParse(story.StoryPoints, out int points) && points > 0)
            confidence += 0.2;

        // Comments presence and quality
        if (story.Comments.Any())
        {
            confidence += 0.15;
            var recentComments = story.Comments.Where(c => c.Created > DateTime.Now.AddDays(-30)).Count();
            if (recentComments > 0) confidence += 0.1;
        }

        // Custom fields
        if (story.CustomFields.Any())
        {
            confidence += Math.Min(0.25, story.CustomFields.Count * 0.05);
        }

        return Math.Min(1.0, confidence);
    }

    /// <summary>
    /// Calculates integration complexity confidence factor
    /// </summary>
    private static double CalculateIntegrationComplexityConfidence(string description)
    {
        if (string.IsNullOrEmpty(description)) return 0.5;

        double confidence = 0.5; // Base confidence
        var lowerDesc = description.ToLower();

        // Integration indicators boost confidence in complexity analysis
        var integrationKeywords = new[] { "api", "service", "integration", "database", "auth", "external", "system" };
        var foundKeywords = integrationKeywords.Count(keyword => lowerDesc.Contains(keyword));
        
        confidence += Math.Min(0.5, foundKeywords * 0.1);

        return Math.Min(1.0, confidence);
    }

    /// <summary>
    /// Calculates test coverage readiness confidence factor
    /// </summary>
    private static double CalculateTestCoverageReadinessConfidence(JiraStory story, double complexityScore)
    {
        double confidence = 0.4; // Base confidence

        // Acceptance criteria provide good test foundation
        if (story.AcceptanceCriteria.Any())
        {
            confidence += 0.3;
            // More criteria = better test coverage potential
            confidence += Math.Min(0.2, story.AcceptanceCriteria.Count * 0.05);
        }

        // Complexity score influences test readiness
        if (complexityScore <= 3.0) confidence += 0.1; // Simple stories easier to test
        else if (complexityScore >= 7.0) confidence += 0.05; // Complex stories need more planning

        // Description quality affects test case generation
        if (!string.IsNullOrEmpty(story.Description) && story.Description.Length > 100)
            confidence += 0.1;

        return Math.Min(1.0, confidence);
    }

    /// <summary>
    /// Determines confidence level based on overall score
    /// </summary>
    private static ConfidenceLevel DetermineConfidenceLevel(double overallConfidence)
    {
        return overallConfidence switch
        {
            >= 0.90 => ConfidenceLevel.High,
            >= 0.75 => ConfidenceLevel.MediumHigh,
            >= 0.60 => ConfidenceLevel.Medium,
            >= 0.45 => ConfidenceLevel.LowMedium,
            _ => ConfidenceLevel.Low
        };
    }

    /// <summary>
    /// Analyzes confidence factors to identify strengths and weaknesses
    /// </summary>
    private static ConfidenceFactors AnalyzeConfidenceFactors(ConfidenceBreakdown breakdown, JiraStory story)
    {
        var strengths = new List<string>();
        var weaknesses = new List<string>();
        var improvementAreas = new List<string>();

        // Analyze each factor
        if (breakdown.XmlQuality >= 0.8) strengths.Add("Valid XML structure with successful parsing");
        else weaknesses.Add("XML quality issues affecting data extraction");

        if (breakdown.StoryCompleteness >= 0.8) strengths.Add("Comprehensive story with all key elements");
        else if (breakdown.StoryCompleteness < 0.5) weaknesses.Add("Incomplete story missing essential details");

        if (breakdown.AnalysisDepth >= 0.7) strengths.Add("Deep component and business logic analysis");
        else improvementAreas.Add("Enhanced component analysis needed");

        if (breakdown.DataQuality >= 0.7) strengths.Add("Rich data with comments and custom fields");
        else improvementAreas.Add("Additional metadata would improve analysis");

        if (breakdown.TestCoverageReadiness >= 0.8) strengths.Add("Strong foundation for test case generation");
        else improvementAreas.Add("More detailed acceptance criteria needed");

        // Ensure we have at least one item in each category
        if (!strengths.Any()) strengths.Add("Basic story structure present");
        if (!weaknesses.Any() && !improvementAreas.Any()) improvementAreas.Add("Minor refinements possible");

        return new ConfidenceFactors
        {
            Strengths = strengths,
            Weaknesses = weaknesses,
            ImprovementAreas = improvementAreas
        };
    }

    /// <summary>
    /// Determines recommended action based on confidence level
    /// </summary>
    private static string DetermineRecommendedAction(ConfidenceLevel level, ConfidenceFactors factors)
    {
        return level switch
        {
            ConfidenceLevel.High => "Proceed with automated test generation - excellent foundation available",
            ConfidenceLevel.MediumHigh => "Proceed with LLM enhancement - good foundation with minor gaps",
            ConfidenceLevel.Medium => "Proceed with LLM enhancement - adequate foundation requiring enhancement",
            ConfidenceLevel.LowMedium => "Consider story refinement before test generation - significant enhancement needed",
            ConfidenceLevel.Low => "Story requires significant improvement before reliable test generation",
            _ => "Review and enhance story details before proceeding"
        };
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

    /// <summary>
    /// Enhanced method to extract structured content from HTML description fields using HtmlAgilityPack
    /// </summary>
    /// <param name="descriptionElement">The XElement containing the description</param>
    /// <returns>TechnicalArtifacts containing structured data</returns>
    public static TechnicalArtifacts ExtractStructuredContent(XElement descriptionElement)
    {
        var artifacts = new TechnicalArtifacts();
        
        if (descriptionElement == null)
            return artifacts;

        var description = descriptionElement.Value ?? string.Empty;
        
        // Parse HTML content using HtmlAgilityPack
        var doc = new HtmlDocument();
        doc.LoadHtml(description);

        // Extract from specific HTML elements
        ExtractFromListElements(doc, artifacts);
        ExtractFromCodeElements(doc, artifacts);
        ExtractFromPreElements(doc, artifacts);
        ExtractFromTechnicalTerms(doc, artifacts);
        ExtractSqlSnippets(description, artifacts);
        ExtractFormatConstraints(description, artifacts);
        ExtractConfigurationHints(description, artifacts);

        return artifacts;
    }

    /// <summary>
    /// Extract structured data from HTML list elements
    /// </summary>
    private static void ExtractFromListElements(HtmlDocument doc, TechnicalArtifacts artifacts)
    {
        var listItems = doc.DocumentNode.SelectNodes("//ul/li | //ol/li");
        if (listItems != null)
        {
            foreach (var li in listItems)
            {
                var text = li.InnerText?.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    // Classify list items based on content
                    if (IsCodeExample(text))
                        artifacts.CodeExamples.Add(text);
                    else if (IsFormatConstraint(text))
                        artifacts.FormatConstraints.Add(text);
                    else if (IsTechnicalTerminology(text))
                        artifacts.TechnicalTerminology.Add(text);
                    else
                        artifacts.SampleData.Add(text);
                }
            }
        }
    }

    /// <summary>
    /// Extract code examples from code and tt elements
    /// </summary>
    private static void ExtractFromCodeElements(HtmlDocument doc, TechnicalArtifacts artifacts)
    {
        var codeElements = doc.DocumentNode.SelectNodes("//code | //tt");
        if (codeElements != null)
        {
            foreach (var code in codeElements)
            {
                var text = code.InnerText?.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    artifacts.CodeExamples.Add(text);
                    
                    // Also check if it's a technical term
                    if (IsTechnicalTerminology(text))
                        artifacts.TechnicalTerminology.Add(text);
                }
            }
        }
    }

    /// <summary>
    /// Extract from pre-formatted elements
    /// </summary>
    private static void ExtractFromPreElements(HtmlDocument doc, TechnicalArtifacts artifacts)
    {
        var preElements = doc.DocumentNode.SelectNodes("//pre");
        if (preElements != null)
        {
            foreach (var pre in preElements)
            {
                var text = pre.InnerText?.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    // Check if it's JSON, XML, or SQL
                    if (IsJsonStructure(text))
                        artifacts.SampleData.Add($"JSON: {text}");
                    else if (IsXmlStructure(text))
                        artifacts.SampleData.Add($"XML: {text}");
                    else if (IsSqlSnippet(text))
                        artifacts.SqlSnippets.Add(text);
                    else
                        artifacts.CodeExamples.Add(text);
                }
            }
        }
    }

    /// <summary>
    /// Extract technical terminology from all text content
    /// </summary>
    private static void ExtractFromTechnicalTerms(HtmlDocument doc, TechnicalArtifacts artifacts)
    {
        var text = doc.DocumentNode.InnerText ?? string.Empty;
        
        // Technical patterns to look for
        var technicalPatterns = new[]
        {
            @"\b[A-Z]{2,}\b", // Acronyms like SSML, IPA, TTS
            @"\bphoneme\b", @"\bSSML\b", @"\bTTS\b", @"\bIVR\b",
            @"\bJSON\b", @"\bXML\b", @"\bSQL\b", @"\bHTTP\b", @"\bAPI\b",
            @"\bUnicode\b", @"\bUTF-8\b", @"\bIPA\b",
            @"\bəɹˈdu\b", // IPA phonetic notation example
            @"\b\w+\s*variants?\b", // Language variants
            @"\b\w+\s*flow\b", // IVR flows
            @"\bno.?pin\b", @"\bathena\b", @"\bavaility\b" // Specific IVR types
        };

        foreach (var pattern in technicalPatterns)
        {
            var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var term = match.Value.Trim();
                if (!artifacts.TechnicalTerminology.Contains(term) && term.Length > 2)
                    artifacts.TechnicalTerminology.Add(term);
            }
        }
    }

    /// <summary>
    /// Extract SQL snippets from description
    /// </summary>
    private static void ExtractSqlSnippets(string description, TechnicalArtifacts artifacts)
    {
        var sqlPatterns = new[]
        {
            @"UPDATE\s+\w+\s+SET.*?(?:WHERE.*?)?(?:;|$)",
            @"INSERT\s+INTO\s+\w+.*?(?:;|$)",
            @"SELECT\s+.*?FROM\s+\w+.*?(?:;|$)",
            @"DELETE\s+FROM\s+\w+.*?(?:;|$)"
        };

        foreach (var pattern in sqlPatterns)
        {
            var matches = Regex.Matches(description, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                artifacts.SqlSnippets.Add(match.Value.Trim());
            }
        }
    }

    /// <summary>
    /// Extract format constraints and validation rules
    /// </summary>
    private static void ExtractFormatConstraints(string description, TechnicalArtifacts artifacts)
    {
        var constraintPatterns = new[]
        {
            @"IPA\s+phonemes?",
            @"Unicode\s+compliance",
            @"format\s+constraint",
            @"validation\s+rule",
            @"must\s+be\s+\w+",
            @"should\s+follow\s+\w+",
            @"required\s+format",
            @"length\s+\w+\s+\d+",
            @"character\s+set"
        };

        foreach (var pattern in constraintPatterns)
        {
            var matches = Regex.Matches(description, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                artifacts.FormatConstraints.Add(match.Value.Trim());
            }
        }
    }

    /// <summary>
    /// Extract configuration hints from description
    /// </summary>
    private static void ExtractConfigurationHints(string description, TechnicalArtifacts artifacts)
    {
        var configPatterns = new[]
        {
            @"IVR\s+variants?",
            @"language\s+codes?",
            @"configuration\s+\w+",
            @"setting\s+\w+",
            @"parameter\s+\w+",
            @"option\s+\w+",
            @"fallback\s+\w+",
            @"default\s+\w+"
        };

        foreach (var pattern in configPatterns)
        {
            var matches = Regex.Matches(description, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                artifacts.ConfigurationHints.Add(match.Value.Trim());
            }
        }
    }

    /// <summary>
    /// Helper methods for content classification
    /// </summary>
    private static bool IsCodeExample(string text)
    {
        return text.Contains("{") || text.Contains("<") || text.Contains("=") || 
               text.Contains("function") || text.Contains("class") || text.Contains("var ");
    }

    private static bool IsFormatConstraint(string text)
    {
        return text.ToLower().Contains("format") || text.ToLower().Contains("constraint") ||
               text.ToLower().Contains("validation") || text.ToLower().Contains("rule");
    }

    private static bool IsTechnicalTerminology(string text)
    {
        var techKeywords = new[] { "API", "JSON", "XML", "SQL", "HTTP", "TTS", "IVR", "SSML", "IPA", "phoneme" };
        return techKeywords.Any(keyword => text.ToUpper().Contains(keyword));
    }

    private static bool IsJsonStructure(string text)
    {
        return text.Trim().StartsWith("{") && text.Trim().EndsWith("}");
    }

    private static bool IsXmlStructure(string text)
    {
        return text.Trim().StartsWith("<") && text.Trim().EndsWith(">");
    }

    private static bool IsSqlSnippet(string text)
    {
        var sqlKeywords = new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "CREATE", "ALTER" };
        return sqlKeywords.Any(keyword => text.ToUpper().Contains(keyword));
    }
}
