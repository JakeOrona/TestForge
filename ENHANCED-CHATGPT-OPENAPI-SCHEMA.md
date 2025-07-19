# Enhanced ChatGPT OpenAPI Schema for TestForge MCP Server

## OpenAPI 3.0 Schema Configuration (Updated)

Use this enhanced schema in your ChatGPT Custom GPT Actions configuration:

```yaml
openapi: 3.0.0
info:
  title: TestForge Enhanced MCP Server API
  description: Comprehensive AI-powered test case generation with semantic analysis, multi-context expansion, and 14 test categories
  version: 3.0.0
servers:
  - url: http://localhost:5001/mcp
    description: Local TestForge Enhanced MCP Server

paths:
  /mcp:
    post:
      summary: Execute Enhanced MCP Tool Calls
      description: Execute any of the 14 available TestForge enhanced MCP tools for comprehensive test case generation with semantic analysis
      operationId: executeEnhancedMcpTool
      requestBody:
        required: true
        content:
          application/json:
            schema:
              type: object
              properties:
                jsonrpc:
                  type: string
                  enum: ["2.0"]
                  description: JSON-RPC protocol version
                id:
                  type: integer
                  description: Request identifier
                method:
                  type: string
                  enum: ["tools/call", "tools/list"]
                  description: MCP method to call
                params:
                  type: object
                  properties:
                    name:
                      type: string
                      enum: [
                        "get_current_time",
                        "analyze_jira_ticket_for_llm", 
                        "analyze_jira_xml_for_llm",
                        "generate_test_case_templates",
                        "extract_ui_components_analysis",
                        "extract_business_logic_analysis", 
                        "generate_test_cases_from_jira_xml",
                        "validate_jira_xml",
                        "clean_jira_xml",
                        "process_jira_workflow",
                        "generate_comprehensive_test_suite",
                        "enhance_test_cases_with_llm",
                        "generate_comprehensive_test_matrix"
                      ]
                      description: Name of the enhanced MCP tool to execute
                    arguments:
                      type: object
                      description: Tool-specific arguments with enhanced parameters
                      anyOf:
                        - $ref: '#/components/schemas/GetCurrentTimeArgs'
                        - $ref: '#/components/schemas/AnalyzeJiraTicketArgs'
                        - $ref: '#/components/schemas/AnalyzeJiraXmlArgs'
                        - $ref: '#/components/schemas/GenerateTestCaseTemplatesArgs'
                        - $ref: '#/components/schemas/ExtractUiComponentsArgs'
                        - $ref: '#/components/schemas/ExtractBusinessLogicArgs'
                        - $ref: '#/components/schemas/GenerateTestCasesFromJiraXmlArgs'
                        - $ref: '#/components/schemas/ValidateJiraXmlArgs'
                        - $ref: '#/components/schemas/CleanJiraXmlArgs'
                        - $ref: '#/components/schemas/ProcessJiraWorkflowArgs'
                        - $ref: '#/components/schemas/GenerateComprehensiveTestSuiteArgs'
                        - $ref: '#/components/schemas/EnhanceTestCasesWithLlmArgs'
                        - $ref: '#/components/schemas/GenerateTestMatrixArgs'
              required: [jsonrpc, id, method, params]
      responses:
        '200':
          description: Successful enhanced MCP tool execution
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/EnhancedMcpResponse'
        '400':
          description: Invalid request format
        '500':
          description: Internal server error

components:
  schemas:
    EnhancedMcpResponse:
      type: object
      properties:
        jsonrpc:
          type: string
          enum: ["2.0"]
        id:
          type: integer
        result:
          type: object
          description: Enhanced tool execution result with comprehensive analysis
          properties:
            content:
              type: array
              items:
                type: object
                properties:
                  type:
                    type: string
                    enum: ["text"]
                  text:
                    type: string
                    description: JSON-formatted comprehensive test suite or analysis result
        error:
          type: object
          description: Error information if execution failed

    GetCurrentTimeArgs:
      type: object
      description: No arguments required for current time

    AnalyzeJiraTicketArgs:
      type: object
      properties:
        ticketId:
          type: string
          description: Jira ticket ID (e.g., PROJ-123)
        jiraBaseUrl:
          type: string
          description: Jira instance base URL
          default: ""
        username:
          type: string
          description: Jira username
          default: ""
        apiToken:
          type: string
          description: Jira API token
          default: ""
      required: [ticketId]

    AnalyzeJiraXmlArgs:
      type: object
      properties:
        jiraXml:
          type: string
          description: Raw Jira XML export content with HTML descriptions
      required: [jiraXml]

    GenerateTestCaseTemplatesArgs:
      type: object
      properties:
        ticketType:
          type: string
          description: Type of Jira ticket (Story, Bug, Task, Epic, etc.)
        priority:
          type: string
          description: Priority level (Critical, High, Medium, Low)
          default: "Medium"
        component:
          type: string
          description: Component or system area (UI, API, Database, etc.)
          default: "UI"
      required: [ticketType]

    ExtractUiComponentsArgs:
      type: object
      properties:
        description:
          type: string
          description: Jira ticket description or requirements text for UI analysis
      required: [description]

    ExtractBusinessLogicArgs:
      type: object
      properties:
        description:
          type: string
          description: Jira ticket description or requirements text for business logic extraction
      required: [description]

    GenerateTestCasesFromJiraXmlArgs:
      type: object
      properties:
        jiraXml:
          type: string
          description: Raw Jira XML export content
      required: [jiraXml]

    ValidateJiraXmlArgs:
      type: object
      properties:
        jiraXml:
          type: string
          description: Raw Jira XML content to validate for structure and formatting
      required: [jiraXml]

    CleanJiraXmlArgs:
      type: object
      properties:
        rawXml:
          type: string
          description: Raw malformed XML content to clean and repair
      required: [rawXml]

    ProcessJiraWorkflowArgs:
      type: object
      properties:
        jiraXml:
          type: string
          description: Raw Jira XML export content for complete automated workflow
      required: [jiraXml]

    GenerateComprehensiveTestSuiteArgs:
      type: object
      properties:
        jiraXml:
          type: string
          description: Raw Jira XML export content for comprehensive test generation
        enhancementConfig:
          type: string
          description: JSON string with enhancement configuration options
          default: ""
      required: [jiraXml]

    EnhanceTestCasesWithLlmArgs:
      type: object
      properties:
        parsedXmlData:
          type: string
          description: Parsed Jira XML data as JSON string from analyze_jira_xml_for_llm
        initialTests:
          type: string
          description: Initial generated test cases as JSON string
        enhancementConfig:
          type: string
          description: Enhancement configuration specifying test categories to generate
          default: ""
      required: [parsedXmlData, initialTests]

    GenerateTestMatrixArgs:
      type: object
      properties:
        parsedXmlData:
          type: string
          description: Parsed Jira XML data as JSON string for matrix analysis
      required: [parsedXmlData]

```

