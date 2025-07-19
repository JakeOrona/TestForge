using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using TestForge.McpServer.Services;
using TestForge.McpServer.Models;

namespace TestForge.McpServer.Tests
{
    /// <summary>
    /// Security-focused tests for JiraStoryParsingService to prevent vulnerabilities
    /// </summary>
    public class JiraStoryParsingServiceSecurityTests
    {
        [Fact]
        public void ParseJiraXml_WithMaliciousXmlBomb_ShouldNotCauseDoS()
        {
            // Arrange - XML bomb attempt (many nested entities)
            var xmlBomb = @"<?xml version=""1.0""?>
<!DOCTYPE lolz [
  <!ENTITY lol ""lol"">
  <!ENTITY lol2 ""&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;"">
  <!ENTITY lol3 ""&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;"">
]>
<root>&lol3;</root>";

            // Act & Assert - Should not crash or consume excessive resources
            var result = JiraStoryParsingService.ParseJiraXml(xmlBomb);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void ParseJiraXml_WithExternalEntityInjection_ShouldBeBlocked()
        {
            // Arrange - XXE attack attempt
            var xxeAttempt = @"<?xml version=""1.0""?>
<!DOCTYPE root [
  <!ENTITY xxe SYSTEM ""file:///etc/passwd"">
]>
<root>
  <item>
    <key>&xxe;</key>
  </item>
</root>";

            // Act
            var result = JiraStoryParsingService.ParseJiraXml(xxeAttempt);
            
            // Assert - Should fail safely without reading external files
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void ParseJiraXml_WithReDoSPatterns_ShouldCompleteQuickly()
        {
            // Arrange - Patterns that could cause ReDoS in regex
            var maliciousPatterns = new[]
            {
                new string('a', 10000) + "X", // Long string that doesn't match expected patterns
                "((((((((((", // Unbalanced parentheses
                "<tag" + new string('>', 1000), // Malformed tags
                "&" + new string('a', 1000) + ";", // Long entity names
            };

            foreach (var pattern in maliciousPatterns)
            {
                var xml = $@"<root><item><key>{pattern}</key></item></root>";
                
                // Act - Should complete quickly
                var startTime = DateTime.UtcNow;
                var result = JiraStoryParsingService.ParseJiraXml(xml);
                var duration = DateTime.UtcNow - startTime;
                
                // Assert - Should not take more than reasonable time
                Assert.True(duration.TotalSeconds < 5, $"Processing took too long: {duration.TotalSeconds}s for pattern length {pattern.Length}");
            }
        }

        [Fact]
        public void IsValidJiraTicketId_WithMaliciousInput_ShouldHandleSafely()
        {
            // Arrange - Various malicious inputs
            var maliciousInputs = new[]
            {
                null,
                "",
                new string('A', 100000), // Very long string
                "PROJ-" + new string('1', 100000), // Very long number
                "<script>alert('xss')</script>",
                "'; DROP TABLE tickets; --",
                "\0\0\0\0", // Null bytes
                "PROJ-123\r\nINJECTED-456", // CRLF injection
            };

            foreach (var input in maliciousInputs)
            {
                // Act & Assert - Should not crash
                var result = JiraStoryParsingService.IsValidJiraTicketId(input ?? string.Empty);
                
                // Valid ticket IDs should only match PROJECT-123 format
                if (input != null && System.Text.RegularExpressions.Regex.IsMatch(input, @"^[A-Z]+-\d+$"))
                {
                    Assert.True(result);
                }
                else
                {
                    Assert.False(result);
                }
            }
        }

        [Theory]
        [InlineData("&lt;script&gt;alert('xss')&lt;/script&gt;")]
        [InlineData("&amp;amp;amp;amp;amp;amp;")]
        [InlineData("&#x3C;script&#x3E;")]
        [InlineData("&#60;script&#62;")]
        [InlineData("&nonexistent;")]
        [InlineData("&amp")]
        public void EscapeXmlEntitiesSafe_WithMaliciousEntities_ShouldEscapeSafely(string input)
        {
            // This tests the internal method through ParseJiraXml
            var xml = $@"<root><item><description>{input}</description></item></root>";
            
            // Act
            var result = JiraStoryParsingService.ParseJiraXml(xml);
            
            // Assert - Should not contain unescaped script tags or malicious content
            if (result.IsSuccess && result.Story != null)
            {
                Assert.DoesNotContain("<script>", result.Story.Description);
                Assert.DoesNotContain("javascript:", result.Story.Description.ToLower());
            }
        }

        [Fact]
        public void ParseJiraXml_WithDeeplyNestedXml_ShouldNotCauseStackOverflow()
        {
            // Arrange - Create deeply nested XML
            var depth = 1000;
            var nestedXml = "<root>";
            for (int i = 0; i < depth; i++)
            {
                nestedXml += $"<level{i}>";
            }
            nestedXml += "<item><key>TEST-123</key></item>";
            for (int i = depth - 1; i >= 0; i--)
            {
                nestedXml += $"</level{i}>";
            }
            nestedXml += "</root>";

            // Act & Assert - Should not cause stack overflow
            var result = JiraStoryParsingService.ParseJiraXml(nestedXml);
            
            // May succeed or fail, but should not crash
            Assert.NotNull(result);
        }

        [Fact]
        public void ParseJiraXml_WithHugeCDataSection_ShouldHandleGracefully()
        {
            // Arrange - Large CDATA section that could cause memory issues
            var largeCData = new string('A', 1000000); // 1MB of data
            var xml = $@"<root>
                <item>
                    <key>TEST-123</key>
                    <description><![CDATA[{largeCData}]]></description>
                </item>
            </root>";

            // Act
            var startTime = DateTime.UtcNow;
            var result = JiraStoryParsingService.ParseJiraXml(xml);
            var duration = DateTime.UtcNow - startTime;

            // Assert - Should complete in reasonable time
            Assert.True(duration.TotalSeconds < 10, $"Processing large CDATA took too long: {duration.TotalSeconds}s");
            
            if (result.IsSuccess)
            {
                Assert.NotNull(result.Story);
                Assert.Equal("TEST-123", result.Story.IssueKey);
            }
        }

        [Fact]
        public void ParseJiraXml_WithMaliciousCommentContent_ShouldSanitize()
        {
            // Arrange - Comments with potentially malicious content
            var xml = @"<root>
                <item>
                    <key>TEST-123</key>
                    <comments>
                        <comment id=""1"" author=""test"" created=""2023-01-01 12:00:00"">
                            &lt;script&gt;alert('xss')&lt;/script&gt;
                            &lt;img src=x onerror=alert('xss')&gt;
                            javascript:alert('xss')
                        </comment>
                    </comments>
                </item>
            </root>";

            // Act
            var result = JiraStoryParsingService.ParseJiraXml(xml);

            // Assert
            if (result.IsSuccess && result.Story?.Comments?.Any() == true)
            {
                var comment = result.Story.Comments.First();
                Assert.DoesNotContain("<script>", comment.CleanContent);
                Assert.DoesNotContain("<img", comment.CleanContent);
                Assert.DoesNotContain("javascript:", comment.CleanContent.ToLower());
            }
        }
    }
}
