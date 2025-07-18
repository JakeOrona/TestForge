using System.Text.Json;
using TestForge.McpServer.Models;

namespace TestForge.McpServer.Services;

/// <summary>
/// Service for orchestrating complete Jira XML workflow processing
/// </summary>
public static class JiraWorkflowService
{
    /// <summary>
    /// Processes complete Jira XML workflow from validation to TestRail-ready test cases
    /// </summary>
    /// <param name="jiraXml">The raw Jira XML content to process</param>
    /// <returns>Complete workflow results with all analysis steps and TestRail-ready test cases</returns>
    public static string ProcessComplete(string jiraXml)
    {
        try
        {
            var workflowResult = new WorkflowResult();

            // Step 1: Validate XML
            var validationStep = ProcessValidationStep(jiraXml);
            workflowResult.Steps.Add("validation", validationStep);

            // Step 2: Clean XML if validation failed
            var processedXml = ProcessCleaningStep(jiraXml, validationStep, workflowResult);

            // Step 3: Primary analysis
            var analysisStep = ProcessAnalysisStep(processedXml);
            workflowResult.Steps.Add("analysis", analysisStep);

            // Step 4: Extract metadata from analysis for subsequent steps
            var metadata = ExtractMetadata(analysisStep.Result);

            // Step 5-8: Process remaining steps
            ProcessRemainingSteps(workflowResult, metadata);

            // Create workflow summary
            workflowResult.Summary = CreateWorkflowSummary(workflowResult.Steps, metadata);

            return JsonSerializer.Serialize(workflowResult, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return HandleWorkflowError(ex);
        }
    }

    /// <summary>
    /// Processes XML validation step
    /// </summary>
    /// <param name="jiraXml">The raw Jira XML content to validate</param>
    /// <returns>Validation step result</returns>
    private static WorkflowStep ProcessValidationStep(string jiraXml)
    {
        var validationResult = JiraXmlValidationService.Validate(jiraXml);
        return new WorkflowStep
        {
            Result = validationResult,
            Status = validationResult.Contains("error") ? "failed" : "passed",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Processes XML cleaning step if validation failed
    /// </summary>
    /// <param name="originalXml">The original XML content</param>
    /// <param name="validationStep">The validation step result</param>
    /// <param name="workflowResult">The workflow result to update</param>
    /// <returns>Processed XML (cleaned if needed, original if validation passed)</returns>
    private static string ProcessCleaningStep(string originalXml, WorkflowStep validationStep, WorkflowResult workflowResult)
    {
        if (validationStep.Status == "failed")
        {
            var cleaningResult = JiraXmlCleaningService.Clean(originalXml);
            var cleaningStep = new WorkflowStep
            {
                Result = cleaningResult,
                Status = "completed",
                Timestamp = DateTime.UtcNow
            };
            workflowResult.Steps.Add("cleaning", cleaningStep);

            // Extract cleaned XML from the cleaning result
            try
            {
                var cleaningJson = JsonSerializer.Deserialize<JsonElement>(cleaningResult);
                if (cleaningJson.TryGetProperty("cleanedXml", out var cleanedXmlElement))
                {
                    return cleanedXmlElement.GetString() ?? originalXml;
                }
            }
            catch
            {
                // If extraction fails, use original XML
            }
        }

        return originalXml;
    }

    /// <summary>
    /// Processes primary analysis step
    /// </summary>
    /// <param name="processedXml">The processed XML content</param>
    /// <returns>Analysis step result</returns>
    private static WorkflowStep ProcessAnalysisStep(string processedXml)
    {
        var analysisResult = JiraXmlAnalysisService.AnalyzeForLLM(processedXml);
        return new WorkflowStep
        {
            Result = analysisResult,
            Status = "completed",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Processes remaining workflow steps (templates, UI analysis, business logic, test cases)
    /// </summary>
    /// <param name="workflowResult">The workflow result to update</param>
    /// <param name="metadata">Extracted metadata for processing</param>
    private static void ProcessRemainingSteps(WorkflowResult workflowResult, WorkflowMetadata metadata)
    {
        // Step 5: Generate test case templates
        var templatesResult = TestCaseTemplateService.GenerateTemplates(metadata.TicketType, metadata.Priority, metadata.Component);
        workflowResult.Steps.Add("templates", new WorkflowStep
        {
            Result = templatesResult,
            Status = "completed",
            Timestamp = DateTime.UtcNow
        });

        // Step 6: UI component analysis
        var uiAnalysisResult = UiComponentAnalysisService.ExtractComponents(metadata.Description);
        workflowResult.Steps.Add("ui_analysis", new WorkflowStep
        {
            Result = uiAnalysisResult,
            Status = "completed",
            Timestamp = DateTime.UtcNow
        });

        // Step 7: Business logic analysis
        var businessAnalysisResult = BusinessLogicAnalysisService.ExtractLogic(metadata.Description);
        workflowResult.Steps.Add("business_analysis", new WorkflowStep
        {
            Result = businessAnalysisResult,
            Status = "completed",
            Timestamp = DateTime.UtcNow
        });

        // Step 8: Generate final TestRail test cases
        var testCasesResult = TestRailGenerationService.GenerateFromXml(workflowResult.Steps["analysis"].Result);
        workflowResult.Steps.Add("test_cases", new WorkflowStep
        {
            Result = testCasesResult,
            Status = "completed",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Extracts metadata from analysis results for subsequent workflow steps
    /// </summary>
    /// <param name="analysisResult">JSON analysis result from JiraXmlAnalysisService</param>
    /// <returns>Extracted metadata for workflow processing</returns>
    private static WorkflowMetadata ExtractMetadata(string analysisResult)
    {
        var metadata = new WorkflowMetadata();

        try
        {
            var analysisJson = JsonSerializer.Deserialize<JsonElement>(analysisResult);
            if (analysisJson.TryGetProperty("ticketInfo", out var ticketInfo))
            {
                if (ticketInfo.TryGetProperty("type", out var typeElement))
                    metadata.TicketType = typeElement.GetString() ?? "Story";
                if (ticketInfo.TryGetProperty("priority", out var priorityElement))
                    metadata.Priority = priorityElement.GetString() ?? "Medium";
                if (ticketInfo.TryGetProperty("description", out var descElement))
                    metadata.Description = descElement.GetString() ?? "";
                if (ticketInfo.TryGetProperty("key", out var keyElement))
                    metadata.TicketKey = keyElement.GetString() ?? "";
                if (ticketInfo.TryGetProperty("summary", out var summaryElement))
                    metadata.Summary = summaryElement.GetString() ?? "";
            }
        }
        catch
        {
            // Use defaults if extraction fails
        }

        return metadata;
    }

    /// <summary>
    /// Creates workflow summary with processing statistics
    /// </summary>
    /// <param name="steps">Dictionary of workflow steps and their results</param>
    /// <param name="metadata">Extracted workflow metadata</param>
    /// <returns>Workflow summary object</returns>
    private static WorkflowSummary CreateWorkflowSummary(Dictionary<string, WorkflowStep> steps, WorkflowMetadata metadata)
    {
        var successfulSteps = steps.Count(kvp => kvp.Value.Status != "failed");
        
        return new WorkflowSummary
        {
            TotalSteps = steps.Count,
            SuccessfulSteps = successfulSteps,
            XmlCleaned = steps.ContainsKey("cleaning"),
            TicketType = metadata.TicketType,
            Priority = metadata.Priority,
            Component = metadata.Component,
            ProcessingTime = DateTime.UtcNow,
            Recommendation = successfulSteps == steps.Count 
                ? "All analysis steps completed successfully. Test cases are ready for TestRail import."
                : $"Workflow completed with {steps.Count - successfulSteps} failed steps. Review individual step results for details."
        };
    }

    /// <summary>
    /// Handles workflow errors and returns structured error response
    /// </summary>
    /// <param name="ex">The exception that occurred</param>
    /// <returns>Structured error response as JSON</returns>
    private static string HandleWorkflowError(Exception ex)
    {
        var errorResponse = new
        {
            error = $"Workflow processing failed: {ex.Message}",
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            recommendation = "Use individual tools for more detailed error analysis.",
            details = ex.StackTrace
        };

        return JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions { WriteIndented = true });
    }
}
