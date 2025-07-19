# TestForge - AI-Powered Test Case Generation Platform with Model Context Protocol

A comprehensive AI-powered test case generation platform built with .NET and Angular, featuring advanced Model Context Protocol (MCP) integration for intelligent Jira analysis and TestRail test case generation.

## Project Structure

- **TestForge.McpServer** - Model Context Protocol server with 10 specialized tools for intelligent test case generation
- **guidance/** - Documentation and instruction files
- **Documentation files** - Various .md files for setup, debugging, and enhancement guides

## Enhanced Features

- 🧠 **Semantic Test Generation** - Intelligent categorization across 14 test categories with keyword mapping
- 🔄 **Multi-Context Scenario Expansion** - Automatic test variants for IVR flows, user roles, environments
- �️ **Technical Artifact Processing** - Extract SQL, JSON, HTML, and configuration data for targeted testing
- 🌳 **Conditional Logic Analysis** - Decision tree extraction and comprehensive branch coverage
- ⚡ **Performance Test Generation** - Domain-specific performance testing based on content analysis
- 🎯 **Comprehensive Coverage Analysis** - 80%+ coverage across all testing dimensions
- � **Advanced XML Processing** - Robust parsing with HtmlAgilityPack and automatic error recovery
- 📊 **Enhanced Complexity Scoring** - AI-powered risk assessment and confidence metrics
- 🎯 **Claude Desktop Integration** - Full MCP integration with debugging tools
- 📋 **Enhanced Flow Support** - User → LLM → MCP Tools → Intelligent Test Suites
- 🔧 Model Context Protocol (MCP) server with 14 specialized enhanced tools

### 🚀 **New Enhanced Capabilities**

- **Semantic Keyword Mapping**: Audio/TTS → Performance tests, Data formats → Validation tests
- **IVR Context Expansion**: Automatic variants for no-pin, athena, and availity IVR systems  
- **Technical Content Parsing**: HTML within XML descriptions with HtmlAgilityPack
- **Business Logic Extraction**: Pattern-based conditional logic and fallback path analysis
- **Performance Triggers**: Domain-aware performance test generation (latency, load, stress)
- **Comprehensive Test Matrix**: 14-category coverage analysis with gap identification

### **All 14 enhanced tools are available**:

### 🛠️ Enhanced XML Cleaning Capabilities

The XML processing tools include comprehensive cleaning to handle common issues in raw Jira XML exports:

- **Missing Closing Tags** - Automatically adds missing `</comments>` and other structural closing tags
- **Malformed Comment Content** - Wraps HTML content in CDATA sections to prevent parsing errors
- **Duplicate Attributes** - Resolves duplicate `rel`, `class`, `data-account-id` attributes (keeps last value)
- **HTML Entity Escaping** - Properly escapes unescaped ampersands and special characters
- **Invalid XML Characters** - Removes control characters that break XML parsing
- **Nested Structure Repair** - Fixes common structural problems in Jira exports
- **Automatic Error Recovery** - If initial parsing fails, automatically applies cleaning and retries

**Result**: You can copy-paste raw Jira XML exports directly without manual cleanup!

## 🚀 Enhanced Flow: User → LLM → MCP Tools → Intelligent Test Suites

This platform enables sophisticated test case generation through semantic analysis and AI collaboration:

### Enhanced Flow Architecture

**Primary Enhanced XML Flow**:
1. **User** provides raw Jira XML with complex HTML descriptions
2. **LLM** calls enhanced MCP tools in intelligent sequence:
   - `generate_comprehensive_test_suite` - Complete semantic analysis and test generation ⭐ **RECOMMENDED**
   - `validate_jira_xml` - XML structure validation (diagnostic)
   - `clean_jira_xml` - Advanced XML repair with HtmlAgilityPack
   - `analyze_jira_xml_for_llm` - Enhanced structured data extraction
   - `extract_business_logic_analysis` - Conditional logic and decision tree analysis
   - `extract_ui_components_analysis` - UI element identification with complexity scoring
   - `generate_comprehensive_test_matrix` - Coverage analysis across 14 categories
3. **Enhanced MCP Tools** provide intelligent JSON data with semantic categorization
4. **LLM** synthesizes insights across 14 test categories with confidence scoring
5. **Intelligent Test Suites** - 20-50 comprehensive, contextually-aware test cases ready for TestRail

**Future Enhanced Jira API Flow**:
1. **User** provides Jira ticket ID
2. **LLM** calls enhanced MCP tools for comprehensive analysis:
   - `analyze_jira_ticket_for_llm` - Enhanced API-based extraction (requires auth)
   - Plus all specialized analysis tools for complete coverage
3. **Enhanced Processing** with semantic mapping and multi-context expansion
4. **Intelligent Test Generation** across all 14 categories
5. **Comprehensive Test Suites** - Maximum coverage with intelligent prioritization

### Enhanced Benefits
- **Semantic Intelligence** - Automatic test categorization based on content analysis
- **Multi-Context Awareness** - IVR variants, user roles, environment-specific testing
- **Technical Artifact Processing** - SQL, JSON, HTML extraction for targeted testing
- **Conditional Logic Analysis** - Decision tree and fallback path coverage
- **Performance Optimization** - Domain-specific performance test generation
- **80%+ Coverage** - Comprehensive testing across all dimensions
- **Enhanced Recovery** - Advanced XML processing with HtmlAgilityPack
- **Confidence Transparency** - Clear quality metrics for test prioritization

### Enhanced LLM Guidance for Tool Selection

**When user provides raw Jira XML:**
1. **Primary recommendation:** `generate_comprehensive_test_suite` - Complete semantic analysis and test generation ⭐
2. **Alternative workflow:** Start with `validate_jira_xml` - Check XML structure and diagnostics
3. **If validation fails:** `clean_jira_xml` - Advanced XML repair with HtmlAgilityPack
4. **Detailed analysis:** `analyze_jira_xml_for_llm` - Comprehensive structured data extraction
5. **Specialized analysis:** Use description from Step 4 to call:
   - `extract_ui_components_analysis` - UI element identification with complexity scoring
   - `extract_business_logic_analysis` - Conditional logic and decision tree analysis
   - `generate_comprehensive_test_matrix` - Complete coverage analysis across 14 categories
6. **Enhancement:** `enhance_test_cases_with_llm` - Add additional coverage categories
7. **Complete automation:** `process_jira_workflow` - Single call for full workflow automation

**When user provides ticket ID (future):**
1. **Primary analysis:** `analyze_jira_ticket_for_llm` - Enhanced API-based extraction (requires authentication)
2. **Additional analysis:** Same enhanced workflow as above using extracted data

**For complete automation:**
- **Use:** `generate_comprehensive_test_suite` - Handles semantic analysis, multi-context expansion, and comprehensive test generation ⭐ **RECOMMENDED**
- **Alternative:** `process_jira_workflow` - Handles entire workflow automatically with error recovery
- **Benefits:** Validates, cleans, analyzes, and generates 20-50 comprehensive test cases in one call
- **Enhanced features:** Semantic categorization, IVR context expansion, technical artifact processing

**Enhanced Best Practices:**
- Always use `generate_comprehensive_test_suite` for maximum coverage and intelligence
- Validate XML first for better user experience and diagnostics
- Use semantic analysis results to guide additional specialized tool calls
- Check confidence scores (>0.8 recommended) to prioritize high-quality tests
- Leverage multi-context expansion for IVR variants and user role testing
- Extract technical artifacts (SQL, JSON, HTML) for targeted test scenarios
- Combine results from multiple tools for 80%+ comprehensive coverage

## Getting Started

### Prerequisites

- .NET 8.0 SDK or higher

### Running the MCP Server

1. **Start the MCP Server:**
   ```bash
   cd TestForge.McpServer
   ASPNETCORE_URLS="http://localhost:5001" dotnet run
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

# Test the new XML analysis tool for LLM workflows
http POST localhost:5001/mcp Content-Type:application/json jsonrpc=2.0 id:=5 method=tools/call params:='{"name": "analyze_jira_xml_for_llm", "arguments": {"jiraXml": "<item><key>TEST-456</key><summary>User Registration Feature</summary><description>As a new user, I want to register an account so that I can access the application features.</description><type>Story</type><priority>Medium</priority><acceptance-criteria>Given I am on the registration page\nWhen I fill out the registration form with valid information\nThen I should receive a confirmation email\nAnd I should be able to log in with my new credentials</acceptance-criteria></item>"}}'

# Test with raw Jira XML export (with duplicate attributes and HTML content)
http POST localhost:5001/mcp Content-Type:application/json jsonrpc=2.0 id:=6 method=tools/call params:='{"name": "generate_test_cases_from_jira_xml", "arguments": {"jiraXml": "<item><key>DEV-15860</key><summary>[Rates] - Minimum input should be empty when the override rate does not have a minimum set</summary><description><p><b>Steps to reproduce</b>:</p> <ul> <li>Navigate to the Account Viewer → Rates Tab → Account SubTab</li> <li>Select a rate that has a <b>minimum</b> value and that also has <b>override</b> the rate. Click on that row.</li> </ul> <p><b>Actual result:</b></p> <p>When the rate has a minimum and the override rate does not have a minimum set, the override rate minimum input is being filled with the Rate minimum instead of being empty</p> <p><b>Expected Result:</b></p> <p>When the rate has a minimum and the override rate does not have a minimum, the minimum input is empty and has a placeholder showing \"Use here goes rate minimum value\".</p></description><type>Defect</type><priority>Medium</priority><status>Ready For Testing</status></item>"}}'
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

# Test the new XML analysis tool for LLM workflows
curl -X POST http://localhost:5001/mcp \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc": "2.0", "id": 5, "method": "tools/call", "params": {"name": "analyze_jira_xml_for_llm", "arguments": {"jiraXml": "<item><key>PROJ-123</key><summary>User Login Feature</summary><description>As a user, I want to log in to the application so that I can access my account.</description><type>Story</type><priority>High</priority><acceptance-criteria>Given I am on the login page, When I enter valid credentials, Then I should be logged in successfully</acceptance-criteria></item>"}}}'
```

**Note**: The MCP Inspector (`npx @modelcontextprotocol/inspector`) is designed for STDIO-based MCP servers, not HTTP-based servers like this .NET implementation.

### 🎯 Claude Desktop Integration

**Configuration File**: `~/Library/Application Support/Claude/claude_desktop_config.json`

**Recommended Configuration**:
```json
{
  "mcpServers": {
    "testforge-mcp-server": {
      "command": "/usr/local/bin/npx",
      "args": [
        "mcp-remote",
        "http://localhost:5001/mcp"
      ],
      "env": {
        "NODE_ENV": "production"
      }
    }
  }
}
```

**Prerequisites**:
```bash
# Install mcp-remote package
npm install -g mcp-remote

# Verify installation
which npx
npx mcp-remote --help
```

**Testing Steps**:
1. Start the MCP server: `ASPNETCORE_URLS="http://localhost:5001" dotnet run`
2. Test manually: `npx mcp-remote http://localhost:5001/mcp`
3. Restart Claude Desktop
4. Verify all 10 tools are available in Claude Desktop

**Debugging**: 
- See `CLAUDE-DESKTOP-DEBUG-PROMPT.md` for comprehensive debugging steps
- Use `validate_jira_xml` tool for XML diagnostics
- Check logs: `~/Library/Logs/Claude/mcp-server-testforge-mcp-server.log`

### 🤖 ChatGPT Custom GPT Integration

**Quick Setup**:
```bash
# Run the setup script
./setup-chatgpt-integration.sh

# Start the server
cd TestForge.McpServer
ASPNETCORE_URLS="http://localhost:5001" dotnet run
```

**Configuration Files**:
- `CHATGPT-CUSTOM-GPT-INSTRUCTIONS.md` - Complete instruction set for Custom GPT
- `CHATGPT-OPENAPI-SCHEMA.md` - OpenAPI schema for Actions configuration

**Custom GPT Setup**:
1. Create a new Custom GPT in ChatGPT
2. Copy instructions from `CHATGPT-CUSTOM-GPT-INSTRUCTIONS.md`
3. Import OpenAPI schema from `CHATGPT-OPENAPI-SCHEMA.md` 
4. Set server URL to `http://localhost:5001` (or ngrok URL for company access)
5. Test with sample Jira XML

**For Company-Wide Access**:
```bash
# Expose server publicly with ngrok
ngrok http 5001

# Update OpenAPI schema with ngrok URL
# Share Custom GPT within your organization
```

**Features**:
- ✅ All 10 TestForge tools available via ChatGPT Actions
- ✅ Intelligent workflow orchestration 
- ✅ Automatic XML validation and cleaning
- ✅ Comprehensive test case generation
- ✅ Company-wide deployment ready


## Technology Stack

- **Backend**: .NET 9, ASP.NET Core, Model Context Protocol SDK
- **Frontend**: Angular 19, TypeScript, RxJS
- **AI Integration**: LLM-optimized JSON output, confidence scoring, structured analysis
- **XML Processing**: Advanced parsing with automatic error recovery and validation
- **Test Case Generation**: TestRail-compatible output with comprehensive step generation
- **Tools**: VS Code, GitHub Copilot, MCP integration

## Development

### Building the Solution

```bash
# Build the MCP server
dotnet build

# Run the MCP server
cd TestForge.McpServer
ASPNETCORE_URLS="http://localhost:5001" dotnet run
```

### 🔧 Troubleshooting XML Issues

If you encounter XML parsing errors:

1. **Use the diagnostic tools**:
   ```bash
   # Test XML validation
   curl -X POST http://localhost:5001/mcp -H "Content-Type: application/json" -d '{"jsonrpc": "2.0", "id": 1, "method": "tools/call", "params": {"name": "validate_jira_xml", "arguments": {"jiraXml": "YOUR_XML_HERE"}}}'
   ```

2. **Clean problematic XML**:
   ```bash
   # Use the cleaning tool
   curl -X POST http://localhost:5001/mcp -H "Content-Type: application/json" -d '{"jsonrpc": "2.0", "id": 2, "method": "tools/call", "params": {"name": "clean_jira_xml", "arguments": {"rawXml": "YOUR_XML_HERE"}}}'
   ```

3. **Common Issues Fixed Automatically**:
   - Missing `</comments>` closing tags
   - Malformed HTML in comment content
   - Duplicate XML attributes
   - Invalid characters and entities

### MCP Server Development

The MCP server demonstrates basic tool creation with the new C# SDK. Tools are defined as static methods with attributes:

```csharp
[McpServerToolType]
public static class TestForgeTools
{
    [McpServerTool, Description("Says hello to the user.")]
    public static string SayHello(string name = "World") => 
        $"Hello from TestForge MCP Server, {name}! 🎉";
}
```

## 📚 Documentation

- **Enhanced Flow Guide** - `ENHANCED-FLOW-PROMPT.md`
- **XML Parser Debugging** - `XML-PARSER-FIX-PROMPT.md`
- **Claude Desktop Setup** - `CLAUDE-DESKTOP-DEBUG-PROMPT.md`
- **Fix Summary** - `XML-PARSER-FIX-SUMMARY.md`

## 🎯 Use Cases

### For QA Teams
- **Automated Test Case Generation** - Convert Jira tickets to TestRail test cases
- **Complexity Analysis** - Prioritize testing efforts based on AI-driven complexity scoring
- **Comprehensive Coverage** - Generate positive, negative, and edge case scenarios

### For Development Teams
- **Requirements Analysis** - Extract UI components and business logic from tickets
- **Test Planning** - Get structured analysis for better test coverage planning
- **Integration Testing** - Identify integration points and complexity areas

### For LLM Applications
- **Structured Data** - Clean, consistent JSON output optimized for AI consumption
- **Confidence Scoring** - Transparent quality metrics for AI decision-making
- **Enhanced Workflows** - User → LLM → MCP → Enhanced Output patterns

---

*Last updated: July 18, 2025*
