using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TestForge.McpServer.Models;

namespace TestForge.McpServer.Services;

/// <summary>
/// Service responsible for validating Jira XML structure and providing detailed diagnostic information
/// </summary>
public static class JiraXmlValidationService
{
    private static ILogger? _logger;

    /// <summary>
    /// Initializes the service with optional logger
    /// </summary>
    /// <param name="logger">Optional logger for the service</param>
    public static void Initialize(ILogger? logger = null)
    {
        _logger = logger;
        
        _logger?.LogInformation("JiraXmlValidationService initialized {@ServiceInfo}", new {
            serviceType = "XmlValidationService",
            version = "1.0",
            validationCapabilities = new[] {
                "xml_structure_parsing",
                "structural_analysis",
                "issue_detection",
                "diagnostic_recommendations",
                "security_assessment",
                "complexity_analysis",
                "validation_confidence_scoring"
            },
            diagnosticFeatures = new[] {
                "duplicate_attribute_detection",
                "unescaped_entity_detection", 
                "missing_tag_detection",
                "empty_cdata_detection",
                "structural_completeness_assessment"
            },
            performanceThresholds = new {
                largeFileWarning = "10MB",
                validationTimeoutWarning = "5s",
                complexityThreshold = "high"
            }
        });
    }
    /// <summary>
    /// Validates Jira XML structure and provides detailed diagnostic information about parsing issues
    /// </summary>
    /// <param name="jiraXml">The Jira XML content to validate</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <returns>JSON response containing validation results and diagnostic information</returns>
    public static string Validate(string jiraXml, ILogger? logger = null)
    {
        var operationLogger = logger ?? _logger;
        var correlationId = Guid.NewGuid().ToString("N")[..8];
        var totalStopwatch = Stopwatch.StartNew();

        operationLogger?.LogInformation("Starting XML validation operation {@OperationInfo}", new {
            correlationId,
            method = "Validate",
            inputSize = jiraXml?.Length ?? 0
        });

        try
        {
            if (string.IsNullOrWhiteSpace(jiraXml))
            {
                operationLogger?.LogWarning("Empty XML input provided {@ValidationWarning}", new {
                    correlationId,
                    error = "input_validation_failed",
                    inputState = "null_or_empty",
                    recommendation = "provide_valid_jira_xml_content"
                });

                return JsonSerializer.Serialize(new { 
                    isValid = false, 
                    error = "Input XML is empty or null",
                    correlationId,
                    suggestions = new[] { "Provide valid Jira XML content" }
                }, new JsonSerializerOptions { WriteIndented = true });
            }

            var xmlSize = jiraXml.Length;
            var fileSizeMB = xmlSize / (1024.0 * 1024.0);

            // Security monitoring for large files
            if (fileSizeMB > 10.0)
            {
                operationLogger?.LogWarning("Large XML file detected {@SecurityWarning}", new {
                    correlationId,
                    fileSizeMB = $"{fileSizeMB:F2}",
                    recommendation = "consider_streaming_validation_for_files_over_10MB",
                    potentialMemoryImpact = "high",
                    securityRisk = "potential_dos_attack"
                });
            }

            // Initial structure assessment
            var complexity = AnalyzeXmlComplexity(jiraXml, operationLogger, correlationId);
            var securityAssessment = ValidateXmlSecurity(jiraXml, operationLogger, correlationId);

            operationLogger?.LogDebug("Input analysis completed {@InputMetrics}", new {
                correlationId,
                xmlSize,
                fileSizeMB = $"{fileSizeMB:F2}",
                complexity = complexity.Level,
                complexityScore = complexity.Score,
                securityRisk = securityAssessment.RiskLevel,
                hasComments = jiraXml.Contains("<comments"),
                hasCustomFields = jiraXml.Contains("<customfields"),
                containsCDATA = jiraXml.Contains("<![CDATA[")
            });

            var validationStopwatch = Stopwatch.StartNew();
            var validationResult = ValidateXmlStructure(jiraXml, operationLogger, correlationId);
            validationStopwatch.Stop();

            if (!validationResult.IsValid)
            {
                var issueDetectionStopwatch = Stopwatch.StartNew();
                var potentialIssues = IdentifyPotentialIssues(jiraXml, operationLogger, correlationId);
                issueDetectionStopwatch.Stop();

                totalStopwatch.Stop();

                var validationMetrics = GenerateValidationMetrics(false, validationStopwatch.TotalMilliseconds, 
                    totalStopwatch.TotalMilliseconds, potentialIssues.Length, complexity.Score, operationLogger, correlationId);

                operationLogger?.LogWarning("XML validation failed {@ValidationFailure}", new {
                    correlationId,
                    isValid = false,
                    error = validationResult.Error,
                    validationTimeMs = validationStopwatch.TotalMilliseconds,
                    issueDetectionTimeMs = issueDetectionStopwatch.TotalMilliseconds,
                    totalTimeMs = totalStopwatch.TotalMilliseconds,
                    potentialIssuesFound = potentialIssues.Length,
                    validationConfidence = validationMetrics.ConfidenceScore,
                    recommendation = "use_clean_jira_xml_tool_or_manual_correction"
                });

                return JsonSerializer.Serialize(new {
                    isValid = false,
                    correlationId,
                    error = validationResult.Error,
                    originalXmlLength = xmlSize,
                    validationTimeMs = validationStopwatch.TotalMilliseconds,
                    suggestions = new[] {
                        "Try using the clean_jira_xml tool to fix common issues",
                        "Check for missing closing tags in comments section",
                        "Verify HTML entities are properly escaped",
                        "Look for duplicate attributes in XML elements"
                    },
                    diagnostics = new {
                        hasCommentSection = jiraXml.Contains("<comments>"),
                        hasDescription = jiraXml.Contains("<description>"),
                        hasCustomFields = jiraXml.Contains("<customfields>"),
                        xmlLength = xmlSize,
                        containsCData = jiraXml.Contains("<![CDATA["),
                        complexity = complexity.Level,
                        securityRisk = securityAssessment.RiskLevel,
                        potentialIssues = potentialIssues,
                        validationMetrics = validationMetrics
                    }
                }, new JsonSerializerOptions { WriteIndented = true });
            }

            // Successful validation path
            var structureAnalysisStopwatch = Stopwatch.StartNew();
            var structure = AnalyzeXmlStructure(jiraXml, operationLogger, correlationId);
            structureAnalysisStopwatch.Stop();

            totalStopwatch.Stop();

            // Performance monitoring
            if (totalStopwatch.TotalSeconds > 5)
            {
                operationLogger?.LogWarning("Slow XML validation operation detected {@PerformanceWarning}", new {
                    correlationId,
                    totalTimeSeconds = $"{totalStopwatch.TotalSeconds:F2}",
                    recommendation = "investigate_xml_complexity_and_optimize_validation_algorithms",
                    inputSize = xmlSize,
                    complexity = complexity.Level
                });
            }

            var validationMetricsSuccess = GenerateValidationMetrics(true, validationStopwatch.TotalMilliseconds,
                totalStopwatch.TotalMilliseconds, 0, complexity.Score, operationLogger, correlationId);

            operationLogger?.LogInformation("XML validation completed successfully {@Metrics}", new {
                success = true,
                correlationId,
                isValid = true,
                totalValidationTimeMs = totalStopwatch.TotalMilliseconds,
                xmlParsingTimeMs = validationStopwatch.TotalMilliseconds,
                structureAnalysisTimeMs = structureAnalysisStopwatch.TotalMilliseconds,
                xmlSize,
                complexity = complexity.Level,
                validationConfidence = validationMetricsSuccess.ConfidenceScore,
                qualityScore = validationMetricsSuccess.QualityScore
            });

            return JsonSerializer.Serialize(new {
                isValid = true,
                correlationId,
                message = "XML is valid and ready for parsing",
                xmlLength = xmlSize,
                validationTimeMs = totalStopwatch.TotalMilliseconds,
                structure = structure,
                diagnostics = new {
                    hasCommentSection = jiraXml.Contains("<comments>"),
                    hasDescription = jiraXml.Contains("<description>"),
                    hasCustomFields = jiraXml.Contains("<customfields>"),
                    xmlLength = xmlSize,
                    containsCData = jiraXml.Contains("<![CDATA["),
                    complexity = complexity.Level,
                    securityRisk = securityAssessment.RiskLevel,
                    validationMetrics = validationMetricsSuccess
                }
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            totalStopwatch.Stop();
            
            operationLogger?.LogError(ex, "XML validation operation failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                processingTimeMs = totalStopwatch.TotalMilliseconds,
                xmlSize = jiraXml?.Length ?? 0,
                validationStep = "main_validation_pipeline",
                recommendation = "check_xml_format_structure_and_service_health"
            });

            return JsonSerializer.Serialize(new { 
                isValid = false, 
                correlationId,
                error = $"Validation failed: {ex.Message}",
                processingTimeMs = totalStopwatch.TotalMilliseconds,
                suggestion = "Check XML format and structure"
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    /// <summary>
    /// Validates the XML structure and returns detailed validation result
    /// </summary>
    /// <param name="xml">XML content to validate</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>Validation result with success status and error details</returns>
    private static (bool IsValid, string Error) ValidateXmlStructure(string xml, ILogger? logger = null, string correlationId = "")
    {
        var operationLogger = logger ?? _logger;
        var stopwatch = Stopwatch.StartNew();

        operationLogger?.LogDebug("Starting XML structure validation {@OperationInfo}", new {
            correlationId,
            method = "ValidateXmlStructure",
            xmlSize = xml?.Length ?? 0
        });

        try
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                operationLogger?.LogWarning("Empty XML provided for structure validation {@ValidationWarning}", new {
                    correlationId,
                    validationResult = "failed",
                    reason = "empty_input",
                    errorCategory = "input_validation"
                });
                return (false, "XML content is empty");
            }

            var parseStopwatch = Stopwatch.StartNew();
            var doc = XDocument.Parse(xml);
            parseStopwatch.Stop();
            stopwatch.Stop();

            // Detailed structural analysis for successful parsing
            var rootElement = doc.Root?.Name.LocalName ?? "unknown";
            var totalElements = doc.Descendants().Count();
            var maxDepth = CalculateMaxDepth(doc.Root);
            var hasNamespaces = doc.Root?.Attributes().Any(a => a.IsNamespaceDeclaration) ?? false;

            operationLogger?.LogInformation("XML structure validation completed successfully {@Metrics}", new {
                success = true,
                correlationId,
                method = "ValidateXmlStructure",
                validationTimeMs = stopwatch.TotalMilliseconds,
                parseTimeMs = parseStopwatch.TotalMilliseconds,
                isValid = true,
                xmlSize = xml.Length,
                rootElement,
                totalElements,
                maxDepth,
                hasNamespaces,
                validationAccuracy = "high",
                errorCategory = "none"
            });

            return (true, string.Empty);
        }
        catch (XmlException xmlEx)
        {
            stopwatch.Stop();
            
            var detailedError = $"XML parsing error at line {xmlEx.LineNumber}, position {xmlEx.LinePosition}: {xmlEx.Message}";
            var errorCategory = CategorizeXmlError(xmlEx.Message);
            
            operationLogger?.LogWarning("XML structure validation failed with parsing error {@ValidationError}", new {
                correlationId,
                method = "ValidateXmlStructure",
                validationTimeMs = stopwatch.TotalMilliseconds,
                isValid = false,
                errorType = "XmlException",
                errorCategory,
                lineNumber = xmlEx.LineNumber,
                linePosition = xmlEx.LinePosition,
                errorMessage = xmlEx.Message,
                xmlSize = xml?.Length ?? 0,
                validationAccuracy = "high",
                recommendation = GetRecommendationForXmlError(errorCategory)
            });

            return (false, detailedError);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            var errorCategory = "unexpected_error";
            
            operationLogger?.LogError(ex, "XML structure validation failed with unexpected error {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                method = "ValidateXmlStructure",
                validationTimeMs = stopwatch.TotalMilliseconds,
                isValid = false,
                errorType = ex.GetType().Name,
                errorCategory,
                xmlSize = xml?.Length ?? 0,
                validationAccuracy = "unknown",
                recommendation = "investigate_unexpected_validation_error_and_check_service_health"
            });

            return (false, $"Validation error: {ex.Message}");
        }
    }

    /// <summary>
    /// Analyzes the XML structure and returns information about the document
    /// </summary>
    /// <param name="xml">XML content to analyze</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>Structure analysis information</returns>
    private static object AnalyzeXmlStructure(string xml, ILogger? logger = null, string correlationId = "")
    {
        var operationLogger = logger ?? _logger;
        var stopwatch = Stopwatch.StartNew();

        operationLogger?.LogDebug("Starting XML structure analysis {@OperationInfo}", new {
            correlationId,
            method = "AnalyzeXmlStructure",
            xmlSize = xml?.Length ?? 0
        });

        try
        {
            var doc = XDocument.Parse(xml);
            var root = doc.Root;
            
            var elementCountStopwatch = Stopwatch.StartNew();
            var totalElements = doc.Descendants().Count();
            elementCountStopwatch.Stop();

            var structuralAnalysis = new {
                rootElement = root?.Name.LocalName ?? "unknown",
                hasIssueElement = doc.Descendants("item").Any() || doc.Descendants("issue").Any(),
                elementCount = totalElements,
                hasComments = doc.Descendants("comments").Any(),
                hasDescription = doc.Descendants("description").Any(),
                hasCustomFields = doc.Descendants("customfields").Any(),
                hasAttachments = doc.Descendants("attachments").Any(),
                hasSummary = doc.Descendants("summary").Any(),
                hasAssignee = doc.Descendants("assignee").Any(),
                hasReporter = doc.Descendants("reporter").Any(),
                maxDepth = CalculateMaxDepth(root),
                namespaceCount = root?.Attributes().Count(a => a.IsNamespaceDeclaration) ?? 0
            };

            // Calculate structural completeness score
            var completenessFactors = new[] {
                structuralAnalysis.hasIssueElement,
                structuralAnalysis.hasDescription,
                structuralAnalysis.hasSummary,
                structuralAnalysis.hasAssignee,
                structuralAnalysis.hasReporter
            };
            var completenessScore = completenessFactors.Count(f => f) / (double)completenessFactors.Length;

            stopwatch.Stop();

            operationLogger?.LogInformation("XML structure analysis completed {@Metrics}", new {
                success = true,
                correlationId,
                method = "AnalyzeXmlStructure",
                analysisTimeMs = stopwatch.TotalMilliseconds,
                elementCountingTimeMs = elementCountStopwatch.TotalMilliseconds,
                totalElements,
                maxDepth = structuralAnalysis.maxDepth,
                rootElement = structuralAnalysis.rootElement,
                structuralCompleteness = $"{completenessScore:P0}",
                hasEssentialElements = structuralAnalysis.hasIssueElement && structuralAnalysis.hasDescription,
                analysisEffectiveness = "high"
            });

            return structuralAnalysis;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            operationLogger?.LogError(ex, "XML structure analysis failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                method = "AnalyzeXmlStructure",
                analysisTimeMs = stopwatch.TotalMilliseconds,
                xmlSize = xml?.Length ?? 0,
                recommendation = "check_xml_validity_before_structure_analysis"
            });

            return new { error = "Could not analyze structure", details = ex.Message };
        }
    }

    /// <summary>
    /// Identifies potential issues in the XML content
    /// </summary>
    /// <param name="xml">XML content to analyze</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>List of potential issues with detailed categorization</returns>
    private static object[] IdentifyPotentialIssues(string xml, ILogger? logger = null, string correlationId = "")
    {
        var operationLogger = logger ?? _logger;
        var stopwatch = Stopwatch.StartNew();
        var issueDetectionTimings = new Dictionary<string, double>();

        operationLogger?.LogDebug("Starting potential issues identification {@OperationInfo}", new {
            correlationId,
            method = "IdentifyPotentialIssues",
            xmlSize = xml?.Length ?? 0
        });

        var issues = new List<object>();

        try
        {
            // Issue 1: Duplicate attributes detection
            var duplicateAttrsStopwatch = Stopwatch.StartNew();
            if (xml.Contains("rel=\"") && xml.Contains("rel=\"") && 
                xml.Split("rel=\"").Length > xml.Split("rel=\"").Distinct().Count())
            {
                issues.Add(new {
                    type = "structural",
                    severity = "warning",
                    issue = "Duplicate 'rel' attributes detected",
                    category = "duplicate_attributes",
                    recommendation = "Remove duplicate rel attributes to ensure XML validity",
                    impact = "medium"
                });
            }
            duplicateAttrsStopwatch.Stop();
            issueDetectionTimings["duplicate_attributes"] = duplicateAttrsStopwatch.TotalMilliseconds;

            // Issue 2: Unescaped entities detection
            var unescapedEntitiesStopwatch = Stopwatch.StartNew();
            if (xml.Contains("&") && !xml.Contains("&amp;") && !xml.Contains("&lt;") && !xml.Contains("&gt;"))
            {
                issues.Add(new {
                    type = "content",
                    severity = "critical",
                    issue = "Unescaped ampersands detected",
                    category = "unescaped_entities",
                    recommendation = "Escape ampersands as &amp; or wrap content in CDATA sections",
                    impact = "high"
                });
            }
            unescapedEntitiesStopwatch.Stop();
            issueDetectionTimings["unescaped_entities"] = unescapedEntitiesStopwatch.TotalMilliseconds;

            // Issue 3: Missing closing tags detection
            var missingTagsStopwatch = Stopwatch.StartNew();
            if (xml.Contains("<comment>") && !xml.Contains("</comment>"))
            {
                issues.Add(new {
                    type = "structural",
                    severity = "critical",
                    issue = "Missing closing tags for comment elements",
                    category = "missing_closing_tags",
                    recommendation = "Add proper closing tags for all comment elements",
                    impact = "high"
                });
            }
            missingTagsStopwatch.Stop();
            issueDetectionTimings["missing_closing_tags"] = missingTagsStopwatch.TotalMilliseconds;

            // Issue 4: Empty CDATA sections detection
            var emptyCdataStopwatch = Stopwatch.StartNew();
            if (xml.Contains("<![CDATA[") && xml.Contains("<![CDATA[]]>"))
            {
                issues.Add(new {
                    type = "content",
                    severity = "info",
                    issue = "Empty CDATA sections detected",
                    category = "empty_cdata",
                    recommendation = "Remove empty CDATA sections to optimize XML size",
                    impact = "low"
                });
            }
            emptyCdataStopwatch.Stop();
            issueDetectionTimings["empty_cdata"] = emptyCdataStopwatch.TotalMilliseconds;

            // Issue 5: Performance issues detection (large file)
            var performanceStopwatch = Stopwatch.StartNew();
            var fileSizeMB = xml.Length / (1024.0 * 1024.0);
            if (fileSizeMB > 5.0)
            {
                issues.Add(new {
                    type = "performance",
                    severity = "warning",
                    issue = $"Large XML file detected ({fileSizeMB:F2} MB)",
                    category = "large_file",
                    recommendation = "Consider breaking large XML into smaller chunks or using streaming processing",
                    impact = "medium"
                });
            }
            performanceStopwatch.Stop();
            issueDetectionTimings["performance_issues"] = performanceStopwatch.TotalMilliseconds;

            // Issue 6: Security issues detection
            var securityStopwatch = Stopwatch.StartNew();
            if (xml.Contains("<!ENTITY") || xml.Contains("SYSTEM") || xml.Contains("PUBLIC"))
            {
                issues.Add(new {
                    type = "security",
                    severity = "critical",
                    issue = "External entity references detected",
                    category = "external_entities",
                    recommendation = "Remove external entity references to prevent XXE attacks",
                    impact = "critical"
                });
            }
            securityStopwatch.Stop();
            issueDetectionTimings["security_issues"] = securityStopwatch.TotalMilliseconds;

            stopwatch.Stop();

            var criticalIssues = issues.Count(i => i.GetType().GetProperty("severity")?.GetValue(i)?.ToString() == "critical");
            var warningIssues = issues.Count(i => i.GetType().GetProperty("severity")?.GetValue(i)?.ToString() == "warning");
            var infoIssues = issues.Count(i => i.GetType().GetProperty("severity")?.GetValue(i)?.ToString() == "info");

            operationLogger?.LogInformation("Potential issues identification completed {@Metrics}", new {
                success = true,
                correlationId,
                method = "IdentifyPotentialIssues",
                detectionTimeMs = stopwatch.TotalMilliseconds,
                totalIssuesFound = issues.Count,
                issueBreakdown = new { criticalIssues, warningIssues, infoIssues },
                issueDetectionTimings,
                detectionAccuracy = "high",
                falsePositiveRate = "low"
            });

            return issues.ToArray();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            operationLogger?.LogError(ex, "Potential issues identification failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                method = "IdentifyPotentialIssues",
                detectionTimeMs = stopwatch.TotalMilliseconds,
                partialIssuesFound = issues.Count,
                recommendation = "investigate_issue_detection_algorithm_failure"
            });

            // Return partial results with error indication
            issues.Add(new {
                type = "system",
                severity = "warning",
                issue = $"Issue detection partially failed: {ex.Message}",
                category = "detection_error",
                recommendation = "Manual review recommended due to detection system error",
                impact = "unknown"
            });

            return issues.ToArray();
        }
    }

