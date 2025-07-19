using System;
using System.Diagnostics;
using TestForge.McpServer.Services;

namespace TestForge.SecurityTests
{
    /// <summary>
    /// Security test program to verify ReDoS vulnerabilities are fixed
    /// Run this to validate that the fixes prevent exponential time complexity
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== TestForge JiraStoryParsingService Security Tests ===");
            Console.WriteLine();

            var allTestsPassed = true;

            // Run security tests
            allTestsPassed &= RunEntityEscapingTest();
            allTestsPassed &= RunHtmlCleaningTest();
            allTestsPassed &= RunLargeValidInputTest();
            allTestsPassed &= RunMixedEntityTest();
            allTestsPassed &= RunTicketIdValidationTest();

            Console.WriteLine();
            
            // Run functional regression tests
            allTestsPassed &= FunctionalTests.RunAllFunctionalTests();

            Console.WriteLine();
            if (allTestsPassed)
            {
                Console.WriteLine("=== ✅ All Security and Functional Tests Passed Successfully ===");
                Console.WriteLine("ReDoS vulnerabilities have been eliminated without breaking functionality!");
            }
            else
            {
                Console.WriteLine("=== ❌ Some Tests Failed ===");
                Console.WriteLine("Please review the failed tests above.");
            }
        }

        static bool RunEntityEscapingTest()
        {
            Console.WriteLine("Testing Entity Escaping ReDoS Fix...");
            
            // This would cause exponential backtracking with the old regex
            var maliciousInput = "&" + new string('&', 50000);
            var xmlContent = $"<root><description>{maliciousInput}</description></root>";
            
            var stopwatch = Stopwatch.StartNew();
            var result = JiraStoryParsingService.ParseJiraXml(xmlContent);
            stopwatch.Stop();
            
            Console.WriteLine($"  Time taken: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"  Success: {result.IsSuccess}");
            
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                Console.WriteLine("  ❌ FAILED: Entity escaping took too long!");
                return false;
            }
            else
            {
                Console.WriteLine("  ✅ Entity escaping ReDoS fix working correctly");
                return true;
            }
        }

        static bool RunHtmlCleaningTest()
        {
            Console.WriteLine("Testing HTML Tag Cleaning ReDoS Fix...");
            
            // Malformed HTML that would cause catastrophic backtracking
            var maliciousHtml = "<script" + new string('<', 10000) + "content";
            var xmlContent = $@"
                <root>
                    <comments>
                        <comment id='1' author='test' created='2025-01-01 12:00:00'>
                            {maliciousHtml}
                        </comment>
                    </comments>
                </root>";
            
            var stopwatch = Stopwatch.StartNew();
            var result = JiraStoryParsingService.ParseJiraXml(xmlContent);
            stopwatch.Stop();
            
            Console.WriteLine($"  Time taken: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"  Success: {result.IsSuccess}");
            
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                Console.WriteLine("  ❌ FAILED: HTML cleaning took too long!");
                return false;
            }
            else
            {
                Console.WriteLine("  ✅ HTML tag cleaning ReDoS fix working correctly");
                return true;
            }
        }

        static bool RunLargeValidInputTest()
        {
            Console.WriteLine("Testing Performance with Large Valid Input...");
            
            var largeDescription = new string('a', 50000);
            var xmlContent = $@"
                <root>
                    <item>
                        <key>TEST-123</key>
                        <summary>Test Story</summary>
                        <description>{largeDescription}</description>
                        <comments>
                            <comment id='1' author='user1' created='2025-01-01 12:00:00'>
                                <p>This is a <strong>valid</strong> HTML comment with &amp; entities</p>
                            </comment>
                        </comments>
                    </item>
                </root>";
            
            var stopwatch = Stopwatch.StartNew();
            var result = JiraStoryParsingService.ParseJiraXml(xmlContent);
            stopwatch.Stop();
            
            Console.WriteLine($"  Time taken: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"  Success: {result.IsSuccess}");
            Console.WriteLine($"  Issue Key: {result.Story?.IssueKey}");
            Console.WriteLine($"  Summary: {result.Story?.Summary}");
            
            if (stopwatch.ElapsedMilliseconds > 2000)
            {
                Console.WriteLine("  ❌ FAILED: Large input processing took too long!");
                return false;
            }
            else
            {
                Console.WriteLine("  ✅ Large valid input processed efficiently");
                return true;
            }
        }

        static bool RunMixedEntityTest()
        {
            Console.WriteLine("Testing Mixed Valid/Invalid Entity Handling...");
            
            var testCases = new[]
            {
                ("&amp;", "Valid entity"),
                ("&invalid;", "Invalid entity"),
                ("&lt;test&gt;", "Multiple valid entities"),
                ("&#123;", "Numeric entity"),
                ("&#xAB;", "Hex entity")
            };

            bool allPassed = true;
            foreach (var (input, description) in testCases)
            {
                var xmlContent = $"<root><description>{input}</description></root>";
                
                var stopwatch = Stopwatch.StartNew();
                var result = JiraStoryParsingService.ParseJiraXml(xmlContent);
                stopwatch.Stop();
                
                bool testPassed = result.IsSuccess && stopwatch.ElapsedMilliseconds < 100;
                Console.WriteLine($"  {description}: {stopwatch.ElapsedMilliseconds}ms - {(testPassed ? "✅" : "❌")}");
                allPassed &= testPassed;
            }
            
            return allPassed;
        }

        static bool RunTicketIdValidationTest()
        {
            Console.WriteLine("Testing Ticket ID Validation Performance...");
            
            // Test the existing regex with large input
            var largeInvalidId = "INVALID" + new string('A', 50000) + "-123";
            
            var stopwatch = Stopwatch.StartNew();
            var isValid = JiraStoryParsingService.IsValidJiraTicketId(largeInvalidId);
            stopwatch.Stop();
            
            Console.WriteLine($"  Time taken: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"  Valid: {isValid}");
            
            if (stopwatch.ElapsedMilliseconds > 100)
            {
                Console.WriteLine("  ❌ FAILED: Ticket ID validation took too long!");
                return false;
            }
            else
            {
                Console.WriteLine("  ✅ Ticket ID validation performed well");
                return true;
            }
        }
    }
}
