# ChatDemo - AI Chat Application with Model Context Protocol

A modern AI-powered chat application built with .NET and Angular, featuring Model Context Protocol (MCP) integration for enhanced AI capabilities.

## Project Structure

- **ChatDemo.Api** - ASP.NET Core Web API (configured for future API endpoints)
- **ChatDemo.Client** - Angular frontend with chat interface
- **ChatDemo.McpServer** - Model Context Protocol server with demo tools

## Features

- 🤖 Interactive chat interface built with Angular
- 🔧 Model Context Protocol (MCP) server with hello world tools
- 🎯 CORS-enabled API ready for integration
- ⚡ Real-time chat with ChatGPT client

## MCP Tools Available

The MCP server includes the following demo tools:

1. **SayHello** - Personalized greeting with optional name parameter
2. **GetAppInfo** - Information about the ChatDemo application
3. **Echo** - Echoes messages back to the client
4. **ReverseText** - Reverses any provided text
5. **GetCurrentTime** - Returns current date and time
6. **GenerateRandomNumber** - Generates random number between specified ranges
7. **GenerateTestCasesFromJiraXml** - Parses Jira story XML and generates test case steps for web UI automation (scaffolding)

## Getting Started

### Prerequisites

- .NET 8.0 SDK or higher
- Node.js 18+ and npm
- Angular CLI (`npm install -g @angular/cli`)

### Running the Application

1. **Start the Angular Client:**
   ```bash
   cd ChatDemo.Client
   ng serve
   ```
   Navigate to `http://localhost:4200`

2. **Run the API (Optional):**
   ```bash
   cd ChatDemo.Api
   dotnet run
   ```

3. **Test the MCP Server:**
   ```bash
   cd ChatDemo.McpServer
   dotnet run
   ```

### Claude Integration
```json
{
  "mcpServers": {
    "my-mcp-server": {
      "command": "node",
      "args": [
        "C:\\Program Files\\nodejs\\node_modules\\npm\\bin\\npx-cli.js",
        "mcp-remote",
        "https://[YOUR_IP]:5001/mcp"
      ],
      "env": {
        "NODE_TLS_REJECT_UNAUTHORIZED": "0"
      }
    }
  }
}
```


## Technology Stack

- **Backend**: .NET 9, ASP.NET Core, Model Context Protocol SDK
- **Frontend**: Angular 19, TypeScript, RxJS
- **Tools**: VS Code, GitHub Copilot, MCP integration

## Development

### Building the Solution

```bash
# Build entire solution
dotnet build

# Build and test Angular app
cd ChatDemo.Client
ng build
ng test
```

### MCP Server Development

The MCP server demonstrates basic tool creation with the new C# SDK. Tools are defined as static methods with attributes:

```csharp
[McpServerToolType]
public static class ChatDemoTools
{
    [McpServerTool, Description("Says hello to the user.")]
    public static string SayHello(string name = "World") => 
        $"Hello from ChatDemo MCP Server, {name}! 🎉";
}
```
