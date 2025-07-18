# MCP-Demo Project Cleanup Prompt

## Objective
Clean up the MCP-Demo project by removing unused Angular components, empty files, and reorganizing the structure to focus on the core MCP server functionality.

## Current State Analysis
The project currently contains:
- **ChatDemo.Api** - Basic API that's not being used
- **ChatDemo.Client** - Angular frontend (no longer needed)
- **ChatDemo.McpServer** - Core MCP server (primary focus)
- Multiple empty C# files that were created but never implemented
- Large node_modules directory from Angular (taking up space)

## Cleanup Tasks

### 1. Remove Unused Angular Client
**Action**: Remove the entire Angular client directory and all its contents

**Rationale**: 
- Angular frontend is no longer used
- Large node_modules directory (12K+ files) taking up unnecessary space
- Package.json and build configurations are redundant
- Focus is now on MCP server functionality

**Commands**:
```bash
# Remove Angular client directory completely
rm -rf /Users/jakeorona/MCP-POC/MCP-demo/ChatDemo.Client

# Verify removal
ls -la /Users/jakeorona/MCP-POC/MCP-demo/
```

### 2. Remove or Consolidate ChatDemo.Api
**Decision Required**: The API appears to be minimal and unused. Options:
- **Option A**: Remove entirely if not needed
- **Option B**: Keep as placeholder for future API endpoints

**If removing (Option A)**:
```bash
# Remove API directory
rm -rf /Users/jakeorona/MCP-POC/MCP-demo/ChatDemo.Api

# Update solution file to remove API project reference
# Edit ChatDemo.sln to remove API project
```

**If keeping (Option B)**:
```bash
# Keep as is, but document its purpose in README
```

### 3. Remove Empty C# Files
**Files to remove** (currently empty):
- `/Users/jakeorona/MCP-POC/MCP-demo/ChatDemo.McpServer/Models/ServiceResults.cs`
- `/Users/jakeorona/MCP-POC/MCP-demo/ChatDemo.McpServer/Models/TestCaseModels.cs`
- `/Users/jakeorona/MCP-POC/MCP-demo/ChatDemo.McpServer/Models/JiraModels.cs`
- `/Users/jakeorona/MCP-POC/MCP-demo/ChatDemo.McpServer/Extensions/XElementExtensions.cs`
- `/Users/jakeorona/MCP-POC/MCP-demo/ChatDemo.McpServer/Services/TestCaseFormattingService.cs`
- `/Users/jakeorona/MCP-POC/MCP-demo/ChatDemo.McpServer/Services/Interfaces.cs`
- `/Users/jakeorona/MCP-POC/MCP-demo/ChatDemo.McpServer/Services/TestCaseGenerationService.cs`
- `/Users/jakeorona/MCP-POC/MCP-demo/ChatDemo.McpServer/Services/TextAnalysisService.cs`
- `/Users/jakeorona/MCP-POC/MCP-demo/ChatDemo.McpServer/Services/JiraXmlParsingService.cs`

**Commands**:
```bash
# Remove empty C# files
find /Users/jakeorona/MCP-POC/MCP-demo -name "*.cs" -empty -delete

# Verify removal
find /Users/jakeorona/MCP-POC/MCP-demo -name "*.cs" -empty
```

### 4. Remove Empty Directories
**Action**: Remove any directories that become empty after file cleanup

**Commands**:
```bash
# Remove empty directories
find /Users/jakeorona/MCP-POC/MCP-demo -type d -empty -delete

# Verify directory structure
tree /Users/jakeorona/MCP-POC/MCP-demo -I 'bin|obj|node_modules'
```

### 5. Clean Build Artifacts
**Action**: Remove build artifacts and temporary files

