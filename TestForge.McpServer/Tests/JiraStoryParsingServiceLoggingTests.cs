using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using TestForge.McpServer.Services;
using TestForge.McpServer.Models;

namespace TestForge.McpServer.Tests
{
    /// <summary>
    /// Tests for logging functionality in JiraStoryParsingService
    /// </summary>
    public class JiraStoryParsingServiceLoggingTests
    {
        [Fact]
        public void ParseJiraXml_WithLogger_ShouldLogSuccessfully()
        {
            // Arrange
            var logger = NullLogger.Instance;
            JiraStoryParsingService.Initialize(logger);
            
            var validXml = @"<?xml version=""1.0""?>
<root>
  <item>
    <key>TEST-123</key>
    <summary>Test Summary</summary>
    <description>Test Description</description>
  </item>
</root>";

            // Act
            var result = JiraStoryParsingService.ParseJiraXml(validXml);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Story);
            Assert.Equal("TEST-123", result.Story.IssueKey);
            Assert.Equal("Test Summary", result.Story.Summary);
        }

        [Fact]
        public void ParseJiraXml_WithoutLogger_ShouldStillWork()
        {
            // Arrange
            var validXml = @"<?xml version=""1.0""?>
<root>
  <item>
    <key>TEST-456</key>
    <summary>Test Summary Without Logger</summary>
    <description>Test Description</description>
  </item>
</root>";

            // Act - Call without initializing logger
            var result = JiraStoryParsingService.ParseJiraXml(validXml);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Story);
            Assert.Equal("TEST-456", result.Story.IssueKey);
        }

        [Fact]
        public void ParseJiraXml_WithInvalidXml_ShouldLogAndReturnError()
        {
            // Arrange
            var logger = NullLogger.Instance;
            var invalidXml = @"<?xml version=""1.0""?>
<root>
  <item>
    <key>TEST-789
  </item>
</root>";

            // Act
            var result = JiraStoryParsingService.ParseJiraXml(invalidXml, logger);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.ErrorMessage);
        }

        [Fact]
        public void Initialize_WithValidLogger_ShouldNotThrow()
        {
            // Arrange
            var logger = NullLogger.Instance;

            // Act & Assert
            var exception = Record.Exception(() => JiraStoryParsingService.Initialize(logger));
            Assert.Null(exception);
        }

        [Fact]
        public void Initialize_WithNullLogger_ShouldNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() => JiraStoryParsingService.Initialize(null));
            Assert.Null(exception);
        }
    }
}
