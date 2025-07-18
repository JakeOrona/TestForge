# MCP Server Setup and Testing Guide

## Current Status ✅
Your MCP server is successfully running on `http://localhost:4200` with the Jira XML parser tool implemented.

## Server Status
- **Local URL**: `http://localhost:4200/mcp`
- **External URL**: `http://192.168.0.54:4200/mcp`
- **Status**: Running and responding correctly
- **Tools Available**: Jira XML Parser with test case generation

## Direct Local Network Access (No ngrok needed!)

### 1. Your Server Configuration
Your server is already configured to accept external connections:
- **Local IP**: `192.168.0.54`
- **Port**: `4200`
- **External Access URL**: `http://192.168.0.54:4200/mcp`

### 2. Test External Access
```bash
# Test from another device on your network
curl http://192.168.0.54:4200/mcp
```

### 3. Configure ChatGPT/Claude for Direct Access

#### For Claude Desktop:
Update `~/Library/Application Support/Claude/claude_desktop_config.json`:
```json
{
  "mcpServers": {
    "local-jira-parser": {
      "command": "node",
      "args": [
        "/opt/homebrew/bin/npx",
        "mcp-remote",
        "http://192.168.0.54:4200"
      ]
    }
  }
}
```

#### For ChatGPT Enterprise/API:
Use the direct IP endpoint:
```
http://192.168.0.54:4200/mcp
```

#### For any external LLM service:
Your MCP server endpoint is:
```
http://192.168.0.54:4200
```

## Testing the Jira XML Parser

### Sample Jira XML
A sample Jira story XML file has been created at:
`/Users/jakeorona/MCP-POC/MCP-demo/sample-jira-story.xml`

### Available Tool
- **Tool Name**: `generate_test_cases_from_jira_xml`
- **Purpose**: Parses Jira XML and generates comprehensive test cases
- **Output**: Given/When/Then format test scenarios

### Test Case Generation Features
- ✅ Extracts story details (key, summary, description, acceptance criteria)
- ✅ Parses components and technical requirements
- ✅ Generates positive test scenarios
- ✅ Generates negative test scenarios
- ✅ Generates edge case scenarios
- ✅ Structured Given/When/Then format
- ✅ Comprehensive test coverage

### Example Usage in External LLM
```
Use the MCP tool to generate test cases from this Jira XML:
[paste contents of sample-jira-story.xml]
```

## Firewall Considerations

If external access doesn't work, you may need to:

### macOS Firewall
```bash
# Check if firewall is blocking port 4200
sudo pfctl -s nat
```

### Alternative - Test from same machine first:
```bash
# This should work (testing external IP from same machine)
curl http://192.168.0.54:4200/mcp
```

## Server Management

### Start the server:
```bash
cd /Users/jakeorona/MCP-POC/MCP-demo/ChatDemo.McpServer
dotnet run
```

### Test server locally:
```bash
/Users/jakeorona/MCP-POC/MCP-demo/test-mcp-server.sh
```

### Test external access:
```bash
curl http://192.168.0.54:4200/mcp
```

### Stop the server:
Press `Ctrl+C` in the terminal running the server

## Benefits of Direct Local Access
- ✅ No external service dependencies (no ngrok)
- ✅ Faster response times
- ✅ No bandwidth limitations
- ✅ Complete privacy (stays on your network)
- ✅ No authentication setup required
- ✅ Works with any LLM service that supports HTTP endpoints

## Files Created/Modified
- ✅ `ChatDemoTools.cs` - Complete Jira XML parser implementation (922 lines)
- ✅ `appsettings.json` - Port configuration (4200, listening on all interfaces)
- ✅ `Program.cs` - Removed hardcoded ports
- ✅ `test-mcp-server.sh` - Server testing script
- ✅ `sample-jira-story.xml` - Sample XML for testing
- ✅ `README.md` - Updated documentation

## Ready for Direct Testing
Your MCP server is accessible at:
**`http://192.168.0.54:4200`**

Use this URL directly in any external LLM service that supports MCP or HTTP endpoints!
````

## Now let's test your external access:

```bash
# Test external access from your machine
curl http://192.168.0.54:4200/mcp
```