## Enhanced Tool Descriptions

### 🎯 Primary Workflow Tools (RECOMMENDED)

#### `generate_comprehensive_test_suite` ⭐ **BEST CHOICE**
- **Purpose**: Complete end-to-end test generation with semantic analysis
- **Input**: Raw Jira XML content
- **Output**: 20-50 comprehensive test cases across 14 categories
- **Features**: 
  - Semantic keyword mapping to test categories
  - Multi-context scenario expansion (IVR variants, user roles)
  - Technical artifact extraction (SQL, JSON, HTML)
  - Conditional logic analysis and decision tree testing
  - Performance test generation based on domain context
  - 80%+ coverage across all test dimensions
- **Use Case**: Any Jira ticket requiring comprehensive test coverage

#### `process_jira_workflow`
- **Purpose**: Fully automated workflow with validation, cleaning, and generation
- **Input**: Raw Jira XML (handles malformed XML automatically)
- **Output**: Complete TestRail-ready test cases with error recovery
- **Use Case**: Fully automated processing without manual intervention

### 🔍 Analysis & Validation Tools

#### `analyze_jira_xml_for_llm`
- **Purpose**: Primary XML analysis with structured JSON output optimized for AI
- **Input**: Raw Jira XML content
- **Output**: Comprehensive ticket analysis with confidence scoring
- **Features**:
  - Complexity analysis and scoring
  - UI component identification
  - Business logic extraction
  - Comments analysis and activity summary
  - LLM guidance with suggested prompts
