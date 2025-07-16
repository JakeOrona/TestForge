using ChatDemo.Api.Models;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System.Text.Json;
using System.Net.Http;

namespace ChatDemo.Api.Services;

public class ChatbotAIService
{
    private readonly ILogger<ChatbotAIService> _logger;
    private readonly IChatClient _chatClient;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public ChatbotAIService(
        ILogger<ChatbotAIService> logger,
        IChatClient chatClient,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _chatClient = chatClient;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Process a message sent by a user to the chatbot and return an AI-generated response
    /// </summary>
    /// <param name="request">The user's message request</param>
    /// <returns>Response from the chatbot</returns>
    public async Task<ChatResponseModel> ProcessMessageAsync(ChatRequestModel request)
    {
        try
        {
            _logger.LogInformation("Processing message for conversation: {ConversationId}", request.ConversationId);

            // Get conversation history if an ID was provided, otherwise create a new one
            var conversationId = request.ConversationId ?? Guid.NewGuid().ToString();
            var history = await GetConversationHistoryAsync(conversationId);
            
            // Add the new user message to history
            var userMessage = new ChatMessageModel
            {
                Message = request.Message,
                IsFromUser = true,
                Timestamp = DateTime.UtcNow
            };
            history.Add(userMessage);                
            
            // Get MCP client for tools
            var mcpClient = await GetMcpClient();
            
            // Generate AI response using the chat client with tools
            var response = await GenerateResponseWithToolsAsync(history, mcpClient, request.TimeZoneOffset);
            
            // Save the AI response to history
            var aiMessage = new ChatMessageModel
            {
                Message = response,
                IsFromUser = false,
                Timestamp = DateTime.UtcNow
            };
            history.Add(aiMessage);

            // Save the updated conversation
            await SaveConversationHistoryAsync(conversationId, history);

            return new ChatResponseModel
            {
                Success = true,
                Message = aiMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chatbot message: {Message}", ex.Message);
            return new ChatResponseModel
            {
                Success = false,
                Error = "Failed to process your message. Please try again later."
            };
        }
    }

    /// <summary>
    /// Get the conversation history for a specific conversation ID
    /// </summary>
    /// <param name="conversationId">Unique identifier for the conversation</param>
    /// <returns>List of chat messages in the conversation</returns>
    public async Task<List<ChatMessageModel>> GetConversationHistoryAsync(string conversationId)
    {
        try
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                return new List<ChatMessageModel>();
            }

            // For this demo, we'll store conversations in memory or use a simple file-based storage
            // In a real application, you would use a database or Redis
            var filePath = Path.Combine(Path.GetTempPath(), $"chat_{conversationId}.json");
            
            if (!File.Exists(filePath))
            {
                return new List<ChatMessageModel>();
            }

            var jsonContent = await File.ReadAllTextAsync(filePath);
            var history = JsonSerializer.Deserialize<List<ChatMessageModel>>(jsonContent);

            return history ?? new List<ChatMessageModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation history: {Message}", ex.Message);
            return new List<ChatMessageModel>();
        }
    }

    /// <summary>
    /// Get MCP client for accessing tools
    /// </summary>
    private async Task<IMcpClient?> GetMcpClient()
    {
        try
        {
            _logger.LogInformation("Setting up MCP client connection");

            // Read MCP endpoint from configuration (fallback to localhost:5001 for development)
            var endpoint = _configuration["McpServer:McpEndpoint"] ?? "http://localhost:5001/mcp";

            _logger.LogInformation("Using MCP endpoint: {endpoint}", endpoint);

            // Create transport options with proper initialization
            var options = new SseClientTransportOptions 
            { 
                Endpoint = new Uri(endpoint),
                TransportMode = HttpTransportMode.Sse
            };

            // Create HTTP client with SSL certificate validation bypass for development
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true; // Accept all certificates for development
                };

            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(endpoint),
                Timeout = TimeSpan.FromSeconds(30)
            };

            // Create the SSE transport with our options
            var transport = new SseClientTransport(options, httpClient);
            await transport.ConnectAsync(CancellationToken.None);

            var mcpClient = await McpClientFactory.CreateAsync(transport);
            
            if (mcpClient != null)
            {
                _logger.LogInformation("Successfully connected to MCP server at {endpoint}", endpoint);
            }
            
