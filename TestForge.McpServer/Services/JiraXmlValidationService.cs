using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using TestForge.McpServer.Models;

namespace TestForge.McpServer.Services;

/// <summary>
/// Service responsible for validating Jira XML structure and providing detailed diagnostic information
/// </summary>
public static class JiraXmlValidationService
{
    /// <summary>
    /// Validates Jira XML structure and provides detailed diagnostic information about parsing issues
    /// </summary>
    /// <param name="jiraXml">The Jira XML content to validate</param>
    /// <returns>JSON response containing validation results and diagnostic information</returns>
    public static string Validate(string jiraXml)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(jiraXml))
            {
                return JsonSerializer.Serialize(new { 
                    isValid = false, 
                    error = "Input XML is empty or null",
                    suggestions = new[] { "Provide valid Jira XML content" }
                }, new JsonSerializerOptions { WriteIndented = true });
            }

            var validationResult = ValidateXmlStructure(jiraXml);
            
            if (!validationResult.IsValid)
            {
                return JsonSerializer.Serialize(new {
                    isValid = false,
                    error = validationResult.Error,
                    originalXmlLength = jiraXml.Length,
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
                        xmlLength = jiraXml.Length,
                        containsCData = jiraXml.Contains("<![CDATA["),
                        potentialIssues = IdentifyPotentialIssues(jiraXml)
                    }
                }, new JsonSerializerOptions { WriteIndented = true });
            }

            return JsonSerializer.Serialize(new {
                isValid = true,
                message = "XML is valid and ready for parsing",
                xmlLength = jiraXml.Length,
                structure = AnalyzeXmlStructure(jiraXml),
                diagnostics = new {
                    hasCommentSection = jiraXml.Contains("<comments>"),
                    hasDescription = jiraXml.Contains("<description>"),
                    hasCustomFields = jiraXml.Contains("<customfields>"),
                    xmlLength = jiraXml.Length,
                    containsCData = jiraXml.Contains("<![CDATA[")
                }
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { 
                isValid = false, 
                error = $"Validation failed: {ex.Message}",
                suggestion = "Check XML format and structure"
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    /// <summary>
    /// Validates the XML structure and returns detailed validation result
    /// </summary>
    /// <param name="xml">XML content to validate</param>
    /// <returns>Validation result with success status and error details</returns>
    private static (bool IsValid, string Error) ValidateXmlStructure(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            return (true, string.Empty);
        }
        catch (XmlException xmlEx)
        {
            return (false, $"XML parsing error at line {xmlEx.LineNumber}, position {xmlEx.LinePosition}: {xmlEx.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Validation error: {ex.Message}");
        }
    }

    /// <summary>
    /// Analyzes the XML structure and returns information about the document
    /// </summary>
    /// <param name="xml">XML content to analyze</param>
    /// <returns>Structure analysis information</returns>
    private static object AnalyzeXmlStructure(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            var root = doc.Root;
            
            return new {
                rootElement = root?.Name.LocalName ?? "unknown",
                hasIssueElement = doc.Descendants("item").Any() || doc.Descendants("issue").Any(),
                elementCount = doc.Descendants().Count(),
                hasComments = doc.Descendants("comments").Any(),
                hasDescription = doc.Descendants("description").Any(),
                hasCustomFields = doc.Descendants("customfields").Any(),
                hasAttachments = doc.Descendants("attachments").Any()
            };
        }
        catch
        {
            return new { error = "Could not analyze structure" };
        }
    }

    /// <summary>
    /// Identifies potential issues in the XML content
    /// </summary>
    /// <param name="xml">XML content to analyze</param>
    /// <returns>List of potential issues</returns>
    private static string[] IdentifyPotentialIssues(string xml)
    {
        var issues = new List<string>();

        if (xml.Contains("rel=\"") && xml.Contains("rel=\"") && 
            xml.Split("rel=\"").Length > xml.Split("rel=\"").Distinct().Count())
        {
            issues.Add("Duplicate 'rel' attributes detected");
        }

        if (xml.Contains("&") && !xml.Contains("&amp;") && !xml.Contains("&lt;") && !xml.Contains("&gt;"))
        {
            issues.Add("Unescaped ampersands detected");
        }

        if (xml.Contains("<comment>") && !xml.Contains("</comment>"))
        {
            issues.Add("Missing closing tags for comment elements");
        }

        if (xml.Contains("<![CDATA[") && xml.Contains("<![CDATA[]]>"))
        {
            issues.Add("Empty CDATA sections detected");
        }

        return issues.ToArray();
    }
}