- **Use Case**: Detailed analysis before specialized test generation

#### `validate_jira_xml`
- **Purpose**: XML structure validation and diagnostic reporting
- **Input**: Raw Jira XML content
- **Output**: Validation status, error details, and recommendations
- **Use Case**: First step for any XML processing to identify structural issues

#### `clean_jira_xml`
- **Purpose**: Automatic XML repair and error correction
- **Input**: Malformed XML content
- **Output**: Cleaned, valid XML ready for processing
- **Features**:
  - Fixes missing closing tags
  - Resolves duplicate attributes
  - Escapes HTML entities
  - Removes invalid characters
- **Use Case**: When validation fails or XML has structural problems

### 🧠 Specialized Analysis Tools

#### `extract_business_logic_analysis`
- **Purpose**: Extract business rules and conditional logic patterns
- **Input**: Jira description text
- **Output**: Business rules with test implications and risk assessment
- **Features**:
  - Conditional logic pattern detection
  - Decision tree analysis
  - Risk level assessment
  - Test scenario suggestions
- **Use Case**: Complex business logic requiring comprehensive decision path testing

#### `extract_ui_components_analysis`
- **Purpose**: Identify UI elements and interaction patterns
- **Input**: Jira description text
- **Output**: UI component analysis with complexity scoring
- **Features**:
  - Form, button, modal, navigation detection
  - Complexity scoring for UI elements
  - Test area recommendations
  - Confidence metrics
- **Use Case**: UI-heavy tickets requiring interface and interaction testing

#### `generate_comprehensive_test_matrix`
- **Purpose**: Complete test coverage matrix and gap analysis
- **Input**: Parsed XML data from analysis
- **Output**: Test coverage matrix across all categories with gap identification
- **Features**:
  - 14-category coverage analysis
  - Potential test count estimation
  - Feature-to-test mapping
  - Identified coverage gaps
- **Use Case**: Coverage planning and gap identification

### 🚀 Enhancement & Template Tools

#### `enhance_test_cases_with_llm`
- **Purpose**: Enhance existing test cases with additional coverage categories
- **Input**: Parsed XML data + initial test cases
- **Output**: Enhanced test suite with intelligent deduplication
- **Features**:
  - 10+ test category enhancement
  - Intelligent deduplication
  - TestRail-compatible output
  - Coverage optimization
- **Use Case**: Expand existing test cases with comprehensive coverage

#### `generate_test_case_templates`
- **Purpose**: Generate baseline test templates by ticket type
- **Input**: Ticket type, priority, component
- **Output**: Foundation test templates for enhancement
- **Use Case**: Creating structured baseline tests for manual enhancement

## Enhanced Semantic Categories (14 Categories)

### High-Priority Categories
1. **Performance** - TTS rendering, latency, concurrent requests, memory optimization
2. **Security** - Authentication, authorization, XSS, SQL injection, session management
3. **Accessibility** - Screen readers, keyboard navigation, WCAG compliance, ARIA
4. **Integration** - Database connections, API endpoints, third-party services, workflows

### Core Testing Categories
5. **DataValidation** - Format compliance, data integrity, type validation, constraints
6. **ConditionalLogic** - Decision trees, if-else paths, fallback mechanisms, branches
7. **ScenarioExpansion** - Multi-context variants (IVR types, user roles, environments)
8. **ErrorHandling** - Network failures, timeouts, exception recovery, graceful degradation

### Advanced Categories
9. **UserExperience** - Cross-browser compatibility, mobile responsiveness, usability
10. **StateManagement** - Session handling, state transitions, data persistence, cache
11. **BoundaryTests** - Min/max values, edge cases, system limits, threshold testing
12. **FormatCompliance** - JSON/XML schemas, SQL syntax, Unicode support, protocols

