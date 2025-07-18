# ChatDemo - AI Chat Appl6. **GetCurrentTime** - Returns current date and time
7. **GenerateRandomNumber** - Generates random number between specified ranges
8. **GenerateTestCasesFromJiraXml** - Parses Jira story XML and generates Given/When/Then test case steps for web UI automation. **Now includes automatic XML cleaning** to handle raw Jira XML exports with duplicate attributes, malformed HTML, and other common formatting issues without requiring manual cleanup.tion with Model Context Protocol

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
7. **GenerateTestCasesFromJiraXml** - Parses Jira story XML and generates Given/When/Then test case steps for web UI automation

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
   cd /MCP-POC/MCP-demo/ChatDemo.McpServer
   # Run on different port to avoid conflict with Angular
   ASPNETCORE_URLS="http://0.0.0.0:5001" dotnet run
   ```

### Testing the MCP Server

Since this is an HTTP-based MCP server (not STDIO), you can test it directly with HTTP requests:

### XML Cleaning Feature

The `GenerateTestCasesFromJiraXml` tool now includes automatic XML cleaning to handle common issues found in raw Jira XML exports:

- **Duplicate attributes** (e.g., `rel="value1" rel="value2"`) - keeps the last value
- **Malformed HTML entities** - properly escapes unescaped ampersands
- **Empty CDATA sections** - removes empty `<![CDATA[]]>` blocks
- **Invalid XML characters** - removes control characters that break XML parsing
- **Duplicate class, data-account-id, and accountid attributes** - resolves conflicts

This means you can copy-paste raw Jira XML exports directly without manual cleanup!

**Option 1: Using HTTPie (Recommended)**
```bash
# Install HTTPie for better HTTP testing
brew install httpie

# Test the MCP capabilities endpoint
http POST localhost:5001/mcp Content-Type:application/json jsonrpc=2.0 id:=1 method=initialize params:='{"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "test-client", "version": "1.0.0"}}'

# List available tools
http POST localhost:5001/mcp Content-Type:application/json jsonrpc=2.0 id:=2 method=tools/list params:='{}'

# Test the SayHello tool
http POST localhost:5001/mcp Content-Type:application/json jsonrpc=2.0 id:=3 method=tools/call params:='{"name": "say_hello", "arguments": {"name": "World"}}'

# Test the Jira XML parser
http POST localhost:5001/mcp Content-Type:application/json jsonrpc=2.0 id:=4 method=tools/call params:='{"name": "generate_test_cases_from_jira_xml", "arguments": {"jiraXml": "<item><key>TEST-456</key><summary>User Registration Feature</summary><description>As a new user, I want to register an account so that I can access the application features.</description><type>Story</type><priority>Medium</priority><acceptance-criteria>Given I am on the registration page\nWhen I fill out the registration form with valid information\nThen I should receive a confirmation email\nAnd I should be able to log in with my new credentials</acceptance-criteria></item>"}}'

# Test with raw Jira XML export (with duplicate attributes and HTML content)
http POST localhost:5001/mcp Content-Type:application/json jsonrpc=2.0 id:=5 method=tools/call params:='{"name": "generate_test_cases_from_jira_xml", "arguments": {"jiraXml": "<item><key>DEV-15860</key><summary>[Rates] - Minimum input should be empty when the override rate does not have a minimum set</summary><description><p><b>Steps to reproduce</b>:</p> <ul> <li>Navigate to the Account Viewer → Rates Tab → Account SubTab</li> <li>Select a rate that has a <b>minimum</b> value and that also has <b>override</b> the rate. Click on that row.</li> </ul> <p><b>Actual result:</b></p> <p>When the rate has a minimum and the override rate does not have a minimum set, the override rate minimum input is being filled with the Rate minimum instead of being empty</p> <p><b>Expected Result:</b></p> <p>When the rate has a minimum and the override rate does not have a minimum, the minimum input is empty and has a placeholder showing \"Use here goes rate minimum value\".</p></description><type>Defect</type><priority>Medium</priority><status>Ready For Testing</status></item>"}}'
```

**Option 2: Using curl**
```bash
# Test the MCP capabilities endpoint
curl -X POST http://localhost:5001/mcp \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc": "2.0", "id": 1, "method": "initialize", "params": {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "test-client", "version": "1.0.0"}}}'

# List available tools
curl -X POST http://localhost:5001/mcp \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc": "2.0", "id": 2, "method": "tools/list", "params": {}}'

# Test the SayHello tool
curl -X POST http://localhost:5001/mcp \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc": "2.0", "id": 3, "method": "tools/call", "params": {"name": "say_hello", "arguments": {"name": "World"}}}'

# Test the Jira XML parser with sample XML
curl -X POST http://localhost:5001/mcp \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc": "2.0", "id": 4, "method": "tools/call", "params": {"name": "generate_test_cases_from_jira_xml", "arguments": {"jiraXml": "<item><key>PROJ-123</key><summary>User Login Feature</summary><description>As a user, I want to log in to the application so that I can access my account.</description><type>Story</type><priority>High</priority><acceptance-criteria>Given I am on the login page, When I enter valid credentials, Then I should be logged in successfully</acceptance-criteria></item>"}}}'
```

**Note**: The MCP Inspector (`npx @modelcontextprotocol/inspector`) is designed for STDIO-based MCP servers, not HTTP-based servers like this .NET implementation.

### Claude Integration
```json
{
  "mcpServers": {
    "my-mcp-server": {
      "command": "node",
      "args": [
        "C:\\Program Files\\nodejs\\node_modules\\npm\\bin\\npx-cli.js",
        "mcp-remote",
        "https://[YOUR_IP]:5003/mcp"
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
