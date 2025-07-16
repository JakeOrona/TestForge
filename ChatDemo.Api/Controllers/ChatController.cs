using Microsoft.AspNetCore.Mvc;
using ChatDemo.Api.Models;
using ChatDemo.Api.Services;

namespace ChatDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> _logger;
    private readonly ChatbotAIService _chatbotService;

    public ChatController(ILogger<ChatController> logger, ChatbotAIService chatbotService)
    {
        _logger = logger;
        _chatbotService = chatbotService;
    }

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequestModel request)
    {
        try
        {
            _logger.LogInformation("Processing chat request: {Message}", request.Message);

            var response = await _chatbotService.ProcessMessageAsync(request);

            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat request");
            return BadRequest(new ChatResponseModel 
            { 
                Success = false, 
                Error = "An error occurred processing your request." 
            });
        }
    }

    [HttpGet("history/{conversationId}")]
    public async Task<IActionResult> GetConversationHistory(string conversationId)
    {
        try
        {
            _logger.LogInformation("Getting conversation history: {ConversationId}", conversationId);

            var history = await _chatbotService.GetConversationHistoryAsync(conversationId);

            return Ok(new ConversationHistoryResponse
            {
                Success = true,
                Messages = history
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversation history");
            return BadRequest(new ConversationHistoryResponse 
            { 
                Success = false, 
                Error = "An error occurred retrieving conversation history." 
            });
        }
    }

    [HttpGet("tools")]
    public IActionResult GetAvailableTools()
    {
        try
        {
            // Return the available tools that we know about
            var tools = new[]
            {
                new { name = "sayHello", description = "Says hello to the user with a personalized greeting" },
                new { name = "getAppInfo", description = "Provides information about this ChatDemo application" },
                new { name = "echo", description = "Echoes the message back to the client" },
                new { name = "reverseText", description = "Reverses the given text" },
                new { name = "getCurrentTime", description = "Gets current date and time" },
                new { name = "generateRandomNumber", description = "Generates a random number between min and max values" }
            };

            return Ok(tools);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available tools");
            return BadRequest(new { error = "An error occurred getting available tools." });
        }
    }
}
