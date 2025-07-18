using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace TestForge.McpServer.Services;

/// <summary>
/// Service responsible for cleaning raw Jira XML exports to fix common formatting issues
/// </summary>
public static class JiraXmlCleaningService
{
    /// <summary>
    /// Cleans raw Jira XML exports to fix common formatting issues, missing closing tags, and malformed content
    /// </summary>
    /// <param name="rawXml">The raw Jira XML content to clean</param>
    /// <returns>JSON response containing cleaned XML and processing information</returns>
    public static string Clean(string rawXml)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(rawXml))
            {
                return JsonSerializer.Serialize(new { 
                    success = false, 
                    error = "Input XML is empty" 
                }, new JsonSerializerOptions { WriteIndented = true });
            }

            var originalLength = rawXml.Length;
            var cleanedXml = CleanJiraXmlInternal(rawXml);
            var cleanedLength = cleanedXml.Length;

            // Validate the cleaned XML
            var validationResult = ValidateXmlStructure(cleanedXml);

            return JsonSerializer.Serialize(new {
                success = validationResult.IsValid,
                originalLength = originalLength,
                cleanedLength = cleanedLength,
                bytesChanged = Math.Abs(originalLength - cleanedLength),
                isValid = validationResult.IsValid,
                validationError = validationResult.Error,
                cleanedXml = cleanedXml,
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
            return JsonSerializer.Serialize(new { 
                success = false, 
                error = $"Cleaning failed: {ex.Message}" 
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    /// <summary>
    /// Enhanced XML cleaning to handle comment section and structural issues
    /// </summary>
    /// <param name="rawXml">Raw XML content to clean</param>
    /// <returns>Cleaned XML content</returns>
    private static string CleanJiraXmlInternal(string rawXml)
    {
        if (string.IsNullOrWhiteSpace(rawXml))
            return rawXml;

        // Step 1: Fix missing closing tags for comments section
        rawXml = FixMissingCommentClosingTags(rawXml);
        
        // Step 2: Fix duplicate attributes BEFORE processing comments content
        rawXml = FixDuplicateAttributes(rawXml);
        
        // Step 3: Clean malformed comment content
        rawXml = CleanCommentContent(rawXml);
        
        // Step 4: Fix malformed HTML entities
        rawXml = Regex.Replace(rawXml, @"&(?!(?:amp|lt|gt|quot|apos|#\d+|#x[0-9a-fA-F]+);)", "&amp;");
        
        // Step 5: Remove invalid XML characters
        rawXml = Regex.Replace(rawXml, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");
        
        // Step 6: Remove empty CDATA sections
        rawXml = Regex.Replace(rawXml, 
            @"<!\[CDATA\[\s*\]\]>", 
            "", 
            RegexOptions.IgnoreCase);
        
        // Step 7: Fix nested XML structure issues
        rawXml = FixNestedXmlStructure(rawXml);
        
        return rawXml;
    }

    /// <summary>
    /// Fixes duplicate attributes in XML elements
    /// </summary>
    /// <param name="xml">XML content to fix</param>
    /// <returns>XML with duplicate attributes removed</returns>
    private static string FixDuplicateAttributes(string xml)
    {
        // Fix duplicate rel attributes
        xml = Regex.Replace(xml, 
            @"rel=""[^""]*""\s+([^>]*?)rel=""([^""]*?)""", 
            @"rel=""$2"" $1", 
            RegexOptions.IgnoreCase);
        
        // Fix duplicate class attributes
        xml = Regex.Replace(xml, 
            @"class=""[^""]*""\s+([^>]*?)class=""([^""]*?)""", 
            @"class=""$2"" $1", 
            RegexOptions.IgnoreCase);
        
        // Fix duplicate data-account-id attributes
        xml = Regex.Replace(xml, 
            @"data-account-id=""[^""]*""\s+([^>]*?)data-account-id=""([^""]*?)""", 
            @"data-account-id=""$2"" $1", 
            RegexOptions.IgnoreCase);
        
        // Fix duplicate accountid attributes
        xml = Regex.Replace(xml, 
            @"accountid=""[^""]*""\s+([^>]*?)accountid=""([^""]*?)""", 
            @"accountid=""$2"" $1", 
            RegexOptions.IgnoreCase);
        
        return xml;
    }

    /// <summary>
    /// Fixes missing closing tags for comments section
    /// </summary>
    /// <param name="xml">XML content to fix</param>
    /// <returns>XML with fixed closing tags</returns>
    private static string FixMissingCommentClosingTags(string xml)
    {
        // Only fix if comments section is missing its closing tag
        // Pattern to find <comments> that runs until another major element or end of document
        var unclosedCommentsPattern = @"<comments[^>]*>((?:(?!<\/comments>)[\s\S])*?)(?=<\/item>|<issuelinks>|<attachments>|<subtasks>|<customfields>|</channel>|$)";
        
        // First check if there's actually a comments section missing closing tag
        var hasUnclosedComments = Regex.IsMatch(xml, unclosedCommentsPattern, RegexOptions.IgnoreCase);
        
        if (hasUnclosedComments)
        {
            xml = Regex.Replace(xml, unclosedCommentsPattern, match =>
            {
                var commentsContent = match.Groups[1].Value;
                return $"<comments>{commentsContent}</comments>";
            }, RegexOptions.IgnoreCase);
        }
        
        return xml;
    }

    /// <summary>
    /// Cleans malformed content within the entire comments section
    /// </summary>
    /// <param name="xml">XML content to clean</param>
    /// <returns>XML with cleaned comment content</returns>
    private static string CleanCommentContent(string xml)
    {
        // Find the entire comments section using a more robust pattern
        // Handle both empty and non-empty comments sections
        var commentsPattern = @"<comments[^>]*>(.*?)</comments>";
        
        return Regex.Replace(xml, commentsPattern, match =>
        {
            var commentsContent = match.Groups[1].Value;
            
            // Always wrap complex comments content in CDATA for safety
            if (!string.IsNullOrWhiteSpace(commentsContent))
            {
                // Clean the content before wrapping in CDATA
                var cleanedContent = CleanCommentsContentForCdata(commentsContent);
                return $"<comments><![CDATA[{cleanedContent}]]></comments>";
            }
            
            // Return empty comments section as is
            return "<comments></comments>";
        }, RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }

    /// <summary>
    /// Cleans content specifically for CDATA wrapping to prevent nested CDATA issues
    /// </summary>
    /// <param name="content">Content to clean for CDATA wrapping</param>
    /// <returns>Cleaned content safe for CDATA</returns>
    private static string CleanCommentsContentForCdata(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return content;
        
        // Remove existing CDATA sections to prevent nesting
        content = Regex.Replace(content, @"<!\[CDATA\[(.*?)\]\]>", "$1", RegexOptions.Singleline);
        
        // Handle the ]]> sequence that would break CDATA - this is critical
        // Replace any ]]> with ]]&gt; to prevent CDATA termination
        content = content.Replace("]]>", "]]&gt;");
        
        // Also handle other potentially problematic sequences
        content = content.Replace("]]", "]]");  // Keep as is, it's only ]]> that's problematic
        
        // Clean up any remaining XML escaping since we're putting it in CDATA
        // But be careful with HTML entities that make sense in HTML context
        content = content.Replace("&amp;", "&");
        content = content.Replace("&lt;", "<");
        content = content.Replace("&gt;", ">");
        content = content.Replace("&quot;", "\"");
        content = content.Replace("&apos;", "'");
        
        // Handle problematic Unicode characters that might break XML parsing
        content = Regex.Replace(content, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");
        
        // Ensure line breaks are preserved properly
        content = content.Replace("\r\n", "\n").Replace("\r", "\n");
        
        // Make sure there are no unescaped control characters
        content = Regex.Replace(content, @"[\uE000-\uF8FF]", ""); // Private use area
        
        return content;
    }

    /// <summary>
    /// Fixes nested XML structure issues
    /// </summary>
    /// <param name="xml">XML content to fix</param>
    /// <returns>XML with fixed nested structure</returns>
    private static string FixNestedXmlStructure(string xml)
    {
        // Remove any duplicate consecutive closing tags for comments
        xml = Regex.Replace(xml, @"<\/comments>\s*<\/comments>", "</comments>", RegexOptions.IgnoreCase);
        
        // Remove any orphaned closing tags that might be left over
        xml = Regex.Replace(xml, @"<\/comment>\s*<\/comments>\s*<\/item>", "</comments></item>", RegexOptions.IgnoreCase);
        
        // Clean up any malformed comments structure that might remain
        xml = Regex.Replace(xml, @"<comments>\s*(?=<\/comments>)", "<comments>", RegexOptions.IgnoreCase);
        
        return xml;
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
}
