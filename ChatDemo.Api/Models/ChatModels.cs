using System.ComponentModel.DataAnnotations;

namespace ChatDemo.Api.Models;

public class ChatRequestModel
{
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public string? ConversationId { get; set; }
    
    public float? TimeZoneOffset { get; set; }
}

public class ChatResponseModel
{
    public bool Success { get; set; }
    
    public ChatMessageModel? Message { get; set; }
    
    public string? Error { get; set; }
}

public class ChatMessageModel
{
    public string Message { get; set; } = string.Empty;
    
    public bool IsFromUser { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ConversationHistoryResponse
{
    public bool Success { get; set; }
    
    public List<ChatMessageModel> Messages { get; set; } = new();
    
    public string? Error { get; set; }
}
