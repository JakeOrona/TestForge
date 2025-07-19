using System.Collections.Concurrent;
using System.Diagnostics; // Add for Stopwatch
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml;
using System.Xml.XPath;
using Microsoft.Extensions.Logging; // Add this
using TestForge.McpServer.Models;

namespace TestForge.McpServer.Services;

/// <summary>
/// Service responsible for parsing Jira XML content and extracting story data
/// </summary>
public static class JiraStoryParsingService
{
    // XML element lookup cache for performance optimization
    private static readonly ConcurrentDictionary<string, Dictionary<string, XElement>> _elementCache = new();
    
    // Date format caching for performance optimization
    private static readonly ConcurrentDictionary<string, string> _dateFormatCache = new();
    private static volatile string _mostSuccessfulFormat = "ddd, dd MMM yyyy HH:mm:ss zzz";
    
    // Logger for observability and debugging
    private static ILogger? _logger;

    /// <summary>
    /// Initialize the service with logger for structured logging
    /// </summary>
    /// <param name="logger">Logger instance for structured logging</param>
    public static void Initialize(ILogger? logger) => _logger = logger;
    /// <summary>
    /// Enhanced XML parsing with better error handling and automatic fixing
    /// </summary>
    /// <param name="xmlContent">Raw Jira XML content</param>
    /// <param name="logger">Optional logger for this operation (overrides static logger)</param>
    /// <returns>Parsing result with extracted story data</returns>
    public static JiraParseResult ParseJiraXml(string xmlContent, ILogger? logger = null)
    {
        var operationLogger = logger ?? _logger;
        var startTime = DateTime.UtcNow;
        var xmlSize = xmlContent?.Length ?? 0;
        var correlationId = Guid.NewGuid().ToString("N")[..8];
        
        using var scope = operationLogger?.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["XmlSize"] = xmlSize,
            ["Operation"] = "JiraXmlParsing"
        });
        
        operationLogger?.LogInformation("Starting Jira XML parsing {@Metrics}", new { xmlSize, correlationId });
        
        try
        {
            // Step 1: Validate original XML
            operationLogger?.LogDebug("Validating XML structure {@Context}", new { correlationId, step = "validation" });
            var validation = ValidateXmlStructure(xmlContent ?? string.Empty);
            
            string xmlToUse = xmlContent ?? string.Empty;
            bool wasXmlCleaned = false;
            
            // Step 2: If invalid, try cleaning
            if (!validation.IsValid)
            {
                operationLogger?.LogWarning("XML validation failed, attempting cleaning {@ValidationError}", 
                    new { correlationId, error = validation.Error });
                    
                xmlToUse = CleanJiraXmlInternal(xmlContent ?? string.Empty, operationLogger, correlationId);
                wasXmlCleaned = true;
                var cleanValidation = ValidateXmlStructure(xmlToUse);
                
                if (!cleanValidation.IsValid)
                {
                    operationLogger?.LogError("XML validation failed even after cleaning {@ValidationContext}", 
                        new { correlationId, originalError = validation.Error, cleanedError = cleanValidation.Error });
                        
                    return new JiraParseResult 
                    { 
                        IsSuccess = false, 
                        ErrorMessage = $"XML validation failed even after cleaning. Original: {validation.Error}. After cleaning: {cleanValidation.Error}" 
                    };
                }
                
                operationLogger?.LogInformation("XML cleaning successful {@CleaningResult}", 
                    new { correlationId, originalSize = (xmlContent ?? string.Empty).Length, cleanedSize = xmlToUse.Length });
            }

            // Step 3: Parse the XML
            operationLogger?.LogDebug("Parsing XML document {@Context}", new { correlationId, step = "parsing" });
            var doc = XDocument.Parse(xmlToUse);
            
            // Handle different Jira XML export formats
            var issueElement = doc.Descendants("item").FirstOrDefault() ?? 
                              doc.Descendants("issue").FirstOrDefault() ??
                              doc.Root;

            if (issueElement == null)
            {
                operationLogger?.LogError("No valid Jira issue structure found {@Context}", new { correlationId });
                return new JiraParseResult 
                { 
                    IsSuccess = false, 
                    ErrorMessage = "No valid Jira issue structure found in XML" 
                };
            }

            operationLogger?.LogDebug("Extracting story fields {@Context}", new { correlationId, step = "extraction" });
            
            // Extract core fields using multiple possible XML structures
            var story = new JiraStory
            {
                IssueKey = ExtractFieldValue(issueElement, operationLogger, correlationId, "key", "issue-key", "number"),
                Summary = ExtractFieldValue(issueElement, operationLogger, correlationId, "summary", "title", "subject"),
                Description = ExtractFieldValue(issueElement, operationLogger, correlationId, "description", "desc", "details"),
                StoryPoints = ExtractFieldValue(issueElement, operationLogger, correlationId, "story-points", "storypoints", "points"),
                Priority = ExtractFieldValue(issueElement, operationLogger, correlationId, "priority", "prio"),
                IssueType = ExtractFieldValue(issueElement, operationLogger, correlationId, "type", "issuetype", "issue-type"),
                AcceptanceCriteria = ExtractAcceptanceCriteria(issueElement, operationLogger, correlationId),
                Comments = ExtractComments(issueElement, operationLogger, correlationId),
                CustomFields = ExtractCustomFields(issueElement, operationLogger, correlationId)
            };

            var processingTime = DateTime.UtcNow - startTime;
            operationLogger?.LogInformation("Completed Jira XML parsing {@Result}", new 
            { 
                success = true, 
                processingTimeMs = processingTime.TotalMilliseconds,
                issueKey = story?.IssueKey,
                correlationId,
                wasXmlCleaned,
                customFieldCount = story?.CustomFields?.Count ?? 0,
                commentCount = story?.Comments?.Count ?? 0,
                acceptanceCriteriaCount = story?.AcceptanceCriteria?.Count ?? 0
            });

            return new JiraParseResult { IsSuccess = true, Story = story };
        }
        catch (XmlException xmlEx)
        {
            var processingTime = DateTime.UtcNow - startTime;
            operationLogger?.LogError(xmlEx, "XML parsing error {@ErrorContext}", new 
            { 
                correlationId, 
                lineNumber = xmlEx.LineNumber, 
                linePosition = xmlEx.LinePosition,
                processingTimeMs = processingTime.TotalMilliseconds,
                xmlSize
            });
            
            return new JiraParseResult 
            { 
                IsSuccess = false, 
                ErrorMessage = $"XML parsing error at line {xmlEx.LineNumber}, position {xmlEx.LinePosition}: {xmlEx.Message}" 
            };
        }
        catch (Exception ex)
        {
            var processingTime = DateTime.UtcNow - startTime;
            operationLogger?.LogError(ex, "Unexpected parsing error {@ErrorContext}", new 
            { 
                correlationId, 
                processingTimeMs = processingTime.TotalMilliseconds,
                xmlSize
            });
            
            return new JiraParseResult 
            { 
                IsSuccess = false, 
                ErrorMessage = $"Unexpected parsing error: {ex.Message}" 
            };
        }
    }

    /// <summary>
    /// Extracts field values from XML element using multiple possible field names with caching optimization
    /// </summary>
    /// <param name="element">The XML element to search in</param>
    /// <param name="logger">Logger for performance tracking</param>
    /// <param name="correlationId">Correlation ID for tracing</param>
    /// <param name="fieldNames">Possible field names to look for</param>
    /// <returns>The extracted field value or empty string if not found</returns>
    private static string ExtractFieldValue(XElement element, ILogger? logger, string correlationId, params string[] fieldNames)
    {
        var stopwatch = Stopwatch.StartNew();
        var fieldsAttempted = 0;
        
        // Cache key based on element content hash for performance
        var cacheKey = GenerateElementCacheKey(element);
        
        // Get or create element lookup dictionary
        var elementLookup = _elementCache.GetOrAdd(cacheKey, _ => {
            logger?.LogDebug("Building element lookup cache {@CacheMetrics}", new { correlationId, cacheKey });
            return BuildElementLookup(element);
        });
        
        bool cacheHit = _elementCache.ContainsKey(cacheKey);
        
        // Fast dictionary lookup instead of O(n) traversal
        foreach (var fieldName in fieldNames)
        {
            fieldsAttempted++;
            
            if (elementLookup.TryGetValue(fieldName.ToLowerInvariant(), out var foundElement))
            {
                var result = ExtractTextContent(foundElement);
                if (!string.IsNullOrEmpty(result))
                {
                    logger?.LogDebug("Field extraction success {@ExtractionMetrics}", new 
                    { 
                        fieldName, 
                        attemptNumber = fieldsAttempted, 
                        durationMs = stopwatch.ElapsedMilliseconds,
                        correlationId,
                        cacheHit,
                        method = "element_lookup"
                    });
                    return result;
                }
            }
            
            // Try attributes as fallback
            var attribute = element.Attribute(fieldName);
            if (attribute != null)
            {
                var result = attribute.Value?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(result))
                {
                    logger?.LogDebug("Field extraction success {@ExtractionMetrics}", new 
                    { 
                        fieldName, 
                        attemptNumber = fieldsAttempted, 
                        durationMs = stopwatch.ElapsedMilliseconds,
                        correlationId,
                        cacheHit,
                        method = "attribute_lookup"
                    });
                    return result;
                }
            }
        }
        
        logger?.LogDebug("Field extraction failed {@ExtractionMetrics}", new 
        { 
            fieldNames, 
            totalAttempts = fieldsAttempted, 
            durationMs = stopwatch.ElapsedMilliseconds,
            correlationId,
            cacheHit
        });
        
        return string.Empty;
    }

    /// <summary>
    /// Generates a cache key based on element characteristics
    /// </summary>
    /// <param name="element">Element to generate key for</param>
    /// <returns>Cache key string</returns>
    private static string GenerateElementCacheKey(XElement element)
    {
        // Simple hash based on element name and child count for reasonable uniqueness
        var childCount = element.Elements().Count();
        var descendantCount = element.Descendants().Count();
        return $"{element.Name.LocalName}_{childCount}_{descendantCount}_{element.GetHashCode()}";
    }

    /// <summary>
    /// Builds element lookup dictionary with single traversal
    /// </summary>
    /// <param name="root">Root element to traverse</param>
    /// <returns>Dictionary mapping element names to elements</returns>
    private static Dictionary<string, XElement> BuildElementLookup(XElement root)
    {
        var lookup = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
        
        // Single traversal to build complete lookup
        foreach (var element in root.DescendantsAndSelf())
        {
            var localName = element.Name.LocalName.ToLowerInvariant();
            if (!lookup.ContainsKey(localName))
            {
                lookup[localName] = element;
            }
        }
        
        return lookup;
    }

    /// <summary>
    /// Extracts text content from an XML element, handling CDATA sections
    /// </summary>
    /// <param name="element">The XML element to extract text from</param>
    /// <returns>The extracted text content</returns>
    private static string ExtractTextContent(XElement element)
    {
        if (element == null)
            return string.Empty;
        
        // Handle CDATA sections
        var cdataContent = element.Nodes().OfType<XCData>().FirstOrDefault();
        if (cdataContent != null)
        {
            return cdataContent.Value?.Trim() ?? string.Empty;
        }
        
        // Handle regular text content
        return element.Value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Extracts acceptance criteria from various possible locations in the XML with memory optimization
    /// </summary>
    /// <param name="element">The XML element to search in</param>
    /// <param name="logger">Logger for performance tracking</param>
    /// <param name="correlationId">Correlation ID for tracing</param>
    /// <returns>List of acceptance criteria strings</returns>
    private static List<string> ExtractAcceptanceCriteria(XElement element, ILogger? logger, string correlationId)
    {
        var stopwatch = Stopwatch.StartNew();
        var criteria = new List<string>();
        
        logger?.LogDebug("Starting acceptance criteria extraction {@Context}", new { correlationId, step = "acceptance_criteria" });
        
        // Try different field names where acceptance criteria might be stored
        var possibleFields = new[] { "acceptancecriteria", "acceptance-criteria", "acceptance_criteria", "ac", "criteria" };
        
        foreach (var fieldName in possibleFields)
        {
            var fieldElement = element.Descendants()
                .FirstOrDefault(e => string.Equals(e.Name.LocalName, fieldName, StringComparison.OrdinalIgnoreCase));
            
            if (fieldElement != null)
            {
                var text = ExtractTextContent(fieldElement);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    logger?.LogDebug("Found acceptance criteria in field {@FieldInfo}", new 
                    { 
                        fieldName, 
                        textLength = text.Length, 
                        correlationId 
                    });
                    
                    // Use Span<T> for zero-allocation text processing
                    ProcessCriteriaTextOptimized(text.AsSpan(), criteria);
                }
            }
        }
        
        var processingTime = stopwatch.ElapsedMilliseconds;
        logger?.LogDebug("Completed acceptance criteria extraction {@ExtractionResult}", new 
        { 
            criteriaCount = criteria.Count, 
            durationMs = processingTime,
            correlationId 
        });
        
        return criteria;
    }

    /// <summary>
    /// Processes acceptance criteria text using Span<T> to minimize memory allocations
    /// </summary>
    /// <param name="text">Text to process</param>
    /// <param name="criteria">List to add criteria to</param>
    private static void ProcessCriteriaTextOptimized(ReadOnlySpan<char> text, List<string> criteria)
    {
        var separators = new[] { '\n', '\r', ';', '*', '-' };
        var sb = new StringBuilder(256); // Pre-sized for typical criteria length
        
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            
            if (separators.Contains(c))
            {
                if (sb.Length > 0)
                {
                    var criterion = sb.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(criterion))
                    {
                        criteria.Add(criterion);
                    }
                    sb.Clear();
                }
            }
            else
            {
                sb.Append(c);
            }
        }
        
        // Handle final criterion
        if (sb.Length > 0)
        {
            var criterion = sb.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(criterion))
            {
                criteria.Add(criterion);
            }
        }
    }

    /// <summary>
    /// Extracts custom fields from the XML element
    /// </summary>
    /// <param name="element">The XML element to search in</param>
    /// <param name="logger">Logger for performance tracking</param>
    /// <param name="correlationId">Correlation ID for tracing</param>
    /// <returns>Dictionary of custom field names and values</returns>
    private static Dictionary<string, string> ExtractCustomFields(XElement element, ILogger? logger, string correlationId)
    {
        var stopwatch = Stopwatch.StartNew();
        var customFields = new Dictionary<string, string>();
        
        logger?.LogDebug("Starting custom fields extraction {@Context}", new { correlationId, step = "custom_fields" });
        
        var customFieldElements = element.Descendants()
                                       .Where(e => e.Name.LocalName.StartsWith("customfield") ||
                                                  e.Name.LocalName.StartsWith("custom-field") ||
                                                  e.Name.LocalName.Contains("field"));
        
        var processedFields = 0;
        foreach (var field in customFieldElements)
        {
            processedFields++;
            var fieldName = field.Attribute("name")?.Value ?? 
                           field.Attribute("id")?.Value ?? 
                           field.Name.LocalName;
            
            var fieldValue = ExtractTextContent(field);
            
            if (!string.IsNullOrWhiteSpace(fieldName) && !string.IsNullOrWhiteSpace(fieldValue))
            {
                customFields[fieldName] = fieldValue;
            }
        }
        
        var processingTime = stopwatch.ElapsedMilliseconds;
        logger?.LogDebug("Completed custom fields extraction {@ExtractionResult}", new 
        { 
            fieldsFound = customFields.Count, 
            fieldsProcessed = processedFields,
            durationMs = processingTime,
            correlationId 
        });
        
        return customFields;
    }

    /// <summary>
    /// Validates if a string matches the expected Jira ticket ID format (PROJECT-123)
    /// </summary>
    /// <param name="ticketId">The ticket ID to validate</param>
    /// <returns>True if the format is valid, false otherwise</returns>
    public static bool IsValidJiraTicketId(string ticketId)
    {
        if (string.IsNullOrWhiteSpace(ticketId))
            return false;

        // Pattern: PROJECT-123 (letters, dash, numbers)
        var pattern = @"^[A-Z]+-\d+$";
        return Regex.IsMatch(ticketId, pattern);
    }

    /// <summary>
    /// Validates XML structure and provides detailed error information
    /// </summary>
    /// <param name="xml">XML content to validate</param>
    /// <returns>Validation result with success status and error details</returns>
    private static (bool IsValid, string Error) ValidateXmlStructure(string xml)
    {
        try
        {
            XDocument.Parse(xml);
            return (true, string.Empty);
        }
        catch (XmlException ex)
        {
            return (false, $"Line {ex.LineNumber}, Position {ex.LinePosition}: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Basic XML cleaning functionality for internal use with security monitoring
    /// </summary>
    /// <param name="rawXml">Raw XML content to clean</param>
    /// <param name="logger">Logger for security monitoring</param>
    /// <param name="correlationId">Correlation ID for tracing</param>
    /// <returns>Cleaned XML content</returns>
    private static string CleanJiraXmlInternal(string rawXml, ILogger? logger = null, string correlationId = "")
    {
        if (string.IsNullOrWhiteSpace(rawXml))
            return rawXml;

        var startTime = DateTime.UtcNow;
        var originalSize = rawXml.Length;
        
        // Security monitoring - check for unusually large payloads
        if (originalSize > 50_000_000) // 50MB limit
        {
            logger?.LogWarning("Unusually large XML detected {@SecurityMetrics}", new 
            { 
                xmlSize = originalSize, 
                potentialThreat = "Large payload attack",
                correlationId
            });
        }
        
        // Security monitoring - check for suspicious patterns
        var suspiciousPatterns = new[]
        {
            (@"&{10,}", "Potential ReDoS attack - excessive ampersands"),
            (@"<{100,}", "Potential ReDoS attack - excessive angle brackets"),
            (@"<!ENTITY", "Potential XXE attack - entity declaration"),
            (@"<!--.*?-->", "Potential comment injection"),
            (@"\x00", "Null byte injection attempt")
        };
        
        foreach (var (pattern, threat) in suspiciousPatterns)
        {
            try
            {
                if (Regex.IsMatch(rawXml, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)))
                {
                    logger?.LogWarning("Suspicious XML pattern detected {@SecurityAlert}", new 
                    { 
                        pattern, 
                        threat, 
                        xmlLength = originalSize,
                        correlationId
                    });
                }
            }
            catch (RegexMatchTimeoutException)
            {
                logger?.LogWarning("Regex timeout during security scan {@SecurityAlert}", new 
                { 
                    pattern, 
                    threat = "Potential ReDoS attempt - regex timeout",
                    correlationId
                });
            }
        }

        logger?.LogDebug("Starting XML cleaning {@CleaningContext}", new { originalSize, correlationId });

        // Basic cleaning operations - use safe entity escaping to prevent ReDoS
        rawXml = EscapeXmlEntitiesSafe(rawXml);
        rawXml = RemoveInvalidXmlCharactersSafe(rawXml);
        
        var cleanedSize = rawXml.Length;
        var processingTime = DateTime.UtcNow - startTime;
        
        logger?.LogInformation("XML cleaning completed {@CleaningResult}", new 
        { 
            originalSize, 
            cleanedSize, 
            bytesRemoved = originalSize - cleanedSize,
            processingTimeMs = processingTime.TotalMilliseconds,
            correlationId
        });
        
        return rawXml;
    }

    /// <summary>
    /// Safely escapes XML entities without ReDoS vulnerability
    /// Uses character-by-character validation instead of complex regex lookahead
    /// </summary>
    /// <param name="input">Input string to escape</param>
    /// <returns>String with safely escaped XML entities</returns>
    private static string EscapeXmlEntitiesSafe(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        var result = new System.Text.StringBuilder(input.Length);
        var validEntities = new HashSet<string> { "amp", "lt", "gt", "quot", "apos" };
        
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '&')
            {
                // Check if this is a valid entity
                int semicolonIndex = input.IndexOf(';', i);
                if (semicolonIndex > i + 1 && semicolonIndex - i <= 10) // Limit entity length to prevent abuse
                {
                    string entity = input.Substring(i + 1, semicolonIndex - i - 1);
                    if (validEntities.Contains(entity) || IsValidNumericEntity(entity))
                    {
                        result.Append(input[i]); // Keep valid entity
                        continue;
                    }
                }
                result.Append("&amp;"); // Escape invalid &
            }
            else
            {
                result.Append(input[i]);
            }
        }
        return result.ToString();
    }

    /// <summary>
    /// Validates if a string is a valid numeric XML entity (&#123; or &#x1A;)
    /// </summary>
    /// <param name="entity">Entity string without & and ;</param>
    /// <returns>True if valid numeric entity</returns>
    private static bool IsValidNumericEntity(string entity)
    {
        if (string.IsNullOrEmpty(entity) || !entity.StartsWith("#"))
            return false;
            
        if (entity.Length > 8) // Reasonable limit for numeric entities
            return false;
            
        if (entity.Length > 1 && entity[1] == 'x')
        {
            // Hexadecimal entity &#xNN;
            for (int i = 2; i < entity.Length; i++)
            {
                char c = entity[i];
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                    return false;
            }
            return entity.Length > 2;
        }
        else
        {
            // Decimal entity &#123;
            for (int i = 1; i < entity.Length; i++)
            {
                if (entity[i] < '0' || entity[i] > '9')
                    return false;
            }
            return entity.Length > 1;
        }
    }

    /// <summary>
    /// Removes invalid XML characters safely without regex backtracking
    /// </summary>
    /// <param name="input">Input string to clean</param>
    /// <returns>String with invalid XML characters removed</returns>
    private static string RemoveInvalidXmlCharactersSafe(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        var result = new System.Text.StringBuilder(input.Length);
        
        foreach (char c in input)
        {
            // Valid XML characters: #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]
            if (c == 0x09 || c == 0x0A || c == 0x0D || 
                (c >= 0x20 && c <= 0xD7FF) ||
                (c >= 0xE000 && c <= 0xFFFD))
            {
                result.Append(c);
            }
            // Skip invalid characters
        }
        
        return result.ToString();
    }

    /// <summary>
    /// Extracts comments from the Jira XML comments section
    /// </summary>
    /// <param name="element">The XML element containing comments</param>
    /// <param name="logger">Logger for performance tracking</param>
    /// <param name="correlationId">Correlation ID for tracing</param>
    /// <returns>List of parsed comments</returns>
    private static List<JiraComment> ExtractComments(XElement element, ILogger? logger, string correlationId)
    {
        var stopwatch = Stopwatch.StartNew();
        var comments = new List<JiraComment>();
        
        logger?.LogDebug("Starting comments extraction {@Context}", new { correlationId, step = "comments" });
        
        var commentsElement = element.Element("comments") ?? element.Descendants("comments").FirstOrDefault();
        if (commentsElement == null) 
        {
            logger?.LogDebug("No comments element found {@Context}", new { correlationId });
            return comments;
        }
        
        var processedComments = 0;
        foreach (var commentElement in commentsElement.Elements("comment"))
        {
            processedComments++;
            var comment = new JiraComment
            {
                Id = commentElement.Attribute("id")?.Value ?? string.Empty,
                Author = ExtractAuthorName(commentElement.Attribute("author")?.Value ?? string.Empty),
                AuthorId = commentElement.Attribute("author")?.Value ?? string.Empty,
                Created = ParseCommentDate(commentElement.Attribute("created")?.Value ?? string.Empty),
                Content = commentElement.Value,
                CleanContent = CleanHtmlFromComment(commentElement.Value)
            };
            
            comments.Add(comment);
        }
        
        var processingTime = stopwatch.ElapsedMilliseconds;
        logger?.LogDebug("Completed comments extraction {@ExtractionResult}", new 
        { 
            commentsFound = comments.Count, 
            commentsProcessed = processedComments,
            durationMs = processingTime,
            correlationId 
        });
        
        return comments.OrderBy(c => c.Created).ToList();
    }

    /// <summary>
    /// Cleans HTML content from comments while preserving meaningful text
    /// Uses safe state machine approach to prevent ReDoS attacks
    /// </summary>
    /// <param name="htmlContent">Raw HTML comment content</param>
    /// <returns>Clean text content</returns>
    private static string CleanHtmlFromComment(string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent)) return string.Empty;
        
        // Use safe HTML cleaning with state machine to prevent ReDoS
        var cleanContent = RemoveHtmlTagsSafe(htmlContent);
        
        // Decode HTML entities safely
        cleanContent = System.Net.WebUtility.HtmlDecode(cleanContent);
        
        // Normalize whitespace safely
        cleanContent = NormalizeWhitespaceSafe(cleanContent);
        
        return cleanContent.Trim();
    }

    /// <summary>
    /// Removes HTML tags using a safe state machine approach instead of regex
    /// Prevents ReDoS attacks from malformed HTML tags
    /// </summary>
    /// <param name="htmlContent">HTML content to clean</param>
    /// <returns>Text content with HTML tags removed</returns>
    private static string RemoveHtmlTagsSafe(string htmlContent)
    {
        if (string.IsNullOrEmpty(htmlContent)) return htmlContent;
        
        var result = new System.Text.StringBuilder();
        bool insideTag = false;
        int tagDepth = 0;
        
        for (int i = 0; i < htmlContent.Length; i++)
        {
            char c = htmlContent[i];
            
            if (c == '<')
            {
                if (!insideTag)
                {
                    insideTag = true;
                    tagDepth = 1;
                    result.Append(' '); // Replace tag start with space
                }
                else
                {
                    tagDepth++; // Handle nested < characters
                }
            }
            else if (c == '>' && insideTag)
            {
                tagDepth--;
                if (tagDepth <= 0)
                {
                    insideTag = false;
                    tagDepth = 0;
                }
            }
            else if (!insideTag)
            {
                result.Append(c);
            }
            // Skip characters inside tags
        }
        
        return result.ToString();
    }

    /// <summary>
    /// Normalizes whitespace without using regex to prevent ReDoS
    /// </summary>
    /// <param name="input">Input string to normalize</param>
    /// <returns>String with normalized whitespace</returns>
    private static string NormalizeWhitespaceSafe(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        var result = new System.Text.StringBuilder();
        bool lastWasSpace = false;
        
        foreach (char c in input)
        {
            if (char.IsWhiteSpace(c))
            {
                if (!lastWasSpace)
                {
                    result.Append(' ');
                    lastWasSpace = true;
                }
            }
            else
            {
                result.Append(c);
                lastWasSpace = false;
            }
        }
        
        return result.ToString();
    }

    /// <summary>
    /// Extracts author display name from user links or accountId
    /// </summary>
    /// <param name="authorInfo">Author information from XML</param>
    /// <returns>Display name or accountId</returns>
    private static string ExtractAuthorName(string authorInfo)
    {
        if (string.IsNullOrWhiteSpace(authorInfo)) return "Unknown Author";
        
        // Extract from accountId format (557058:d005829d-f994-4adf-ac20-54fa72b78108)
        if (authorInfo.Contains(":"))
        {
            return authorInfo.Split(':').LastOrDefault() ?? authorInfo;
        }
        
        return authorInfo;
    }

    /// <summary>
    /// Parses comment creation date from various formats with smart caching
    /// </summary>
    /// <param name="dateString">Date string from XML</param>
    /// <returns>Parsed DateTime or MinValue if parsing fails</returns>
    private static DateTime ParseCommentDate(string dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString)) return DateTime.MinValue;
        
        // Try cached format for this pattern first
        var patternKey = GenerateDatePatternKey(dateString);
        if (_dateFormatCache.TryGetValue(patternKey, out var cachedFormat))
        {
            if (DateTime.TryParseExact(dateString, cachedFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var cachedResult))
            {
                return cachedResult;
            }
        }
        
        // Try most successful format first
        if (DateTime.TryParseExact(dateString, _mostSuccessfulFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fastResult))
        {
            CacheSuccessfulFormat(patternKey, _mostSuccessfulFormat);
            return fastResult;
        }
        
        // Fallback to format iteration
        var formats = new[]
        {
            "yyyy-MM-ddTHH:mm:ss.fffZ",       // ISO format (try second)
            "yyyy-MM-dd HH:mm:ss",            // Simple format (try third)
            "ddd, dd MMM yyyy HH:mm:ss zzz"   // Already tried above
        };
        
        foreach (var format in formats)
        {
            if (format == _mostSuccessfulFormat) continue; // Skip already tried
            
            if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                CacheSuccessfulFormat(patternKey, format);
                UpdateMostSuccessfulFormat(format);
                return result;
            }
        }
        
        // Final fallback to general parsing
        if (DateTime.TryParse(dateString, out var fallbackResult))
        {
            return fallbackResult;
        }
        
        return DateTime.MinValue;
    }

    /// <summary>
    /// Generates a pattern key based on date string characteristics
    /// </summary>
    /// <param name="dateString">Date string to analyze</param>
    /// <returns>Pattern key for caching</returns>
    private static string GenerateDatePatternKey(string dateString)
    {
        // Generate pattern key based on string characteristics
        var length = dateString.Length;
        var hasT = dateString.Contains('T');
        var hasZ = dateString.Contains('Z');
        var hasComma = dateString.Contains(',');
        
        return $"{length}_{hasT}_{hasZ}_{hasComma}";
    }

    /// <summary>
    /// Caches successful date format for pattern
    /// </summary>
    /// <param name="patternKey">Pattern key</param>
    /// <param name="format">Successful format</param>
    private static void CacheSuccessfulFormat(string patternKey, string format)
    {
        _dateFormatCache.TryAdd(patternKey, format);
    }

    /// <summary>
    /// Updates the most successful format globally
    /// </summary>
    /// <param name="format">Format that succeeded</param>
    private static void UpdateMostSuccessfulFormat(string format)
    {
        // Simple heuristic: update if we see this format succeeding
        _mostSuccessfulFormat = format;
    }
}
