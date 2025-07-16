import { Injectable } from '@angular/core';
import { marked } from 'marked';
import hljs from 'highlight.js';

@Injectable({
  providedIn: 'root'
})
export class MarkdownService {
  
  constructor() {
    this.configureMarked();
  }

  private configureMarked(): void {
    // Configure marked with basic options
    marked.setOptions({
      breaks: true, // Convert \n to <br>
      gfm: true, // GitHub Flavored Markdown
    });

    // Configure custom renderer for better styling
    const renderer = new marked.Renderer();
    
    // Custom link renderer to open external links in new tab
    renderer.link = (token: any) => {
      const href = token.href;
      const title = token.title ? ` title="${token.title}"` : '';
      const text = token.text;
      
      if (href && (href.startsWith('http://') || href.startsWith('https://'))) {
        return `<a href="${href}"${title} target="_blank" rel="noopener noreferrer">${text}</a>`;
      }
      return `<a href="${href}"${title}>${text}</a>`;
    };

    // Custom code renderer for better styling
    renderer.code = (token: any) => {
      const code = token.text;
      const language = token.lang || 'plaintext';
      
      const validLang = hljs.getLanguage(language) ? language : 'plaintext';
      const highlighted = hljs.highlight(code, { language: validLang }).value;
      return `<pre class="code-block"><code class="hljs language-${validLang}">${highlighted}</code></pre>`;
    };

    // Custom blockquote renderer
    renderer.blockquote = (token: any) => {
      return `<blockquote class="markdown-blockquote">${token.text}</blockquote>`;
    };

    // Custom table renderer
    renderer.table = (token: any) => {
      const header = token.header.map((cell: any) => `<th>${cell.text}</th>`).join('');
      const body = token.rows.map((row: any) => 
        `<tr>${row.map((cell: any) => `<td>${cell.text}</td>`).join('')}</tr>`
      ).join('');
      
      return `<table class="markdown-table">
        <thead><tr>${header}</tr></thead>
        <tbody>${body}</tbody>
      </table>`;
    };

    marked.use({ renderer });
  }

  /**
   * Parse markdown text to HTML
   * @param markdown The markdown text to parse
   * @returns HTML string
   */
  parseMarkdown(markdown: string): string {
    if (!markdown) return '';
    
    try {
      return marked.parse(markdown) as string;
    } catch (error) {
      console.error('Error parsing markdown:', error);
      // Fallback to escaped HTML if markdown parsing fails
      return this.escapeHtml(markdown);
    }
  }

  /**
   * Escape HTML characters for safe display
   * @param text Text to escape
   * @returns Escaped HTML string
   */
  private escapeHtml(text: string): string {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
  }

  /**
   * Check if text contains markdown-like syntax
   * @param text Text to check
   * @returns True if text appears to contain markdown
   */
  isMarkdown(text: string): boolean {
    if (!text) return false;
    
    // Simple heuristics to detect markdown
    const markdownPatterns = [
      /#{1,6}\s/, // Headers
      /\*\*.*\*\*/, // Bold
      /\*.*\*/, // Italic
      /`.*`/, // Inline code
      /```[\s\S]*```/, // Code blocks
      /^\s*[-*+]\s/, // Lists
      /^\s*\d+\.\s/, // Ordered lists
      /\[.*\]\(.*\)/, // Links
      /^\s*>/, // Blockquotes
    ];

    return markdownPatterns.some(pattern => pattern.test(text));
  }
}
