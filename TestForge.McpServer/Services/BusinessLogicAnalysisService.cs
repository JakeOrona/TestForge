using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace TestForge.McpServer.Services;

/// <summary>
/// Service responsible for identifying business rules and validation logic from requirements
/// </summary>
public static class BusinessLogicAnalysisService
{
    // Logger for observability and debugging
    private static ILogger? _logger;

    /// <summary>
    /// Initialize the service with logger for structured logging
    /// </summary>
    /// <param name="logger">Logger instance for structured logging</param>
    public static void Initialize(ILogger? logger) => _logger = logger;
    /// <summary>
    /// Identifies business rules and validation logic from ticket descriptions
    /// </summary>
    /// <param name="description">The ticket description or requirements text</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <returns>Structured analysis of business logic with confidence scores and test scenario suggestions</returns>
    public static string ExtractLogic(string description, ILogger? logger = null)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..8];
        var processingStopwatch = Stopwatch.StartNew();
        var operationLogger = logger ?? _logger;
        
        operationLogger?.LogInformation("Starting business logic analysis {@Metrics}", new { 
            correlationId, 
            descriptionSize = description?.Length ?? 0, 
            operation = "ExtractLogic" 
        });

        try
        {
            // Security monitoring for large inputs
            if (description?.Length > 50_000)
            {
                operationLogger?.LogWarning("Large description input detected {@SecurityContext}", new {
                    correlationId,
                    descriptionSize = description.Length,
                    securityRisk = "potential_memory_pressure",
                    recommendation = "consider_input_validation_limits"
                });
            }

            using (operationLogger?.BeginScope(new Dictionary<string, object> 
            { 
                ["CorrelationId"] = correlationId,
                ["Operation"] = "BusinessLogicAnalysis",
                ["DescriptionSize"] = description?.Length ?? 0
            }))
            {
                var businessLogic = new List<object>();
                var stepTimings = new Dictionary<string, double>();
                
                if (!string.IsNullOrWhiteSpace(description))
                {
                    // Step 1: Validation Rules Analysis
                    var validationStopwatch = Stopwatch.StartNew();
                    var validationRules = AnalyzeValidationRules(description, operationLogger, correlationId);
                    validationStopwatch.Stop();
                    stepTimings["ValidationRules"] = validationStopwatch.TotalMilliseconds;
                    businessLogic.AddRange(validationRules);
                    
                    operationLogger?.LogInformation("Validation rules analysis completed {@Metrics}", new { 
                        correlationId, 
                        rulesFound = validationRules.Count, 
                        processingTimeMs = validationStopwatch.TotalMilliseconds 
                    });

                    // Step 2: Business Rules Analysis
                    var businessStopwatch = Stopwatch.StartNew();
                    var businessRules = AnalyzeBusinessRules(description, operationLogger, correlationId);
                    businessStopwatch.Stop();
                    stepTimings["BusinessRules"] = businessStopwatch.TotalMilliseconds;
                    businessLogic.AddRange(businessRules);
                    
                    operationLogger?.LogInformation("Business rules analysis completed {@Metrics}", new { 
                        correlationId, 
                        rulesFound = businessRules.Count, 
                        processingTimeMs = businessStopwatch.TotalMilliseconds 
                    });

                    // Step 3: Workflow Logic Analysis
                    var workflowStopwatch = Stopwatch.StartNew();
                    var workflowRules = AnalyzeWorkflowLogic(description, operationLogger, correlationId);
                    workflowStopwatch.Stop();
                    stepTimings["WorkflowLogic"] = workflowStopwatch.TotalMilliseconds;
                    businessLogic.AddRange(workflowRules);
                    
                    operationLogger?.LogInformation("Workflow logic analysis completed {@Metrics}", new { 
                        correlationId, 
                        rulesFound = workflowRules.Count, 
                        processingTimeMs = workflowStopwatch.TotalMilliseconds 
                    });

                    // Step 4: Data Logic Analysis
                    var dataStopwatch = Stopwatch.StartNew();
                    var dataRules = AnalyzeDataLogic(description, operationLogger, correlationId);
                    dataStopwatch.Stop();
                    stepTimings["DataLogic"] = dataStopwatch.TotalMilliseconds;
                    businessLogic.AddRange(dataRules);
                    
                    operationLogger?.LogInformation("Data logic analysis completed {@Metrics}", new { 
                        correlationId, 
                        rulesFound = dataRules.Count, 
                        processingTimeMs = dataStopwatch.TotalMilliseconds 
                    });

                    // Step 5: Security Logic Analysis
                    var securityStopwatch = Stopwatch.StartNew();
                    var securityRules = AnalyzeSecurityLogic(description, operationLogger, correlationId);
                    securityStopwatch.Stop();
                    stepTimings["SecurityLogic"] = securityStopwatch.TotalMilliseconds;
                    businessLogic.AddRange(securityRules);
                    
                    operationLogger?.LogInformation("Security logic analysis completed {@Metrics}", new { 
                        correlationId, 
                        rulesFound = securityRules.Count, 
                        processingTimeMs = securityStopwatch.TotalMilliseconds 
                    });
                }

                // Generate comprehensive analysis
                var complexityAnalysis = AnalyzeComplexity(businessLogic, operationLogger, correlationId);
                var testScenarios = GenerateTestScenarios(businessLogic, operationLogger, correlationId);
                var riskAssessment = AssessRisks(businessLogic, operationLogger, correlationId);
                var testingStrategy = DetermineTestingStrategy(businessLogic, operationLogger, correlationId);

                var analysis = new
                {
                    businessLogic = businessLogic,
                    ruleCount = businessLogic.Count,
                    complexityAnalysis = complexityAnalysis,
                    testScenarioSuggestions = testScenarios,
                    riskAssessment = riskAssessment,
                    testingStrategy = testingStrategy
                };

                processingStopwatch.Stop();

                // Rule explosion detection
                if (businessLogic.Count > 50)
                {
                    operationLogger?.LogWarning("High rule count detected {@PerformanceContext}", new {
                        correlationId,
                        ruleCount = businessLogic.Count,
                        performanceRisk = "potential_processing_overhead",
                        recommendation = "consider_rule_optimization"
                    });
                }

                operationLogger?.LogInformation("Business logic analysis completed successfully {@Metrics}", new { 
                    success = true, 
                    correlationId,
                    totalProcessingTimeMs = processingStopwatch.TotalMilliseconds,
                    totalRules = businessLogic.Count,
                    stepTimings,
                    riskLevel = riskAssessment?.GetType()?.GetProperty("riskLevel")?.GetValue(riskAssessment),
                    complexityScore = complexityAnalysis?.GetType()?.GetProperty("complexityScore")?.GetValue(complexityAnalysis)
                });

                return JsonSerializer.Serialize(analysis, new JsonSerializerOptions { WriteIndented = true });
            }
        }
        catch (Exception ex)
        {
            processingStopwatch.Stop();
            
            operationLogger?.LogError(ex, "Business logic analysis failed {@ErrorContext}", new { 
                correlationId,
                error = ex.Message,
                processingTimeMs = processingStopwatch.TotalMilliseconds,
                descriptionSize = description?.Length ?? 0,
                recommendation = "check_input_format_and_content"
            });
            
            return JsonSerializer.Serialize(new { 
                error = $"Business logic analysis failed: {ex.Message}",
                correlationId = correlationId,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Analyzes validation rules in the description
    /// </summary>
    /// <param name="description">The description text</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>List of validation rules</returns>
    private static List<object> AnalyzeValidationRules(string description, ILogger? logger = null, string correlationId = "")
    {
        var methodStopwatch = Stopwatch.StartNew();
        var operationLogger = logger ?? _logger;
        
        operationLogger?.LogDebug("Starting validation rules analysis {@Metrics}", new { 
            correlationId, 
            method = "AnalyzeValidationRules",
            descriptionLength = description.Length 
        });

        var rules = new List<object>();
        var lowerDesc = description.ToLower();
        var rulesDetected = new List<string>();
        
        try
        {
            // Required field validation
            if (lowerDesc.Contains("required") || lowerDesc.Contains("mandatory") || lowerDesc.Contains("must"))
            {
                var rule = new { 
                    rule = "required_field", 
                    description = "Required field validation", 
                    confidence = 0.85, 
                    testScenarios = new[] { "missing required field", "present required field" },
                    riskLevel = "medium"
                };
                rules.Add(rule);
                rulesDetected.Add("required_field");
                
                operationLogger?.LogDebug("Validation rule detected {@RuleDetection}", new {
                    correlationId,
                    ruleType = "required_field",
                    confidence = 0.85,
                    riskLevel = "medium"
                });
            }
            
            // Format validation
            if (lowerDesc.Contains("format") || lowerDesc.Contains("pattern") || lowerDesc.Contains("regex"))
            {
                var rule = new { 
                    rule = "format_validation", 
                    description = "Input format validation", 
                    confidence = 0.88, 
                    testScenarios = new[] { "valid format", "invalid format", "empty format" },
                    riskLevel = "high"
                };
                rules.Add(rule);
                rulesDetected.Add("format_validation");
                
                operationLogger?.LogDebug("Validation rule detected {@RuleDetection}", new {
                    correlationId,
                    ruleType = "format_validation",
                    confidence = 0.88,
                    riskLevel = "high"
                });
            }
            
            // Range validation
            if (lowerDesc.Contains("minimum") || lowerDesc.Contains("maximum") || lowerDesc.Contains("range"))
            {
                var rule = new { 
                    rule = "range_validation", 
                    description = "Value range validation", 
                    confidence = 0.95, 
                    testScenarios = new[] { "below minimum", "above maximum", "within range", "boundary values" },
                    riskLevel = "high"
                };
                rules.Add(rule);
                rulesDetected.Add("range_validation");
                
                operationLogger?.LogDebug("Validation rule detected {@RuleDetection}", new {
                    correlationId,
                    ruleType = "range_validation",
                    confidence = 0.95,
                    riskLevel = "high"
                });
            }
            
            // Length validation
            if (lowerDesc.Contains("length") || lowerDesc.Contains("character") || lowerDesc.Contains("limit"))
            {
                var rule = new { 
                    rule = "length_validation", 
                    description = "Input length validation", 
                    confidence = 0.87, 
                    testScenarios = new[] { "too short", "too long", "exact length", "empty input" },
                    riskLevel = "medium"
                };
                rules.Add(rule);
                rulesDetected.Add("length_validation");
                
                operationLogger?.LogDebug("Validation rule detected {@RuleDetection}", new {
                    correlationId,
                    ruleType = "length_validation",
                    confidence = 0.87,
                    riskLevel = "medium"
                });
            }
            
            // Email validation
            if (lowerDesc.Contains("email") || lowerDesc.Contains("@"))
            {
                var rule = new { 
                    rule = "email_validation", 
                    description = "Email format validation", 
                    confidence = 0.92, 
                    testScenarios = new[] { "valid email", "invalid email", "missing @ symbol", "invalid domain" },
                    riskLevel = "medium"
                };
                rules.Add(rule);
                rulesDetected.Add("email_validation");
                
                operationLogger?.LogDebug("Validation rule detected {@RuleDetection}", new {
                    correlationId,
                    ruleType = "email_validation",
                    confidence = 0.92,
                    riskLevel = "medium"
                });
            }
            
            // Date validation
            if (lowerDesc.Contains("date") || lowerDesc.Contains("time") || lowerDesc.Contains("calendar"))
            {
                var rule = new { 
                    rule = "date_validation", 
                    description = "Date/time validation", 
                    confidence = 0.89, 
                    testScenarios = new[] { "valid date", "invalid date", "future date", "past date", "leap year" },
                    riskLevel = "medium"
                };
                rules.Add(rule);
                rulesDetected.Add("date_validation");
                
                operationLogger?.LogDebug("Validation rule detected {@RuleDetection}", new {
                    correlationId,
                    ruleType = "date_validation",
                    confidence = 0.89,
                    riskLevel = "medium"
                });
            }
            
            methodStopwatch.Stop();
            
            var averageConfidence = rules.Count > 0 ? 
                rules.Cast<dynamic>().Average(r => (double)r.confidence) : 0.0;
            
            operationLogger?.LogDebug("Validation rules analysis completed {@Metrics}", new { 
                correlationId,
                method = "AnalyzeValidationRules",
                rulesFound = rules.Count,
                rulesDetected = rulesDetected.ToArray(),
                averageConfidence,
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });
            
            return rules;
        }
        catch (Exception ex)
        {
            methodStopwatch.Stop();
            
            operationLogger?.LogWarning(ex, "Validation rules analysis failed, continuing with partial results {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                partialRulesFound = rules.Count,
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });
            
            return rules; // Return partial results
        }
    }

    /// <summary>
    /// Analyzes business rules in the description
    /// </summary>
    /// <param name="description">The description text</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>List of business rules</returns>
    private static List<object> AnalyzeBusinessRules(string description, ILogger? logger = null, string correlationId = "")
    {
        var methodStopwatch = Stopwatch.StartNew();
        var operationLogger = logger ?? _logger;
        
        operationLogger?.LogDebug("Starting business rules analysis {@Metrics}", new { 
            correlationId, 
            method = "AnalyzeBusinessRules",
            descriptionLength = description.Length 
        });

        var rules = new List<object>();
        var lowerDesc = description.ToLower();
        var rulesDetected = new List<string>();
        
        try
        {
            // Approval workflow
            if (lowerDesc.Contains("approval") || lowerDesc.Contains("approve") || lowerDesc.Contains("reject"))
            {
                var rule = new { 
                    rule = "approval_workflow", 
                    description = "Approval process business logic", 
                    confidence = 0.88, 
                    testScenarios = new[] { "approve request", "reject request", "pending approval", "auto-approval" },
                    riskLevel = "high"
                };
                rules.Add(rule);
                rulesDetected.Add("approval_workflow");
                
                operationLogger?.LogDebug("Business rule detected {@RuleDetection}", new {
                    correlationId,
                    ruleType = "approval_workflow",
                    confidence = 0.88,
                    riskLevel = "high"
                });
            }
            
            // Status transitions
            if (lowerDesc.Contains("status") || lowerDesc.Contains("state") || lowerDesc.Contains("transition"))
            {
                var rule = new { 
                    rule = "status_transition", 
                    description = "Status transition logic", 
                    confidence = 0.85, 
                    testScenarios = new[] { "valid transition", "invalid transition", "status persistence" },
                    riskLevel = "medium"
                };
                rules.Add(rule);
                rulesDetected.Add("status_transition");
                
                operationLogger?.LogDebug("Business rule detected {@RuleDetection}", new {
                    correlationId,
                    ruleType = "status_transition",
                    confidence = 0.85,
                    riskLevel = "medium"
                });
            }
            
            // Business calculations
            if (lowerDesc.Contains("calculate") || lowerDesc.Contains("compute") || lowerDesc.Contains("formula"))
            {
                var rule = new { 
                    rule = "business_calculation", 
                    description = "Business calculation logic", 
                    confidence = 0.92, 
                    testScenarios = new[] { "valid calculation", "edge case values", "precision testing", "overflow handling" },
                    riskLevel = "high"
                };
                rules.Add(rule);
                rulesDetected.Add("business_calculation");
                
                operationLogger?.LogDebug("Business rule detected {@RuleDetection}", new {
                    correlationId,
                    ruleType = "business_calculation",
                    confidence = 0.92,
                    riskLevel = "high"
                });
            }
            
            // Conditional logic
            if (lowerDesc.Contains("if") || lowerDesc.Contains("when") || lowerDesc.Contains("condition"))
            {
                var rule = new { 
                    rule = "conditional_logic", 
                    description = "Conditional business logic", 
                    confidence = 0.83, 
                    testScenarios = new[] { "condition met", "condition not met", "multiple conditions", "nested conditions" },
                    riskLevel = "medium"
                };
                rules.Add(rule);
                rulesDetected.Add("conditional_logic");
                
                operationLogger?.LogDebug("Business rule detected {@RuleDetection}", new {
                    correlationId,
                    ruleType = "conditional_logic",
                    confidence = 0.83,
                    riskLevel = "medium"
                });
            }
            
            methodStopwatch.Stop();
            
            var averageConfidence = rules.Count > 0 ? 
                rules.Cast<dynamic>().Average(r => (double)r.confidence) : 0.0;
            
            operationLogger?.LogDebug("Business rules analysis completed {@Metrics}", new { 
                correlationId,
                method = "AnalyzeBusinessRules",
                rulesFound = rules.Count,
                rulesDetected = rulesDetected.ToArray(),
                averageConfidence,
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });
            
            return rules;
        }
        catch (Exception ex)
        {
            methodStopwatch.Stop();
            
            operationLogger?.LogWarning(ex, "Business rules analysis failed, continuing with partial results {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                partialRulesFound = rules.Count,
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });
            
            return rules; // Return partial results
        }
    }

    /// <summary>
    /// Analyzes workflow logic in the description
    /// </summary>
    /// <param name="description">The description text</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>List of workflow rules</returns>
    private static List<object> AnalyzeWorkflowLogic(string description, ILogger? logger = null, string correlationId = "")
    {
        var methodStopwatch = Stopwatch.StartNew();
        var operationLogger = logger ?? _logger;
        
        operationLogger?.LogDebug("Starting workflow logic analysis {@Metrics}", new { 
            correlationId, 
            method = "AnalyzeWorkflowLogic",
            descriptionLength = description.Length 
        });

        var rules = new List<object>();
        var lowerDesc = description.ToLower();
        var rulesDetected = new List<string>();
        
        try
        {
            // Sequential workflow
            if (lowerDesc.Contains("step") || lowerDesc.Contains("sequence") || lowerDesc.Contains("order"))
            {
                var rule = new { 
                    rule = "sequential_workflow", 
                    description = "Sequential step processing", 
                    confidence = 0.81, 
                    testScenarios = new[] { "complete sequence", "interrupted sequence", "skip steps", "restart workflow" },
                    riskLevel = "medium"
                };
                rules.Add(rule);
                rulesDetected.Add("sequential_workflow");
                
                operationLogger?.LogDebug("Workflow rule detected {@RuleDetection}", new {
                    correlationId,
                    ruleType = "sequential_workflow",
                    confidence = 0.81,
                    riskLevel = "medium"
                });
            }
            
            // Parallel processing
            if (lowerDesc.Contains("parallel") || lowerDesc.Contains("concurrent") || lowerDesc.Contains("simultaneous"))
            {
                var rule = new { 
                    rule = "parallel_processing", 
                    description = "Parallel workflow processing", 
                    confidence = 0.87, 
                    testScenarios = new[] { "parallel execution", "race conditions", "synchronization", "resource conflicts" },
                    riskLevel = "high"
                };
                rules.Add(rule);
                rulesDetected.Add("parallel_processing");
                
                operationLogger?.LogDebug("Workflow rule detected {@RuleDetection}", new {
                    correlationId,
                    ruleType = "parallel_processing",
                    confidence = 0.87,
                    riskLevel = "high"
                });
            }
            
            // Timeout handling
            if (lowerDesc.Contains("timeout") || lowerDesc.Contains("expir") || lowerDesc.Contains("deadline"))
            {
                var rule = new { 
                    rule = "timeout_handling", 
                    description = "Timeout and expiration logic", 
                    confidence = 0.89, 
                    testScenarios = new[] { "before timeout", "after timeout", "timeout recovery", "extend timeout" },
                    riskLevel = "medium"
                };
                rules.Add(rule);
                rulesDetected.Add("timeout_handling");
                
                operationLogger?.LogDebug("Workflow rule detected {@RuleDetection}", new {
                    correlationId,
                    ruleType = "timeout_handling",
                    confidence = 0.89,
                    riskLevel = "medium"
                });
            }
            
            methodStopwatch.Stop();
            
            var averageConfidence = rules.Count > 0 ? 
                rules.Cast<dynamic>().Average(r => (double)r.confidence) : 0.0;
            
            operationLogger?.LogDebug("Workflow logic analysis completed {@Metrics}", new { 
                correlationId,
                method = "AnalyzeWorkflowLogic",
                rulesFound = rules.Count,
                rulesDetected = rulesDetected.ToArray(),
                averageConfidence,
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });
            
            return rules;
        }
        catch (Exception ex)
        {
            methodStopwatch.Stop();
            
            operationLogger?.LogWarning(ex, "Workflow logic analysis failed, continuing with partial results {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                partialRulesFound = rules.Count,
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });
            
            return rules; // Return partial results
        }
    }

    /// <summary>
    /// Analyzes data logic in the description
    /// </summary>
    /// <param name="description">The description text</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>List of data rules</returns>
    private static List<object> AnalyzeDataLogic(string description, ILogger? logger = null, string correlationId = "")
    {
        var methodStopwatch = Stopwatch.StartNew();
        var operationLogger = logger ?? _logger;
        
        operationLogger?.LogDebug("Starting data logic analysis {@Metrics}", new { 
            correlationId, 
            method = "AnalyzeDataLogic",
            descriptionLength = description.Length 
        });

        var rules = new List<object>();
        var lowerDesc = description.ToLower();
        var rulesDetected = new List<string>();
        
        try
        {
            // Data integrity
            if (lowerDesc.Contains("integrity") || lowerDesc.Contains("consistent") || lowerDesc.Contains("duplicate"))
            {
                var rule = new { 
                    rule = "data_integrity", 
                    description = "Data integrity constraints", 
                    confidence = 0.91, 
                    testScenarios = new[] { "data consistency", "duplicate prevention", "referential integrity", "data corruption" },
                    riskLevel = "high"
                };
                rules.Add(rule);
                rulesDetected.Add("data_integrity");
                
                operationLogger?.LogDebug("Data rule detected {@RuleDetection}", new {
                    correlationId,
                    ruleType = "data_integrity",
                    confidence = 0.91,
                    riskLevel = "high"
                });
            }
            
            // Data transformation
            if (lowerDesc.Contains("transform") || lowerDesc.Contains("convert") || lowerDesc.Contains("mapping"))
            {
                var rule = new { 
                    rule = "data_transformation", 
                    description = "Data transformation logic", 
                    confidence = 0.86, 
                    testScenarios = new[] { "successful transformation", "transformation failure", "data type conversion", "mapping accuracy" },
                    riskLevel = "medium"
                };
                rules.Add(rule);
                rulesDetected.Add("data_transformation");
                
                operationLogger?.LogDebug("Data rule detected {@RuleDetection}", new {
                    correlationId,
                    ruleType = "data_transformation",
                    confidence = 0.86,
                    riskLevel = "medium"
                });
            }
            
            // Data archival
            if (lowerDesc.Contains("archive") || lowerDesc.Contains("retain") || lowerDesc.Contains("purge"))
            {
                var rule = new { 
                    rule = "data_archival", 
                    description = "Data archival and retention", 
                    confidence = 0.84, 
                    testScenarios = new[] { "archive old data", "retain active data", "purge expired data", "restore archived data" },
                    riskLevel = "low"
                };
                rules.Add(rule);
                rulesDetected.Add("data_archival");
                
                operationLogger?.LogDebug("Data rule detected {@RuleDetection}", new {
                    correlationId,
                    ruleType = "data_archival",
                    confidence = 0.84,
                    riskLevel = "low"
                });
            }
            
            methodStopwatch.Stop();
            
            var averageConfidence = rules.Count > 0 ? 
                rules.Cast<dynamic>().Average(r => (double)r.confidence) : 0.0;
            
            operationLogger?.LogDebug("Data logic analysis completed {@Metrics}", new { 
                correlationId,
                method = "AnalyzeDataLogic",
                rulesFound = rules.Count,
                rulesDetected = rulesDetected.ToArray(),
                averageConfidence,
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });
            
            return rules;
        }
        catch (Exception ex)
        {
            methodStopwatch.Stop();
            
            operationLogger?.LogWarning(ex, "Data logic analysis failed, continuing with partial results {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                partialRulesFound = rules.Count,
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });
            
            return rules; // Return partial results
        }
    }

    /// <summary>
    /// Analyzes security logic in the description
    /// </summary>
    /// <param name="description">The description text</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>List of security rules</returns>
    private static List<object> AnalyzeSecurityLogic(string description, ILogger? logger = null, string correlationId = "")
    {
        var methodStopwatch = Stopwatch.StartNew();
        var operationLogger = logger ?? _logger;
        
        operationLogger?.LogDebug("Starting security logic analysis {@Metrics}", new { 
            correlationId, 
            method = "AnalyzeSecurityLogic",
            descriptionLength = description.Length 
        });

        var rules = new List<object>();
        var lowerDesc = description.ToLower();
        var rulesDetected = new List<string>();
        
        try
        {
            // Access control
            if (lowerDesc.Contains("permission") || lowerDesc.Contains("access") || lowerDesc.Contains("authorize"))
            {
                var rule = new { 
                    rule = "access_control", 
                    description = "Access control logic", 
                    confidence = 0.90, 
                    testScenarios = new[] { "authorized access", "unauthorized access", "role-based access", "permission inheritance" },
                    riskLevel = "high"
                };
                rules.Add(rule);
                rulesDetected.Add("access_control");
                
                operationLogger?.LogDebug("Security rule detected {@RuleDetection}", new {
                    correlationId,
                    ruleType = "access_control",
                    confidence = 0.90,
                    riskLevel = "high"
                });
            }
            
            // Authentication
            if (lowerDesc.Contains("authenticate") || lowerDesc.Contains("login") || lowerDesc.Contains("credential"))
            {
                var rule = new { 
                    rule = "authentication", 
                    description = "Authentication logic", 
                    confidence = 0.93, 
                    testScenarios = new[] { "valid credentials", "invalid credentials", "session management", "multi-factor auth" },
                    riskLevel = "high"
                };
                rules.Add(rule);
                rulesDetected.Add("authentication");
                
                operationLogger?.LogDebug("Security rule detected {@RuleDetection}", new {
                    correlationId,
                    ruleType = "authentication",
                    confidence = 0.93,
                    riskLevel = "high"
                });
            }
            
            // Data encryption
            if (lowerDesc.Contains("encrypt") || lowerDesc.Contains("secure") || lowerDesc.Contains("protect"))
            {
                var rule = new { 
                    rule = "data_encryption", 
                    description = "Data encryption and security", 
                    confidence = 0.87, 
                    testScenarios = new[] { "encrypted storage", "secure transmission", "key management", "decryption accuracy" },
                    riskLevel = "high"
                };
                rules.Add(rule);
                rulesDetected.Add("data_encryption");
                
                operationLogger?.LogDebug("Security rule detected {@RuleDetection}", new {
                    correlationId,
                    ruleType = "data_encryption",
                    confidence = 0.87,
                    riskLevel = "high"
                });
            }
            
            methodStopwatch.Stop();
            
            var averageConfidence = rules.Count > 0 ? 
                rules.Cast<dynamic>().Average(r => (double)r.confidence) : 0.0;
            
            operationLogger?.LogDebug("Security logic analysis completed {@Metrics}", new { 
                correlationId,
                method = "AnalyzeSecurityLogic",
                rulesFound = rules.Count,
                rulesDetected = rulesDetected.ToArray(),
                averageConfidence,
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });
            
            return rules;
        }
        catch (Exception ex)
        {
            methodStopwatch.Stop();
            
            operationLogger?.LogWarning(ex, "Security logic analysis failed, continuing with partial results {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                partialRulesFound = rules.Count,
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });
            
            return rules; // Return partial results
        }
    }

    /// <summary>
    /// Analyzes complexity of business logic rules
    /// </summary>
    /// <param name="businessLogic">List of business logic rules</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>Complexity analysis</returns>
    private static object AnalyzeComplexity(List<object> businessLogic, ILogger? logger = null, string correlationId = "")
    {
        var methodStopwatch = Stopwatch.StartNew();
        var operationLogger = logger ?? _logger;
        
        operationLogger?.LogDebug("Starting complexity analysis {@Metrics}", new { 
            correlationId, 
            method = "AnalyzeComplexity",
            ruleCount = businessLogic.Count 
        });

        try
        {
            var highRisk = businessLogic.Count(rule => rule?.ToString()?.Contains("high") == true);
            var mediumRisk = businessLogic.Count(rule => rule?.ToString()?.Contains("medium") == true);
            var lowRisk = businessLogic.Count(rule => rule?.ToString()?.Contains("low") == true);
            
            var overallComplexity = highRisk > 0 ? "high" : 
                                   mediumRisk > 0 ? "medium" : 
                                   lowRisk > 0 ? "low" : "none";
            
            var complexityScore = (highRisk * 3) + (mediumRisk * 2) + (lowRisk * 1);
            
            methodStopwatch.Stop();
            
            var result = new
            {
                totalRules = businessLogic.Count,
                highRiskRules = highRisk,
                mediumRiskRules = mediumRisk,
                lowRiskRules = lowRisk,
                overallComplexity = overallComplexity,
                complexityScore = complexityScore
            };
            
            operationLogger?.LogDebug("Complexity analysis completed {@Metrics}", new { 
                correlationId,
                method = "AnalyzeComplexity",
                totalRules = businessLogic.Count,
                highRiskRules = highRisk,
                mediumRiskRules = mediumRisk,
                lowRiskRules = lowRisk,
                overallComplexity,
                complexityScore,
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });
            
            return result;
        }
        catch (Exception ex)
        {
            methodStopwatch.Stop();
            
            operationLogger?.LogWarning(ex, "Complexity analysis failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                ruleCount = businessLogic.Count,
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });
            
            // Return fallback complexity analysis
            return new
            {
                totalRules = businessLogic.Count,
                highRiskRules = 0,
                mediumRiskRules = 0,
                lowRiskRules = 0,
                overallComplexity = "unknown",
                complexityScore = 0,
                analysisError = true
            };
        }
    }

    /// <summary>
    /// Generates test scenarios based on business logic
    /// </summary>
    /// <param name="businessLogic">List of business logic rules</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>Test scenario suggestions</returns>
    private static string[] GenerateTestScenarios(List<object> businessLogic, ILogger? logger = null, string correlationId = "")
    {
        var methodStopwatch = Stopwatch.StartNew();
        var operationLogger = logger ?? _logger;
        
        operationLogger?.LogDebug("Starting test scenario generation {@Metrics}", new { 
            correlationId, 
            method = "GenerateTestScenarios",
            ruleCount = businessLogic.Count 
        });

        try
        {
            var scenarios = new List<string>
            {
                "Test happy path scenarios for all business rules",
                "Verify error handling for invalid inputs",
                "Test boundary conditions and edge cases",
                "Validate business rule interactions and dependencies"
            };
            
            var scenarioTypes = new List<string>();
            
            if (businessLogic.Any(rule => rule?.ToString()?.Contains("validation") == true))
            {
                scenarios.Add("Comprehensive validation testing with various input combinations");
                scenarioTypes.Add("validation");
            }
            
            if (businessLogic.Any(rule => rule?.ToString()?.Contains("workflow") == true))
            {
                scenarios.Add("End-to-end workflow testing with different paths");
                scenarioTypes.Add("workflow");
            }
            
            if (businessLogic.Any(rule => rule?.ToString()?.Contains("security") == true))
            {
                scenarios.Add("Security testing with unauthorized access attempts");
                scenarioTypes.Add("security");
            }
            
            methodStopwatch.Stop();
            
            operationLogger?.LogDebug("Test scenario generation completed {@Metrics}", new { 
                correlationId,
                method = "GenerateTestScenarios",
                scenariosGenerated = scenarios.Count,
                scenarioTypes = scenarioTypes.ToArray(),
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });
            
            return scenarios.ToArray();
        }
        catch (Exception ex)
        {
            methodStopwatch.Stop();
            
            operationLogger?.LogWarning(ex, "Test scenario generation failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                ruleCount = businessLogic.Count,
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });
            
            // Return basic scenarios as fallback
            return new[]
            {
                "Test happy path scenarios for all business rules",
                "Verify error handling for invalid inputs"
            };
        }
    }

    /// <summary>
    /// Assesses risks based on business logic
    /// </summary>
    /// <param name="businessLogic">List of business logic rules</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>Risk assessment</returns>
    private static object AssessRisks(List<object> businessLogic, ILogger? logger = null, string correlationId = "")
    {
        var methodStopwatch = Stopwatch.StartNew();
        var operationLogger = logger ?? _logger;
        
        operationLogger?.LogDebug("Starting risk assessment {@Metrics}", new { 
            correlationId, 
            method = "AssessRisks",
            ruleCount = businessLogic.Count 
        });

        try
        {
            var risks = new List<string>();
            var riskCategories = new List<string>();
            
            if (businessLogic.Any(rule => rule?.ToString()?.Contains("calculation") == true))
            {
                risks.Add("Financial calculation errors could have significant impact");
                riskCategories.Add("financial");
            }
            
            if (businessLogic.Any(rule => rule?.ToString()?.Contains("security") == true))
            {
                risks.Add("Security vulnerabilities could lead to data breaches");
                riskCategories.Add("security");
            }
            
            if (businessLogic.Any(rule => rule?.ToString()?.Contains("workflow") == true))
            {
                risks.Add("Workflow failures could disrupt business processes");
                riskCategories.Add("operational");
            }
            
            if (businessLogic.Any(rule => rule?.ToString()?.Contains("data_integrity") == true))
            {
                risks.Add("Data integrity issues could compromise system reliability");
                riskCategories.Add("data");
            }
            
            var riskLevel = risks.Count switch
            {
                0 => "low",
                1 => "low",
                2 => "medium",
                3 => "high",
                _ => "very high"
            };
            
            methodStopwatch.Stop();
            
            var result = new
            {
                identifiedRisks = risks.ToArray(),
                riskLevel = riskLevel,
                riskCategories = riskCategories.ToArray(),
                mitigationStrategies = new[]
                {
                    "Implement comprehensive test coverage",
                    "Add extensive validation and error handling",
                    "Perform security testing and code review",
                    "Plan for monitoring and alerting"
                }
            };
            
            operationLogger?.LogDebug("Risk assessment completed {@Metrics}", new { 
                correlationId,
                method = "AssessRisks",
                risksIdentified = risks.Count,
                riskLevel,
                riskCategories = riskCategories.ToArray(),
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });
            
            return result;
        }
        catch (Exception ex)
        {
            methodStopwatch.Stop();
            
            operationLogger?.LogWarning(ex, "Risk assessment failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                ruleCount = businessLogic.Count,
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });
            
            // Return fallback risk assessment
            return new
            {
                identifiedRisks = new string[0],
                riskLevel = "unknown",
                riskCategories = new string[0],
                mitigationStrategies = new[]
                {
                    "Implement comprehensive test coverage",
                    "Add extensive validation and error handling"
                },
                assessmentError = true
            };
        }
    }

    /// <summary>
    /// Determines testing strategy based on business logic
    /// </summary>
    /// <param name="businessLogic">List of business logic rules</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>Testing strategy recommendations</returns>
    private static object DetermineTestingStrategy(List<object> businessLogic, ILogger? logger = null, string correlationId = "")
    {
        var methodStopwatch = Stopwatch.StartNew();
        var operationLogger = logger ?? _logger;
        
        operationLogger?.LogDebug("Starting testing strategy determination {@Metrics}", new { 
            correlationId, 
            method = "DetermineTestingStrategy",
            ruleCount = businessLogic.Count 
        });

        try
        {
            var strategies = new List<string>
            {
                "Unit testing for individual business rules",
                "Integration testing for rule interactions",
                "End-to-end testing for complete workflows"
            };
            
            var strategTypes = new List<string> { "unit", "integration", "e2e" };
            
            if (businessLogic.Any(rule => rule?.ToString()?.Contains("security") == true))
            {
                strategies.Add("Security testing with penetration testing");
                strategTypes.Add("security");
            }
            
            if (businessLogic.Any(rule => rule?.ToString()?.Contains("performance") == true))
            {
                strategies.Add("Performance testing under load");
                strategTypes.Add("performance");
            }
            
            var testingPriority = businessLogic.Count switch
            {
                0 => "low",
                <= 3 => "medium",
                <= 6 => "high",
                _ => "critical"
            };
            
            var estimatedTestCases = businessLogic.Count * 3; // Average 3 test cases per rule
            var estimatedHours = Math.Max(businessLogic.Count * 4, 8); // Minimum 8 hours
            var complexity = businessLogic.Count > 5 ? "high" : businessLogic.Count > 2 ? "medium" : "low";
            
            methodStopwatch.Stop();
            
            var result = new
            {
                recommendedStrategies = strategies.ToArray(),
                strategyTypes = strategTypes.ToArray(),
                testingPriority = testingPriority,
                estimatedEffort = new
                {
                    testCases = estimatedTestCases,
                    estimatedHours = estimatedHours,
                    complexity = complexity
                }
            };
            
            operationLogger?.LogDebug("Testing strategy determination completed {@Metrics}", new { 
                correlationId,
                method = "DetermineTestingStrategy",
                strategiesRecommended = strategies.Count,
                testingPriority,
                estimatedTestCases,
                estimatedHours,
                complexity,
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });
            
            return result;
        }
        catch (Exception ex)
        {
            methodStopwatch.Stop();
            
            operationLogger?.LogWarning(ex, "Testing strategy determination failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                ruleCount = businessLogic.Count,
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });
            
            // Return fallback testing strategy
            return new
            {
                recommendedStrategies = new[]
                {
                    "Unit testing for individual business rules",
                    "Integration testing for rule interactions"
                },
                strategyTypes = new[] { "unit", "integration" },
                testingPriority = "medium",
                estimatedEffort = new
                {
                    testCases = Math.Max(businessLogic.Count * 2, 4),
                    estimatedHours = 8,
                    complexity = "unknown"
                },
                strategyError = true
            };
        }
    }

    /// <summary>
    /// Enhanced method to extract conditional logic and fallback mechanisms from descriptions
    /// </summary>
    /// <param name="description">The description text to analyze</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>List of conditional tests with branch coverage scenarios</returns>
    public static List<ConditionalTest> ExtractConditionalLogic(string description, ILogger? logger = null, string correlationId = "")
    {
        var methodStopwatch = Stopwatch.StartNew();
        var operationLogger = logger ?? _logger;
        
        if (string.IsNullOrEmpty(correlationId))
            correlationId = Guid.NewGuid().ToString("N")[..8];
        
        operationLogger?.LogDebug("Starting conditional logic extraction {@Metrics}", new { 
            correlationId, 
            method = "ExtractConditionalLogic",
            descriptionLength = description?.Length ?? 0 
        });

        var conditionalTests = new List<ConditionalTest>();
        
        if (string.IsNullOrWhiteSpace(description))
        {
            operationLogger?.LogDebug("Empty description provided for conditional logic extraction {@Metrics}", new { 
                correlationId, 
                result = "no_analysis_performed" 
            });
            return conditionalTests;
        }

        try
        {
            var patternStopwatch = Stopwatch.StartNew();
            
            // Fallback patterns
            var fallbackPatterns = new[]
            {
                @"if\s+(.+?)\s+else\s+(.+?)(?:\.|$|,)",
                @"when\s+(.+?)\s+then\s+(.+?)(?:\.|$|,)",
                @"if\s+(.+?)\s+is\s+(?:null|empty|missing)\s*,?\s*(.+?)(?:\.|$)",
                @"fallback\s+to\s+(.+?)(?:\.|$|,)",
                @"use\s+(.+?)\s+when\s+(.+?)(?:\.|$|,)",
                @"apply\s+(.+?)\s+if\s+(.+?)(?:\.|$|,)"
            };

            // Optional feature patterns
            var optionalPatterns = new[]
            {
                @"if\s+present,?\s*(.+?)(?:\.|$|,)",
                @"when\s+available,?\s*(.+?)(?:\.|$|,)",
                @"optional\s+(.+?)(?:\.|$|,)",
                @"if\s+(?:provided|supplied|given),?\s*(.+?)(?:\.|$|,)"
            };

            // Decision flow patterns
            var decisionPatterns = new[]
            {
                @"use\s+(.+?)\s+when\s+(.+?)(?:\.|$|,)",
                @"apply\s+(.+?)\s+if\s+(.+?)(?:\.|$|,)",
                @"switch\s+to\s+(.+?)\s+when\s+(.+?)(?:\.|$|,)",
                @"default\s+to\s+(.+?)(?:\.|$|,)"
            };

            var patternsProcessed = 0;
            var matchesFound = 0;

            // Extract fallback scenarios
            foreach (var pattern in fallbackPatterns)
            {
                var patternStopwatchInner = Stopwatch.StartNew();
                var matches = Regex.Matches(description, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                patternStopwatchInner.Stop();
                
                patternsProcessed++;
                
                // Monitor slow regex performance
                if (patternStopwatchInner.TotalMilliseconds > 100)
                {
                    operationLogger?.LogWarning("Slow regex pattern detected {@PerformanceContext}", new {
                        correlationId,
                        pattern = pattern.Substring(0, Math.Min(50, pattern.Length)),
                        processingTimeMs = patternStopwatchInner.TotalMilliseconds,
                        recommendation = "consider_pattern_optimization"
                    });
                }
                
                foreach (Match match in matches)
                {
                    matchesFound++;
                    if (match.Groups.Count >= 3)
                    {
                        conditionalTests.Add(new ConditionalTest
                        {
                            Condition = match.Groups[1].Value.Trim(),
                            Action = match.Groups[2].Value.Trim(),
                            TestCase = $"Verify {match.Groups[2].Value.Trim()} when {match.Groups[1].Value.Trim()}",
                            Category = TestCategory.ConditionalLogic
                        });

                        // Add negative test case
                        conditionalTests.Add(new ConditionalTest
                        {
                            Condition = $"NOT ({match.Groups[1].Value.Trim()})",
                            Action = "fallback behavior",
                            TestCase = $"Verify fallback behavior when {match.Groups[1].Value.Trim()} is false",
                            Category = TestCategory.EdgeCases
                        });
                    }
                }
            }

            // Extract optional feature scenarios
            foreach (var pattern in optionalPatterns)
            {
                var matches = Regex.Matches(description, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                patternsProcessed++;
                
                foreach (Match match in matches)
                {
                    matchesFound++;
                    var feature = match.Groups[1].Value.Trim();
                    
                    conditionalTests.Add(new ConditionalTest
                    {
                        Condition = "feature is present",
                        Action = feature,
                        TestCase = $"Verify {feature} when feature is available",
                        Category = TestCategory.ConditionalLogic
                    });

                    conditionalTests.Add(new ConditionalTest
                    {
                        Condition = "feature is absent",
                        Action = "graceful degradation",
                        TestCase = $"Verify graceful handling when feature is not available",
                        Category = TestCategory.EdgeCases
                    });
                }
            }

            // Extract decision flow scenarios
            foreach (var pattern in decisionPatterns)
            {
                var matches = Regex.Matches(description, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                patternsProcessed++;
                
                foreach (Match match in matches)
                {
                    matchesFound++;
                    if (match.Groups.Count >= 3)
                    {
                        conditionalTests.Add(new ConditionalTest
                        {
                            Condition = match.Groups[2].Value.Trim(),
                            Action = match.Groups[1].Value.Trim(),
                            TestCase = $"Verify {match.Groups[1].Value.Trim()} applied when {match.Groups[2].Value.Trim()}",
                            Category = TestCategory.ConditionalLogic
                        });
                    }
                }
            }

            patternStopwatch.Stop();

            // Add specific phoneme/SSML conditional tests if detected
            if (description.ToLower().Contains("phoneme") && description.ToLower().Contains("ssml"))
            {
                conditionalTests.Add(new ConditionalTest
                {
                    Condition = "phoneme exists",
                    Action = "apply phoneme pronunciation",
                    TestCase = "Verify phoneme application when phoneme data is available",
                    Category = TestCategory.DataValidation
                });

                conditionalTests.Add(new ConditionalTest
                {
                    Condition = "phoneme is null or empty",
                    Action = "use SSML fallback",
                    TestCase = "Verify SSML fallback when phoneme data is missing",
                    Category = TestCategory.ConditionalLogic
                });
                
                operationLogger?.LogDebug("Phoneme/SSML conditional logic detected {@SpecialCase}", new {
                    correlationId,
                    specialCaseType = "phoneme_ssml",
                    testsAdded = 2
                });
            }

            // Add IVR variant conditional tests if detected
            if (description.ToLower().Contains("ivr") && (description.ToLower().Contains("variant") || description.ToLower().Contains("type")))
            {
                var ivrTypes = ExtractIVRTypes(description);
                foreach (var ivrType in ivrTypes)
                {
                    conditionalTests.Add(new ConditionalTest
                    {
                        Condition = $"IVR type is {ivrType}",
                        Action = $"apply {ivrType} specific behavior",
                        TestCase = $"Verify {ivrType} specific behavior when IVR type is {ivrType}",
                        Category = TestCategory.ScenarioExpansion
                    });
                }
                
                operationLogger?.LogDebug("IVR variant conditional logic detected {@SpecialCase}", new {
                    correlationId,
                    specialCaseType = "ivr_variants",
                    ivrTypes = ivrTypes.ToArray(),
                    testsAdded = ivrTypes.Count
                });
            }

            methodStopwatch.Stop();

            operationLogger?.LogInformation("Conditional logic extraction completed {@Metrics}", new { 
                correlationId,
                method = "ExtractConditionalLogic",
                conditionalTestsFound = conditionalTests.Count,
                patternsProcessed,
                matchesFound,
                regexProcessingTimeMs = patternStopwatch.TotalMilliseconds,
                totalProcessingTimeMs = methodStopwatch.TotalMilliseconds
            });

            return conditionalTests;
        }
        catch (Exception ex)
        {
            methodStopwatch.Stop();
            
            operationLogger?.LogError(ex, "Conditional logic extraction failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                partialTestsFound = conditionalTests.Count,
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });
            
            return conditionalTests; // Return partial results
        }
    }

    /// <summary>
    /// Analyzes decision tree complexity and generates branch coverage tests
    /// </summary>
    /// <param name="description">The description to analyze</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>Decision tree analysis with branch coverage recommendations</returns>
    public static object AnalyzeDecisionTrees(string description, ILogger? logger = null, string correlationId = "")
    {
        var methodStopwatch = Stopwatch.StartNew();
        var operationLogger = logger ?? _logger;
        
        if (string.IsNullOrEmpty(correlationId))
            correlationId = Guid.NewGuid().ToString("N")[..8];
        
        operationLogger?.LogDebug("Starting decision tree analysis {@Metrics}", new { 
            correlationId, 
            method = "AnalyzeDecisionTrees",
            descriptionLength = description?.Length ?? 0 
        });

        try
        {
            var conditionalTests = ExtractConditionalLogic(description, operationLogger, correlationId);
            var decisionPaths = new List<object>();
            var branchCoverage = new List<object>();

            // Group by condition types
            var groupingStopwatch = Stopwatch.StartNew();
            var conditionGroups = conditionalTests.GroupBy(t => ExtractConditionType(t.Condition));
            groupingStopwatch.Stop();

            operationLogger?.LogDebug("Condition grouping completed {@Metrics}", new { 
                correlationId,
                groupsFound = conditionGroups.Count(),
                totalTests = conditionalTests.Count,
                groupingTimeMs = groupingStopwatch.TotalMilliseconds
            });

            foreach (var group in conditionGroups)
            {
                var conditionType = group.Key;
                var tests = group.ToList();

                decisionPaths.Add(new
                {
                    conditionType = conditionType,
                    pathCount = tests.Count,
                    conditions = tests.Select(t => t.Condition).ToArray(),
                    actions = tests.Select(t => t.Action).ToArray(),
                    testCases = tests.Select(t => t.TestCase).ToArray()
                });

                // Generate branch coverage scenarios
                branchCoverage.Add(new
                {
                    branchType = conditionType,
                    coverage = new
                    {
                        totalBranches = tests.Count,
                        testedBranches = tests.Count, // All branches should be tested
                        coveragePercentage = 100.0,
                        missingBranches = new string[0] // No missing branches in generated tests
                    },
                    testScenarios = tests.Select(t => new
                    {
                        scenario = t.TestCase,
                        condition = t.Condition,
                        expectedAction = t.Action,
                        category = t.Category.ToString(),
                        priority = DeterminePriority(t)
                    }).ToArray()
                });
                
                operationLogger?.LogDebug("Branch coverage analyzed for condition type {@BranchAnalysis}", new {
                    correlationId,
                    conditionType,
                    branchCount = tests.Count,
                    coveragePercentage = 100.0
                });
            }

            var complexity = DetermineDecisionComplexity(conditionalTests.Count);
            var minimumTestCases = conditionalTests.Count;
            var recommendedTestCases = conditionalTests.Count * 2; // Include edge cases
            var automationFeasibility = conditionalTests.Count < 20 ? "High" : "Medium";

            methodStopwatch.Stop();

            var result = new
            {
                decisionTreeAnalysis = new
                {
                    totalDecisionPoints = conditionalTests.Count,
                    decisionPaths = decisionPaths.ToArray(),
                    complexity = complexity,
                    branchCoverageAnalysis = branchCoverage.ToArray()
                },
                testingRecommendations = new
                {
                    minimumTestCases = minimumTestCases,
                    recommendedTestCases = recommendedTestCases,
                    testingApproach = "Branch coverage with edge case validation",
                    automationFeasibility = automationFeasibility
                },
                conditionalLogicTests = conditionalTests.Select(t => new
                {
                    condition = t.Condition,
                    action = t.Action,
                    testCase = t.TestCase,
                    category = t.Category.ToString(),
                    priority = DeterminePriority(t)
                }).ToArray()
            };

            operationLogger?.LogInformation("Decision tree analysis completed {@Metrics}", new { 
                correlationId,
                method = "AnalyzeDecisionTrees",
                totalDecisionPoints = conditionalTests.Count,
                complexity,
                conditionGroups = conditionGroups.Count(),
                minimumTestCases,
                recommendedTestCases,
                automationFeasibility,
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });

            return result;
        }
        catch (Exception ex)
        {
            methodStopwatch.Stop();
            
            operationLogger?.LogError(ex, "Decision tree analysis failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                processingTimeMs = methodStopwatch.TotalMilliseconds
            });

            // Return fallback analysis
            return new
            {
                decisionTreeAnalysis = new
                {
                    totalDecisionPoints = 0,
                    decisionPaths = new object[0],
                    complexity = "unknown",
                    branchCoverageAnalysis = new object[0]
                },
                testingRecommendations = new
                {
                    minimumTestCases = 0,
                    recommendedTestCases = 2,
                    testingApproach = "Basic validation testing",
                    automationFeasibility = "Unknown"
                },
                conditionalLogicTests = new object[0],
                analysisError = true
            };
        }
    }

    #region Helper Methods for Conditional Logic Analysis

    private static List<string> ExtractIVRTypes(string description)
    {
        var ivrTypes = new List<string>();
        var ivrPatterns = new[]
        {
            @"no[\-\s]?pin\s+ivr",
            @"athena\s+ivr",
            @"availity\s+ivr",
            @"(\w+)\s+ivr\s+(?:type|variant)",
            @"ivr\s+(?:type|variant)\s+(\w+)"
        };

        foreach (var pattern in ivrPatterns)
        {
            var matches = Regex.Matches(description, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                if (match.Value.ToLower().Contains("no-pin") || match.Value.ToLower().Contains("no pin"))
                    ivrTypes.Add("no-pin");
                else if (match.Value.ToLower().Contains("athena"))
                    ivrTypes.Add("athena");
                else if (match.Value.ToLower().Contains("availity"))
                    ivrTypes.Add("availity");
                else if (match.Groups.Count > 1 && !string.IsNullOrWhiteSpace(match.Groups[1].Value))
                    ivrTypes.Add(match.Groups[1].Value.Trim());
            }
        }

        return ivrTypes.Distinct().ToList();
    }

    private static string ExtractConditionType(string condition)
    {
        var lowerCondition = condition.ToLower();
        
        if (lowerCondition.Contains("null") || lowerCondition.Contains("empty") || lowerCondition.Contains("missing"))
            return "Null_Check";
        
        if (lowerCondition.Contains("present") || lowerCondition.Contains("available") || lowerCondition.Contains("exists"))
            return "Presence_Check";
        
        if (lowerCondition.Contains("type") || lowerCondition.Contains("variant"))
            return "Type_Check";
        
        if (lowerCondition.Contains("phoneme") || lowerCondition.Contains("ssml"))
            return "Audio_Condition";
        
        if (lowerCondition.Contains("ivr") || lowerCondition.Contains("flow"))
            return "IVR_Condition";
        
        return "Generic_Condition";
    }

    private static string DeterminePriority(ConditionalTest test)
    {
        if (test.Category == TestCategory.ConditionalLogic)
            return "High";
        
        if (test.Category == TestCategory.DataValidation)
            return "High";
        
        if (test.Category == TestCategory.EdgeCases)
            return "Medium";
        
        return "Medium";
    }

    private static string DetermineDecisionComplexity(int decisionCount)
    {
        return decisionCount switch
        {
            0 => "None",
            1 => "Simple",
            <= 3 => "Low",
            <= 6 => "Medium",
            <= 10 => "High",
            _ => "Very High"
        };
    }

    #endregion
}