            return mcpClient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to MCP server: {Message}", ex.Message);
            if (ex.InnerException != null)
            {
                _logger.LogError(ex, "Inner exception: {Message}", ex.InnerException.Message);
            }
            return null;
        }
    }

    /// <summary>
    /// Generate a response using the chat client with tools
    /// </summary>
    private async Task<string> GenerateResponseWithToolsAsync(List<ChatMessageModel> history, IMcpClient? mcpClient, float? timezoneOffset)
    {
        try
        {
            var messages = new List<ChatMessage>();
            var systemMessage = "Your name is ChatDemo AI Assistant. You are an AI assistant for ChatDemo application. " +
                "You help users with their questions and provide clear, helpful responses. " +
                "You have access to various tools to help answer questions and perform tasks. " +
                "When dealing with time-related matters, always use the tools to get the current time. " +
                "Deliver all responses in Markdown formatting, and make sure to properly format any URL. " +
                "Be helpful, accurate, and friendly in your responses.";

            if (mcpClient == null)
            {
                systemMessage += " Note: We are currently experiencing technical issues connecting to some tools, so some functionality may be limited.";
            }

            // Add system message
            messages.Add(new ChatMessage(ChatRole.System, systemMessage));

            // Add conversation history
            foreach (var msg in history)
            {
                var role = msg.IsFromUser ? ChatRole.User : ChatRole.Assistant;
                messages.Add(new ChatMessage(role, msg.Message));
            }                
            
            // Get available tools from MCP client
            var tools = new List<AITool>();
            if (mcpClient != null)
            {
                try
                {
                    var mcpTools = await mcpClient.ListToolsAsync();
                    var aiTools = mcpTools.Select(t => t as AITool).Where(t => t != null).ToList();
                    tools.AddRange(aiTools);
                    _logger.LogInformation("Retrieved {count} tools from MCP client", aiTools.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving tools from MCP client: {Message}", ex.Message);
                }
            }
            
            // Create chat options with tools
            var options = new ChatOptions
            {
                Temperature = 0.7f,
                Tools = tools,
                ToolMode = tools.Any() ? ChatToolMode.Auto : ChatToolMode.None
            };

            // Get initial response
            var response = await _chatClient.GetResponseAsync(messages, options, CancellationToken.None);
            _logger.LogInformation("Received response from AI: {Response}", response.Text);

            // Handle tool calls if present
            int maxToolIterations = 10;
            int toolIterations = 0;
            
            while ((string.IsNullOrEmpty(response.Text) || response.FinishReason == ChatFinishReason.ToolCalls) && toolIterations < maxToolIterations)
            {
                toolIterations++;
                _logger.LogInformation("Tool iteration {Iteration} of {MaxIterations}", toolIterations, maxToolIterations);
                
                var lastAssistantMessage = response.Messages.LastOrDefault(m => m.Role == ChatRole.Assistant);
                if (lastAssistantMessage == null)
                {
                    break;
                }
                
                messages.Add(lastAssistantMessage);
                
                var functionCalls = lastAssistantMessage.Contents
                    .OfType<FunctionCallContent>()
                    .Where(fc => fc != null && !string.IsNullOrEmpty(fc.CallId))
                    .ToList();
                
                if (functionCalls.Count == 0)
                {
                    break;
                }
                
                _logger.LogInformation("Found {Count} tool calls to process", functionCalls.Count);
                bool anyToolProcessed = false;
                
                // Process each function call
                foreach (var functionCall in functionCalls)
                {
                    try
                    {
                        string toolCallId = functionCall.CallId;
                        string toolName = functionCall.Name ?? "unknown_tool";

                        _logger.LogInformation("Executing tool call: ID={ToolCallId}, Name={ToolName}", toolCallId, toolName);
                        
                        // Validate arguments before calling the tool
                        if (functionCall.Arguments == null) { 
                            _logger.LogWarning("Tool call has null arguments: {ToolName}", toolName);
                            messages.Add(new ChatMessage(ChatRole.Tool, 
                                new List<AIContent>() { new FunctionResultContent(toolCallId, $"Error: Missing arguments for tool {toolName}") }));
                            anyToolProcessed = true;
                            continue;
                        }
                        
                        // Ensure the arguments can be properly cast to the required type
                        if (!(functionCall.Arguments is IReadOnlyDictionary<string, object>))
                        {
                            _logger.LogWarning("Tool call arguments are not of the expected type: {ToolName}, Type: {Type}", 
                                toolName, functionCall.Arguments.GetType().Name);
                            messages.Add(new ChatMessage(ChatRole.Tool, 
                                new List<AIContent>() { new FunctionResultContent(toolCallId, $"Error: Invalid argument format for tool {toolName}") }));
                            anyToolProcessed = true;
                            continue;
                        }
                        
                        // Execute the tool using MCP client
                        if (mcpClient != null)
                        {
                            try
                            {
                                var mcpResult = await mcpClient.CallToolAsync(toolName, 
                                    (IReadOnlyDictionary<string, object?>)functionCall.Arguments);
                                
                                _logger.LogInformation("Tool execution completed: {ToolName}, Success={Success}", 
                                    toolName, mcpResult.IsError != true);
                                
                                if (mcpResult.IsError != true && mcpResult.Content != null && mcpResult.Content.Any())
                                {
                                    // Handle successful tool execution with content
                                    foreach (var mcpMessage in mcpResult.Content)
                                    {
                                        var messageText = mcpMessage switch
                                        {
                                            { Type: "text" } textBlock => JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(textBlock)).GetProperty("text").GetString(),
                                            _ => mcpMessage.ToString()
                                        };
                                        
                                        if (!string.IsNullOrEmpty(messageText))
                                        {
                                            messages.Add(new ChatMessage(ChatRole.Tool, 
                                                new List<AIContent>() { new FunctionResultContent(toolCallId, messageText) }));
                                            
                                            _logger.LogDebug("Added tool message: {MessageText}", 
                                                messageText.Length > 50 ? messageText.Substring(0, 50) + "..." : messageText);
                                        }
                                    }
                                    
                                    _logger.LogInformation("Tool execution successful: {ToolName}, Messages={MessageCount}", 
                                        toolName, mcpResult.Content.Count());
                                }
                                else if (mcpResult.IsError != true)
                                {
                                    // Handle successful execution but with no content
                                    messages.Add(new ChatMessage(ChatRole.Tool, 
                                        new List<AIContent>() { new FunctionResultContent(toolCallId, $"Tool {toolName} executed successfully but returned no data") }));
                                    
                                    _logger.LogWarning("Tool {ToolName} executed successfully but returned no content", toolName);
                                }
                                else
                                {
                                    // Handle tool execution error
                                    string errorMessage = $"Error executing tool {toolName}: Tool execution failed";
                                    if (mcpResult.Content != null)
                                    {
                                        var errorDetails = string.Join(", ", mcpResult.Content.Select(c => c.ToString()));
                                        if (!string.IsNullOrEmpty(errorDetails))
                                        {
                                            errorMessage = $"Error executing tool {toolName}: {errorDetails}";
                                        }
                                    }
                                    
                                    messages.Add(new ChatMessage(ChatRole.Tool, 
                                        new List<AIContent>() { new FunctionResultContent(toolCallId, errorMessage) }));

                                    _logger.LogWarning("{ErrorMessage}", errorMessage);
                                }
                                
                                anyToolProcessed = true;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error executing tool {ToolName}: {Message}", toolName, ex.Message);
                                messages.Add(new ChatMessage(ChatRole.Tool, 
                                    new List<AIContent>() { new FunctionResultContent(toolCallId, $"Error executing tool {toolName}: {ex.Message}") }));
                                anyToolProcessed = true;
                            }
                        }
                        else
                        {
                            // MCP client not available
                            messages.Add(new ChatMessage(ChatRole.Tool, 
                                new List<AIContent>() { new FunctionResultContent(toolCallId, $"Tool {toolName} is not available - MCP client connection failed") }));
                            anyToolProcessed = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle unexpected exceptions during tool execution
                        string errorMessage = $"Exception while executing tool {functionCall.Name ?? "unknown"}: {ex.Message}";
                        messages.Add(new ChatMessage(ChatRole.Tool, 
                            new List<AIContent>() { new FunctionResultContent(functionCall.CallId, errorMessage) }));
                        _logger.LogError(ex, "Exception in tool execution: {ErrorMessage}", errorMessage);
                        anyToolProcessed = true;
                    }
                }
                
                if (!anyToolProcessed)
                {
                    _logger.LogWarning("None of the tool calls could be processed");
                    break;
                }
                
                _logger.LogInformation("All tool responses added, sending back to LLM for further processing");
                
                // Get next response
                response = await _chatClient.GetResponseAsync(messages, options, CancellationToken.None);
            }
            
            if (toolIterations >= maxToolIterations && string.IsNullOrEmpty(response.Text))
            {
                _logger.LogWarning("Reached maximum tool iterations ({MaxIterations}). Forcing final response.", maxToolIterations);
                // Force a final response if we've hit the limit
                messages.Add(new ChatMessage(ChatRole.User, "Maximum number of tool calls reached. Please provide a final response."));
                options.ToolMode = ChatToolMode.None; // Disable further tool usage
                response = await _chatClient.GetResponseAsync(messages, options, CancellationToken.None);
            }

            return response.Text ?? "I apologize, but I'm unable to provide a response at this time.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating response with tools: {Message}", ex.Message);
            return "I'm sorry, I'm having trouble connecting to my AI service right now. Please try again later.";
        }
    }

    /// <summary>
    /// Save conversation history to storage
    /// </summary>
    private async Task SaveConversationHistoryAsync(string conversationId, List<ChatMessageModel> history)
    {
        try
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                conversationId = Guid.NewGuid().ToString();
            }

            // For this demo, save to a temporary file
            // In a real application, you would use a database or Redis
            var filePath = Path.Combine(Path.GetTempPath(), $"chat_{conversationId}.json");
            var jsonContent = JsonSerializer.Serialize(history, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            await File.WriteAllTextAsync(filePath, jsonContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving conversation history: {Message}", ex.Message);
        }
    }
}
