using System;
using System.Text.Json;
using TestForge.McpServer.Services;
using TestForge.McpServer.Models;
using TestForge.McpServer;

namespace TestForge.ConfidenceValidation;

/// <summary>
/// Quick validation script for confidence implementation
/// </summary>
public class ConfidenceValidationTest
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Testing TestForge Confidence Implementation...");
        
        // Test 1: Basic XML Analysis
        TestBasicXmlAnalysis();
        
        // Test 2: Confidence Validation
        TestConfidenceValidation();
        
        Console.WriteLine("All confidence validation tests completed!");
    }
    
    private static void TestBasicXmlAnalysis()
    {
        Console.WriteLine("\n=== Test 1: Basic XML Analysis ===");
        
        string sampleXml = @"<?xml version='1.0' encoding='UTF-8'?>
<rss version='0.92'>
  <channel>
    <item>
      <key>DEV-123</key>
      <summary>Test Story</summary>
      <description>This is a test story for validation</description>
      <type>Story</type>
      <priority>High</priority>
      <acceptancecriteria>
        <criterion>User can login</criterion>
        <criterion>User can logout</criterion>
      </acceptancecriteria>
    </item>
  </channel>
</rss>";

        try
        {
            var result = JiraXmlAnalysisService.AnalyzeForLLM(sampleXml);
            Console.WriteLine("✓ XML Analysis completed successfully");
            
            // Check if result contains executiveSummary
            if (result.Contains("executiveSummary"))
            {
                Console.WriteLine("✓ Executive summary confidence included");
            }
            else
            {
                Console.WriteLine("✗ Executive summary confidence missing");
            }
            
            // Parse and display confidence info
            using var doc = JsonDocument.Parse(result);
            if (doc.RootElement.TryGetProperty("executiveSummary", out var execSummary))
            {
                if (execSummary.TryGetProperty("overallConfidence", out var confidence))
                {
                    Console.WriteLine($"✓ Overall Confidence: {confidence.GetDouble():F3}");
                }
                if (execSummary.TryGetProperty("confidenceLevel", out var level))
                {
                    Console.WriteLine($"✓ Confidence Level: {level.GetString()}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ XML Analysis failed: {ex.Message}");
        }
    }
    
    private static void TestConfidenceValidation()
    {
        Console.WriteLine("\n=== Test 2: Confidence Validation ===");
        
        try
        {
            // Create sample data for validation
            var baselineConfidence = new ExecutiveSummaryConfidence
            {
                OverallConfidence = 0.75,
                ConfidenceLevel = ConfidenceLevel.MediumHigh,
                ConfidenceBreakdown = new ConfidenceBreakdown
                {
                    XmlQuality = 0.9,
                    StoryCompleteness = 0.8,
                    AnalysisDepth = 0.7,
                    DataQuality = 0.6,
                    IntegrationComplexity = 0.5,
                    TestCoverageReadiness = 0.8
                }
            };
            
            var testCases = JsonSerializer.Serialize(new { 
                testCase1 = new { type = "happy_path", confidence = 0.9 },
                testCase2 = new { type = "error_handling", confidence = 0.8 }
            });
            
            var originalAnalysis = JsonSerializer.Serialize(new {
                summary = "Test analysis",
                complexity = 5.0
            });
            
            var baselineJson = JsonSerializer.Serialize(baselineConfidence);
            
            var result = TestCaseTemplateService.ValidateAndAdjustConfidence(
                testCases, originalAnalysis, baselineConfidence);
            
            Console.WriteLine("✓ Confidence validation completed successfully");
            Console.WriteLine($"✓ Baseline: {result.BaselineConfidence:F3}");
            Console.WriteLine($"✓ LLM Validation: {result.LLMValidationScore:F3}");
            Console.WriteLine($"✓ Final: {result.FinalConfidence:F3}");
            Console.WriteLine($"✓ Adjustment: {result.ConfidenceAdjustment:F3}");
            Console.WriteLine($"✓ Reasoning: {result.AdjustmentReasoning}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Confidence validation failed: {ex.Message}");
        }
    }
}
