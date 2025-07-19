using System.Text.Json;
using System.Text.RegularExpressions;

namespace TestForge.McpServer.Services;

/// <summary>
/// Service responsible for identifying business rules and validation logic from requirements
/// </summary>
public static class BusinessLogicAnalysisService
{
    /// <summary>
    /// Identifies business rules and validation logic from ticket descriptions
    /// </summary>
    /// <param name="description">The ticket description or requirements text</param>
    /// <returns>Structured analysis of business logic with confidence scores and test scenario suggestions</returns>
    public static string ExtractLogic(string description)
    {
        try
        {
            var businessLogic = new List<object>();
            
            if (!string.IsNullOrWhiteSpace(description))
            {
                businessLogic.AddRange(AnalyzeValidationRules(description));
                businessLogic.AddRange(AnalyzeBusinessRules(description));
                businessLogic.AddRange(AnalyzeWorkflowLogic(description));
                businessLogic.AddRange(AnalyzeDataLogic(description));
                businessLogic.AddRange(AnalyzeSecurityLogic(description));
            }
            
            var analysis = new
            {
                businessLogic = businessLogic,
                ruleCount = businessLogic.Count,
                complexityAnalysis = AnalyzeComplexity(businessLogic),
                testScenarioSuggestions = GenerateTestScenarios(businessLogic),
                riskAssessment = AssessRisks(businessLogic),
                testingStrategy = DetermineTestingStrategy(businessLogic)
            };
            
            return JsonSerializer.Serialize(analysis, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = $"Business logic analysis failed: {ex.Message}" });
        }
    }

    /// <summary>
    /// Analyzes validation rules in the description
    /// </summary>
    /// <param name="description">The description text</param>
    /// <returns>List of validation rules</returns>
    private static List<object> AnalyzeValidationRules(string description)
    {
        var rules = new List<object>();
        var lowerDesc = description.ToLower();
        
        // Required field validation
        if (lowerDesc.Contains("required") || lowerDesc.Contains("mandatory") || lowerDesc.Contains("must"))
        {
            rules.Add(new { 
                rule = "required_field", 
                description = "Required field validation", 
                confidence = 0.85, 
                testScenarios = new[] { "missing required field", "present required field" },
                riskLevel = "medium"
            });
        }
        
        // Format validation
        if (lowerDesc.Contains("format") || lowerDesc.Contains("pattern") || lowerDesc.Contains("regex"))
        {
            rules.Add(new { 
                rule = "format_validation", 
                description = "Input format validation", 
                confidence = 0.88, 
                testScenarios = new[] { "valid format", "invalid format", "empty format" },
                riskLevel = "high"
            });
        }
        
        // Range validation
        if (lowerDesc.Contains("minimum") || lowerDesc.Contains("maximum") || lowerDesc.Contains("range"))
        {
            rules.Add(new { 
                rule = "range_validation", 
                description = "Value range validation", 
                confidence = 0.95, 
                testScenarios = new[] { "below minimum", "above maximum", "within range", "boundary values" },
                riskLevel = "high"
            });
        }
        
        // Length validation
        if (lowerDesc.Contains("length") || lowerDesc.Contains("character") || lowerDesc.Contains("limit"))
        {
            rules.Add(new { 
                rule = "length_validation", 
                description = "Input length validation", 
                confidence = 0.87, 
                testScenarios = new[] { "too short", "too long", "exact length", "empty input" },
                riskLevel = "medium"
            });
        }
        
        // Email validation
        if (lowerDesc.Contains("email") || lowerDesc.Contains("@"))
        {
            rules.Add(new { 
                rule = "email_validation", 
                description = "Email format validation", 
                confidence = 0.92, 
                testScenarios = new[] { "valid email", "invalid email", "missing @ symbol", "invalid domain" },
                riskLevel = "medium"
            });
        }
        
        // Date validation
        if (lowerDesc.Contains("date") || lowerDesc.Contains("time") || lowerDesc.Contains("calendar"))
        {
            rules.Add(new { 
                rule = "date_validation", 
                description = "Date/time validation", 
                confidence = 0.89, 
                testScenarios = new[] { "valid date", "invalid date", "future date", "past date", "leap year" },
                riskLevel = "medium"
            });
        }
        
        return rules;
    }

    /// <summary>
    /// Analyzes business rules in the description
    /// </summary>
    /// <param name="description">The description text</param>
    /// <returns>List of business rules</returns>
    private static List<object> AnalyzeBusinessRules(string description)
    {
        var rules = new List<object>();
        var lowerDesc = description.ToLower();
        
        // Approval workflow
        if (lowerDesc.Contains("approval") || lowerDesc.Contains("approve") || lowerDesc.Contains("reject"))
        {
            rules.Add(new { 
                rule = "approval_workflow", 
                description = "Approval process business logic", 
                confidence = 0.88, 
                testScenarios = new[] { "approve request", "reject request", "pending approval", "auto-approval" },
                riskLevel = "high"
            });
        }
        
        // Status transitions
        if (lowerDesc.Contains("status") || lowerDesc.Contains("state") || lowerDesc.Contains("transition"))
        {
            rules.Add(new { 
                rule = "status_transition", 
                description = "Status transition logic", 
                confidence = 0.85, 
                testScenarios = new[] { "valid transition", "invalid transition", "status persistence" },
                riskLevel = "medium"
            });
        }
        
        // Business calculations
        if (lowerDesc.Contains("calculate") || lowerDesc.Contains("compute") || lowerDesc.Contains("formula"))
        {
            rules.Add(new { 
                rule = "business_calculation", 
                description = "Business calculation logic", 
                confidence = 0.92, 
                testScenarios = new[] { "valid calculation", "edge case values", "precision testing", "overflow handling" },
                riskLevel = "high"
            });
        }
        
        // Conditional logic
        if (lowerDesc.Contains("if") || lowerDesc.Contains("when") || lowerDesc.Contains("condition"))
        {
            rules.Add(new { 
                rule = "conditional_logic", 
                description = "Conditional business logic", 
                confidence = 0.83, 
                testScenarios = new[] { "condition met", "condition not met", "multiple conditions", "nested conditions" },
                riskLevel = "medium"
            });
        }
        
        return rules;
    }

    /// <summary>
    /// Analyzes workflow logic in the description
    /// </summary>
    /// <param name="description">The description text</param>
    /// <returns>List of workflow rules</returns>
    private static List<object> AnalyzeWorkflowLogic(string description)
    {
        var rules = new List<object>();
        var lowerDesc = description.ToLower();
        
        // Sequential workflow
        if (lowerDesc.Contains("step") || lowerDesc.Contains("sequence") || lowerDesc.Contains("order"))
        {
            rules.Add(new { 
                rule = "sequential_workflow", 
                description = "Sequential step processing", 
                confidence = 0.81, 
                testScenarios = new[] { "complete sequence", "interrupted sequence", "skip steps", "restart workflow" },
                riskLevel = "medium"
            });
        }
        
        // Parallel processing
        if (lowerDesc.Contains("parallel") || lowerDesc.Contains("concurrent") || lowerDesc.Contains("simultaneous"))
        {
            rules.Add(new { 
                rule = "parallel_processing", 
                description = "Parallel workflow processing", 
                confidence = 0.87, 
                testScenarios = new[] { "parallel execution", "race conditions", "synchronization", "resource conflicts" },
                riskLevel = "high"
            });
        }
        
        // Timeout handling
        if (lowerDesc.Contains("timeout") || lowerDesc.Contains("expir") || lowerDesc.Contains("deadline"))
        {
            rules.Add(new { 
                rule = "timeout_handling", 
                description = "Timeout and expiration logic", 
                confidence = 0.89, 
                testScenarios = new[] { "before timeout", "after timeout", "timeout recovery", "extend timeout" },
                riskLevel = "medium"
            });
        }
        
        return rules;
    }

    /// <summary>
    /// Analyzes data logic in the description
    /// </summary>
    /// <param name="description">The description text</param>
    /// <returns>List of data rules</returns>
    private static List<object> AnalyzeDataLogic(string description)
    {
        var rules = new List<object>();
        var lowerDesc = description.ToLower();
        
        // Data integrity
        if (lowerDesc.Contains("integrity") || lowerDesc.Contains("consistent") || lowerDesc.Contains("duplicate"))
        {
            rules.Add(new { 
                rule = "data_integrity", 
                description = "Data integrity constraints", 
                confidence = 0.91, 
                testScenarios = new[] { "data consistency", "duplicate prevention", "referential integrity", "data corruption" },
                riskLevel = "high"
            });
        }
        
        // Data transformation
        if (lowerDesc.Contains("transform") || lowerDesc.Contains("convert") || lowerDesc.Contains("mapping"))
        {
            rules.Add(new { 
                rule = "data_transformation", 
                description = "Data transformation logic", 
                confidence = 0.86, 
                testScenarios = new[] { "successful transformation", "transformation failure", "data type conversion", "mapping accuracy" },
                riskLevel = "medium"
            });
        }
        
        // Data archival
        if (lowerDesc.Contains("archive") || lowerDesc.Contains("retain") || lowerDesc.Contains("purge"))
        {
            rules.Add(new { 
                rule = "data_archival", 
                description = "Data archival and retention", 
                confidence = 0.84, 
                testScenarios = new[] { "archive old data", "retain active data", "purge expired data", "restore archived data" },
                riskLevel = "low"
            });
        }
        
        return rules;
    }

    /// <summary>
    /// Analyzes security logic in the description
    /// </summary>
    /// <param name="description">The description text</param>
    /// <returns>List of security rules</returns>
    private static List<object> AnalyzeSecurityLogic(string description)
    {
        var rules = new List<object>();
        var lowerDesc = description.ToLower();
        
        // Access control
        if (lowerDesc.Contains("permission") || lowerDesc.Contains("access") || lowerDesc.Contains("authorize"))
        {
            rules.Add(new { 
                rule = "access_control", 
                description = "Access control logic", 
                confidence = 0.90, 
                testScenarios = new[] { "authorized access", "unauthorized access", "role-based access", "permission inheritance" },
                riskLevel = "high"
            });
        }
        
        // Authentication
        if (lowerDesc.Contains("authenticate") || lowerDesc.Contains("login") || lowerDesc.Contains("credential"))
        {
            rules.Add(new { 
                rule = "authentication", 
                description = "Authentication logic", 
                confidence = 0.93, 
                testScenarios = new[] { "valid credentials", "invalid credentials", "session management", "multi-factor auth" },
                riskLevel = "high"
            });
        }
        
        // Data encryption
        if (lowerDesc.Contains("encrypt") || lowerDesc.Contains("secure") || lowerDesc.Contains("protect"))
        {
            rules.Add(new { 
                rule = "data_encryption", 
                description = "Data encryption and security", 
                confidence = 0.87, 
                testScenarios = new[] { "encrypted storage", "secure transmission", "key management", "decryption accuracy" },
                riskLevel = "high"
            });
        }
        
        return rules;
    }

    /// <summary>
    /// Analyzes complexity of business logic rules
    /// </summary>
    /// <param name="businessLogic">List of business logic rules</param>
    /// <returns>Complexity analysis</returns>
    private static object AnalyzeComplexity(List<object> businessLogic)
    {
        var highRisk = businessLogic.Count(rule => rule?.ToString()?.Contains("high") == true);
        var mediumRisk = businessLogic.Count(rule => rule?.ToString()?.Contains("medium") == true);
        var lowRisk = businessLogic.Count(rule => rule?.ToString()?.Contains("low") == true);
        
        var overallComplexity = highRisk > 0 ? "high" : 
                               mediumRisk > 0 ? "medium" : 
                               lowRisk > 0 ? "low" : "none";
        
        return new
        {
            totalRules = businessLogic.Count,
            highRiskRules = highRisk,
            mediumRiskRules = mediumRisk,
            lowRiskRules = lowRisk,
            overallComplexity = overallComplexity,
            complexityScore = (highRisk * 3) + (mediumRisk * 2) + (lowRisk * 1)
        };
    }

    /// <summary>
    /// Generates test scenarios based on business logic
    /// </summary>
    /// <param name="businessLogic">List of business logic rules</param>
    /// <returns>Test scenario suggestions</returns>
    private static string[] GenerateTestScenarios(List<object> businessLogic)
    {
        var scenarios = new List<string>
        {
            "Test happy path scenarios for all business rules",
            "Verify error handling for invalid inputs",
            "Test boundary conditions and edge cases",
            "Validate business rule interactions and dependencies"
        };
        
        if (businessLogic.Any(rule => rule?.ToString()?.Contains("validation") == true))
        {
            scenarios.Add("Comprehensive validation testing with various input combinations");
        }
        
        if (businessLogic.Any(rule => rule?.ToString()?.Contains("workflow") == true))
        {
            scenarios.Add("End-to-end workflow testing with different paths");
        }
        
        if (businessLogic.Any(rule => rule?.ToString()?.Contains("security") == true))
        {
            scenarios.Add("Security testing with unauthorized access attempts");
        }
        
        return scenarios.ToArray();
    }

    /// <summary>
    /// Assesses risks based on business logic
    /// </summary>
    /// <param name="businessLogic">List of business logic rules</param>
    /// <returns>Risk assessment</returns>
    private static object AssessRisks(List<object> businessLogic)
    {
        var risks = new List<string>();
        
        if (businessLogic.Any(rule => rule?.ToString()?.Contains("calculation") == true))
        {
            risks.Add("Financial calculation errors could have significant impact");
        }
        
        if (businessLogic.Any(rule => rule?.ToString()?.Contains("security") == true))
        {
            risks.Add("Security vulnerabilities could lead to data breaches");
        }
        
        if (businessLogic.Any(rule => rule?.ToString()?.Contains("workflow") == true))
        {
            risks.Add("Workflow failures could disrupt business processes");
        }
        
        if (businessLogic.Any(rule => rule?.ToString()?.Contains("data_integrity") == true))
        {
            risks.Add("Data integrity issues could compromise system reliability");
        }
        
        return new
        {
            identifiedRisks = risks.ToArray(),
            riskLevel = risks.Count switch
            {
                0 => "low",
                1 => "low",
                2 => "medium",
                3 => "high",
                _ => "very high"
            },
            mitigationStrategies = new[]
            {
                "Implement comprehensive test coverage",
                "Add extensive validation and error handling",
                "Perform security testing and code review",
                "Plan for monitoring and alerting"
            }
        };
    }

    /// <summary>
    /// Determines testing strategy based on business logic
    /// </summary>
    /// <param name="businessLogic">List of business logic rules</param>
    /// <returns>Testing strategy recommendations</returns>
    private static object DetermineTestingStrategy(List<object> businessLogic)
    {
        var strategies = new List<string>
        {
            "Unit testing for individual business rules",
            "Integration testing for rule interactions",
            "End-to-end testing for complete workflows"
        };
        
        if (businessLogic.Any(rule => rule?.ToString()?.Contains("security") == true))
        {
            strategies.Add("Security testing with penetration testing");
        }
        
        if (businessLogic.Any(rule => rule?.ToString()?.Contains("performance") == true))
        {
            strategies.Add("Performance testing under load");
        }
        
        return new
        {
            recommendedStrategies = strategies.ToArray(),
            testingPriority = businessLogic.Count switch
            {
                0 => "low",
                <= 3 => "medium",
                <= 6 => "high",
                _ => "critical"
            },
            estimatedEffort = new
            {
                testCases = businessLogic.Count * 3, // Average 3 test cases per rule
                estimatedHours = Math.Max(businessLogic.Count * 4, 8), // Minimum 8 hours
                complexity = businessLogic.Count > 5 ? "high" : businessLogic.Count > 2 ? "medium" : "low"
            }
        };
    }

    /// <summary>
    /// Enhanced method to extract conditional logic and fallback mechanisms from descriptions
    /// </summary>
    /// <param name="description">The description text to analyze</param>
    /// <returns>List of conditional tests with branch coverage scenarios</returns>
    public static List<ConditionalTest> ExtractConditionalLogic(string description)
    {
        var conditionalTests = new List<ConditionalTest>();
        
        if (string.IsNullOrWhiteSpace(description))
            return conditionalTests;

        // Fallback patterns
        var fallbackPatterns = new[]
        {
            @"if\s+(.+?)\s+else\s+(.+?)(?:\.|$|,)",
            @"when\s+(.+?)\s+then\s+(.+?)(?:\.|$|,)",
            @"if\s+(.+?)\s+is\s+(?:null|empty|missing)\s*,?\s*(.+?)(?:\.|$)",
            @"fallback\s+to\s+(.+?)(?:\.|$|,)",
            @"use\s+(.+?)\s+when\s+(.+?)(?:\.|$|,)",
            @"apply\s+(.+?)\s+if\s+(.+?)(?:\.|$|,)"
        };

        // Optional feature patterns
        var optionalPatterns = new[]
        {
            @"if\s+present,?\s*(.+?)(?:\.|$|,)",
            @"when\s+available,?\s*(.+?)(?:\.|$|,)",
            @"optional\s+(.+?)(?:\.|$|,)",
            @"if\s+(?:provided|supplied|given),?\s*(.+?)(?:\.|$|,)"
        };

        // Decision flow patterns
        var decisionPatterns = new[]
        {
            @"use\s+(.+?)\s+when\s+(.+?)(?:\.|$|,)",
            @"apply\s+(.+?)\s+if\s+(.+?)(?:\.|$|,)",
            @"switch\s+to\s+(.+?)\s+when\s+(.+?)(?:\.|$|,)",
            @"default\s+to\s+(.+?)(?:\.|$|,)"
        };

        // Extract fallback scenarios
        foreach (var pattern in fallbackPatterns)
        {
            var matches = Regex.Matches(description, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 3)
                {
                    conditionalTests.Add(new ConditionalTest
                    {
                        Condition = match.Groups[1].Value.Trim(),
                        Action = match.Groups[2].Value.Trim(),
                        TestCase = $"Verify {match.Groups[2].Value.Trim()} when {match.Groups[1].Value.Trim()}",
                        Category = TestCategory.ConditionalLogic
                    });

                    // Add negative test case
                    conditionalTests.Add(new ConditionalTest
                    {
                        Condition = $"NOT ({match.Groups[1].Value.Trim()})",
                        Action = "fallback behavior",
                        TestCase = $"Verify fallback behavior when {match.Groups[1].Value.Trim()} is false",
                        Category = TestCategory.EdgeCases
                    });
                }
            }
        }

        // Extract optional feature scenarios
        foreach (var pattern in optionalPatterns)
        {
            var matches = Regex.Matches(description, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                var feature = match.Groups[1].Value.Trim();
                
                conditionalTests.Add(new ConditionalTest
                {
                    Condition = "feature is present",
                    Action = feature,
                    TestCase = $"Verify {feature} when feature is available",
                    Category = TestCategory.ConditionalLogic
                });

                conditionalTests.Add(new ConditionalTest
                {
                    Condition = "feature is absent",
                    Action = "graceful degradation",
                    TestCase = $"Verify graceful handling when feature is not available",
                    Category = TestCategory.EdgeCases
                });
            }
        }

        // Extract decision flow scenarios
        foreach (var pattern in decisionPatterns)
        {
            var matches = Regex.Matches(description, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 3)
                {
                    conditionalTests.Add(new ConditionalTest
                    {
                        Condition = match.Groups[2].Value.Trim(),
                        Action = match.Groups[1].Value.Trim(),
                        TestCase = $"Verify {match.Groups[1].Value.Trim()} applied when {match.Groups[2].Value.Trim()}",
                        Category = TestCategory.ConditionalLogic
                    });
                }
            }
        }

        // Add specific phoneme/SSML conditional tests if detected
        if (description.ToLower().Contains("phoneme") && description.ToLower().Contains("ssml"))
        {
            conditionalTests.Add(new ConditionalTest
            {
                Condition = "phoneme exists",
                Action = "apply phoneme pronunciation",
                TestCase = "Verify phoneme application when phoneme data is available",
                Category = TestCategory.DataValidation
            });

            conditionalTests.Add(new ConditionalTest
            {
                Condition = "phoneme is null or empty",
                Action = "use SSML fallback",
                TestCase = "Verify SSML fallback when phoneme data is missing",
                Category = TestCategory.ConditionalLogic
            });
        }

        // Add IVR variant conditional tests if detected
        if (description.ToLower().Contains("ivr") && (description.ToLower().Contains("variant") || description.ToLower().Contains("type")))
        {
            var ivrTypes = ExtractIVRTypes(description);
            foreach (var ivrType in ivrTypes)
            {
                conditionalTests.Add(new ConditionalTest
                {
                    Condition = $"IVR type is {ivrType}",
                    Action = $"apply {ivrType} specific behavior",
                    TestCase = $"Verify {ivrType} specific behavior when IVR type is {ivrType}",
                    Category = TestCategory.ScenarioExpansion
                });
            }
        }

        return conditionalTests;
    }

    /// <summary>
    /// Analyzes decision tree complexity and generates branch coverage tests
    /// </summary>
    /// <param name="description">The description to analyze</param>
    /// <returns>Decision tree analysis with branch coverage recommendations</returns>
    public static object AnalyzeDecisionTrees(string description)
    {
        var conditionalTests = ExtractConditionalLogic(description);
        var decisionPaths = new List<object>();
        var branchCoverage = new List<object>();

        // Group by condition types
        var conditionGroups = conditionalTests.GroupBy(t => ExtractConditionType(t.Condition));

        foreach (var group in conditionGroups)
        {
            var conditionType = group.Key;
            var tests = group.ToList();

            decisionPaths.Add(new
            {
                conditionType = conditionType,
                pathCount = tests.Count,
                conditions = tests.Select(t => t.Condition).ToArray(),
                actions = tests.Select(t => t.Action).ToArray(),
                testCases = tests.Select(t => t.TestCase).ToArray()
            });

            // Generate branch coverage scenarios
            branchCoverage.Add(new
            {
                branchType = conditionType,
                coverage = new
                {
                    totalBranches = tests.Count,
                    testedBranches = tests.Count, // All branches should be tested
                    coveragePercentage = 100.0,
                    missingBranches = new string[0] // No missing branches in generated tests
                },
                testScenarios = tests.Select(t => new
                {
                    scenario = t.TestCase,
                    condition = t.Condition,
                    expectedAction = t.Action,
                    category = t.Category.ToString(),
                    priority = DeterminePriority(t)
                }).ToArray()
            });
        }

        return new
        {
            decisionTreeAnalysis = new
            {
                totalDecisionPoints = conditionalTests.Count,
                decisionPaths = decisionPaths.ToArray(),
                complexity = DetermineDecisionComplexity(conditionalTests.Count),
                branchCoverageAnalysis = branchCoverage.ToArray()
            },
            testingRecommendations = new
            {
                minimumTestCases = conditionalTests.Count,
                recommendedTestCases = conditionalTests.Count * 2, // Include edge cases
                testingApproach = "Branch coverage with edge case validation",
                automationFeasibility = conditionalTests.Count < 20 ? "High" : "Medium"
            },
            conditionalLogicTests = conditionalTests.Select(t => new
            {
                condition = t.Condition,
                action = t.Action,
                testCase = t.TestCase,
                category = t.Category.ToString(),
                priority = DeterminePriority(t)
            }).ToArray()
        };
    }

    #region Helper Methods for Conditional Logic Analysis

    private static List<string> ExtractIVRTypes(string description)
    {
        var ivrTypes = new List<string>();
        var ivrPatterns = new[]
        {
            @"no[\-\s]?pin\s+ivr",
            @"athena\s+ivr",
            @"availity\s+ivr",
            @"(\w+)\s+ivr\s+(?:type|variant)",
            @"ivr\s+(?:type|variant)\s+(\w+)"
        };

        foreach (var pattern in ivrPatterns)
        {
            var matches = Regex.Matches(description, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                if (match.Value.ToLower().Contains("no-pin") || match.Value.ToLower().Contains("no pin"))
                    ivrTypes.Add("no-pin");
                else if (match.Value.ToLower().Contains("athena"))
                    ivrTypes.Add("athena");
                else if (match.Value.ToLower().Contains("availity"))
                    ivrTypes.Add("availity");
                else if (match.Groups.Count > 1 && !string.IsNullOrWhiteSpace(match.Groups[1].Value))
                    ivrTypes.Add(match.Groups[1].Value.Trim());
            }
        }

        return ivrTypes.Distinct().ToList();
    }

    private static string ExtractConditionType(string condition)
    {
        var lowerCondition = condition.ToLower();
        
        if (lowerCondition.Contains("null") || lowerCondition.Contains("empty") || lowerCondition.Contains("missing"))
            return "Null_Check";
        
        if (lowerCondition.Contains("present") || lowerCondition.Contains("available") || lowerCondition.Contains("exists"))
            return "Presence_Check";
        
        if (lowerCondition.Contains("type") || lowerCondition.Contains("variant"))
            return "Type_Check";
        
        if (lowerCondition.Contains("phoneme") || lowerCondition.Contains("ssml"))
            return "Audio_Condition";
        
        if (lowerCondition.Contains("ivr") || lowerCondition.Contains("flow"))
            return "IVR_Condition";
        
        return "Generic_Condition";
    }

    private static string DeterminePriority(ConditionalTest test)
    {
        if (test.Category == TestCategory.ConditionalLogic)
            return "High";
        
        if (test.Category == TestCategory.DataValidation)
            return "High";
        
        if (test.Category == TestCategory.EdgeCases)
            return "Medium";
        
        return "Medium";
    }

    private static string DetermineDecisionComplexity(int decisionCount)
    {
        return decisionCount switch
        {
            0 => "None",
            1 => "Simple",
            <= 3 => "Low",
            <= 6 => "Medium",
            <= 10 => "High",
            _ => "Very High"
        };
    }

    #endregion
}
