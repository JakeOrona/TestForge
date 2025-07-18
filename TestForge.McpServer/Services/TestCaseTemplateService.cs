using System.Text.Json;

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
}
