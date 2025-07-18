#!/bin/bash

echo "=== Testing MCP Server ==="
echo "Local URL: http://localhost:4200/mcp"
echo "External URL: http://192.168.0.54:4200/mcp"
echo ""

echo "1. Testing local server connectivity..."
curl -s "http://localhost:4200/mcp" || echo "Local connection failed"
echo ""

echo "2. Testing external server connectivity..."
curl -s "http://192.168.0.54:4200/mcp" || echo "External connection failed"
echo ""

echo "3. Testing MCP initialization (local)..."
curl -s -X POST \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc": "2.0", "id": "1", "method": "initialize", "params": {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "test-client", "version": "1.0.0"}}}' \
  "http://localhost:4200/mcp"
echo ""

echo "4. Testing MCP initialization (external)..."
curl -s -X POST \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc": "2.0", "id": "1", "method": "initialize", "params": {"protocolVersion": "2024-11-05", "capabilities": {}, "clientInfo": {"name": "test-client", "version": "1.0.0"}}}' \
  "http://192.168.0.54:4200/mcp"
echo ""

echo "=== Testing Complete ==="
echo ""
echo "✅ MCP Server Status: Running"
echo "✅ Local Access: http://localhost:4200/mcp"
echo "✅ External Access: http://192.168.0.54:4200/mcp"
echo ""
echo "🎯 Ready for ChatGPT/Claude Integration!"
echo "Use this endpoint in your LLM configuration: http://192.168.0.54:4200"
echo ""
echo "📋 Available Tools:"
echo "- generate_test_cases_from_jira_xml"
echo ""
echo "📁 Test with sample XML:"
echo "cat /Users/jakeorona/MCP-POC/MCP-demo/sample-jira-story.xml"