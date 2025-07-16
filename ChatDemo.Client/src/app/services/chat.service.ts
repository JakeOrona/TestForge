import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ChatRequestModel {
  message: string;
  conversationId?: string;
  timeZoneOffset?: number;
}

export interface ChatResponseModel {
  success: boolean;
  message?: ChatMessageModel;
  error?: string;
}

export interface ChatMessageModel {
  message: string;
  isFromUser: boolean;
  timestamp: Date;
}

export interface ConversationHistoryResponse {
  success: boolean;
  messages: ChatMessageModel[];
  error?: string;
}

export interface Tool {
  name: string;
  description: string;
}

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private readonly apiUrl = 'https://localhost:7173/api'; // API base URL

  constructor(private http: HttpClient) {}

  sendMessage(message: string, conversationId?: string, timeZoneOffset?: number): Observable<ChatResponseModel> {
    const payload: ChatRequestModel = { 
      message, 
      conversationId,
      timeZoneOffset: timeZoneOffset ?? new Date().getTimezoneOffset()
    };
    return this.http.post<ChatResponseModel>(`${this.apiUrl}/chat`, payload);
  }

  getConversationHistory(conversationId: string): Observable<ConversationHistoryResponse> {
    return this.http.get<ConversationHistoryResponse>(`${this.apiUrl}/chat/history/${conversationId}`);
  }

  getAvailableTools(): Observable<Tool[]> {
    return this.http.get<Tool[]>(`${this.apiUrl}/chat/tools`);
  }
}
