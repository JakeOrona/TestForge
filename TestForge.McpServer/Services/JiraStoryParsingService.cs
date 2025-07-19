using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml;
using TestForge.McpServer.Models;

namespace TestForge.McpServer.Services;

/// <summary>
/// Service responsible for parsing Jira XML content and extracting story data
/// </summary>
public static class JiraStoryParsingService
{
    /// <summary>
    /// Enhanced XML parsing with better error handling and automatic fixing
    /// </summary>
    /// <param name="xmlContent">Raw Jira XML content</param>
    /// <returns>Parsing result with extracted story data</returns>
    public static JiraParseResult ParseJiraXml(string xmlContent)
    {
        try
        {
            // Step 1: Validate original XML
            var validation = ValidateXmlStructure(xmlContent);
            
            string xmlToUse = xmlContent;
            
            // Step 2: If invalid, try cleaning
            if (!validation.IsValid)
            {
                xmlToUse = CleanJiraXmlInternal(xmlContent);
                var cleanValidation = ValidateXmlStructure(xmlToUse);
                
                if (!cleanValidation.IsValid)
                {
                    return new JiraParseResult 
                    { 
                        IsSuccess = false, 
                        ErrorMessage = $"XML validation failed even after cleaning. Original: {validation.Error}. After cleaning: {cleanValidation.Error}" 
                    };
                }
            }

            // Step 3: Parse the XML
            var doc = XDocument.Parse(xmlToUse);
            
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
                Comments = ExtractComments(issueElement),
                CustomFields = ExtractCustomFields(issueElement)
            };

            return new JiraParseResult { IsSuccess = true, Story = story };
        }
        catch (XmlException xmlEx)
        {
            return new JiraParseResult 
            { 
                IsSuccess = false, 
                ErrorMessage = $"XML parsing error at line {xmlEx.LineNumber}, position {xmlEx.LinePosition}: {xmlEx.Message}" 
            };
        }
        catch (Exception ex)
        {
            return new JiraParseResult 
            { 
                IsSuccess = false, 
                ErrorMessage = $"Unexpected parsing error: {ex.Message}" 
            };
        }
    }

    /// <summary>
    /// Extracts field values from XML element using multiple possible field names
    /// </summary>
    /// <param name="element">The XML element to search in</param>
    /// <param name="fieldNames">Possible field names to look for</param>
    /// <returns>The extracted field value or empty string if not found</returns>
    private static string ExtractFieldValue(XElement element, params string[] fieldNames)
    {
        foreach (var fieldName in fieldNames)
        {
            // Try direct child elements
            var childElement = element.Element(fieldName);
            if (childElement != null)
            {
                return ExtractTextContent(childElement);
            }
            
            // Try with case-insensitive search
            var caseInsensitiveElement = element.Elements()
                .FirstOrDefault(e => string.Equals(e.Name.LocalName, fieldName, StringComparison.OrdinalIgnoreCase));
            if (caseInsensitiveElement != null)
            {
                return ExtractTextContent(caseInsensitiveElement);
            }
            
            // Try descendants
            var descendantElement = element.Descendants()
                .FirstOrDefault(e => string.Equals(e.Name.LocalName, fieldName, StringComparison.OrdinalIgnoreCase));
            if (descendantElement != null)
            {
                return ExtractTextContent(descendantElement);
            }
            
            // Try attributes
            var attribute = element.Attribute(fieldName);
            if (attribute != null)
            {
                return attribute.Value?.Trim() ?? string.Empty;
            }
        }
        
        return string.Empty;
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
    /// Extracts acceptance criteria from various possible locations in the XML
    /// </summary>
    /// <param name="element">The XML element to search in</param>
    /// <returns>List of acceptance criteria strings</returns>
    private static List<string> ExtractAcceptanceCriteria(XElement element)
    {
        var criteria = new List<string>();
        
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
                    var splitCriteria = text.Split(new[] { '\n', '\r', ';', '*', '-' }, 
                                                 StringSplitOptions.RemoveEmptyEntries)
                                           .Select(c => c.Trim())
                                           .Where(c => !string.IsNullOrWhiteSpace(c));
                    
                    criteria.AddRange(splitCriteria);
                }
            }
        }
        
        return criteria;
    }

    /// <summary>
    /// Extracts custom fields from the XML element
    /// </summary>
    /// <param name="element">The XML element to search in</param>
    /// <returns>Dictionary of custom field names and values</returns>
    private static Dictionary<string, string> ExtractCustomFields(XElement element)
    {
        var customFields = new Dictionary<string, string>();
        
        var customFieldElements = element.Descendants()
                                       .Where(e => e.Name.LocalName.StartsWith("customfield") ||
                                                  e.Name.LocalName.StartsWith("custom-field") ||
                                                  e.Name.LocalName.Contains("field"));
        
        foreach (var field in customFieldElements)
        {
            var fieldName = field.Attribute("name")?.Value ?? 
                           field.Attribute("id")?.Value ?? 
                           field.Name.LocalName;
            
            var fieldValue = ExtractTextContent(field);
            
            if (!string.IsNullOrWhiteSpace(fieldName) && !string.IsNullOrWhiteSpace(fieldValue))
            {
                customFields[fieldName] = fieldValue;
            }
        }
        
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
    /// Basic XML cleaning functionality for internal use
    /// </summary>
    /// <param name="rawXml">Raw XML content to clean</param>
    /// <returns>Cleaned XML content</returns>
    private static string CleanJiraXmlInternal(string rawXml)
    {
        if (string.IsNullOrWhiteSpace(rawXml))
            return rawXml;

        // Basic cleaning operations - use safe entity escaping to prevent ReDoS
        rawXml = EscapeXmlEntitiesSafe(rawXml);
        rawXml = RemoveInvalidXmlCharactersSafe(rawXml);
        
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
    /// <returns>List of parsed comments</returns>
    private static List<JiraComment> ExtractComments(XElement element)
    {
        var comments = new List<JiraComment>();
        
        var commentsElement = element.Element("comments") ?? element.Descendants("comments").FirstOrDefault();
        if (commentsElement == null) return comments;
        
        foreach (var commentElement in commentsElement.Elements("comment"))
        {
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
    /// Parses comment creation date from various formats
    /// </summary>
    /// <param name="dateString">Date string from XML</param>
    /// <returns>Parsed DateTime or MinValue if parsing fails</returns>
    private static DateTime ParseCommentDate(string dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString)) return DateTime.MinValue;
        
        // Try multiple date formats commonly used in Jira exports
        var formats = new[]
        {
            "ddd, dd MMM yyyy HH:mm:ss zzz",  // Mon, 30 Jun 2025 16:46:04 -0500
            "yyyy-MM-ddTHH:mm:ss.fffZ",       // ISO format
            "yyyy-MM-dd HH:mm:ss",            // Simple format
        };
        
        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }
        }
        
        // Fallback to general parsing
        return DateTime.TryParse(dateString, out var fallbackResult) ? fallbackResult : DateTime.MinValue;
    }
}