**Commands**:
```bash
# Remove build artifacts
find /Users/jakeorona/MCP-POC/MCP-demo -name "bin" -type d -exec rm -rf {} + 2>/dev/null || true
find /Users/jakeorona/MCP-POC/MCP-demo -name "obj" -type d -exec rm -rf {} + 2>/dev/null || true

# Remove backup files
find /Users/jakeorona/MCP-POC/MCP-demo -name "*.bak" -delete
```

### 6. Update Project Structure Documentation
**Action**: Update README.md to reflect the cleaned-up structure

**Changes needed**:
- Remove references to Angular client
- Remove references to ChatDemo.Api (if removed)
- Update project structure section
- Update getting started instructions
- Remove Angular-related prerequisites

**Updated Project Structure**:
```markdown
## Project Structure

- **ChatDemo.McpServer** - Model Context Protocol server with 13 specialized tools for intelligent test case generation
- **Guidance/** - Documentation and instruction files
- **Documentation files** - Various .md files for setup, debugging, and enhancement guides
```

### 7. Update Solution File
**Action**: Update ChatDemo.sln to remove references to deleted projects

**Commands**:
```bash
# Edit the solution file to remove deleted project references
# If removing ChatDemo.Api and ChatDemo.Client, remove their project entries
```

### 8. Validation Steps
**After cleanup, verify**:

1. **Project builds successfully**:
   ```bash
   cd /Users/jakeorona/MCP-POC/MCP-demo/ChatDemo.McpServer
   dotnet build
   ```

2. **MCP server runs correctly**:
   ```bash
   cd /Users/jakeorona/MCP-POC/MCP-demo/ChatDemo.McpServer
   ASPNETCORE_URLS="http://localhost:5001" dotnet run
   ```

3. **All 13 tools are available**:
   ```bash
   # Test tools list
   curl -X POST http://localhost:5001/mcp \
     -H "Content-Type: application/json" \
     -d '{"jsonrpc": "2.0", "id": 2, "method": "tools/list", "params": {}}'
   ```

4. **Directory structure is clean**:
   ```bash
   tree /Users/jakeorona/MCP-POC/MCP-demo -I 'bin|obj'
   ```

## Expected Results After Cleanup

### New Project Structure
```
MCP-demo/
├── ChatDemo.McpServer/          # Core MCP server
│   ├── appsettings.json
│   ├── ChatDemo.McpServer.csproj
│   ├── ChatDemoTools.cs
│   ├── Program.cs
│   └── Models/
│       ├── JiraConfig.cs
│       └── LLMAnalysisModels.cs
├── guidance/                    # Documentation
│   ├── code-review.instructions.md
│   ├── mcp-dev.instructions.md
│   └── playwrightTS.instructions.md
├── ChatDemo.sln                 # Solution file (updated)
├── README.md                    # Updated documentation
└── Various .md files            # Setup and debugging guides
```

### Benefits of Cleanup
- **Reduced disk space** - Remove ~12K+ files from node_modules
- **Cleaner structure** - Focus on core MCP functionality
- **Easier maintenance** - Remove unused/empty files
- **Better performance** - Faster file operations and searches
- **Clearer purpose** - Project structure matches actual usage

### Files/Directories to Remove
- `ChatDemo.Client/` (entire directory)
- `ChatDemo.Api/` (if not needed)
- All empty .cs files (9 files identified)
- Build artifacts (bin/, obj/ directories)
- Backup files (*.bak)

### Files to Keep
- `ChatDemo.McpServer/` (core functionality)
- `guidance/` (documentation)
- All .md documentation files
- `ChatDemo.sln` (updated)
- Non-empty .cs files with actual implementation

## Implementation Order
1. **Backup current state** (git commit or copy)
2. **Remove Angular client** (largest space saver)
3. **Remove empty C# files** (cleanup dead code)
4. **Remove build artifacts** (cleanup temporary files)
5. **Update documentation** (README.md)
6. **Update solution file** (if needed)
7. **Validate functionality** (build and test)

This cleanup will transform the project from a multi-component demo into a focused MCP server implementation with clear documentation and minimal overhead.
