using System.Globalization;
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

        // Basic cleaning operations
        rawXml = Regex.Replace(rawXml, @"&(?!(?:amp|lt|gt|quot|apos|#\d+|#x[0-9a-fA-F]+);)", "&amp;");
        rawXml = Regex.Replace(rawXml, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");
        
        return rawXml;
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
    /// </summary>
    /// <param name="htmlContent">Raw HTML comment content</param>
    /// <returns>Clean text content</returns>
    private static string CleanHtmlFromComment(string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(htmlContent)) return string.Empty;
        
        // Remove HTML tags but preserve structure
        var cleanContent = Regex.Replace(htmlContent, @"<[^>]+>", " ");
        
        // Decode HTML entities
        cleanContent = System.Net.WebUtility.HtmlDecode(cleanContent);
        
        // Normalize whitespace
        cleanContent = Regex.Replace(cleanContent, @"\s+", " ");
        
        return cleanContent.Trim();
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
