using System;
using TestForge.McpServer.Services;

namespace TestForge.SecurityTests
{
    /// <summary>
    /// Functional regression tests to ensure security fixes don't break existing functionality
    /// </summary>
    class FunctionalTests
    {
        public static bool RunAllFunctionalTests()
        {
            Console.WriteLine("=== Functional Regression Tests ===");
            Console.WriteLine();
            
            bool allPassed = true;
            
            allPassed &= TestBasicJiraXmlParsing();
            allPassed &= TestEntityPreservation();
            allPassed &= TestHtmlCommentCleaning();
            allPassed &= TestTicketIdValidation();
            
            return allPassed;
        }

        static bool TestBasicJiraXmlParsing()
        {
            Console.WriteLine("Testing Basic Jira XML Parsing...");
            
            var xmlContent = @"
                <root>
                    <item>
                        <key>PROJ-456</key>
                        <summary>Sample User Story</summary>
                        <description>As a user, I want to test functionality</description>
                        <priority>High</priority>
                        <type>Story</type>
                    </item>
                </root>";
            
            var result = JiraStoryParsingService.ParseJiraXml(xmlContent);
            
            bool passed = result.IsSuccess &&
                         result.Story.IssueKey == "PROJ-456" &&
                         result.Story.Summary == "Sample User Story" &&
                         result.Story.Description == "As a user, I want to test functionality" &&
                         result.Story.Priority == "High" &&
                         result.Story.IssueType == "Story";
            
            Console.WriteLine($"  {(passed ? "✅" : "❌")} Basic parsing: {passed}");
            return passed;
        }

        static bool TestEntityPreservation()
        {
            Console.WriteLine("Testing Entity Preservation...");
            
            var xmlContent = @"
                <root>
                    <item>
                        <key>TEST-789</key>
                        <description>Content with &amp; and &lt;tag&gt; and &#123; entities</description>
                    </item>
                </root>";
            
            var result = JiraStoryParsingService.ParseJiraXml(xmlContent);
            
            bool passed = result.IsSuccess &&
                         result.Story.Description.Contains("&") &&
                         result.Story.Description.Contains("<") &&
                         result.Story.Description.Contains(">");
            
            Console.WriteLine($"  {(passed ? "✅" : "❌")} Entity preservation: {passed}");
            Console.WriteLine($"    Description: {result.Story?.Description}");
            return passed;
        }

        static bool TestHtmlCommentCleaning()
        {
            Console.WriteLine("Testing HTML Comment Cleaning...");
            
            var xmlContent = @"
                <root>
                    <comments>
                        <comment id='1' author='testuser' created='2025-01-01 12:00:00'>
                            <p>This is a <strong>formatted</strong> comment with <em>HTML</em> tags.</p>
                        </comment>
                    </comments>
                </root>";
            
            var result = JiraStoryParsingService.ParseJiraXml(xmlContent);
            
            bool passed = result.IsSuccess &&
                         result.Story.Comments.Count > 0 &&
                         result.Story.Comments[0].CleanContent.Contains("formatted") &&
                         result.Story.Comments[0].CleanContent.Contains("comment") &&
                         !result.Story.Comments[0].CleanContent.Contains("<") &&
                         !result.Story.Comments[0].CleanContent.Contains(">");
            
            Console.WriteLine($"  {(passed ? "✅" : "❌")} HTML cleaning: {passed}");
            Console.WriteLine($"    Clean content: {result.Story?.Comments?[0]?.CleanContent}");
            return passed;
        }

        static bool TestTicketIdValidation()
        {
            Console.WriteLine("Testing Ticket ID Validation...");
            
            var validIds = new[] { "PROJ-123", "ABC-1", "TEST-9999" };
            var invalidIds = new[] { "invalid", "PROJ123", "proj-123", "PROJ-", "-123" };
            
            bool allValidPassed = true;
            bool allInvalidPassed = true;
            
            foreach (var id in validIds)
            {
                bool isValid = JiraStoryParsingService.IsValidJiraTicketId(id);
                Console.WriteLine($"    {id}: {(isValid ? "✅" : "❌")} (should be valid)");
                allValidPassed &= isValid;
            }
            
            foreach (var id in invalidIds)
            {
                bool isValid = JiraStoryParsingService.IsValidJiraTicketId(id);
                Console.WriteLine($"    {id}: {(!isValid ? "✅" : "❌")} (should be invalid)");
                allInvalidPassed &= !isValid;
            }
            
            bool passed = allValidPassed && allInvalidPassed;
            Console.WriteLine($"  {(passed ? "✅" : "❌")} Ticket ID validation: {passed}");
            return passed;
        }
    }
}
