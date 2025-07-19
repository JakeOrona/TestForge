using System.Text.Json;
using TestForge.McpServer.Models;

namespace TestForge.McpServer.Services;

/// <summary>
/// Service responsible for generating test case templates that LLM can enhance and expand
/// </summary>
public static class TestCaseTemplateService
{
    /// <summary>
    /// Generates baseline test case templates that LLM can enhance and expand
    /// </summary>
    /// <param name="ticketType">The type of ticket (Story, Bug, Epic, Task, etc.)</param>
    /// <param name="priority">Priority level (High, Medium, Low, Critical)</param>
    /// <param name="component">Component or area (UI, API, Database, Integration, etc.)</param>
    /// <returns>Structured test case templates with metadata for LLM enhancement</returns>
    public static string GenerateTemplates(string ticketType, string priority = "Medium", string component = "UI")
    {
        try
        {
            var templates = new
            {
                ticketType = ticketType,
                priority = priority,
                component = component,
                templates = GenerateTemplatesByType(ticketType, priority, component),
                metadata = new
                {
                    generatedAt = DateTime.UtcNow,
                    version = "1.0",
                    llmOptimized = true,
                    enhancementSuggestions = new[]
                    {
                        "Add domain-specific test scenarios",
                        "Include accessibility considerations",
                        "Expand with security test cases",
                        "Consider integration testing aspects",
                        "Add performance testing scenarios",
                        "Include user experience validation"
                    }
                }
            };
            
            return JsonSerializer.Serialize(templates, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = $"Template generation failed: {ex.Message}" });
        }
    }

    /// <summary>
    /// Generates templates based on ticket type
    /// </summary>
    /// <param name="ticketType">The ticket type</param>
    /// <param name="priority">The priority level</param>
    /// <param name="component">The component type</param>
    /// <returns>Array of template objects</returns>
    private static object[] GenerateTemplatesByType(string ticketType, string priority, string component)
    {
        var templates = new List<object>();
        
        // Add common templates
        templates.AddRange(GenerateCommonTemplates(ticketType, priority, component));
        
        // Add type-specific templates
        switch (ticketType.ToLower())
        {
            case "story":
                templates.AddRange(GenerateStoryTemplates(priority, component));
                break;
            case "bug":
                templates.AddRange(GenerateBugTemplates(priority, component));
                break;
            case "epic":
                templates.AddRange(GenerateEpicTemplates(priority, component));
                break;
            case "task":
                templates.AddRange(GenerateTaskTemplates(priority, component));
                break;
            case "improvement":
                templates.AddRange(GenerateImprovementTemplates(priority, component));
                break;
            default:
                templates.AddRange(GenerateDefaultTemplates(priority, component));
                break;
        }
        
        return templates.ToArray();
    }

    /// <summary>
    /// Generates common templates applicable to all ticket types
    /// </summary>
    /// <param name="ticketType">The ticket type</param>
    /// <param name="priority">The priority level</param>
    /// <param name="component">The component type</param>
    /// <returns>Common templates</returns>
    private static object[] GenerateCommonTemplates(string ticketType, string priority, string component)
    {
        return new[]
        {
            new
            {
                type = "happy_path",
                title = $"[{component}] Happy Path - {ticketType}",
                priority = priority,
                category = "Functional",
                steps = new[]
                {
                    "Navigate to the target page/component",
                    "Perform the primary action with valid data",
                    "Verify successful completion",
                    "Confirm expected results are displayed"
                },
                expectedResult = "Action completes successfully with expected outcome",
                testData = "Valid test data set appropriate for the scenario",
                confidence = 0.95,
                llmGuidance = new
                {
                    expandAreas = new[] { "Add specific validation steps", "Include realistic test data examples", "Detail expected UI changes" },
                    considerations = new[] { "User experience", "Performance impact", "Error prevention" }
                }
            },
            new
            {
                type = "error_handling",
                title = $"[{component}] Error Handling - {ticketType}",
                priority = priority,
                category = "Negative",
                steps = new[]
                {
                    "Navigate to the target page/component",
                    "Perform action with invalid/missing data",
                    "Verify appropriate error handling",
                    "Confirm error messages are clear and actionable"
                },
                expectedResult = "System handles errors gracefully with clear user feedback",
                testData = "Invalid, missing, or boundary condition data",
                confidence = 0.87,
                llmGuidance = new
                {
                    expandAreas = new[] { "Add specific error conditions", "Include validation message tests", "Test error recovery scenarios" },
                    considerations = new[] { "User guidance", "System stability", "Security implications" }
                }
            },
            new
            {
                type = "boundary_testing",
                title = $"[{component}] Boundary Conditions - {ticketType}",
                priority = priority,
                category = "Edge Case",
                steps = new[]
                {
                    "Identify input boundaries and limits",
                    "Test values at the boundary limits",
                    "Test values just outside the boundaries",
                    "Verify system behavior at extremes"
                },
                expectedResult = "System handles boundary conditions appropriately",
                testData = "Boundary values, edge cases, and limit conditions",
                confidence = 0.82,
                llmGuidance = new
                {
                    expandAreas = new[] { "Define specific boundary values", "Include numeric limits", "Test string length limits" },
                    considerations = new[] { "System limits", "Data validation", "Performance degradation" }
                }
            }
        };
    }

    /// <summary>
    /// Generates story-specific templates
    /// </summary>
    /// <param name="priority">The priority level</param>
    /// <param name="component">The component type</param>
    /// <returns>Story templates</returns>
    private static object[] GenerateStoryTemplates(string priority, string component)
    {
        return new[]
        {
            new
            {
                type = "acceptance_criteria",
                title = $"[{component}] Acceptance Criteria Validation",
                priority = priority,
                category = "Functional",
                steps = new[]
                {
                    "Review all acceptance criteria defined in the story",
                    "Create test scenarios for each acceptance criterion",
                    "Execute test scenarios in sequence",
                    "Verify all criteria are met"
                },
                expectedResult = "All acceptance criteria are satisfied",
                testData = "Data scenarios covering all acceptance criteria",
                confidence = 0.92,
                llmGuidance = new
                {
                    expandAreas = new[] { "Map each AC to specific test steps", "Include user persona considerations", "Add workflow validation" },
                    considerations = new[] { "User stories", "Business value", "Definition of done" }
                }
            },
            new
            {
                type = "user_journey",
                title = $"[{component}] End-to-End User Journey",
                priority = priority,
                category = "Integration",
                steps = new[]
                {
                    "Start from user entry point",
                    "Navigate through complete user workflow",
                    "Perform all required actions in sequence",
                    "Verify end-to-end functionality"
                },
                expectedResult = "Complete user journey works seamlessly",
                testData = "Real-world user scenario data",
                confidence = 0.88,
                llmGuidance = new
                {
                    expandAreas = new[] { "Define user personas", "Include multiple user paths", "Add cross-browser testing" },
                    considerations = new[] { "User experience", "Workflow efficiency", "System integration" }
                }
            }
        };
    }

    /// <summary>
    /// Generates bug-specific templates
    /// </summary>
    /// <param name="priority">The priority level</param>
    /// <param name="component">The component type</param>
    /// <returns>Bug templates</returns>
    private static object[] GenerateBugTemplates(string priority, string component)
    {
        return new[]
        {
            new
            {
                type = "bug_reproduction",
                title = $"[{component}] Bug Reproduction Verification",
                priority = priority,
                category = "Defect",
                steps = new[]
                {
                    "Set up environment conditions described in bug report",
                    "Follow exact steps to reproduce the bug",
                    "Verify the bug manifests as described",
                    "Document any variations in behavior"
                },
                expectedResult = "Bug can be consistently reproduced",
                testData = "Exact data and conditions from bug report",
                confidence = 0.93,
                llmGuidance = new
                {
                    expandAreas = new[] { "Add environment details", "Include browser/device specifics", "Test related scenarios" },
                    considerations = new[] { "Reproducibility", "Root cause analysis", "Impact assessment" }
                }
            },
            new
            {
                type = "regression_testing",
                title = $"[{component}] Regression Testing After Fix",
                priority = priority,
                category = "Regression",
                steps = new[]
                {
                    "Verify the original bug is fixed",
                    "Test related functionality for side effects",
                    "Execute automated regression suite",
                    "Confirm no new issues introduced"
                },
                expectedResult = "Bug is fixed without introducing new issues",
                testData = "Original bug data plus related test scenarios",
                confidence = 0.89,
                llmGuidance = new
                {
                    expandAreas = new[] { "Identify related components", "Add smoke testing", "Include performance validation" },
                    considerations = new[] { "Side effects", "System stability", "Performance impact" }
                }
            }
        };
    }

    /// <summary>
    /// Generates epic-specific templates
    /// </summary>
    /// <param name="priority">The priority level</param>
    /// <param name="component">The component type</param>
    /// <returns>Epic templates</returns>
    private static object[] GenerateEpicTemplates(string priority, string component)
    {
        return new[]
        {
            new
            {
                type = "epic_integration",
                title = $"[{component}] Epic Integration Testing",
                priority = priority,
                category = "Integration",
                steps = new[]
                {
                    "Identify all components involved in the epic",
                    "Test integration points between components",
                    "Verify data flow across the epic scope",
                    "Validate end-to-end epic functionality"
                },
                expectedResult = "All epic components work together seamlessly",
                testData = "Cross-component test data and scenarios",
                confidence = 0.85,
                llmGuidance = new
                {
                    expandAreas = new[] { "Map component dependencies", "Add system integration tests", "Include performance testing" },
                    considerations = new[] { "System architecture", "Data consistency", "Performance scaling" }
                }
            },
            new
            {
                type = "epic_validation",
                title = $"[{component}] Epic Business Value Validation",
                priority = priority,
                category = "Business",
                steps = new[]
                {
                    "Review epic business objectives",
                    "Test scenarios that deliver business value",
                    "Measure success criteria and KPIs",
                    "Validate against original epic goals"
                },
                expectedResult = "Epic delivers expected business value",
                testData = "Business scenario data and success metrics",
                confidence = 0.80,
                llmGuidance = new
                {
                    expandAreas = new[] { "Define success metrics", "Add user feedback collection", "Include analytics validation" },
                    considerations = new[] { "Business impact", "User satisfaction", "ROI measurement" }
                }
            }
        };
    }

    /// <summary>
    /// Generates task-specific templates
    /// </summary>
    /// <param name="priority">The priority level</param>
    /// <param name="component">The component type</param>
    /// <returns>Task templates</returns>
    private static object[] GenerateTaskTemplates(string priority, string component)
    {
        return new[]
        {
            new
            {
                type = "task_completion",
                title = $"[{component}] Task Completion Verification",
                priority = priority,
                category = "Functional",
                steps = new[]
                {
                    "Review task requirements and deliverables",
                    "Test task functionality meets requirements",
                    "Verify task integration with existing system",
                    "Confirm task completion criteria are met"
                },
                expectedResult = "Task is completed according to requirements",
                testData = "Task-specific test data and scenarios",
                confidence = 0.90,
                llmGuidance = new
                {
                    expandAreas = new[] { "Detail specific requirements", "Add integration checkpoints", "Include documentation validation" },
                    considerations = new[] { "Requirement satisfaction", "System compatibility", "Documentation completeness" }
                }
            }
        };
    }

    /// <summary>
    /// Generates improvement-specific templates
    /// </summary>
    /// <param name="priority">The priority level</param>
    /// <param name="component">The component type</param>
    /// <returns>Improvement templates</returns>
    private static object[] GenerateImprovementTemplates(string priority, string component)
    {
        return new[]
        {
            new
            {
                type = "improvement_validation",
                title = $"[{component}] Improvement Effectiveness Validation",
                priority = priority,
                category = "Performance",
                steps = new[]
                {
                    "Establish baseline metrics before improvement",
                    "Test improved functionality",
                    "Measure performance improvements",
                    "Compare against baseline and targets"
                },
                expectedResult = "Improvement achieves expected performance gains",
                testData = "Baseline and target performance data",
                confidence = 0.87,
                llmGuidance = new
                {
                    expandAreas = new[] { "Define measurable metrics", "Add comparative analysis", "Include user experience metrics" },
                    considerations = new[] { "Performance gains", "User impact", "System efficiency" }
                }
            }
        };
    }

    /// <summary>
    /// Generates default templates for unspecified types
    /// </summary>
    /// <param name="priority">The priority level</param>
    /// <param name="component">The component type</param>
    /// <returns>Default templates</returns>
    private static object[] GenerateDefaultTemplates(string priority, string component)
    {
        return new[]
        {
            new
            {
                type = "functional_validation",
                title = $"[{component}] Functional Validation",
                priority = priority,
                category = "Functional",
                steps = new[]
                {
                    "Review functional requirements",
                    "Test core functionality",
                    "Verify expected behavior",
                    "Confirm system stability"
                },
                expectedResult = "Functionality works as expected",
                testData = "Standard functional test data",
                confidence = 0.85,
                llmGuidance = new
                {
                    expandAreas = new[] { "Add specific requirements", "Include edge case testing", "Detail expected outcomes" },
                    considerations = new[] { "Functional correctness", "System stability", "User expectations" }
                }
            }
        };
    }

    /// <summary>
    /// Expands test scenarios for multiple contexts using cartesian product approach
    /// </summary>
    /// <param name="contexts">List of contexts to expand across (e.g., IVR types, user roles, environments)</param>
    /// <param name="baseTestCase">Base test case template to expand</param>
    /// <returns>List of context-specific test case variants</returns>
    public static List<object> ExpandScenariosForMultipleContexts(List<string> contexts, object baseTestCase)
    {
        var expandedTests = new List<object>();
        
        if (!contexts.Any())
        {
            expandedTests.Add(baseTestCase);
            return expandedTests;
        }

        // Extract base test properties using reflection or dynamic
        var baseTest = baseTestCase as dynamic ?? throw new ArgumentException("Invalid base test case format");
        
        foreach (var context in contexts)
        {
            var contextSpecificTest = new
            {
                type = $"{baseTest.type}_{SanitizeContextName(context)}",
                title = $"{baseTest.title} in {context} context",
                priority = baseTest.priority,
                category = baseTest.category,
                context = context,
                steps = ExpandStepsForContext(baseTest.steps, context),
                expectedResult = $"{baseTest.expectedResult} in {context} environment",
                testData = $"{baseTest.testData} - {context} specific",
                confidence = AdjustConfidenceForContext(baseTest.confidence, context),
                llmGuidance = new
                {
                    expandAreas = new[] { 
                        $"Context-specific requirements for {context}",
                        $"Environmental considerations for {context}",
                        $"Integration points in {context}"
                    },
                    considerations = new[] { 
                        $"{context} specific behavior",
                        "Context switching scenarios",
                        "Cross-context compatibility"
                    },
                    contextType = DetermineContextType(context)
                },
                metadata = new
                {
                    isContextExpanded = true,
                    baseTestType = baseTest.type,
                    expandedFrom = "multi_context_expansion",
                    contextCategory = CategorizeContext(context)
                }
            };
            
            expandedTests.Add(contextSpecificTest);
        }

        // Add cross-context interaction tests
        if (contexts.Count > 1)
        {
            expandedTests.AddRange(GenerateCrossContextTests(contexts, baseTest));
        }

        return expandedTests;
    }

    /// <summary>
    /// Enhanced data validation test generation based on technical artifacts
    /// </summary>
    /// <param name="artifacts">Technical artifacts extracted from content</param>
    /// <returns>Comprehensive data validation test templates</returns>
    public static List<object> GenerateDataValidationTests(TechnicalArtifacts artifacts)
    {
        var validationTests = new List<object>();

        // Format validation tests
        validationTests.AddRange(GenerateFormatValidationTemplates(artifacts));

        // Boundary tests
        validationTests.AddRange(GenerateBoundaryTestTemplates(artifacts));

        // Malformed input tests
        validationTests.AddRange(GenerateMalformedInputTemplates(artifacts));

        // SQL-specific validation tests
        if (artifacts.SqlSnippets.Any())
        {
            validationTests.AddRange(GenerateSqlValidationTemplates(artifacts));
        }

        // Configuration validation tests
        if (artifacts.ConfigurationHints.Any())
        {
            validationTests.AddRange(GenerateConfigValidationTemplates(artifacts));
        }

        return validationTests;
    }

    #region Context Expansion Helper Methods

    private static string SanitizeContextName(string context)
    {
        return context.ToLower()
                     .Replace(" ", "_")
                     .Replace("-", "_")
                     .Replace(".", "")
                     .Trim();
    }

    private static string[] ExpandStepsForContext(dynamic steps, string context)
    {
        var baseSteps = steps as string[] ?? new string[0];
        var contextSteps = new List<string>();

        // Add context setup step
        contextSteps.Add($"Configure {context} environment");

        // Expand existing steps with context
        foreach (var step in baseSteps)
        {
            contextSteps.Add($"{step} in {context} context");
        }

        // Add context-specific validation step
        contextSteps.Add($"Verify {context} specific behavior");

        return contextSteps.ToArray();
    }

    private static double AdjustConfidenceForContext(dynamic baseConfidence, string context)
    {
        var confidence = Convert.ToDouble(baseConfidence);
        
        // Adjust confidence based on context complexity
        if (context.ToLower().Contains("no-pin") || context.ToLower().Contains("athena"))
        {
            return Math.Max(0.6, confidence - 0.1); // Slightly reduce for complex IVR contexts
        }
        
        if (context.ToLower().Contains("fallback") || context.ToLower().Contains("error"))
        {
            return Math.Max(0.5, confidence - 0.2); // Reduce for error scenarios
        }

        return confidence;
    }

    private static string DetermineContextType(string context)
    {
        var lowerContext = context.ToLower();
        
        if (lowerContext.Contains("ivr") || lowerContext.Contains("pin") || lowerContext.Contains("athena"))
            return "IVR_Context";
        
        if (lowerContext.Contains("language") || lowerContext.Contains("locale"))
            return "Language_Context";
        
        if (lowerContext.Contains("user") || lowerContext.Contains("role"))
            return "User_Context";
        
        if (lowerContext.Contains("environment") || lowerContext.Contains("env"))
            return "Environment_Context";
        
        return "Generic_Context";
    }

    private static string CategorizeContext(string context)
    {
        var lowerContext = context.ToLower();
        
        if (lowerContext.Contains("performance") || lowerContext.Contains("load"))
            return "Performance";
        
        if (lowerContext.Contains("security") || lowerContext.Contains("auth"))
            return "Security";
        
        if (lowerContext.Contains("ui") || lowerContext.Contains("interface"))
            return "UserInterface";
        
        if (lowerContext.Contains("integration") || lowerContext.Contains("api"))
            return "Integration";
        
        return "Functional";
    }

    private static List<object> GenerateCrossContextTests(List<string> contexts, dynamic baseTest)
    {
        var crossContextTests = new List<object>();
        
        // Generate tests for context transitions
        for (int i = 0; i < contexts.Count - 1; i++)
        {
            for (int j = i + 1; j < contexts.Count; j++)
            {
                var fromContext = contexts[i];
                var toContext = contexts[j];
                
                crossContextTests.Add(new
                {
                    type = $"context_transition_{SanitizeContextName(fromContext)}_to_{SanitizeContextName(toContext)}",
                    title = $"Context Transition: {fromContext} to {toContext}",
                    priority = baseTest.priority,
                    category = "Integration",
                    contexts = new[] { fromContext, toContext },
                    steps = new[]
                    {
                        $"Initialize {fromContext} context",
                        $"Perform base operation in {fromContext}",
                        $"Transition to {toContext} context",
                        $"Verify state preservation during transition",
                        $"Complete operation in {toContext}",
                        "Validate cross-context behavior"
                    },
                    expectedResult = $"Successful transition from {fromContext} to {toContext} with state preservation",
                    testData = $"Cross-context test data for {fromContext} and {toContext}",
                    confidence = AdjustConfidenceForContext(baseTest.confidence, "cross_context") - 0.1,
                    llmGuidance = new
                    {
                        expandAreas = new[] { 
                            "State preservation requirements",
                            "Context switching protocols",
                            "Error handling during transitions"
                        },
                        considerations = new[] { 
                            "Data consistency across contexts",
                            "Performance impact of context switching",
                            "User experience during transitions"
                        },
                        contextType = "Cross_Context_Transition"
                    },
                    metadata = new
                    {
                        isCrossContext = true,
                        fromContext = fromContext,
                        toContext = toContext,
                        transitionType = "sequential"
                    }
                });
            }
        }

        return crossContextTests;
    }

    #endregion

    #region Data Validation Template Generators

    private static List<object> GenerateFormatValidationTemplates(TechnicalArtifacts artifacts)
    {
        var templates = new List<object>();

        // IPA phoneme validation
        if (artifacts.TechnicalTerminology.Any(t => t.ToLower().Contains("phoneme") || t.ToLower().Contains("ipa")))
        {
            templates.Add(new
            {
                type = "ipa_phoneme_format_validation",
                title = "IPA Phoneme Format Validation",
                priority = "High",
                category = "DataValidation",
                steps = new[]
                {
                    "Submit valid IPA phoneme format (e.g., ərˈdu)",
                    "Verify phoneme acceptance",
                    "Test phoneme processing",
                    "Validate output format"
                },
                expectedResult = "Valid IPA phonemes accepted and processed correctly",
                testData = "Valid IPA phoneme samples: ərˈdu, ˈæpəl, həˈloʊ",
                confidence = 0.9,
                llmGuidance = new
                {
                    expandAreas = new[] { "IPA character set validation", "Unicode support", "Pronunciation accuracy" },
                    considerations = new[] { "Character encoding", "Language support", "Audio output quality" }
                }
            });

            templates.Add(new
            {
                type = "invalid_phoneme_rejection",
                title = "Invalid Phoneme Character Rejection",
                priority = "High",
                category = "NegativeValidation",
                steps = new[]
                {
                    "Submit invalid phoneme characters",
                    "Verify rejection with clear error",
                    "Test various invalid formats",
                    "Confirm error message clarity"
                },
                expectedResult = "Invalid phonemes rejected with descriptive error messages",
                testData = "Invalid samples: abc123, @@##, empty string, null",
                confidence = 0.85,
                llmGuidance = new
                {
                    expandAreas = new[] { "Error message quality", "Input sanitization", "User guidance" },
                    considerations = new[] { "User experience", "Security", "Error recovery" }
                }
            });
        }

        // Unicode compliance validation
        if (artifacts.FormatConstraints.Any(c => c.ToLower().Contains("unicode")))
        {
            templates.Add(new
            {
                type = "unicode_compliance_validation",
                title = "Unicode Compliance Verification",
                priority = "Medium",
                category = "FormatCompliance",
                steps = new[]
                {
                    "Submit Unicode test strings",
                    "Verify proper encoding handling",
                    "Test special Unicode characters",
                    "Validate character preservation"
                },
                expectedResult = "Unicode characters handled correctly with proper encoding",
                testData = "Unicode samples: 你好, مرحبا, עברית, русский",
                confidence = 0.8,
                llmGuidance = new
                {
                    expandAreas = new[] { "Character encoding standards", "Multi-language support", "Display rendering" },
                    considerations = new[] { "Internationalization", "Accessibility", "Performance" }
                }
            });
        }

        return templates;
    }

    private static List<object> GenerateBoundaryTestTemplates(TechnicalArtifacts artifacts)
    {
        var templates = new List<object>();

        templates.Add(new
        {
            type = "maximum_length_boundary",
            title = "Maximum Input Length Boundary Test",
            priority = "Medium",
            category = "BoundaryValidation",
            steps = new[]
            {
                "Determine maximum allowed input length",
                "Submit input at maximum length",
                "Submit input exceeding maximum by 1 character",
                "Verify appropriate handling"
            },
            expectedResult = "Maximum length enforced with clear boundary behavior",
            testData = "Strings at and beyond maximum length limits",
            confidence = 0.85,
            llmGuidance = new
            {
                expandAreas = new[] { "Length limits definition", "Error handling", "Performance impact" },
                considerations = new[] { "User experience", "System stability", "Memory usage" }
            }
        });

        templates.Add(new
        {
            type = "empty_null_input_handling",
            title = "Empty and Null Input Handling",
            priority = "High",
            category = "BoundaryValidation",
            steps = new[]
            {
                "Submit empty string input",
                "Submit null value",
                "Submit whitespace-only input",
                "Verify consistent handling"
            },
            expectedResult = "Empty and null inputs handled gracefully with appropriate defaults or errors",
            testData = "Empty string, null, whitespace variations",
            confidence = 0.9,
            llmGuidance = new
            {
                expandAreas = new[] { "Default value behavior", "Error messaging", "Input sanitization" },
                considerations = new[] { "Data integrity", "User guidance", "System robustness" }
            }
        });

        return templates;
    }

    private static List<object> GenerateMalformedInputTemplates(TechnicalArtifacts artifacts)
    {
        var templates = new List<object>();

        if (artifacts.SampleData.Any(s => s.Contains("JSON")))
        {
            templates.Add(new
            {
                type = "malformed_json_handling",
                title = "Malformed JSON Input Handling",
                priority = "High",
                category = "ErrorHandling",
                steps = new[]
                {
                    "Submit JSON with missing brackets",
                    "Submit JSON with invalid syntax",
                    "Submit truncated JSON",
                    "Verify error handling and recovery"
                },
                expectedResult = "Malformed JSON handled gracefully with descriptive errors",
                testData = "Invalid JSON samples: {missing}, {\"key\":}, [{broken]",
                confidence = 0.8,
                llmGuidance = new
                {
                    expandAreas = new[] { "JSON parsing robustness", "Error recovery", "User feedback" },
                    considerations = new[] { "Data validation", "Security", "User experience" }
                }
            });
        }

        return templates;
    }

    private static List<object> GenerateSqlValidationTemplates(TechnicalArtifacts artifacts)
    {
        var templates = new List<object>();

        templates.Add(new
        {
            type = "sql_injection_protection",
            title = "SQL Injection Protection Validation",
            priority = "Critical",
            category = "SecurityValidation",
            steps = new[]
            {
                "Submit SQL injection attempt patterns",
                "Verify input sanitization",
                "Test parameterized query handling",
                "Confirm database security"
            },
            expectedResult = "SQL injection attempts blocked with proper sanitization",
            testData = "Injection patterns: '; DROP TABLE; --, UNION SELECT, etc.",
            confidence = 0.95,
            llmGuidance = new
            {
                expandAreas = new[] { "Input sanitization", "Parameterized queries", "Security logging" },
                considerations = new[] { "Data security", "Compliance", "Attack prevention" }
            }
        });

        return templates;
    }

    private static List<object> GenerateConfigValidationTemplates(TechnicalArtifacts artifacts)
    {
        var templates = new List<object>();

        if (artifacts.ConfigurationHints.Any(c => c.ToLower().Contains("language")))
        {
            templates.Add(new
            {
                type = "language_config_validation",
                title = "Language Configuration Validation",
                priority = "Medium",
                category = "ConfigurationValidation",
                steps = new[]
                {
                    "Submit supported language codes",
                    "Submit unsupported language codes",
                    "Test fallback language behavior",
                    "Verify configuration persistence"
                },
                expectedResult = "Language configuration handled with appropriate fallbacks",
                testData = "Language codes: en-US, es-ES, invalid-XX, null",
                confidence = 0.8,
                llmGuidance = new
                {
                    expandAreas = new[] { "Language support matrix", "Fallback strategies", "User preferences" },
                    considerations = new[] { "Internationalization", "User experience", "Default behavior" }
                }
            });
        }

        return templates;
    }

    #endregion

    #region LLM Validation and Confidence Adjustment

    /// <summary>
    /// Validates generated test cases against original analysis and adjusts confidence
    /// </summary>
    /// <param name="testCases">Generated test cases to validate</param>
    /// <param name="originalAnalysis">Original Jira analysis for comparison</param>
    /// <param name="baselineConfidence">Baseline confidence from executive summary</param>
    /// <returns>Final confidence score with adjustment reasoning</returns>
    public static FinalConfidenceScore ValidateAndAdjustConfidence(
        object testCases, 
        object originalAnalysis, 
        ExecutiveSummaryConfidence baselineConfidence)
    {
        try
        {
            var llmValidation = ValidateGeneratedTestCases(testCases, originalAnalysis);
            var finalConfidence = ReconcileConfidenceScores(baselineConfidence.OverallConfidence, llmValidation);
            
            return finalConfidence;
        }
        catch (Exception ex)
        {
            // Graceful degradation - return baseline confidence if validation fails
            return new FinalConfidenceScore
            {
                BaselineConfidence = baselineConfidence.OverallConfidence,
                LLMValidationScore = 0.5, // Default fallback
                FinalConfidence = Math.Max(0.4, baselineConfidence.OverallConfidence), // Slight penalty for validation failure
                ConfidenceAdjustment = -0.1,
                AdjustmentReasoning = $"Validation failed due to error: {ex.Message}. Applied conservative adjustment.",
                RequiresManualReview = true
            };
        }
    }

    /// <summary>
    /// Validates generated test cases quality against LLM validation criteria
    /// </summary>
    /// <param name="testCases">Generated test cases to validate</param>
    /// <param name="originalAnalysis">Original analysis for comparison</param>
    /// <returns>LLM validation result with quality scores</returns>
    private static LLMValidationResult ValidateGeneratedTestCases(object testCases, object originalAnalysis)
    {
        // Convert test cases to analyzable format
        var testCaseJson = testCases?.ToString() ?? "{}";
        var analysisJson = originalAnalysis?.ToString() ?? "{}";
        
        // Assess test case completeness vs. requirements coverage
        var testCaseCompleteness = AssessTestCaseCompleteness(testCaseJson, analysisJson);
        
        // Evaluate edge case identification accuracy
        var edgeCaseAccuracy = AssessEdgeCaseAccuracy(testCaseJson, analysisJson);
        
        // Analyze business logic validation depth
        var businessLogicDepth = AssessBusinessLogicDepth(testCaseJson, analysisJson);
        
        // Check integration testing coverage adequacy
        var integrationCoverage = AssessIntegrationCoverage(testCaseJson, analysisJson);
        
        // Evaluate error handling scenario completeness
        var errorHandlingCompleteness = AssessErrorHandlingCompleteness(testCaseJson, analysisJson);
        
        var validationNotes = GenerateValidationNotes(
            testCaseCompleteness, edgeCaseAccuracy, businessLogicDepth, 
            integrationCoverage, errorHandlingCompleteness);

        return new LLMValidationResult
        {
            TestCaseCompleteness = testCaseCompleteness,
            EdgeCaseAccuracy = edgeCaseAccuracy,
            BusinessLogicDepth = businessLogicDepth,
            IntegrationCoverage = integrationCoverage,
            ErrorHandlingCompleteness = errorHandlingCompleteness,
            ValidationNotes = validationNotes
        };
    }

    /// <summary>
    /// Reconciles baseline confidence with LLM validation to produce final score
    /// </summary>
    /// <param name="baselineConfidence">Original algorithmic confidence</param>
    /// <param name="llmValidation">LLM validation results</param>
    /// <returns>Final confidence score with reasoning</returns>
    private static FinalConfidenceScore ReconcileConfidenceScores(double baselineConfidence, LLMValidationResult llmValidation)
    {
        var llmScore = llmValidation.OverallQuality;
        var variance = Math.Abs(llmScore - baselineConfidence);
        
        // Apply reconciliation logic based on variance and LLM quality
        double adjustment = 0.0;
        string reasoning = "";
        bool requiresReview = false;

        if (variance > 0.20)
        {
            // High variance - flag for manual review
            requiresReview = true;
            adjustment = -0.05; // Conservative penalty
            reasoning = $"High variance ({variance:F2}) between baseline ({baselineConfidence:F2}) and LLM validation ({llmScore:F2}) - requires manual review";
        }
        else if (llmScore > baselineConfidence)
        {
            // LLM validation higher than baseline - boost confidence
            adjustment = Math.Min(0.10, (llmScore - baselineConfidence) * 0.5);
            reasoning = $"LLM validation quality ({llmScore:F2}) exceeds baseline ({baselineConfidence:F2}) - confidence boosted by {adjustment:F2}";
        }
        else if (llmScore < baselineConfidence)
        {
            // LLM validation lower than baseline - reduce confidence
            adjustment = Math.Max(-0.15, (llmScore - baselineConfidence) * 0.7);
            reasoning = $"LLM validation quality ({llmScore:F2}) below baseline ({baselineConfidence:F2}) - confidence reduced by {Math.Abs(adjustment):F2}";
        }
        else
        {
            reasoning = $"LLM validation confirms baseline confidence ({baselineConfidence:F2}) - no adjustment needed";
        }

        var finalConfidence = Math.Max(0.0, Math.Min(1.0, baselineConfidence + adjustment));

        return new FinalConfidenceScore
        {
            BaselineConfidence = baselineConfidence,
            LLMValidationScore = llmScore,
            FinalConfidence = Math.Round(finalConfidence, 3),
            ConfidenceAdjustment = Math.Round(adjustment, 3),
            AdjustmentReasoning = reasoning,
            RequiresManualReview = requiresReview
        };
    }

    /// <summary>
    /// Assesses test case completeness against requirements
    /// </summary>
    private static double AssessTestCaseCompleteness(string testCaseJson, string analysisJson)
    {
        double completeness = 0.5; // Base score

        // Check for essential test case elements
        if (testCaseJson.Contains("happy_path", StringComparison.OrdinalIgnoreCase)) completeness += 0.15;
        if (testCaseJson.Contains("error_handling", StringComparison.OrdinalIgnoreCase)) completeness += 0.15;
        if (testCaseJson.Contains("boundary", StringComparison.OrdinalIgnoreCase)) completeness += 0.10;
        if (testCaseJson.Contains("acceptance", StringComparison.OrdinalIgnoreCase)) completeness += 0.10;

        // Bonus for detailed steps and expected results
        var stepCount = CountOccurrences(testCaseJson, "step");
        var expectedCount = CountOccurrences(testCaseJson, "expected");
        if (stepCount >= 3 && expectedCount >= 2) completeness += 0.10;

        return Math.Min(1.0, completeness);
    }

    /// <summary>
    /// Assesses edge case identification accuracy
    /// </summary>
    private static double AssessEdgeCaseAccuracy(string testCaseJson, string analysisJson)
    {
        double accuracy = 0.4; // Base score

        // Check for edge case indicators
        if (testCaseJson.Contains("edge", StringComparison.OrdinalIgnoreCase)) accuracy += 0.20;
        if (testCaseJson.Contains("boundary", StringComparison.OrdinalIgnoreCase)) accuracy += 0.15;
        if (testCaseJson.Contains("limit", StringComparison.OrdinalIgnoreCase)) accuracy += 0.10;
        if (testCaseJson.Contains("minimum", StringComparison.OrdinalIgnoreCase) || 
            testCaseJson.Contains("maximum", StringComparison.OrdinalIgnoreCase)) accuracy += 0.15;

        return Math.Min(1.0, accuracy);
    }

    /// <summary>
    /// Assesses business logic validation depth
    /// </summary>
    private static double AssessBusinessLogicDepth(string testCaseJson, string analysisJson)
    {
        double depth = 0.3; // Base score

        // Check for business logic coverage
        if (testCaseJson.Contains("validation", StringComparison.OrdinalIgnoreCase)) depth += 0.20;
        if (testCaseJson.Contains("rule", StringComparison.OrdinalIgnoreCase)) depth += 0.15;
        if (testCaseJson.Contains("condition", StringComparison.OrdinalIgnoreCase)) depth += 0.15;
        if (testCaseJson.Contains("workflow", StringComparison.OrdinalIgnoreCase)) depth += 0.10;
        if (testCaseJson.Contains("logic", StringComparison.OrdinalIgnoreCase)) depth += 0.10;

        return Math.Min(1.0, depth);
    }

    /// <summary>
    /// Assesses integration testing coverage
    /// </summary>
    private static double AssessIntegrationCoverage(string testCaseJson, string analysisJson)
    {
        double coverage = 0.5; // Base score

        // Check for integration test indicators
        if (testCaseJson.Contains("integration", StringComparison.OrdinalIgnoreCase)) coverage += 0.20;
        if (testCaseJson.Contains("api", StringComparison.OrdinalIgnoreCase)) coverage += 0.15;
        if (testCaseJson.Contains("service", StringComparison.OrdinalIgnoreCase)) coverage += 0.10;
        if (testCaseJson.Contains("database", StringComparison.OrdinalIgnoreCase)) coverage += 0.05;

        return Math.Min(1.0, coverage);
    }

    /// <summary>
    /// Assesses error handling scenario completeness
    /// </summary>
    private static double AssessErrorHandlingCompleteness(string testCaseJson, string analysisJson)
    {
        double completeness = 0.3; // Base score

        // Check for error handling coverage
        if (testCaseJson.Contains("error", StringComparison.OrdinalIgnoreCase)) completeness += 0.25;
        if (testCaseJson.Contains("exception", StringComparison.OrdinalIgnoreCase)) completeness += 0.15;
        if (testCaseJson.Contains("invalid", StringComparison.OrdinalIgnoreCase)) completeness += 0.15;
        if (testCaseJson.Contains("timeout", StringComparison.OrdinalIgnoreCase)) completeness += 0.10;
        if (testCaseJson.Contains("failure", StringComparison.OrdinalIgnoreCase)) completeness += 0.05;

        return Math.Min(1.0, completeness);
    }

    /// <summary>
    /// Generates validation notes based on assessment results
    /// </summary>
    private static List<string> GenerateValidationNotes(
        double completeness, double edgeCase, double businessLogic, 
        double integration, double errorHandling)
    {
        var notes = new List<string>();

        if (completeness >= 0.8) notes.Add("Excellent test case completeness");
        else if (completeness < 0.5) notes.Add("Test case completeness needs improvement");

        if (edgeCase >= 0.7) notes.Add("Good edge case coverage");
        else if (edgeCase < 0.5) notes.Add("Edge case identification could be enhanced");

        if (businessLogic >= 0.7) notes.Add("Strong business logic validation");
        else if (businessLogic < 0.5) notes.Add("Business logic testing needs more depth");

        if (integration >= 0.7) notes.Add("Adequate integration testing coverage");
        else if (integration < 0.4) notes.Add("Integration testing coverage is minimal");

        if (errorHandling >= 0.7) notes.Add("Comprehensive error handling scenarios");
        else if (errorHandling < 0.5) notes.Add("Error handling scenarios need expansion");

        return notes;
    }

    /// <summary>
    /// Counts occurrences of a substring in a string (case-insensitive)
    /// </summary>
    private static int CountOccurrences(string text, string substring)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(substring))
            return 0;

        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(substring, index, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            count++;
            index += substring.Length;
        }
        return count;
    }

    #endregion
}