### Specialized Categories
13. **EdgeCases** - Corner cases, unusual inputs, system boundary conditions
14. **NegativeTests** - Invalid inputs, malformed data, error condition validation

## Multi-Context Scenario Expansion

### IVR Context Expansion
**Base Contexts**: `["no-pin IVR", "athena IVR", "availity IVR"]`
**Pattern**: `"{TestTitle} in {Context} flow"`
**Example**: 
- Base: "Language Selection Prompt"
- Generated: 3 context-specific test variants with timeout validation

### User Role Context Expansion
**Base Contexts**: `["guest user", "authenticated user", "admin user", "power user"]`
**Pattern**: `"{TestTitle} for {UserRole}"`
**Example**:
- Base: "Access Control Validation"
- Generated: 4 role-specific authorization tests

### Environment Context Expansion
**Base Contexts**: `["development", "staging", "production", "mobile", "desktop"]`
**Pattern**: `"{TestTitle} in {Environment} environment"`

## Technical Artifact Processing

### SQL Snippet Detection & Testing
**Patterns**: `UPDATE`, `INSERT`, `SELECT`, `DELETE`, `CREATE`, `ALTER`
**Generated Tests**:
- Syntax validation and SQL injection prevention
- Transaction integrity and rollback scenarios
- Performance optimization and indexing validation
- Data consistency and constraint verification

### JSON Structure Analysis
**Patterns**: Schema validation, nested structures, arrays, null handling
**Generated Tests**:
- Schema compliance validation
- Nested object processing
- Array boundary testing
- Null/undefined value handling

### Configuration Data Processing
**Patterns**: Settings, parameters, options, environment variables
**Generated Tests**:
- Valid configuration acceptance
- Invalid configuration rejection
- Default value behavior validation
- Configuration precedence testing

### HTML/XML Content Analysis
**Patterns**: Tags, attributes, CDATA sections, entities
**Generated Tests**:
- Structure validation and parsing
- XSS prevention and sanitization
- Entity encoding/decoding
- Malformed content handling

## Quality Targets & Success Metrics

### Coverage Targets
- **Minimum Coverage**: 80% across all 14 test categories
- **Test Generation**: 20-50 comprehensive tests per ticket (vs. 16 basic)
- **Processing Time**: < 3 seconds for complex XML analysis
- **Semantic Inference**: < 500ms for category mapping

### Test Distribution Guidelines
```
Functional Tests: 40-50%
Security Tests: 10-15% 
Performance Tests: 10-15%
Accessibility Tests: 5-10%
Integration Tests: 10-15%
Error Handling: 5-10%
User Experience: 5-10%
Specialized Categories: 10-15%
```

### Confidence Scoring
- **High Confidence (0.9-1.0)**: Direct keyword matches, clear patterns
- **Medium Confidence (0.7-0.89)**: Partial matches, implied context
- **Low Confidence (0.5-0.69)**: Weak associations, limited context
- **Review Required (<0.5)**: Insufficient information for reliable generation

## Company-Wide Deployment

### For Organization Access:
1. **Local Development**: Use `http://localhost:5001/mcp`
2. **Team Sharing**: Use ngrok to expose server publicly
   ```bash
   ngrok http 5001
   # Update OpenAPI schema with ngrok URL
   ```
3. **Custom GPT Sharing**: Share within organization for company-wide access
4. **Authentication**: Configure as "None" for internal use

### Expected Performance:
- **Processing Speed**: 2-3 seconds for comprehensive analysis
- **Test Quality**: 85%+ average confidence scores
- **Coverage Improvement**: From 32% baseline to 80%+ comprehensive
- **Output Format**: TestRail-compatible JSON, XML, CSV, and Markdown

This enhanced OpenAPI schema and instruction set enables sophisticated test generation with semantic analysis, multi-context expansion, and comprehensive coverage across all testing dimensions.
