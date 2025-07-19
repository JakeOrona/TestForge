using System.Text.Json;

namespace TestForge.McpServer.Services;

/// <summary>
/// Service responsible for extracting and categorizing UI components from ticket descriptions
/// </summary>
public static class UiComponentAnalysisService
{
    /// <summary>
    /// Extracts and categorizes UI components from ticket descriptions
    /// </summary>
    /// <param name="description">The ticket description or requirements text</param>
    /// <returns>Structured analysis of UI components with complexity scoring and test recommendations</returns>
    public static string ExtractComponents(string description)
    {
        try
        {
            // Basic pattern matching for UI components
            var components = new List<object>();
            
            if (!string.IsNullOrWhiteSpace(description))
            {
                var lowerDesc = description.ToLower();
                
                if (lowerDesc.Contains("input") || lowerDesc.Contains("field"))
                {
                    components.Add(new { type = "input", name = "input_field", complexity = "medium", testAreas = new[] { "validation", "formatting" }, confidence = 0.8 });
                }
                
                if (lowerDesc.Contains("button") || lowerDesc.Contains("click"))
                {
                    components.Add(new { type = "button", name = "button_element", complexity = "low", testAreas = new[] { "interaction", "state" }, confidence = 0.85 });
                }
                
                if (lowerDesc.Contains("modal") || lowerDesc.Contains("dialog"))
                {
                    components.Add(new { type = "modal", name = "modal_dialog", complexity = "high", testAreas = new[] { "display", "interaction", "close" }, confidence = 0.9 });
                }
                
                if (lowerDesc.Contains("form"))
                {
                    components.Add(new { type = "form", name = "form_element", complexity = "high", testAreas = new[] { "validation", "submission" }, confidence = 0.88 });
                }
                
                if (lowerDesc.Contains("dropdown") || lowerDesc.Contains("select"))
                {
                    components.Add(new { type = "dropdown", name = "dropdown_element", complexity = "medium", testAreas = new[] { "selection", "options" }, confidence = 0.82 });
                }
                
                if (lowerDesc.Contains("checkbox") || lowerDesc.Contains("radio"))
                {
                    components.Add(new { type = "checkbox", name = "checkbox_element", complexity = "low", testAreas = new[] { "selection", "state" }, confidence = 0.85 });
                }
                
                if (lowerDesc.Contains("table") || lowerDesc.Contains("grid"))
                {
                    components.Add(new { type = "table", name = "table_element", complexity = "high", testAreas = new[] { "data_display", "sorting", "filtering" }, confidence = 0.87 });
                }
                
                if (lowerDesc.Contains("menu") || lowerDesc.Contains("navigation"))
                {
                    components.Add(new { type = "navigation", name = "navigation_element", complexity = "medium", testAreas = new[] { "navigation", "accessibility" }, confidence = 0.83 });
                }
                
                if (lowerDesc.Contains("upload") || lowerDesc.Contains("file"))
                {
                    components.Add(new { type = "file_upload", name = "file_upload_element", complexity = "high", testAreas = new[] { "file_handling", "validation", "security" }, confidence = 0.89 });
                }
                
                if (lowerDesc.Contains("search") || lowerDesc.Contains("filter"))
                {
                    components.Add(new { type = "search", name = "search_element", complexity = "medium", testAreas = new[] { "search_functionality", "results" }, confidence = 0.84 });
                }
                
                if (lowerDesc.Contains("tooltip") || lowerDesc.Contains("popup"))
                {
                    components.Add(new { type = "tooltip", name = "tooltip_element", complexity = "low", testAreas = new[] { "display", "timing" }, confidence = 0.78 });
                }
                
                if (lowerDesc.Contains("tab") || lowerDesc.Contains("accordion"))
                {
                    components.Add(new { type = "tab", name = "tab_element", complexity = "medium", testAreas = new[] { "navigation", "content_switching" }, confidence = 0.81 });
                }
            }
            
            var analysis = new
            {
                uiComponents = components,
                componentCount = components.Count,
                complexityDistribution = CalculateComplexityDistribution(components),
                testingRecommendations = GenerateTestingRecommendations(components),
                accessibilityConsiderations = GenerateAccessibilityConsiderations(components),
                estimatedTestingEffort = EstimateTestingEffort(components)
            };
            
            return JsonSerializer.Serialize(analysis, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = $"UI analysis failed: {ex.Message}" });
        }
    }

    /// <summary>
    /// Calculates the complexity distribution of identified UI components
    /// </summary>
    /// <param name="components">List of UI components</param>
    /// <returns>Complexity distribution summary</returns>
    private static object CalculateComplexityDistribution(List<object> components)
    {
        var high = 0;
        var medium = 0;
        var low = 0;
        
        foreach (dynamic component in components)
        {
            var complexity = component.GetType().GetProperty("complexity")?.GetValue(component)?.ToString();
            switch (complexity)
            {
                case "high":
                    high++;
                    break;
                case "medium":
                    medium++;
                    break;
                case "low":
                    low++;
                    break;
            }
        }
        
        return new
        {
            high = high,
            medium = medium,
            low = low,
            total = components.Count,
            overallComplexity = high > 0 ? "high" : medium > 0 ? "medium" : "low"
        };
    }

    /// <summary>
    /// Generates testing recommendations based on identified UI components
    /// </summary>
    /// <param name="components">List of UI components</param>
    /// <returns>Testing recommendations</returns>
    private static string[] GenerateTestingRecommendations(List<object> components)
    {
        var recommendations = new List<string>
        {
            "Test all UI elements for proper display and positioning",
            "Verify responsive behavior across different screen sizes",
            "Validate keyboard navigation and accessibility"
        };
        
        if (components.Any())
        {
            recommendations.Add("Test user interactions with all identified components");
            recommendations.Add("Validate error states and user feedback");
            recommendations.Add("Verify consistent styling and branding");
        }
        
        return recommendations.ToArray();
    }

    /// <summary>
    /// Generates accessibility considerations based on identified UI components
    /// </summary>
    /// <param name="components">List of UI components</param>
    /// <returns>Accessibility considerations</returns>
    private static string[] GenerateAccessibilityConsiderations(List<object> components)
    {
        var considerations = new List<string>
        {
            "Ensure proper color contrast ratios",
            "Verify keyboard navigation support",
            "Test screen reader compatibility",
            "Validate ARIA labels and roles"
        };
        
        if (components.Any(c => c?.ToString()?.Contains("form") == true))
        {
            considerations.Add("Verify form labels and field associations");
        }
        
        if (components.Any(c => c?.ToString()?.Contains("modal") == true))
        {
            considerations.Add("Test modal focus management and escape functionality");
        }
        
        if (components.Any(c => c?.ToString()?.Contains("table") == true))
        {
            considerations.Add("Verify table headers and data relationships");
        }
        
        return considerations.ToArray();
    }

    /// <summary>
    /// Estimates testing effort based on component complexity
    /// </summary>
    /// <param name="components">List of UI components</param>
    /// <returns>Testing effort estimation</returns>
    private static object EstimateTestingEffort(List<object> components)
    {
        var totalScore = 0;
        var componentCount = components.Count;
        
        foreach (dynamic component in components)
        {
            var complexity = component.GetType().GetProperty("complexity")?.GetValue(component)?.ToString();
            totalScore += complexity switch
            {
                "high" => 3,
                "medium" => 2,
                "low" => 1,
                _ => 1
            };
        }
        
        var effortLevel = totalScore switch
        {
            <= 3 => "Low",
            <= 6 => "Medium",
            <= 10 => "High",
            _ => "Very High"
        };
        
        return new
        {
            effortLevel = effortLevel,
            estimatedHours = Math.Max(componentCount * 2, 4), // Minimum 4 hours
            complexityScore = totalScore,
            componentCount = componentCount,
            recommendations = new[]
            {
                "Focus on high-complexity components first",
                "Consider automation for repetitive UI tests",
                "Plan for cross-browser compatibility testing"
            }
        };
    }
}
