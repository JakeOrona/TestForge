using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TestForge.McpServer.Services;

/// <summary>
/// Service responsible for cleaning raw Jira XML exports to fix common formatting issues
/// </summary>
public static class JiraXmlCleaningService
{
    private static ILogger? _logger;

    /// <summary>
    /// Initializes the service with optional logger
    /// </summary>
    /// <param name="logger">Optional logger for the service</param>
    public static void Initialize(ILogger? logger = null)
    {
        _logger = logger;
        
        _logger?.LogInformation("JiraXmlCleaningService initialized {@ServiceInfo}", new {
            serviceType = "XmlCleaningService",
            version = "1.0",
            capabilities = new[] {
                "missing_closing_tags_fix",
                "duplicate_attributes_removal", 
                "comment_content_cleaning",
                "html_entity_escaping",
                "invalid_character_removal",
                "nested_structure_fix",
                "xml_validation"
            },
            performanceThresholds = new {
                largeFileWarning = "5MB",
                timeoutWarning = "10s"
            }
        });
    }
    /// <summary>
    /// Cleans raw Jira XML exports to fix common formatting issues, missing closing tags, and malformed content
    /// </summary>
    /// <param name="rawXml">The raw Jira XML content to clean</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <returns>JSON response containing cleaned XML and processing information</returns>
    public static string Clean(string rawXml, ILogger? logger = null)
    {
        var operationLogger = logger ?? _logger;
        var correlationId = Guid.NewGuid().ToString("N")[..8];
        var totalStopwatch = Stopwatch.StartNew();

        operationLogger?.LogInformation("Starting XML cleaning operation {@OperationInfo}", new {
            correlationId,
            method = "Clean",
            inputSize = rawXml?.Length ?? 0
        });

        try
        {
            if (string.IsNullOrWhiteSpace(rawXml))
            {
                operationLogger?.LogWarning("Empty XML input provided {@ValidationWarning}", new {
                    correlationId,
                    error = "input_validation_failed",
                    inputState = "null_or_empty"
                });

                return JsonSerializer.Serialize(new { 
                    success = false, 
                    error = "Input XML is empty",
                    correlationId
                }, new JsonSerializerOptions { WriteIndented = true });
            }

            var originalLength = rawXml.Length;
            
            // Security monitoring for large files
            var fileSizeMB = originalLength / (1024.0 * 1024.0);
            if (fileSizeMB > 5.0)
            {
                operationLogger?.LogWarning("Large XML file detected {@SecurityWarning}", new {
                    correlationId,
                    fileSizeMB = $"{fileSizeMB:F2}",
                    recommendation = "consider_streaming_processing_for_files_over_5MB",
                    potentialMemoryImpact = "high"
                });
            }

            operationLogger?.LogDebug("Input validation completed {@InputMetrics}", new {
                correlationId,
                originalLength,
                fileSizeMB = $"{fileSizeMB:F2}",
                containsComments = rawXml.Contains("<comments"),
                containsHtml = Regex.IsMatch(rawXml, @"<[^!?][^>]*>"),
                estimatedComplexity = DetermineCleaningComplexity(rawXml)
            });

            var cleaningStopwatch = Stopwatch.StartNew();
            var stepTimings = new Dictionary<string, double>();
            var cleanedXml = CleanJiraXmlInternal(rawXml, operationLogger, correlationId, stepTimings);
            cleaningStopwatch.Stop();

            var cleanedLength = cleanedXml.Length;
            var bytesChanged = Math.Abs(originalLength - cleanedLength);
            var changePercentage = originalLength > 0 ? (bytesChanged / (double)originalLength) * 100 : 0;

            operationLogger?.LogInformation("XML cleaning completed {@CleaningMetrics}", new {
                correlationId,
                originalLength,
                cleanedLength,
                bytesChanged,
                changePercentage = $"{changePercentage:F2}%",
                cleaningTimeMs = cleaningStopwatch.TotalMilliseconds,
                stepTimings = stepTimings
            });

            // Validate the cleaned XML
            var validationStopwatch = Stopwatch.StartNew();
            var validationResult = ValidateXmlStructure(cleanedXml, operationLogger, correlationId);
            validationStopwatch.Stop();

            totalStopwatch.Stop();

            // Performance monitoring
            if (totalStopwatch.TotalSeconds > 10)
            {
                operationLogger?.LogWarning("Slow XML cleaning operation detected {@PerformanceWarning}", new {
                    correlationId,
                    totalTimeSeconds = $"{totalStopwatch.TotalSeconds:F2}",
                    recommendation = "investigate_input_complexity_and_optimize_regex_patterns",
                    inputSize = originalLength,
                    cleaningComplexity = DetermineCleaningComplexity(rawXml)
                });
            }

            operationLogger?.LogInformation("XML cleaning operation completed {@Metrics}", new {
                success = validationResult.IsValid,
                correlationId,
                totalProcessingTimeMs = totalStopwatch.TotalMilliseconds,
                cleaningTimeMs = cleaningStopwatch.TotalMilliseconds,
                validationTimeMs = validationStopwatch.TotalMilliseconds,
                cleaningEffectiveness = $"{changePercentage:F2}%",
                qualityScore = CalculateQualityScore(validationResult.IsValid, changePercentage, totalStopwatch.TotalMilliseconds)
            });

            return JsonSerializer.Serialize(new {
                success = validationResult.IsValid,
                correlationId,
                originalLength = originalLength,
                cleanedLength = cleanedLength,
                bytesChanged = bytesChanged,
                changePercentage = $"{changePercentage:F2}%",
                isValid = validationResult.IsValid,
                validationError = validationResult.Error,
                cleanedXml = cleanedXml,
                processingTimeMs = totalStopwatch.TotalMilliseconds,
                stepTimings = stepTimings,
                cleaningSteps = new[] {
                    "Fixed missing closing tags",
                    "Cleaned comment content", 
                    "Removed duplicate attributes",
                    "Escaped HTML entities",
                    "Removed invalid characters",
                    "Fixed nested structure"
                }
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            totalStopwatch.Stop();
            
            operationLogger?.LogError(ex, "XML cleaning operation failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                processingTimeMs = totalStopwatch.TotalMilliseconds,
                inputSize = rawXml?.Length ?? 0,
                recommendation = "check_input_xml_format_and_structure"
            });

            return JsonSerializer.Serialize(new { 
                success = false, 
                error = $"Cleaning failed: {ex.Message}",
                correlationId,
                processingTimeMs = totalStopwatch.TotalMilliseconds
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    /// <summary>
    /// Enhanced XML cleaning to handle comment section and structural issues
    /// </summary>
    /// <param name="rawXml">Raw XML content to clean</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <param name="stepTimings">Dictionary to track individual step timings</param>
    /// <returns>Cleaned XML content</returns>
    private static string CleanJiraXmlInternal(string rawXml, ILogger? logger = null, string correlationId = "", Dictionary<string, double>? stepTimings = null)
    {
        var operationLogger = logger ?? _logger;
        stepTimings ??= new Dictionary<string, double>();
        
        if (string.IsNullOrWhiteSpace(rawXml))
        {
            operationLogger?.LogDebug("Empty XML input to internal cleaning {@CleaningInfo}", new {
                correlationId,
                method = "CleanJiraXmlInternal",
                inputState = "empty"
            });
            return rawXml;
        }

        operationLogger?.LogDebug("Starting internal XML cleaning pipeline {@PipelineInfo}", new {
            correlationId,
            method = "CleanJiraXmlInternal",
            inputSize = rawXml.Length,
            pipelineSteps = 7
        });

        var workingXml = rawXml;
        var originalSize = rawXml.Length;

        try
        {
            // Step 1: Fix missing closing tags for comments section
            var step1Stopwatch = Stopwatch.StartNew();
            var beforeStep1Size = workingXml.Length;
            workingXml = FixMissingCommentClosingTags(workingXml, operationLogger, correlationId);
            step1Stopwatch.Stop();
            stepTimings["missing_closing_tags"] = step1Stopwatch.TotalMilliseconds;
            
            operationLogger?.LogDebug("Cleaning step completed {@StepMetrics}", new {
                correlationId,
                step = 1,
                name = "missing_closing_tags",
                processingTimeMs = step1Stopwatch.TotalMilliseconds,
                sizeChange = workingXml.Length - beforeStep1Size,
                effectiveness = beforeStep1Size > 0 ? $"{Math.Abs(workingXml.Length - beforeStep1Size) / (double)beforeStep1Size * 100:F2}%" : "0%"
            });
            
            // Step 2: Fix duplicate attributes BEFORE processing comments content
            var step2Stopwatch = Stopwatch.StartNew();
            var beforeStep2Size = workingXml.Length;
            workingXml = FixDuplicateAttributes(workingXml, operationLogger, correlationId);
            step2Stopwatch.Stop();
            stepTimings["duplicate_attributes"] = step2Stopwatch.TotalMilliseconds;
            
            operationLogger?.LogDebug("Cleaning step completed {@StepMetrics}", new {
                correlationId,
                step = 2,
                name = "duplicate_attributes",
                processingTimeMs = step2Stopwatch.TotalMilliseconds,
                sizeChange = workingXml.Length - beforeStep2Size,
                effectiveness = beforeStep2Size > 0 ? $"{Math.Abs(workingXml.Length - beforeStep2Size) / (double)beforeStep2Size * 100:F2}%" : "0%"
            });
            
            // Step 3: Clean malformed comment content
            var step3Stopwatch = Stopwatch.StartNew();
            var beforeStep3Size = workingXml.Length;
            workingXml = CleanCommentContent(workingXml, operationLogger, correlationId);
            step3Stopwatch.Stop();
            stepTimings["comment_content"] = step3Stopwatch.TotalMilliseconds;
            
            operationLogger?.LogDebug("Cleaning step completed {@StepMetrics}", new {
                correlationId,
                step = 3,
                name = "comment_content",
                processingTimeMs = step3Stopwatch.TotalMilliseconds,
                sizeChange = workingXml.Length - beforeStep3Size,
                effectiveness = beforeStep3Size > 0 ? $"{Math.Abs(workingXml.Length - beforeStep3Size) / (double)beforeStep3Size * 100:F2}%" : "0%"
            });
            
            // Step 4: Fix malformed HTML entities
            var step4Stopwatch = Stopwatch.StartNew();
            var beforeStep4Size = workingXml.Length;
            var entitiesMatches = Regex.Matches(workingXml, @"&(?!(?:amp|lt|gt|quot|apos|#\d+|#x[0-9a-fA-F]+);)").Count;
            workingXml = Regex.Replace(workingXml, @"&(?!(?:amp|lt|gt|quot|apos|#\d+|#x[0-9a-fA-F]+);)", "&amp;");
            step4Stopwatch.Stop();
            stepTimings["html_entities"] = step4Stopwatch.TotalMilliseconds;
            
            operationLogger?.LogDebug("Cleaning step completed {@StepMetrics}", new {
                correlationId,
                step = 4,
                name = "html_entities",
                processingTimeMs = step4Stopwatch.TotalMilliseconds,
                entitiesFixed = entitiesMatches,
                sizeChange = workingXml.Length - beforeStep4Size
            });
            
            // Step 5: Remove invalid XML characters
            var step5Stopwatch = Stopwatch.StartNew();
            var beforeStep5Size = workingXml.Length;
            var invalidCharsMatches = Regex.Matches(workingXml, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]").Count;
            workingXml = Regex.Replace(workingXml, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");
            step5Stopwatch.Stop();
            stepTimings["invalid_characters"] = step5Stopwatch.TotalMilliseconds;
            
            operationLogger?.LogDebug("Cleaning step completed {@StepMetrics}", new {
                correlationId,
                step = 5,
                name = "invalid_characters",
                processingTimeMs = step5Stopwatch.TotalMilliseconds,
                invalidCharsRemoved = invalidCharsMatches,
                sizeChange = workingXml.Length - beforeStep5Size
            });
            
            // Step 6: Remove empty CDATA sections
            var step6Stopwatch = Stopwatch.StartNew();
            var beforeStep6Size = workingXml.Length;
            var emptyCdataMatches = Regex.Matches(workingXml, @"<!\[CDATA\[\s*\]\]>", RegexOptions.IgnoreCase).Count;
            workingXml = Regex.Replace(workingXml, @"<!\[CDATA\[\s*\]\]>", "", RegexOptions.IgnoreCase);
            step6Stopwatch.Stop();
            stepTimings["empty_cdata"] = step6Stopwatch.TotalMilliseconds;
            
            operationLogger?.LogDebug("Cleaning step completed {@StepMetrics}", new {
                correlationId,
                step = 6,
                name = "empty_cdata",
                processingTimeMs = step6Stopwatch.TotalMilliseconds,
                emptyCdataRemoved = emptyCdataMatches,
                sizeChange = workingXml.Length - beforeStep6Size
            });
            
            // Step 7: Fix nested XML structure issues
            var step7Stopwatch = Stopwatch.StartNew();
            var beforeStep7Size = workingXml.Length;
            workingXml = FixNestedXmlStructure(workingXml, operationLogger, correlationId);
            step7Stopwatch.Stop();
            stepTimings["nested_structure"] = step7Stopwatch.TotalMilliseconds;
            
            operationLogger?.LogDebug("Cleaning step completed {@StepMetrics}", new {
                correlationId,
                step = 7,
                name = "nested_structure", 
                processingTimeMs = step7Stopwatch.TotalMilliseconds,
                sizeChange = workingXml.Length - beforeStep7Size,
                effectiveness = beforeStep7Size > 0 ? $"{Math.Abs(workingXml.Length - beforeStep7Size) / (double)beforeStep7Size * 100:F2}%" : "0%"
            });

            var totalSizeChange = workingXml.Length - originalSize;
            var totalEffectiveness = originalSize > 0 ? Math.Abs(totalSizeChange) / (double)originalSize * 100 : 0;

            operationLogger?.LogInformation("XML cleaning pipeline completed {@PipelineMetrics}", new {
                correlationId,
                method = "CleanJiraXmlInternal",
                stepsCompleted = 7,
                originalSize,
                finalSize = workingXml.Length,
                totalSizeChange,
                totalEffectiveness = $"{totalEffectiveness:F2}%",
                totalProcessingTimeMs = stepTimings.Values.Sum()
            });

            return workingXml;
        }
        catch (Exception ex)
        {
            operationLogger?.LogError(ex, "XML cleaning pipeline failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                method = "CleanJiraXmlInternal",
                partialStepsCompleted = stepTimings.Count,
                recommendation = "review_regex_patterns_and_input_xml_structure"
            });
            
            // Return original XML as fallback
            return rawXml;
        }
    }

    /// <summary>
    /// Fixes duplicate attributes in XML elements
    /// </summary>
    /// <param name="xml">XML content to fix</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>XML with duplicate attributes removed</returns>
    private static string FixDuplicateAttributes(string xml, ILogger? logger = null, string correlationId = "")
    {
        var operationLogger = logger ?? _logger;
        var stopwatch = Stopwatch.StartNew();
        var originalSize = xml.Length;
        var attributeFixCounts = new Dictionary<string, int>();

        operationLogger?.LogDebug("Starting duplicate attributes fixing {@OperationInfo}", new {
            correlationId,
            method = "FixDuplicateAttributes",
            inputSize = xml.Length
        });

        try
        {
            var workingXml = xml;

            // Fix duplicate rel attributes
            var relStopwatch = Stopwatch.StartNew();
            var relMatches = Regex.Matches(workingXml, @"rel=""[^""]*""\s+([^>]*?)rel=""([^""]*?)""", RegexOptions.IgnoreCase).Count;
            workingXml = Regex.Replace(workingXml, @"rel=""[^""]*""\s+([^>]*?)rel=""([^""]*?)""", @"rel=""$2"" $1", RegexOptions.IgnoreCase);
            relStopwatch.Stop();
            attributeFixCounts["rel"] = relMatches;
            
            operationLogger?.LogDebug("Attribute type processed {@AttributeMetrics}", new {
                correlationId,
                attributeType = "rel",
                duplicatesFound = relMatches,
                processingTimeMs = relStopwatch.TotalMilliseconds
            });

            // Fix duplicate class attributes
            var classStopwatch = Stopwatch.StartNew();
            var classMatches = Regex.Matches(workingXml, @"class=""[^""]*""\s+([^>]*?)class=""([^""]*?)""", RegexOptions.IgnoreCase).Count;
            workingXml = Regex.Replace(workingXml, @"class=""[^""]*""\s+([^>]*?)class=""([^""]*?)""", @"class=""$2"" $1", RegexOptions.IgnoreCase);
            classStopwatch.Stop();
            attributeFixCounts["class"] = classMatches;
            
            operationLogger?.LogDebug("Attribute type processed {@AttributeMetrics}", new {
                correlationId,
                attributeType = "class",
                duplicatesFound = classMatches,
                processingTimeMs = classStopwatch.TotalMilliseconds
            });

            // Fix duplicate data-account-id attributes
            var dataAccountIdStopwatch = Stopwatch.StartNew();
            var dataAccountIdMatches = Regex.Matches(workingXml, @"data-account-id=""[^""]*""\s+([^>]*?)data-account-id=""([^""]*?)""", RegexOptions.IgnoreCase).Count;
            workingXml = Regex.Replace(workingXml, @"data-account-id=""[^""]*""\s+([^>]*?)data-account-id=""([^""]*?)""", @"data-account-id=""$2"" $1", RegexOptions.IgnoreCase);
            dataAccountIdStopwatch.Stop();
            attributeFixCounts["data-account-id"] = dataAccountIdMatches;
            
            operationLogger?.LogDebug("Attribute type processed {@AttributeMetrics}", new {
                correlationId,
                attributeType = "data-account-id",
                duplicatesFound = dataAccountIdMatches,
                processingTimeMs = dataAccountIdStopwatch.TotalMilliseconds
            });

            // Fix duplicate accountid attributes
            var accountIdStopwatch = Stopwatch.StartNew();
            var accountIdMatches = Regex.Matches(workingXml, @"accountid=""[^""]*""\s+([^>]*?)accountid=""([^""]*?)""", RegexOptions.IgnoreCase).Count;
            workingXml = Regex.Replace(workingXml, @"accountid=""[^""]*""\s+([^>]*?)accountid=""([^""]*?)""", @"accountid=""$2"" $1", RegexOptions.IgnoreCase);
            accountIdStopwatch.Stop();
            attributeFixCounts["accountid"] = accountIdMatches;
            
            operationLogger?.LogDebug("Attribute type processed {@AttributeMetrics}", new {
                correlationId,
                attributeType = "accountid",
                duplicatesFound = accountIdMatches,
                processingTimeMs = accountIdStopwatch.TotalMilliseconds
            });

            stopwatch.Stop();
            
            var totalDuplicatesFixed = attributeFixCounts.Values.Sum();
            var sizeChange = workingXml.Length - originalSize;
            var effectiveness = originalSize > 0 ? Math.Abs(sizeChange) / (double)originalSize * 100 : 0;

            operationLogger?.LogInformation("Duplicate attributes fixing completed {@Metrics}", new {
                success = true,
                correlationId,
                method = "FixDuplicateAttributes",
                processingTimeMs = stopwatch.TotalMilliseconds,
                totalDuplicatesFixed,
                attributeTypeBreakdown = attributeFixCounts,
                sizeChange,
                effectiveness = $"{effectiveness:F2}%"
            });

            return workingXml;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            operationLogger?.LogError(ex, "Duplicate attributes fixing failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                method = "FixDuplicateAttributes",
                processingTimeMs = stopwatch.TotalMilliseconds,
                partialFixesApplied = attributeFixCounts,
                recommendation = "check_regex_patterns_and_xml_structure"
            });

            return xml; // Return original on failure
        }
    }

    /// <summary>
    /// Fixes missing closing tags for comments section
    /// </summary>
    /// <param name="xml">XML content to fix</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>XML with fixed closing tags</returns>
    private static string FixMissingCommentClosingTags(string xml, ILogger? logger = null, string correlationId = "")
    {
        var operationLogger = logger ?? _logger;
        var stopwatch = Stopwatch.StartNew();
        var originalSize = xml.Length;

        operationLogger?.LogDebug("Starting missing comment closing tags fixing {@OperationInfo}", new {
            correlationId,
            method = "FixMissingCommentClosingTags",
            inputSize = xml.Length
        });

        try
        {
            // Only fix if comments section is missing its closing tag
            // Pattern to find <comments> that runs until another major element or end of document
            var unclosedCommentsPattern = @"<comments[^>]*>((?:(?!<\/comments>)[\s\S])*?)(?=<\/item>|<issuelinks>|<attachments>|<subtasks>|<customfields>|</channel>|$)";
            
            var patternStopwatch = Stopwatch.StartNew();
            var hasUnclosedComments = Regex.IsMatch(xml, unclosedCommentsPattern, RegexOptions.IgnoreCase);
            patternStopwatch.Stop();
            
            operationLogger?.LogDebug("Unclosed comments detection completed {@DetectionMetrics}", new {
                correlationId,
                hasUnclosedComments,
                patternMatchingTimeMs = patternStopwatch.TotalMilliseconds,
                totalCommentsElements = Regex.Matches(xml, @"<comments[^>]*>", RegexOptions.IgnoreCase).Count,
                totalClosingComments = Regex.Matches(xml, @"</comments>", RegexOptions.IgnoreCase).Count
            });

            var workingXml = xml;
            var tagsFixed = 0;

            if (hasUnclosedComments)
            {
                var fixStopwatch = Stopwatch.StartNew();
                workingXml = Regex.Replace(xml, unclosedCommentsPattern, match =>
                {
                    var commentsContent = match.Groups[1].Value;
                    tagsFixed++;
                    return $"<comments>{commentsContent}</comments>";
                }, RegexOptions.IgnoreCase);
                fixStopwatch.Stop();
                
                operationLogger?.LogDebug("Comments closing tags fixed {@FixMetrics}", new {
                    correlationId,
                    tagsFixed,
                    fixProcessingTimeMs = fixStopwatch.TotalMilliseconds,
                    contentSizeProcessed = workingXml.Length - originalSize
                });
            }

            stopwatch.Stop();
            
            var sizeChange = workingXml.Length - originalSize;
            var structuralImprovement = tagsFixed > 0 ? "comments_structure_normalized" : "no_fixes_needed";

            operationLogger?.LogInformation("Missing comment closing tags fixing completed {@Metrics}", new {
                success = true,
                correlationId,
                method = "FixMissingCommentClosingTags",
                processingTimeMs = stopwatch.TotalMilliseconds,
                tagsFixed,
                sizeChange,
                structuralImprovement,
                hasUnclosedComments
            });

            return workingXml;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            operationLogger?.LogError(ex, "Missing comment closing tags fixing failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                method = "FixMissingCommentClosingTags",
                processingTimeMs = stopwatch.TotalMilliseconds,
                recommendation = "check_xml_structure_and_regex_pattern_complexity"
            });

            return xml; // Return original on failure
        }
    }

    /// <summary>
    /// Cleans malformed content within the entire comments section
    /// </summary>
    /// <param name="xml">XML content to clean</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>XML with cleaned comment content</returns>
    private static string CleanCommentContent(string xml, ILogger? logger = null, string correlationId = "")
    {
        var operationLogger = logger ?? _logger;
        var stopwatch = Stopwatch.StartNew();
        var originalSize = xml.Length;

        operationLogger?.LogDebug("Starting comment content cleaning {@OperationInfo}", new {
            correlationId,
            method = "CleanCommentContent",
            inputSize = xml.Length
        });

        try
        {
            // Find the entire comments section using a more robust pattern
            // Handle both empty and non-empty comments sections
            var commentsPattern = @"<comments[^>]*>(.*?)</comments>";
            var commentsSections = Regex.Matches(xml, commentsPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            operationLogger?.LogDebug("Comments sections detected {@DetectionMetrics}", new {
                correlationId,
                commentsSectionsFound = commentsSections.Count,
                totalCommentsContentSize = commentsSections.Cast<Match>().Sum(m => m.Groups[1].Value.Length)
            });

            var workingXml = xml;
            var sectionsProcessed = 0;
            var cdataWrappedSections = 0;
            var totalContentSizeBefore = 0;
            var totalContentSizeAfter = 0;

            workingXml = Regex.Replace(xml, commentsPattern, match =>
            {
                var commentsContent = match.Groups[1].Value;
                sectionsProcessed++;
                totalContentSizeBefore += commentsContent.Length;
                
                // Always wrap complex comments content in CDATA for safety
                if (!string.IsNullOrWhiteSpace(commentsContent))
                {
                    // Clean the content before wrapping in CDATA
                    var cleanedContent = CleanCommentsContentForCdata(commentsContent, operationLogger, correlationId);
                    totalContentSizeAfter += cleanedContent.Length;
                    cdataWrappedSections++;
                    
                    operationLogger?.LogDebug("Comments section processed {@SectionMetrics}", new {
                        correlationId,
                        sectionNumber = sectionsProcessed,
                        originalContentSize = commentsContent.Length,
                        cleanedContentSize = cleanedContent.Length,
                        cdataWrapped = true,
                        containsHtml = Regex.IsMatch(commentsContent, @"<[^>]+>"),
                        containsSpecialChars = ContainsHtmlOrSpecialChars(commentsContent)
                    });
                    
                    return $"<comments><![CDATA[{cleanedContent}]]></comments>";
                }
                
                // Return empty comments section as is
                operationLogger?.LogDebug("Empty comments section processed {@SectionMetrics}", new {
                    correlationId,
                    sectionNumber = sectionsProcessed,
                    cdataWrapped = false
                });
                
                return "<comments></comments>";
            }, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            stopwatch.Stop();
            
            var sizeChange = workingXml.Length - originalSize;
            var contentEfficiency = totalContentSizeBefore > 0 ? (totalContentSizeAfter / (double)totalContentSizeBefore) * 100 : 100;

            operationLogger?.LogInformation("Comment content cleaning completed {@Metrics}", new {
                success = true,
                correlationId,
                method = "CleanCommentContent",
                processingTimeMs = stopwatch.TotalMilliseconds,
                sectionsProcessed,
                cdataWrappedSections,
                sizeChange,
                contentEfficiency = $"{contentEfficiency:F2}%",
                totalContentSizeBefore,
                totalContentSizeAfter,
                safetyTransformations = cdataWrappedSections
            });

            return workingXml;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            operationLogger?.LogError(ex, "Comment content cleaning failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                method = "CleanCommentContent",
                processingTimeMs = stopwatch.TotalMilliseconds,
                recommendation = "check_regex_patterns_and_content_structure"
            });

            return xml; // Return original on failure
        }
    }

    /// <summary>
    /// Cleans content specifically for CDATA wrapping to prevent nested CDATA issues
    /// </summary>
    /// <param name="content">Content to clean for CDATA wrapping</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>Cleaned content safe for CDATA</returns>
    private static string CleanCommentsContentForCdata(string content, ILogger? logger = null, string correlationId = "")
    {
        var operationLogger = logger ?? _logger;
        var stopwatch = Stopwatch.StartNew();
        
        if (string.IsNullOrWhiteSpace(content))
        {
            operationLogger?.LogDebug("Empty content for CDATA cleaning {@ContentInfo}", new {
                correlationId,
                method = "CleanCommentsContentForCdata",
                contentState = "empty"
            });
            return content;
        }

        var originalSize = content.Length;
        
        operationLogger?.LogDebug("Starting CDATA content preparation {@OperationInfo}", new {
            correlationId,
            method = "CleanCommentsContentForCdata",
            contentSize = originalSize
        });

        try
        {
            var workingContent = content;
            var transformationCounts = new Dictionary<string, int>();

            // Remove existing CDATA sections to prevent nesting
            var existingCdataMatches = Regex.Matches(workingContent, @"<!\[CDATA\[(.*?)\]\]>", RegexOptions.Singleline);
            workingContent = Regex.Replace(workingContent, @"<!\[CDATA\[(.*?)\]\]>", "$1", RegexOptions.Singleline);
            transformationCounts["existing_cdata_removed"] = existingCdataMatches.Count;
            
            // Handle the ]]> sequence that would break CDATA - this is critical
            var problematicSequences = workingContent.Split("]]>").Length - 1;
            workingContent = workingContent.Replace("]]>", "]]&gt;");
            transformationCounts["problematic_sequences_fixed"] = problematicSequences;
            
            // Also handle other potentially problematic sequences
            workingContent = workingContent.Replace("]]", "]]");  // Keep as is, it's only ]]> that's problematic
            
            // Clean up any remaining XML escaping since we're putting it in CDATA
            var entityCounts = new Dictionary<string, int>();
            entityCounts["amp"] = workingContent.Split("&amp;").Length - 1;
            entityCounts["lt"] = workingContent.Split("&lt;").Length - 1;
            entityCounts["gt"] = workingContent.Split("&gt;").Length - 1;
            entityCounts["quot"] = workingContent.Split("&quot;").Length - 1;
            entityCounts["apos"] = workingContent.Split("&apos;").Length - 1;
            
            workingContent = workingContent.Replace("&amp;", "&");
            workingContent = workingContent.Replace("&lt;", "<");
            workingContent = workingContent.Replace("&gt;", ">");
            workingContent = workingContent.Replace("&quot;", "\"");
            workingContent = workingContent.Replace("&apos;", "'");
            
            // Handle problematic Unicode characters that might break XML parsing
            var invalidCharsBefore = Regex.Matches(workingContent, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]").Count;
            workingContent = Regex.Replace(workingContent, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");
            transformationCounts["invalid_chars_removed"] = invalidCharsBefore;
            
            // Ensure line breaks are preserved properly
            var lineBreaksNormalized = 0;
            if (workingContent.Contains("\r\n"))
            {
                lineBreaksNormalized = workingContent.Split("\r\n").Length - 1;
                workingContent = workingContent.Replace("\r\n", "\n");
            }
            if (workingContent.Contains("\r"))
            {
                lineBreaksNormalized += workingContent.Split("\r").Length - 1;
                workingContent = workingContent.Replace("\r", "\n");
            }
            transformationCounts["line_breaks_normalized"] = lineBreaksNormalized;
            
            // Make sure there are no unescaped control characters
            var privateUseCharsBefore = Regex.Matches(workingContent, @"[\uE000-\uF8FF]").Count;
            workingContent = Regex.Replace(workingContent, @"[\uE000-\uF8FF]", ""); // Private use area
            transformationCounts["private_use_chars_removed"] = privateUseCharsBefore;

            stopwatch.Stop();
            
            var finalSize = workingContent.Length;
            var sizeChange = finalSize - originalSize;
            var compressionRatio = originalSize > 0 ? (finalSize / (double)originalSize) * 100 : 100;

            operationLogger?.LogInformation("CDATA content preparation completed {@Metrics}", new {
                success = true,
                correlationId,
                method = "CleanCommentsContentForCdata",
                processingTimeMs = stopwatch.TotalMilliseconds,
                originalSize,
                finalSize,
                sizeChange,
                compressionRatio = $"{compressionRatio:F2}%",
                transformationCounts,
                entityCounts,
                cdataSafe = !workingContent.Contains("]]>")
            });

            return workingContent;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            operationLogger?.LogError(ex, "CDATA content preparation failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                method = "CleanCommentsContentForCdata",
                processingTimeMs = stopwatch.TotalMilliseconds,
                contentSize = originalSize,
                recommendation = "fallback_to_original_content_with_basic_escaping"
            });

            // Return original content on failure
            return content;
        }
    }

    /// <summary>
    /// Fixes nested XML structure issues
    /// </summary>
    /// <param name="xml">XML content to fix</param>
    /// <param name="logger">Optional logger for this operation</param>
    /// <param name="correlationId">Correlation ID for request tracing</param>
    /// <returns>XML with fixed nested structure</returns>
    private static string FixNestedXmlStructure(string xml, ILogger? logger = null, string correlationId = "")
    {
        var operationLogger = logger ?? _logger;
        var stopwatch = Stopwatch.StartNew();
        var originalSize = xml.Length;

        operationLogger?.LogDebug("Starting nested XML structure fixing {@OperationInfo}", new {
            correlationId,
            method = "FixNestedXmlStructure",
            inputSize = xml.Length
        });

        try
        {
            var workingXml = xml;
            var structuralFixes = new Dictionary<string, int>();

            // Remove any duplicate consecutive closing tags for comments
            var duplicateCommentsMatches = Regex.Matches(workingXml, @"<\/comments>\s*<\/comments>", RegexOptions.IgnoreCase).Count;
            workingXml = Regex.Replace(workingXml, @"<\/comments>\s*<\/comments>", "</comments>", RegexOptions.IgnoreCase);
            structuralFixes["duplicate_comments_closing"] = duplicateCommentsMatches;
            
            operationLogger?.LogDebug("Structural fix applied {@FixMetrics}", new {
                correlationId,
                fixType = "duplicate_comments_closing",
                occurrencesFixed = duplicateCommentsMatches
            });

            // Remove any orphaned closing tags that might be left over
            var orphanedTagMatches = Regex.Matches(workingXml, @"<\/comment>\s*<\/comments>\s*<\/item>", RegexOptions.IgnoreCase).Count;
            workingXml = Regex.Replace(workingXml, @"<\/comment>\s*<\/comments>\s*<\/item>", "</comments></item>", RegexOptions.IgnoreCase);
            structuralFixes["orphaned_comment_tags"] = orphanedTagMatches;
            
            operationLogger?.LogDebug("Structural fix applied {@FixMetrics}", new {
                correlationId,
                fixType = "orphaned_comment_tags",
                occurrencesFixed = orphanedTagMatches
            });

            // Clean up any malformed comments structure that might remain
            var malformedCommentsMatches = Regex.Matches(workingXml, @"<comments>\s*(?=<\/comments>)", RegexOptions.IgnoreCase).Count;
            workingXml = Regex.Replace(workingXml, @"<comments>\s*(?=<\/comments>)", "<comments>", RegexOptions.IgnoreCase);
            structuralFixes["malformed_comments_structure"] = malformedCommentsMatches;
            
            operationLogger?.LogDebug("Structural fix applied {@FixMetrics}", new {
                correlationId,
                fixType = "malformed_comments_structure",
                occurrencesFixed = malformedCommentsMatches
            });

            stopwatch.Stop();
            
            var totalFixesApplied = structuralFixes.Values.Sum();
            var sizeChange = workingXml.Length - originalSize;
            var structuralIntegrityImprovement = totalFixesApplied > 0 ? "structure_normalized" : "no_fixes_needed";

            operationLogger?.LogInformation("Nested XML structure fixing completed {@Metrics}", new {
                success = true,
                correlationId,
                method = "FixNestedXmlStructure",
                processingTimeMs = stopwatch.TotalMilliseconds,
                totalFixesApplied,
                structuralFixes,
                sizeChange,
                structuralIntegrityImprovement,
                xmlWellFormedness = "improved"
            });

            return workingXml;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            operationLogger?.LogError(ex, "Nested XML structure fixing failed {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                method = "FixNestedXmlStructure",
                processingTimeMs = stopwatch.TotalMilliseconds,
                recommendation = "check_xml_structure_and_regex_complexity"
            });

            return xml; // Return original on failure
        }
    }

    /// <summary>
    /// Validates XML structure and provides detailed error information
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
                operationLogger?.LogWarning("Empty XML provided for validation {@ValidationWarning}", new {
                    correlationId,
                    validationResult = "failed",
                    reason = "empty_input"
                });
                return (false, "XML content is empty");
            }

            var parseStopwatch = Stopwatch.StartNew();
            var document = XDocument.Parse(xml);
            parseStopwatch.Stop();
            stopwatch.Stop();

            // Additional structural analysis
            var rootElement = document.Root?.Name.LocalName ?? "unknown";
            var totalElements = document.Descendants().Count();
            var commentsElements = document.Descendants().Count(e => e.Name.LocalName.Equals("comments", StringComparison.OrdinalIgnoreCase));
            var cdataSections = xml.Split("<![CDATA[").Length - 1;

            operationLogger?.LogInformation("XML structure validation completed {@Metrics}", new {
                success = true,
                correlationId,
                method = "ValidateXmlStructure",
                validationTimeMs = stopwatch.TotalMilliseconds,
                parseTimeMs = parseStopwatch.TotalMilliseconds,
                isValid = true,
                xmlSize = xml.Length,
                rootElement,
                totalElements,
                commentsElements,
                cdataSections,
                structuralIntegrity = "valid"
            });

            return (true, string.Empty);
        }
        catch (XmlException ex)
        {
            stopwatch.Stop();
            
            var detailedError = $"Line {ex.LineNumber}, Position {ex.LinePosition}: {ex.Message}";
            
            operationLogger?.LogWarning("XML structure validation failed {@ValidationError}", new {
                correlationId,
                method = "ValidateXmlStructure",
                validationTimeMs = stopwatch.TotalMilliseconds,
                isValid = false,
                errorType = "XmlException",
                lineNumber = ex.LineNumber,
                linePosition = ex.LinePosition,
                errorMessage = ex.Message,
                xmlSize = xml?.Length ?? 0,
                recommendation = "check_xml_syntax_and_structure_around_specified_line"
            });

            return (false, detailedError);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            operationLogger?.LogError(ex, "XML structure validation failed with unexpected error {@ErrorContext}", new {
                correlationId,
                error = ex.Message,
                method = "ValidateXmlStructure",
                validationTimeMs = stopwatch.TotalMilliseconds,
                isValid = false,
                errorType = ex.GetType().Name,
                xmlSize = xml?.Length ?? 0,
                recommendation = "investigate_unexpected_validation_error"
            });

            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Checks if content contains HTML or special characters that need CDATA wrapping
    /// </summary>
    /// <param name="content">Content to check</param>
    /// <returns>True if content needs CDATA wrapping</returns>
    private static bool ContainsHtmlOrSpecialChars(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;
        
        // Check for HTML tags
        if (Regex.IsMatch(content, @"<[^>]+>"))
            return true;
        
        // Check for special XML characters that would break parsing
        if (content.Contains("&") || content.Contains("<") || content.Contains(">"))
            return true;
        
        // Check for user mentions and links that often contain special characters
        if (content.Contains("@") || content.Contains("http"))
            return true;
        
        // Check for XML structure elements that would break parsing
        if (content.Contains("</") || content.Contains("/>"))
            return true;
        
        // Check for quotes that might contain unescaped content
        if (content.Contains("\"") || content.Contains("'"))
            return true;
        
        return false;
    }

    /// <summary>
    /// Determines the cleaning complexity of the input XML for performance prediction
    /// </summary>
    /// <param name="xml">XML content to analyze</param>
    /// <returns>Complexity level (low, medium, high, very_high)</returns>
    private static string DetermineCleaningComplexity(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            return "none";

        var complexity = 0;
        
        // Size-based complexity
        var sizeMB = xml.Length / (1024.0 * 1024.0);
        if (sizeMB > 10) complexity += 3;
        else if (sizeMB > 5) complexity += 2;
        else if (sizeMB > 1) complexity += 1;

        // Content-based complexity
        if (xml.Contains("<comments")) complexity += 1;
        if (Regex.IsMatch(xml, @"<!\[CDATA\[")) complexity += 1;
        if (Regex.IsMatch(xml, @"&\w+;")) complexity += 1;
        if (Regex.IsMatch(xml, @"[\x00-\x1F\x7F]")) complexity += 2;
        
        // Structural complexity
        var commentsSections = Regex.Matches(xml, @"<comments[^>]*>").Count;
        if (commentsSections > 10) complexity += 2;
        else if (commentsSections > 5) complexity += 1;

        return complexity switch
        {
            <= 2 => "low",
            <= 5 => "medium", 
            <= 8 => "high",
            _ => "very_high"
        };
    }

    /// <summary>
    /// Calculates a quality score for the cleaning operation
    /// </summary>
    /// <param name="isValid">Whether the cleaned XML is valid</param>
    /// <param name="changePercentage">Percentage of content changed</param>
    /// <param name="processingTimeMs">Time taken for processing</param>
    /// <returns>Quality score from 0.0 to 1.0</returns>
    private static double CalculateQualityScore(bool isValid, double changePercentage, double processingTimeMs)
    {
        var score = 0.0;
        
        // Base score for validity
        if (isValid) score += 0.5;
        
        // Score for reasonable change percentage (not too little, not too much)
        if (changePercentage >= 0.1 && changePercentage <= 15.0)
            score += 0.3;
        else if (changePercentage < 0.1)
            score += 0.1; // Minimal changes might indicate clean input
        
        // Score for performance (under 5 seconds is good)
        if (processingTimeMs < 5000)
            score += 0.2;
        else if (processingTimeMs < 10000)
            score += 0.1;
        
        return Math.Min(1.0, score);
    }
}
