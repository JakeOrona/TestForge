import { Component, ElementRef, ViewChild, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { ChatService, ChatMessageModel, Tool } from '../../services/chat.service';
import { MarkdownService } from '../../services/markdown.service';

interface ChatMessage {
  type: 'user' | 'assistant';
  sender: string;
  content: string;
  htmlContent?: string; // Parsed markdown content
  timestamp: Date;
}

@Component({
  selector: 'app-chat',
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './chat.html',
  styleUrl: './chat.css'
})
export class ChatComponent implements AfterViewChecked {
  @ViewChild('messagesContainer') private messagesContainer!: ElementRef;

  messages: ChatMessage[] = [
    {
      type: 'assistant',
      sender: 'ChatDemo Assistant',
      content: 'Hello! I\'m your AI assistant powered by ChatGPT and Model Context Protocol (MCP). Ask me anything!',
      htmlContent: 'Hello! I\'m your AI assistant powered by <strong>ChatGPT</strong> and <strong>Model Context Protocol (MCP)</strong>. Ask me anything!',
      timestamp: new Date()
    }
  ];

  currentMessage: string = '';
  isLoading: boolean = false;
  availableTools: Tool[] = [];
  conversationId: string = '';

  constructor(private chatService: ChatService, private markdownService: MarkdownService) {
    this.conversationId = this.generateConversationId();
    this.loadAvailableTools();
  }

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  sendMessage() {
    if (!this.currentMessage.trim()) return;

    // Add user message
    const userMessage: ChatMessage = {
      type: 'user',
      sender: 'You',
      content: this.currentMessage,
      htmlContent: this.markdownService.parseMarkdown(this.currentMessage),
      timestamp: new Date()
    };
    this.messages.push(userMessage);

    const messageText = this.currentMessage;
    this.currentMessage = '';
    this.isLoading = true;

    this.chatService.sendMessage(messageText, this.conversationId).subscribe({
      next: (response) => {
        if (response.success && response.message) {
          const assistantMessage: ChatMessage = {
            type: 'assistant',
            sender: 'ChatDemo Assistant',
            content: response.message.message,
            htmlContent: this.markdownService.parseMarkdown(response.message.message),
            timestamp: new Date(response.message.timestamp)
          };
          this.messages.push(assistantMessage);
        } else {
          const errorMessage: ChatMessage = {
            type: 'assistant',
            sender: 'ChatDemo Assistant',
            content: response.error || 'Sorry, I encountered an error. Please try again.',
            htmlContent: this.markdownService.parseMarkdown(response.error || 'Sorry, I encountered an error. Please try again.'),
            timestamp: new Date()
          };
          this.messages.push(errorMessage);
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Chat error:', error);
        const errorMessage: ChatMessage = {
          type: 'assistant',
          sender: 'ChatDemo Assistant',
          content: 'Sorry, I encountered an error. Please try again.',
          htmlContent: 'Sorry, I encountered an error. Please try again.',
          timestamp: new Date()
        };
        this.messages.push(errorMessage);
        this.isLoading = false;
      }
    });
  }

  private generateConversationId(): string {
    return 'conv_' + Math.random().toString(36).substr(2, 9) + '_' + Date.now();
  }

  private loadAvailableTools() {
    this.chatService.getAvailableTools().subscribe({
      next: (tools) => {
        this.availableTools = tools;
        console.log('Available tools:', tools);
      },
      error: (error) => {
        console.error('Error loading tools:', error);
      }
    });
  }

  private scrollToBottom(): void {
    try {
      this.messagesContainer.nativeElement.scrollTop = this.messagesContainer.nativeElement.scrollHeight;
    } catch(err) { }
  }
}
