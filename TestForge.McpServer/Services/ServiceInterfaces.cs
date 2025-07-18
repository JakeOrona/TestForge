namespace TestForge.McpServer.Services;

/// <summary>
/// Interface for UI component analysis service
/// </summary>
public interface IUiComponentAnalysisService
{
    string ExtractComponents(string description);
}

/// <summary>
/// Interface for business logic analysis service
/// </summary>
public interface IBusinessLogicAnalysisService
{
    string ExtractLogic(string description);
}

/// <summary>
/// Wrapper service for UI component analysis
/// </summary>
public class UiComponentAnalysisServiceWrapper : IUiComponentAnalysisService
{
    public string ExtractComponents(string description)
    {
        return UiComponentAnalysisService.ExtractComponents(description);
    }
}

/// <summary>
/// Wrapper service for business logic analysis
/// </summary>
public class BusinessLogicAnalysisServiceWrapper : IBusinessLogicAnalysisService
{
    public string ExtractLogic(string description)
    {
        return BusinessLogicAnalysisService.ExtractLogic(description);
    }
}
