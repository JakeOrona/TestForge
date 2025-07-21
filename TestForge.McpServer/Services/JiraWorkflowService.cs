using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TestForge.McpServer.Models;

namespace TestForge.McpServer.Services;

/// <summary>
/// Service for orchestrating complete Jira XML workflow processing
/// </summary>
public static class JiraWorkflowService
{
    // Logger for observability and debugging
    private static ILogger? _logger;

    /// <summary>
    /// Initialize the service with logger for structured logging
    /// </summary>
    /// <param name="logger">Logger instance for structured logging</param>
    public static void Initialize(ILogger? logger) => _logger = logger;
    /// <summary>
    /// Processes complete Jira XML workflow from validation to TestRail-ready test cases
    /// </summary>
    /// <param name="jiraXml">The raw Jira XML content to process</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <returns>Complete workflow results with all analysis steps and TestRail-ready test cases</returns>
    public static string ProcessComplete(string jiraXml, ILogger? logger = null)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..8];
        var workflowStopwatch = Stopwatch.StartNew();
        var operationLogger = logger ?? _logger;
        
        operationLogger?.LogInformation("Starting complete Jira workflow processing {@Metrics}", new { 
            correlationId, 
            xmlSize = jiraXml?.Length ?? 0, 
            operation = "ProcessComplete" 
        });

        try
        {
            // Security monitoring for large XML inputs
            if (jiraXml?.Length > 1_048_576) // 1MB
            {
                operationLogger?.LogWarning("Large XML input detected {@SecurityContext}", new {
                    correlationId,
                    xmlSize = jiraXml.Length,
                    securityRisk = "potential_memory_pressure",
                    recommendation = "consider_input_size_validation"
                });
            }

            using (operationLogger?.BeginScope(new Dictionary<string, object> 
            { 
                ["CorrelationId"] = correlationId,
                ["Operation"] = "JiraWorkflowProcessing",
                ["XmlSize"] = jiraXml?.Length ?? 0
            }))
            {
                var workflowResult = new WorkflowResult();
                var stepTimings = new Dictionary<string, double>();
                var totalSteps = 8; // Total planned workflow steps
                var completedSteps = 0;

                // Step 1: Validate XML
                operationLogger?.LogInformation("Starting workflow step {@StepInfo}", new { 
                    correlationId, 
                    stepName = "validation", 
                    stepNumber = 1, 
                    totalSteps,
                    workflowProgress = $"{completedSteps}/{totalSteps}"
                });

                var validationStopwatch = Stopwatch.StartNew();
                var validationStep = ProcessValidationStep(jiraXml, operationLogger, correlationId);
                validationStopwatch.Stop();
                stepTimings["validation"] = validationStopwatch.TotalMilliseconds;
                workflowResult.Steps.Add("validation", validationStep);
                completedSteps++;

                operationLogger?.LogInformation("Workflow step completed {@Metrics}", new { 
                    success = true, 
                    stepName = "validation",
                    processingTimeMs = validationStopwatch.TotalMilliseconds,
                    correlationId,
                    stepStatus = validationStep.Status,
                    workflowProgress = $"{completedSteps}/{totalSteps}"
                });

                // Step 2: Clean XML if validation failed
                var cleaningStopwatch = Stopwatch.StartNew();
                var processedXml = ProcessCleaningStep(jiraXml, validationStep, workflowResult, operationLogger, correlationId);
                cleaningStopwatch.Stop();
                stepTimings["cleaning"] = cleaningStopwatch.TotalMilliseconds;
                
                if (workflowResult.Steps.ContainsKey("cleaning"))
                {
                    completedSteps++;
                    operationLogger?.LogInformation("Workflow step completed {@Metrics}", new { 
                        success = true, 
                        stepName = "cleaning",
                        processingTimeMs = cleaningStopwatch.TotalMilliseconds,
                        correlationId,
                        stepStatus = workflowResult.Steps["cleaning"].Status,
                        workflowProgress = $"{completedSteps}/{totalSteps}",
                        xmlCleaned = true
                    });
                }

                // Step 3: Primary analysis
                operationLogger?.LogInformation("Starting workflow step {@StepInfo}", new { 
                    correlationId, 
                    stepName = "analysis", 
                    stepNumber = 3, 
                    totalSteps,
                    workflowProgress = $"{completedSteps}/{totalSteps}"
                });

                var analysisStopwatch = Stopwatch.StartNew();
                var analysisStep = ProcessAnalysisStep(processedXml, operationLogger, correlationId);
                analysisStopwatch.Stop();
                stepTimings["analysis"] = analysisStopwatch.TotalMilliseconds;
                workflowResult.Steps.Add("analysis", analysisStep);
                completedSteps++;

                operationLogger?.LogInformation("Workflow step completed {@Metrics}", new { 
                    success = true, 
                    stepName = "analysis",
                    processingTimeMs = analysisStopwatch.TotalMilliseconds,
                    correlationId,
                    stepStatus = analysisStep.Status,
                    workflowProgress = $"{completedSteps}/{totalSteps}"
                });

                // Step 4: Extract metadata from analysis for subsequent steps
                var metadataStopwatch = Stopwatch.StartNew();
                var metadata = ExtractMetadata(analysisStep.Result, operationLogger, correlationId);
                metadataStopwatch.Stop();
                stepTimings["metadata_extraction"] = metadataStopwatch.TotalMilliseconds;

                // Step 5-8: Process remaining steps (pass cleaned XML for test case generation)
                var remainingStepsStopwatch = Stopwatch.StartNew();
                ProcessRemainingSteps(workflowResult, metadata, processedXml, operationLogger, correlationId, ref completedSteps, totalSteps, stepTimings);
                remainingStepsStopwatch.Stop();

                // Create workflow summary
                var summaryStopwatch = Stopwatch.StartNew();
                workflowResult.Summary = CreateWorkflowSummary(workflowResult.Steps, metadata, operationLogger, correlationId);
                summaryStopwatch.Stop();
                stepTimings["summary_generation"] = summaryStopwatch.TotalMilliseconds;

                workflowStopwatch.Stop();

                // Workflow timeout monitoring
                if (workflowStopwatch.TotalSeconds > 30)
                {
                    operationLogger?.LogWarning("Slow workflow processing detected {@PerformanceContext}", new {
                        correlationId,
                        processingTimeSeconds = workflowStopwatch.TotalSeconds,
                        performanceRisk = "workflow_timeout_risk",
                        recommendation = "consider_workflow_optimization"
                    });
                }

                // Calculate workflow health score
                var successfulSteps = workflowResult.Steps.Count(kvp => kvp.Value.Status != "failed");
                var workflowHealthScore = (double)successfulSteps / workflowResult.Steps.Count * 100;

                operationLogger?.LogInformation("Complete Jira workflow processing completed successfully {@Metrics}", new { 
                    success = true, 
                    correlationId,
                    totalProcessingTimeMs = workflowStopwatch.TotalMilliseconds,
                    totalSteps = workflowResult.Steps.Count,
                    successfulSteps,
                    workflowHealthScore,
                    stepTimings,
                    xmlProcessed = processedXml?.Length ?? 0,
                    workflowEfficiency = workflowStopwatch.TotalMilliseconds / workflowResult.Steps.Count
                });

                return JsonSerializer.Serialize(workflowResult, new JsonSerializerOptions { WriteIndented = true });
            }
        }
        catch (Exception ex)
        {
            workflowStopwatch.Stop();
            
            operationLogger?.LogError(ex, "Complete Jira workflow processing failed {@ErrorContext}", new { 
                correlationId,
                error = ex.Message,
                processingTimeMs = workflowStopwatch.TotalMilliseconds,
                xmlSize = jiraXml?.Length ?? 0,
                recommendation = "check_xml_format_and_service_availability"
            });
            
            return HandleWorkflowError(ex, operationLogger, correlationId);
        }
    }

    /// <summary>
    /// Processes XML validation step
    /// </summary>
    /// <param name="jiraXml">The raw Jira XML content to validate</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>Validation step result</returns>
    private static WorkflowStep ProcessValidationStep(string jiraXml, ILogger? logger = null, string correlationId = "")
    {
        var stepStopwatch = Stopwatch.StartNew();
        var operationLogger = logger ?? _logger;
        
        operationLogger?.LogDebug("Starting XML validation step {@Metrics}", new { 
            correlationId, 
            method = "ProcessValidationStep",
            xmlSize = jiraXml?.Length ?? 0
        });

        try
        {
            // Track service call
            var serviceCallStopwatch = Stopwatch.StartNew();
            var validationResult = JiraXmlValidationService.Validate(jiraXml);
            serviceCallStopwatch.Stop();
            
            operationLogger?.LogDebug("XML validation service call completed {@ServiceCall}", new {
                correlationId,
                serviceName = "JiraXmlValidationService",
                method = "Validate",
                processingTimeMs = serviceCallStopwatch.TotalMilliseconds,
                responseSize = validationResult?.Length ?? 0
            });

            var stepStatus = validationResult.Contains("error") ? "failed" : "passed";
            
            stepStopwatch.Stop();
            
            var step = new WorkflowStep
            {
                Result = validationResult,
                Status = stepStatus,
                Timestamp = DateTime.UtcNow
            };

            operationLogger?.LogDebug("XML validation step completed {@Metrics}", new { 
                correlationId,
                method = "ProcessValidationStep",
                stepStatus,
                processingTimeMs = stepStopwatch.TotalMilliseconds,
                validationPassed = stepStatus == "passed"
            });

            return step;
        }
        catch (Exception ex)
        {
            stepStopwatch.Stop();
            
            operationLogger?.LogWarning(ex, "XML validation step failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                processingTimeMs = stepStopwatch.TotalMilliseconds,
                recommendation = "check_xml_format_and_structure"
            });

            return new WorkflowStep
            {
                Result = $"Validation failed: {ex.Message}",
                Status = "failed",
                Timestamp = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Processes XML cleaning step if validation failed
    /// </summary>
    /// <param name="originalXml">The original XML content</param>
    /// <param name="validationStep">The validation step result</param>
    /// <param name="workflowResult">The workflow result to update</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>Processed XML (cleaned if needed, original if validation passed)</returns>
    private static string ProcessCleaningStep(string originalXml, WorkflowStep validationStep, WorkflowResult workflowResult, ILogger? logger = null, string correlationId = "")
    {
        var stepStopwatch = Stopwatch.StartNew();
        var operationLogger = logger ?? _logger;
        
        operationLogger?.LogDebug("Processing XML cleaning step {@Metrics}", new { 
            correlationId, 
            method = "ProcessCleaningStep",
            validationStatus = validationStep.Status,
            xmlSize = originalXml?.Length ?? 0
        });

        try
        {
            if (validationStep.Status == "failed")
            {
                operationLogger?.LogInformation("XML validation failed, initiating cleaning process {@CleaningContext}", new { 
                    correlationId,
                    validationError = validationStep.ErrorMessage ?? "validation_error",
                    cleaningRequired = true
                });

                // Track service call
                var serviceCallStopwatch = Stopwatch.StartNew();
                var cleaningResult = JiraXmlCleaningService.Clean(originalXml);
                serviceCallStopwatch.Stop();
                
                operationLogger?.LogDebug("XML cleaning service call completed {@ServiceCall}", new {
                    correlationId,
                    serviceName = "JiraXmlCleaningService",
                    method = "Clean",
                    processingTimeMs = serviceCallStopwatch.TotalMilliseconds,
                    responseSize = cleaningResult?.Length ?? 0
                });

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
                        var cleanedXml = cleanedXmlElement.GetString();
                        if (!string.IsNullOrWhiteSpace(cleanedXml))
                        {
                            // Validate the cleaned XML is actually parseable
                            try
                            {
                                var doc = new System.Xml.XmlDocument();
                                doc.LoadXml(cleanedXml);
                                
                                stepStopwatch.Stop();
                                
                                operationLogger?.LogInformation("XML cleaning completed successfully {@CleaningSuccess}", new {
                                    correlationId,
                                    originalSize = originalXml?.Length ?? 0,
                                    cleanedSize = cleanedXml.Length,
                                    processingTimeMs = stepStopwatch.TotalMilliseconds,
                                    cleaningEffective = true
                                });
                                
                                return cleanedXml;
                            }
                            catch (Exception ex)
                            {
                                // If cleaned XML still fails, log the error and use original
                                cleaningStep.ErrorMessage = $"Cleaned XML validation failed: {ex.Message}";
                                
                                operationLogger?.LogWarning("Cleaned XML validation failed, using original {@CleaningFallback}", new {
                                    correlationId,
                                    cleaningError = ex.Message,
                                    fallbackAction = "use_original_xml"
                                });
                                
                                stepStopwatch.Stop();
                                return originalXml;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // If extraction fails, log the error and use original XML
                    cleaningStep.ErrorMessage = $"Failed to extract cleaned XML: {ex.Message}";
                    
                    operationLogger?.LogWarning("XML cleaning extraction failed {@CleaningError}", new {
                        correlationId,
                        extractionError = ex.Message,
                        fallbackAction = "use_original_xml"
                    });
                }
            }
            else
            {
                operationLogger?.LogDebug("XML validation passed, skipping cleaning step {@ValidationSuccess}", new { 
                    correlationId,
                    validationStatus = "passed",
                    cleaningSkipped = true
                });
            }

            stepStopwatch.Stop();
            
            operationLogger?.LogDebug("XML cleaning step completed {@Metrics}", new { 
                correlationId,
                method = "ProcessCleaningStep",
                processingTimeMs = stepStopwatch.TotalMilliseconds,
                xmlReturned = "original"
            });

            return originalXml;
        }
        catch (Exception ex)
        {
            stepStopwatch.Stop();
            
            operationLogger?.LogError(ex, "XML cleaning step failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                processingTimeMs = stepStopwatch.TotalMilliseconds,
                fallbackAction = "use_original_xml"
            });

            return originalXml;
        }
    }

    /// <summary>
    /// Processes primary analysis step
    /// </summary>
    /// <param name="processedXml">The processed XML content</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>Analysis step result</returns>
    private static WorkflowStep ProcessAnalysisStep(string processedXml, ILogger? logger = null, string correlationId = "")
    {
        var stepStopwatch = Stopwatch.StartNew();
        var operationLogger = logger ?? _logger;
        
        operationLogger?.LogDebug("Starting primary analysis step {@Metrics}", new { 
            correlationId, 
            method = "ProcessAnalysisStep",
            xmlSize = processedXml?.Length ?? 0
        });

        try
        {
            // Track service call
            var serviceCallStopwatch = Stopwatch.StartNew();
            var analysisResult = JiraXmlAnalysisService.AnalyzeForLLM(processedXml);
            serviceCallStopwatch.Stop();
            
            operationLogger?.LogDebug("Primary analysis service call completed {@ServiceCall}", new {
                correlationId,
                serviceName = "JiraXmlAnalysisService",
                method = "AnalyzeForLLM",
                processingTimeMs = serviceCallStopwatch.TotalMilliseconds,
                responseSize = analysisResult?.Length ?? 0
            });

            stepStopwatch.Stop();
            
            var step = new WorkflowStep
            {
                Result = analysisResult,
                Status = "completed",
                Timestamp = DateTime.UtcNow
            };

            operationLogger?.LogDebug("Primary analysis step completed {@Metrics}", new { 
                correlationId,
                method = "ProcessAnalysisStep",
                processingTimeMs = stepStopwatch.TotalMilliseconds,
                analysisCompleted = true
            });

            return step;
        }
        catch (Exception ex)
        {
            stepStopwatch.Stop();
            
            operationLogger?.LogWarning(ex, "Primary analysis step failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                processingTimeMs = stepStopwatch.TotalMilliseconds,
                recommendation = "check_xml_structure_and_service_availability"
            });

            return new WorkflowStep
            {
                Result = $"Analysis failed: {ex.Message}",
                Status = "failed",
                Timestamp = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Processes remaining workflow steps (templates, UI analysis, business logic, test cases)
    /// </summary>
    /// <param name="workflowResult">The workflow result to update</param>
    /// <param name="metadata">Extracted metadata for processing</param>
    /// <param name="processedXml">The processed (cleaned) XML content for test case generation</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <param name="completedSteps">Reference to completed steps counter</param>
    /// <param name="totalSteps">Total number of workflow steps</param>
    /// <param name="stepTimings">Dictionary to track step timings</param>
    private static void ProcessRemainingSteps(WorkflowResult workflowResult, WorkflowMetadata metadata, string processedXml, ILogger? logger = null, string correlationId = "", ref int completedSteps, int totalSteps = 8, Dictionary<string, double>? stepTimings = null)
    {
        var operationLogger = logger ?? _logger;
        stepTimings ??= new Dictionary<string, double>();

        try
        {
            // Step 5: Generate test case templates
            operationLogger?.LogInformation("Starting workflow step {@StepInfo}", new { 
                correlationId, 
                stepName = "templates", 
                stepNumber = 5, 
                totalSteps,
                workflowProgress = $"{completedSteps}/{totalSteps}"
            });

            var templatesStopwatch = Stopwatch.StartNew();
            var serviceCallStopwatch = Stopwatch.StartNew();
            var templatesResult = TestCaseTemplateService.GenerateTemplates(metadata.TicketType, metadata.Priority, metadata.Component);
            serviceCallStopwatch.Stop();
            templatesStopwatch.Stop();
            
            operationLogger?.LogDebug("Templates service call completed {@ServiceCall}", new {
                correlationId,
                serviceName = "TestCaseTemplateService",
                method = "GenerateTemplates",
                processingTimeMs = serviceCallStopwatch.TotalMilliseconds,
                responseSize = templatesResult?.Length ?? 0,
                ticketType = metadata.TicketType,
                priority = metadata.Priority
            });

            stepTimings["templates"] = templatesStopwatch.TotalMilliseconds;
            workflowResult.Steps.Add("templates", new WorkflowStep
            {
                Result = templatesResult,
                Status = "completed",
                Timestamp = DateTime.UtcNow
            });
            completedSteps++;

            operationLogger?.LogInformation("Workflow step completed {@Metrics}", new { 
                success = true, 
                stepName = "templates",
                processingTimeMs = templatesStopwatch.TotalMilliseconds,
                correlationId,
                stepStatus = "completed",
                workflowProgress = $"{completedSteps}/{totalSteps}"
            });

            // Step 6: UI component analysis
            operationLogger?.LogInformation("Starting workflow step {@StepInfo}", new { 
                correlationId, 
                stepName = "ui_analysis", 
                stepNumber = 6, 
                totalSteps,
                workflowProgress = $"{completedSteps}/{totalSteps}"
            });

            var uiAnalysisStopwatch = Stopwatch.StartNew();
            serviceCallStopwatch.Restart();
            var uiAnalysisResult = UiComponentAnalysisService.ExtractComponents(metadata.Description);
            serviceCallStopwatch.Stop();
            uiAnalysisStopwatch.Stop();
            
            operationLogger?.LogDebug("UI analysis service call completed {@ServiceCall}", new {
                correlationId,
                serviceName = "UiComponentAnalysisService",
                method = "ExtractComponents",
                processingTimeMs = serviceCallStopwatch.TotalMilliseconds,
                responseSize = uiAnalysisResult?.Length ?? 0,
                descriptionSize = metadata.Description?.Length ?? 0
            });

            stepTimings["ui_analysis"] = uiAnalysisStopwatch.TotalMilliseconds;
            workflowResult.Steps.Add("ui_analysis", new WorkflowStep
            {
                Result = uiAnalysisResult,
                Status = "completed",
                Timestamp = DateTime.UtcNow
            });
            completedSteps++;

            operationLogger?.LogInformation("Workflow step completed {@Metrics}", new { 
                success = true, 
                stepName = "ui_analysis",
                processingTimeMs = uiAnalysisStopwatch.TotalMilliseconds,
                correlationId,
                stepStatus = "completed",
                workflowProgress = $"{completedSteps}/{totalSteps}"
            });

            // Step 7: Business logic analysis
            operationLogger?.LogInformation("Starting workflow step {@StepInfo}", new { 
                correlationId, 
                stepName = "business_analysis", 
                stepNumber = 7, 
                totalSteps,
                workflowProgress = $"{completedSteps}/{totalSteps}"
            });

            var businessAnalysisStopwatch = Stopwatch.StartNew();
            serviceCallStopwatch.Restart();
            var businessAnalysisResult = BusinessLogicAnalysisService.ExtractLogic(metadata.Description, operationLogger);
            serviceCallStopwatch.Stop();
            businessAnalysisStopwatch.Stop();
            
            operationLogger?.LogDebug("Business logic analysis service call completed {@ServiceCall}", new {
                correlationId,
                serviceName = "BusinessLogicAnalysisService",
                method = "ExtractLogic",
                processingTimeMs = serviceCallStopwatch.TotalMilliseconds,
                responseSize = businessAnalysisResult?.Length ?? 0,
                descriptionSize = metadata.Description?.Length ?? 0
            });

            stepTimings["business_analysis"] = businessAnalysisStopwatch.TotalMilliseconds;
            workflowResult.Steps.Add("business_analysis", new WorkflowStep
            {
                Result = businessAnalysisResult,
                Status = "completed",
                Timestamp = DateTime.UtcNow
            });
            completedSteps++;

            operationLogger?.LogInformation("Workflow step completed {@Metrics}", new { 
                success = true, 
                stepName = "business_analysis",
                processingTimeMs = businessAnalysisStopwatch.TotalMilliseconds,
                correlationId,
                stepStatus = "completed",
                workflowProgress = $"{completedSteps}/{totalSteps}"
            });

            // Step 8: Generate final TestRail test cases using the cleaned XML
            operationLogger?.LogInformation("Starting workflow step {@StepInfo}", new { 
                correlationId, 
                stepName = "test_cases", 
                stepNumber = 8, 
                totalSteps,
                workflowProgress = $"{completedSteps}/{totalSteps}"
            });

            var testCasesStopwatch = Stopwatch.StartNew();
            try
            {
                serviceCallStopwatch.Restart();
                var testCasesResult = TestRailGenerationService.GenerateFromXml(processedXml);
                serviceCallStopwatch.Stop();
                testCasesStopwatch.Stop();
                
                operationLogger?.LogDebug("Test cases generation service call completed {@ServiceCall}", new {
                    correlationId,
                    serviceName = "TestRailGenerationService",
                    method = "GenerateFromXml",
                    processingTimeMs = serviceCallStopwatch.TotalMilliseconds,
                    responseSize = testCasesResult?.Length ?? 0,
                    xmlSize = processedXml?.Length ?? 0
                });

                stepTimings["test_cases"] = testCasesStopwatch.TotalMilliseconds;
                workflowResult.Steps.Add("test_cases", new WorkflowStep
                {
                    Result = testCasesResult,
                    Status = "completed",
                    Timestamp = DateTime.UtcNow
                });
                completedSteps++;

                operationLogger?.LogInformation("Workflow step completed {@Metrics}", new { 
                    success = true, 
                    stepName = "test_cases",
                    processingTimeMs = testCasesStopwatch.TotalMilliseconds,
                    correlationId,
                    stepStatus = "completed",
                    workflowProgress = $"{completedSteps}/{totalSteps}"
                });
            }
            catch (Exception ex)
            {
                testCasesStopwatch.Stop();
                
                operationLogger?.LogWarning(ex, "Test cases generation failed, providing graceful fallback {@StepError}", new {
                    correlationId,
                    stepName = "test_cases",
                    error = ex.Message,
                    processingTimeMs = testCasesStopwatch.TotalMilliseconds,
                    fallbackProvided = true
                });

                // If XML parsing fails, provide a graceful fallback
                stepTimings["test_cases"] = testCasesStopwatch.TotalMilliseconds;
                workflowResult.Steps.Add("test_cases", new WorkflowStep
                {
                    Result = $"XML Parsing Error: {ex.Message}. Consider using the analysis results from previous steps to manually generate test cases.",
                    Status = "completed",
                    Timestamp = DateTime.UtcNow,
                    ErrorMessage = ex.Message
                });
                completedSteps++;
            }

            // Check for step failure cascade
            var failedSteps = workflowResult.Steps.Count(kvp => kvp.Value.Status == "failed");
            if (failedSteps > 1)
            {
                operationLogger?.LogWarning("Multiple workflow step failures detected {@CascadeWarning}", new {
                    correlationId,
                    failedStepsCount = failedSteps,
                    totalSteps = workflowResult.Steps.Count,
                    cascadeRisk = "workflow_degradation",
                    recommendation = "investigate_root_cause_and_dependencies"
                });
            }

            operationLogger?.LogDebug("Remaining workflow steps completed {@Metrics}", new { 
                correlationId,
                method = "ProcessRemainingSteps",
                stepsProcessed = 4,
                completedSteps,
                totalSteps
            });
        }
        catch (Exception ex)
        {
            operationLogger?.LogError(ex, "Remaining workflow steps processing failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                completedSteps,
                recommendation = "check_service_dependencies_and_metadata_quality"
            });

            throw; // Re-throw to be handled by the main workflow
        }
    }

    /// <summary>
    /// Extracts metadata from analysis results for subsequent workflow steps
    /// </summary>
    /// <param name="analysisResult">JSON analysis result from JiraXmlAnalysisService</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>Extracted metadata for workflow processing</returns>
    private static WorkflowMetadata ExtractMetadata(string analysisResult, ILogger? logger = null, string correlationId = "")
    {
        var operationLogger = logger ?? _logger;
        var stopwatch = Stopwatch.StartNew();
        var extractedFieldsCount = 0;
        var validatedFieldsCount = 0;
        
        operationLogger?.LogDebug("Starting metadata extraction {@OperationInfo}", new {
            correlationId,
            method = "ExtractMetadata",
            analysisResultSize = analysisResult?.Length ?? 0
        });

        var metadata = new WorkflowMetadata();

        try
        {
            var analysisJson = JsonSerializer.Deserialize<JsonElement>(analysisResult);
            if (analysisJson.TryGetProperty("ticketInfo", out var ticketInfo))
            {
                if (ticketInfo.TryGetProperty("type", out var typeElement))
                {
                    metadata.TicketType = typeElement.GetString() ?? "Story";
                    extractedFieldsCount++;
                    if (!string.IsNullOrEmpty(metadata.TicketType) && metadata.TicketType != "Unknown") validatedFieldsCount++;
                }
                if (ticketInfo.TryGetProperty("priority", out var priorityElement))
                {
                    metadata.Priority = priorityElement.GetString() ?? "Medium";
                    extractedFieldsCount++;
                    if (!string.IsNullOrEmpty(metadata.Priority) && metadata.Priority != "Unknown") validatedFieldsCount++;
                }
                if (ticketInfo.TryGetProperty("description", out var descElement))
                {
                    metadata.Description = descElement.GetString() ?? "";
                    extractedFieldsCount++;
                    if (!string.IsNullOrEmpty(metadata.Description) && metadata.Description.Length > 10) validatedFieldsCount++;
                }
                if (ticketInfo.TryGetProperty("key", out var keyElement))
                {
                    metadata.TicketKey = keyElement.GetString() ?? "";
                    extractedFieldsCount++;
                    if (!string.IsNullOrEmpty(metadata.TicketKey)) validatedFieldsCount++;
                }
                if (ticketInfo.TryGetProperty("summary", out var summaryElement))
                {
                    metadata.Summary = summaryElement.GetString() ?? "";
                    extractedFieldsCount++;
                    if (!string.IsNullOrEmpty(metadata.Summary) && metadata.Summary.Length > 5) validatedFieldsCount++;
                }
            }

            stopwatch.Stop();

            var metadataQuality = extractedFieldsCount > 0 ? validatedFieldsCount / (double)extractedFieldsCount : 0;
            
            operationLogger?.LogInformation("Metadata extraction completed {@Metrics}", new {
                success = true,
                method = "ExtractMetadata",
                correlationId,
                processingTimeMs = stopwatch.TotalMilliseconds,
                extractedFieldsCount,
                validatedFieldsCount,
                metadataQuality = $"{metadataQuality:P0}",
                ticketType = metadata.TicketType,
                priority = metadata.Priority,
                ticketKey = metadata.TicketKey,
                summarySize = metadata.Summary?.Length ?? 0,
                descriptionSize = metadata.Description?.Length ?? 0
            });

            // Log metadata quality warnings
            if (metadataQuality < 0.75)
            {
                operationLogger?.LogWarning("Low metadata quality detected {@QualityWarning}", new {
                    correlationId,
                    metadataQuality = $"{metadataQuality:P0}",
                    recommendation = "consider_improved_xml_structure_or_analysis_service",
                    impactedServices = new[] { "TestCaseTemplateService", "UiComponentAnalysisService" }
                });
            }

            return metadata;
        }
        catch (JsonException jsonEx)
        {
            stopwatch.Stop();
            
            operationLogger?.LogError(jsonEx, "JSON parsing failed during metadata extraction {@ErrorContext}", new {
                correlationId,
                error = jsonEx.Message,
                processingTimeMs = stopwatch.TotalMilliseconds,
                fallbackUsed = true,
                analysisResultSize = analysisResult?.Length ?? 0
            });

            // Return fallback metadata with sensible defaults
            return new WorkflowMetadata
            {
                TicketType = "Story",
                Priority = "Medium",
                Component = "Unknown",
                Description = "Metadata extraction failed - JSON parsing error"
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            operationLogger?.LogError(ex, "Metadata extraction failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                processingTimeMs = stopwatch.TotalMilliseconds,
                fallbackUsed = true
            });

            // Return fallback metadata
            return new WorkflowMetadata
            {
                TicketType = "Unknown",
                Priority = "Unknown", 
                Component = "Unknown",
                Description = "Metadata extraction failed"
            };
        }
    }

    /// <summary>
    /// Creates workflow summary with processing statistics
    /// </summary>
    /// <param name="steps">Dictionary of workflow steps and their results</param>
    /// <param name="metadata">Extracted workflow metadata</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>Workflow summary object</returns>
    private static WorkflowSummary CreateWorkflowSummary(Dictionary<string, WorkflowStep> steps, WorkflowMetadata metadata, ILogger? logger = null, string correlationId = "")
    {
        var operationLogger = logger ?? _logger;
        var stopwatch = Stopwatch.StartNew();
        
        operationLogger?.LogDebug("Creating workflow summary {@OperationInfo}", new {
            correlationId,
            method = "CreateWorkflowSummary",
            totalSteps = steps.Count
        });

        try
        {
            var successfulSteps = steps.Count(kvp => kvp.Value.Status != "failed");
            var failedSteps = steps.Count(kvp => kvp.Value.Status == "failed");
            var completedSteps = steps.Count(kvp => kvp.Value.Status == "completed");
            var stepsWithErrors = steps.Count(kvp => !string.IsNullOrEmpty(kvp.Value.ErrorMessage));
            
            // Calculate success rate
            var successRate = steps.Count > 0 ? successfulSteps / (double)steps.Count : 0;
            
            // Analyze step performance (if timing data is available)
            var stepNames = steps.Keys.ToList();
            var criticalStepsFailed = stepNames.Where(name => 
                new[] { "validation", "cleaning", "analysis" }.Contains(name) && 
                steps[name].Status == "failed").ToList();

            var workflowHealthScore = CalculateWorkflowHealthScore(successRate, criticalStepsFailed.Count, stepsWithErrors);
            
            var summary = new WorkflowSummary
            {
                TotalSteps = steps.Count,
                SuccessfulSteps = successfulSteps,
                XmlCleaned = steps.ContainsKey("cleaning") && steps["cleaning"].Status == "completed",
                TicketType = metadata.TicketType,
                Priority = metadata.Priority,
                Component = metadata.Component,
                ProcessingTime = DateTime.UtcNow,
                Recommendation = GenerateWorkflowRecommendation(successRate, criticalStepsFailed, stepsWithErrors, workflowHealthScore)
            };

            stopwatch.Stop();

            operationLogger?.LogInformation("Workflow summary created {@Metrics}", new {
                success = true,
                method = "CreateWorkflowSummary",
                correlationId,
                processingTimeMs = stopwatch.TotalMilliseconds,
                totalSteps = steps.Count,
                successfulSteps,
                failedSteps,
                completedSteps,
                successRate = $"{successRate:P0}",
                workflowHealthScore = $"{workflowHealthScore:P0}",
                criticalStepsFailed = criticalStepsFailed.Count,
                stepsWithErrors,
                xmlCleaned = summary.XmlCleaned
            });

            // Log workflow quality insights
            if (successRate < 0.8)
            {
                operationLogger?.LogWarning("Low workflow success rate detected {@QualityWarning}", new {
                    correlationId,
                    successRate = $"{successRate:P0}",
                    failedSteps,
                    criticalStepsFailed = string.Join(", ", criticalStepsFailed),
                    recommendation = "investigate_service_dependencies_and_input_quality"
                });
            }

            if (workflowHealthScore < 0.7)
            {
                operationLogger?.LogWarning("Workflow health score below threshold {@HealthWarning}", new {
                    correlationId,
                    workflowHealthScore = $"{workflowHealthScore:P0}",
                    recommendation = "consider_workflow_optimization_and_error_handling_improvements"
                });
            }

            return summary;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            operationLogger?.LogError(ex, "Workflow summary creation failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                processingTimeMs = stopwatch.TotalMilliseconds,
                fallbackUsed = true
            });

            // Return fallback summary
            return new WorkflowSummary
            {
                TotalSteps = steps.Count,
                SuccessfulSteps = 0,
                XmlCleaned = false,
                TicketType = metadata.TicketType ?? "Unknown",
                Priority = metadata.Priority ?? "Unknown",
                Component = metadata.Component ?? "Unknown",
                ProcessingTime = DateTime.UtcNow,
                Recommendation = "Summary creation failed. Review workflow logs for details."
            };
        }
    }

    /// <summary>
    /// Calculates workflow health score based on success metrics
    /// </summary>
    private static double CalculateWorkflowHealthScore(double successRate, int criticalStepsFailed, int stepsWithErrors)
    {
        var baseScore = successRate;
        var criticalPenalty = criticalStepsFailed * 0.3; // Heavy penalty for critical step failures
        var errorPenalty = stepsWithErrors * 0.1; // Lighter penalty for steps with errors but completion
        
        return Math.Max(0, baseScore - criticalPenalty - errorPenalty);
    }

    /// <summary>
    /// Generates workflow recommendation based on execution metrics
    /// </summary>
    private static string GenerateWorkflowRecommendation(double successRate, List<string> criticalStepsFailed, int stepsWithErrors, double healthScore)
    {
        if (successRate == 1.0 && stepsWithErrors == 0)
        {
            return "All analysis steps completed successfully. Test cases are ready for TestRail import.";
        }
        
        if (criticalStepsFailed.Any())
        {
            return $"Critical workflow steps failed: {string.Join(", ", criticalStepsFailed)}. Address these issues before proceeding with test case generation.";
        }
        
        if (healthScore < 0.5)
        {
            return "Workflow health is poor. Consider reviewing input XML quality and service configurations.";
        }
        
        if (stepsWithErrors > 0)
        {
            return $"Workflow completed with {stepsWithErrors} steps having errors. Review individual step results for details.";
        }
        
        return $"Workflow completed with {successRate:P0} success rate. Some steps may need attention.";
    }

    /// <summary>
    /// Handles workflow errors and returns structured error response
    /// </summary>
    /// <param name="ex">The exception that occurred</param>
    /// <param name="logger">Optional logger for error handling</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>Structured error response as JSON</returns>
    private static string HandleWorkflowError(Exception ex, ILogger? logger = null, string correlationId = "")
    {
        var operationLogger = logger ?? _logger;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Analyze error context and type
            var errorType = ex.GetType().Name;
            var isRecoverable = DetermineIfErrorIsRecoverable(ex);
            var errorCategory = CategorizeError(ex);
            var recoveryRecommendation = GenerateRecoveryRecommendation(ex, errorCategory);

            operationLogger?.LogError(ex, "Workflow error handled {@ErrorContext}", new {
                correlationId,
                errorType,
                errorCategory,
                isRecoverable,
                recommendation = recoveryRecommendation,
                method = "HandleWorkflowError"
            });

            var errorResponse = new
            {
                error = $"Workflow processing failed: {ex.Message}",
                errorType,
                errorCategory,
                isRecoverable,
                correlationId,
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                recommendation = recoveryRecommendation,
                details = ex.StackTrace,
                workflowContext = new
                {
                    step = "unknown",
                    phase = "workflow_execution",
                    suggestedRetry = isRecoverable,
                    alternativeApproach = errorCategory == "service_dependency" 
                        ? "Try using individual service tools for step-by-step processing"
                        : "Review input XML format and structure"
                }
            };

            stopwatch.Stop();

            operationLogger?.LogDebug("Error response generated {@Metrics}", new {
                correlationId,
                processingTimeMs = stopwatch.TotalMilliseconds,
                errorResponseSize = JsonSerializer.Serialize(errorResponse).Length,
                errorHandled = true
            });

            return JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception handlingEx)
        {
            stopwatch.Stop();
            
            operationLogger?.LogCritical(handlingEx, "Error handling failed {@CriticalError}", new {
                correlationId,
                originalError = ex.Message,
                handlingError = handlingEx.Message,
                processingTimeMs = stopwatch.TotalMilliseconds
            });

            // Fallback error response
            var fallbackResponse = new
            {
                error = $"Workflow processing failed: {ex.Message}",
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                recommendation = "Critical error during error handling. Contact system administrator.",
                correlationId
            };

            return JsonSerializer.Serialize(fallbackResponse, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    /// <summary>
    /// Determines if an error is recoverable through retry or alternative approaches
    /// </summary>
    private static bool DetermineIfErrorIsRecoverable(Exception ex)
    {
        return ex switch
        {
            TimeoutException => true,
            HttpRequestException => true,
            JsonException => false,
            ArgumentException => false,
            FileNotFoundException => false,
            UnauthorizedAccessException => false,
            _ => true // Default to recoverable for unknown exceptions
        };
    }

    /// <summary>
    /// Categorizes errors for targeted handling and recommendations
    /// </summary>
    private static string CategorizeError(Exception ex)
    {
        return ex switch
        {
            JsonException => "data_format",
            XmlException => "data_format",
            ArgumentException => "input_validation",
            ArgumentNullException => "input_validation",
            FileNotFoundException => "resource_missing",
            DirectoryNotFoundException => "resource_missing",
            UnauthorizedAccessException => "permission",
            TimeoutException => "performance",
            HttpRequestException => "service_dependency",
            InvalidOperationException => "state_error",
            NotSupportedException => "feature_limitation",
            _ => "general"
        };
    }

    /// <summary>
    /// Generates specific recovery recommendations based on error analysis
    /// </summary>
    private static string GenerateRecoveryRecommendation(Exception ex, string errorCategory)
    {
        return errorCategory switch
        {
            "data_format" => "Check XML/JSON format and structure. Validate input against expected schema.",
            "input_validation" => "Verify input parameters are provided and meet validation requirements.",
            "resource_missing" => "Ensure required files and resources are available and accessible.",
            "permission" => "Check file system permissions and access rights for the workflow service.",
            "performance" => "Consider reducing XML size or optimizing service timeouts. Retry with smaller batches.",
            "service_dependency" => "Verify dependent services are running and accessible. Check network connectivity.",
            "state_error" => "Review workflow state and ensure services are properly initialized.",
            "feature_limitation" => "Use alternative processing approach or contact support for feature enhancement.",
            _ => "Review logs for detailed error analysis and consider using individual service tools."
        };
    }
}
