using ModelContextProtocol.Server;
using System.ComponentModel;

namespace ChatDemo.McpServer;

[McpServerToolType]
public static class ChatDemoTools
{
    [McpServerTool, Description("Says hello to the user with a personalized greeting.")]
    public static string SayHello(string name = "World") => $"Hello from ChatDemo MCP Server, {name}! 🎉";

    [McpServerTool, Description("Provides information about this ChatDemo application.")]
    public static string GetAppInfo() => 
        "This is ChatDemo - an AI-powered chat application built with .NET and Angular, featuring Model Context Protocol (MCP) integration for enhanced AI capabilities.";

    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo(string message) => $"Echo from ChatDemo: {message}";

    [McpServerTool, Description("Reverses the given text.")]
    public static string ReverseText(string text) => new string(text.Reverse().ToArray());

    [McpServerTool, Description("Gets current date and time.")]
    public static string GetCurrentTime() => $"Current time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

    [McpServerTool, Description("Generates a random number between min and max values.")]
    public static int GenerateRandomNumber(int min = 1, int max = 100) => new Random().Next(min, max + 1);
}