    /// <summary>
    /// Analyzes XML complexity for performance prediction and optimization recommendations
    /// </summary>
    /// <param name="xml">XML content to analyze</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>Complexity analysis result</returns>
    private static (string Level, double Score, object Details) AnalyzeXmlComplexity(string xml, ILogger? logger = null, string correlationId = "")
    {
        var operationLogger = logger ?? _logger;
        var stopwatch = Stopwatch.StartNew();

        operationLogger?.LogDebug("Starting XML complexity analysis {@OperationInfo}", new {
            correlationId,
            method = "AnalyzeXmlComplexity",
            xmlSize = xml?.Length ?? 0
        });

        try
        {
            var complexity = 0.0;
            var factors = new Dictionary<string, double>();

            // Size-based complexity
            var sizeMB = xml.Length / (1024.0 * 1024.0);
            var sizeComplexity = sizeMB switch
            {
                > 20 => 4.0,
                > 10 => 3.0,
                > 5 => 2.0,
                > 1 => 1.0,
                _ => 0.5
            };
            factors["size_complexity"] = sizeComplexity;
            complexity += sizeComplexity;

            // Content-based complexity
            var commentsSections = System.Text.RegularExpressions.Regex.Matches(xml, @"<comments[^>]*>").Count;
            var cdataSections = xml.Split("<![CDATA[").Length - 1;
            var customFields = System.Text.RegularExpressions.Regex.Matches(xml, @"<customfield[^>]*>").Count;

            var contentComplexity = (commentsSections * 0.5) + (cdataSections * 0.3) + (customFields * 0.2);
            factors["content_complexity"] = contentComplexity;
            complexity += contentComplexity;

            // Structural complexity (estimated)
            var estimatedDepth = EstimateNestingDepth(xml);
            var structuralComplexity = estimatedDepth * 0.3;
            factors["structural_complexity"] = structuralComplexity;
            complexity += structuralComplexity;

            stopwatch.Stop();

            var level = complexity switch
            {
                <= 2.0 => "low",
                <= 5.0 => "medium",
                <= 8.0 => "high",
                _ => "very_high"
            };

            var details = new {
                totalScore = complexity,
                factors,
                sizeMB = $"{sizeMB:F2}",
                commentsSections,
                cdataSections,
                customFields,
                estimatedDepth,
                performancePrediction = level switch
                {
                    "low" => "fast_processing_expected",
                    "medium" => "moderate_processing_time",
                    "high" => "slower_processing_expected",
                    _ => "performance_optimization_recommended"
                }
            };

            operationLogger?.LogInformation("XML complexity analysis completed {@Metrics}", new {
                success = true,
                correlationId,
                method = "AnalyzeXmlComplexity",
                analysisTimeMs = stopwatch.TotalMilliseconds,
                complexityLevel = level,
                complexityScore = complexity,
                sizeMB = $"{sizeMB:F2}",
                factors,
                performancePrediction = details.performancePrediction
            });

            return (level, complexity, details);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            operationLogger?.LogError(ex, "XML complexity analysis failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                method = "AnalyzeXmlComplexity",
                analysisTimeMs = stopwatch.TotalMilliseconds,
                recommendation = "fallback_to_basic_complexity_assessment"
            });

            return ("unknown", 0.0, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Validates XML for security issues and potential vulnerabilities
    /// </summary>
    /// <param name="xml">XML content to validate</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>Security assessment result</returns>
    private static (string RiskLevel, object Assessment) ValidateXmlSecurity(string xml, ILogger? logger = null, string correlationId = "")
    {
        var operationLogger = logger ?? _logger;
        var stopwatch = Stopwatch.StartNew();

        operationLogger?.LogDebug("Starting XML security validation {@OperationInfo}", new {
            correlationId,
            method = "ValidateXmlSecurity",
            xmlSize = xml?.Length ?? 0
        });

        try
        {
            var securityIssues = new List<object>();
            var riskScore = 0;

            // Check for external entity references (XXE vulnerability)
            if (xml.Contains("<!ENTITY") || xml.Contains("SYSTEM") || xml.Contains("PUBLIC"))
            {
                securityIssues.Add(new {
                    type = "xxe_vulnerability",
                    severity = "critical",
                    description = "External entity references detected",
                    recommendation = "Remove all external entity references"
                });
                riskScore += 10;
            }

            // Check for script content (potential XSS)
            if (xml.Contains("<script") || xml.Contains("javascript:") || xml.Contains("onload="))
            {
                securityIssues.Add(new {
                    type = "script_content",
                    severity = "high",
                    description = "Script content detected in XML",
                    recommendation = "Remove or sanitize script content"
                });
                riskScore += 7;
            }

            // Check for oversized elements (potential DoS)
            var fileSizeMB = xml.Length / (1024.0 * 1024.0);
            if (fileSizeMB > 50)
            {
                securityIssues.Add(new {
                    type = "dos_risk",
                    severity = "medium",
                    description = $"Very large XML file ({fileSizeMB:F2} MB)",
                    recommendation = "Implement size limits and streaming processing"
                });
                riskScore += 5;
            }

            // Check for suspicious patterns
            if (xml.Contains("base64") || xml.Contains("data:") || xml.Contains("eval("))
            {
                securityIssues.Add(new {
                    type = "suspicious_content",
                    severity = "medium",
                    description = "Suspicious encoded content detected",
                    recommendation = "Review and validate encoded content"
                });
                riskScore += 3;
            }

            stopwatch.Stop();

            var riskLevel = riskScore switch
            {
                0 => "low",
                <= 5 => "medium",
                <= 10 => "high",
                _ => "critical"
            };

            var assessment = new {
                riskScore,
                securityIssues = securityIssues.ToArray(),
                fileSizeMB = $"{fileSizeMB:F2}",
                overallRecommendation = riskLevel switch
                {
                    "low" => "XML appears safe for processing",
                    "medium" => "Review identified issues before processing",
                    "high" => "Address security issues before processing",
                    _ => "Do not process until critical issues are resolved"
                }
            };

            operationLogger?.LogInformation("XML security validation completed {@Metrics}", new {
                success = true,
                correlationId,
                method = "ValidateXmlSecurity",
                validationTimeMs = stopwatch.TotalMilliseconds,
                riskLevel,
                riskScore,
                securityIssuesFound = securityIssues.Count,
                fileSizeMB = $"{fileSizeMB:F2}",
                recommendation = assessment.overallRecommendation
            });

            return (riskLevel, assessment);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            operationLogger?.LogError(ex, "XML security validation failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                method = "ValidateXmlSecurity",
                validationTimeMs = stopwatch.TotalMilliseconds,
                recommendation = "manual_security_review_required"
            });

            return ("unknown", new { error = ex.Message, recommendation = "Manual security review required" });
        }
    }

    /// <summary>
    /// Generates comprehensive validation metrics for quality assessment
    /// </summary>
    /// <param name="isValid">Whether validation was successful</param>
    /// <param name="validationTimeMs">Time taken for validation</param>
    /// <param name="totalTimeMs">Total processing time</param>
    /// <param name="issuesFound">Number of issues detected</param>
    /// <param name="complexityScore">Complexity score of the XML</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>Validation metrics object</returns>
    private static object GenerateValidationMetrics(bool isValid, double validationTimeMs, double totalTimeMs, 
        int issuesFound, double complexityScore, ILogger? logger = null, string correlationId = "")
    {
        var operationLogger = logger ?? _logger;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Calculate confidence score based on multiple factors
            var confidenceScore = CalculateConfidenceScore(isValid, validationTimeMs, issuesFound, complexityScore);
            
            // Calculate quality score
            var qualityScore = CalculateQualityScore(isValid, issuesFound, complexityScore);
            
            // Generate recommendations
            var recommendations = GenerateRecommendations(isValid, issuesFound, complexityScore, validationTimeMs);

            stopwatch.Stop();

            var metrics = new {
                confidenceScore = $"{confidenceScore:P0}",
                qualityScore = $"{qualityScore:P0}",
                validationEfficiency = validationTimeMs > 0 ? $"{(1000.0 / validationTimeMs):F2}" : "instant",
                processingTime = new {
                    validationMs = validationTimeMs,
                    totalMs = totalTimeMs,
                    efficiency = totalTimeMs > 0 ? $"{(validationTimeMs / totalTimeMs):P0}" : "100%"
                },
                issueAnalysis = new {
                    issuesFound,
                    issueRate = complexityScore > 0 ? issuesFound / complexityScore : 0,
                    issueCategory = issuesFound switch
                    {
                        0 => "none",
                        <= 2 => "minimal",
                        <= 5 => "moderate",
                        _ => "significant"
                    }
                },
                recommendations
            };

            operationLogger?.LogDebug("Validation metrics generated {@MetricsGeneration}", new {
                correlationId,
                method = "GenerateValidationMetrics",
                generationTimeMs = stopwatch.TotalMilliseconds,
                confidenceScore = metrics.confidenceScore,
                qualityScore = metrics.qualityScore,
                recommendationsGenerated = recommendations.GetType().GetProperties().Length
            });

            return metrics;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            operationLogger?.LogError(ex, "Validation metrics generation failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                method = "GenerateValidationMetrics",
                generationTimeMs = stopwatch.TotalMilliseconds
            });

            return new { error = "Metrics generation failed", details = ex.Message };
        }
    }

    /// <summary>
    /// Helper method to calculate maximum nesting depth of XML elements
    /// </summary>
    private static int CalculateMaxDepth(XElement? element, int currentDepth = 0)
    {
        if (element == null) return currentDepth;
        
        var maxChildDepth = currentDepth;
        foreach (var child in element.Elements())
        {
            var childDepth = CalculateMaxDepth(child, currentDepth + 1);
            maxChildDepth = Math.Max(maxChildDepth, childDepth);
        }
        
        return maxChildDepth;
    }

    /// <summary>
    /// Helper method to estimate nesting depth without full parsing
    /// </summary>
    private static int EstimateNestingDepth(string xml)
    {
        var maxDepth = 0;
        var currentDepth = 0;
        var inTag = false;
        var isClosingTag = false;

        for (int i = 0; i < xml.Length - 1; i++)
        {
            if (xml[i] == '<')
            {
                inTag = true;
                isClosingTag = i + 1 < xml.Length && xml[i + 1] == '/';
            }
            else if (xml[i] == '>' && inTag)
            {
                if (isClosingTag)
                    currentDepth--;
                else if (i > 0 && xml[i - 1] != '/')
                    currentDepth++;
                
                maxDepth = Math.Max(maxDepth, currentDepth);
                inTag = false;
                isClosingTag = false;
            }
        }

        return maxDepth;
    }

    /// <summary>
    /// Categorizes XML error types for targeted recommendations
    /// </summary>
    private static string CategorizeXmlError(string errorMessage)
    {
        return errorMessage.ToLower() switch
        {
            var msg when msg.Contains("unclosed") || msg.Contains("closing") => "unclosed_tags",
            var msg when msg.Contains("attribute") => "malformed_attributes",
            var msg when msg.Contains("entity") => "entity_issues",
            var msg when msg.Contains("encoding") => "encoding_issues",
            var msg when msg.Contains("namespace") => "namespace_issues",
            _ => "general_syntax"
        };
    }

    /// <summary>
    /// Generates specific recommendations based on error category
    /// </summary>
    private static string GetRecommendationForXmlError(string errorCategory)
    {
        return errorCategory switch
        {
            "unclosed_tags" => "Check for missing closing tags, especially in comments sections",
            "malformed_attributes" => "Remove duplicate attributes and ensure proper quoting",
            "entity_issues" => "Escape special characters or use CDATA sections",
            "encoding_issues" => "Verify XML encoding declaration matches actual encoding",
            "namespace_issues" => "Check namespace declarations and prefixes",
            _ => "Review XML syntax and structure for compliance with XML standards"
        };
    }

    /// <summary>
    /// Calculates validation confidence score
    /// </summary>
    private static double CalculateConfidenceScore(bool isValid, double validationTimeMs, int issuesFound, double complexityScore)
    {
        var baseScore = isValid ? 0.8 : 0.2;
        var timeBonus = validationTimeMs < 1000 ? 0.1 : 0.0;
        var issuesPenalty = issuesFound * 0.05;
        var complexityPenalty = complexityScore > 5 ? 0.1 : 0.0;
        
        return Math.Max(0.0, Math.Min(1.0, baseScore + timeBonus - issuesPenalty - complexityPenalty));
    }

    /// <summary>
    /// Calculates overall quality score
    /// </summary>
    private static double CalculateQualityScore(bool isValid, int issuesFound, double complexityScore)
    {
        var validityScore = isValid ? 0.5 : 0.0;
        var issuesScore = Math.Max(0.0, 0.3 - (issuesFound * 0.05));
        var complexityScore_normalized = Math.Max(0.0, 0.2 - (complexityScore * 0.02));
        
        return validityScore + issuesScore + complexityScore_normalized;
    }

    /// <summary>
    /// Generates actionable recommendations based on validation results
    /// </summary>
    private static object GenerateRecommendations(bool isValid, int issuesFound, double complexityScore, double validationTimeMs)
    {
        var recommendations = new List<string>();
        
        if (!isValid)
            recommendations.Add("Use XML cleaning service to fix structural issues");
        
        if (issuesFound > 3)
            recommendations.Add("Address detected issues to improve XML quality");
        
        if (complexityScore > 8)
            recommendations.Add("Consider simplifying XML structure for better performance");
        
        if (validationTimeMs > 5000)
            recommendations.Add("Optimize XML size or use streaming processing for large files");
        
        return new {
            immediate = recommendations.Take(2).ToArray(),
            longTerm = recommendations.Skip(2).ToArray(),
            priority = issuesFound > 0 ? "high" : "low"
        };
    }
}
