import { useCallback, useRef, useState } from 'react';
import { HttpAgent } from '@ag-ui/client';
import type { AgentSubscriber, Message } from '@ag-ui/client';

export interface ChatToolCall {
  id: string;
  name: string;
  args: string;
  isComplete: boolean;
}

export interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  isStreaming: boolean;
  toolCalls: ChatToolCall[];
  isError: boolean;
}

export interface UseChatReturn {
  messages: ChatMessage[];
  isRunning: boolean;
  error: string | null;
  sendMessage: (content: string) => void;
  abortRun: () => void;
  clearChat: () => void;
}

export function useChat(agentUrl: string = 'http://localhost:5050'): UseChatReturn {
  const agentRef = useRef<HttpAgent>(new HttpAgent({ url: agentUrl }));

  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [isRunning, setIsRunning] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const sendMessage = useCallback((content: string) => {
    const trimmed = content.trim();
    if (!trimmed) return;

    const userMessage: ChatMessage = {
      id: crypto.randomUUID(),
      role: 'user',
      content: trimmed,
      isStreaming: false,
      toolCalls: [],
      isError: false,
    };

    setMessages((prev) => [...prev, userMessage]);
    setIsRunning(true);
    setError(null);

    const agent = agentRef.current;

    // Convert all messages (including the new one) to AG-UI Message format
    const agentMessages: Message[] = [...messages, userMessage].map((msg) => ({
      id: msg.id,
      role: msg.role,
      content: msg.content,
    }));
    agent.setMessages(agentMessages);

    const subscriber: AgentSubscriber = {
      onTextMessageStartEvent: ({ event }) => {
        const assistantMessage: ChatMessage = {
          id: event.messageId,
          role: 'assistant',
          content: '',
          isStreaming: true,
          toolCalls: [],
          isError: false,
        };
        setMessages((prev) => [...prev, assistantMessage]);
      },

      onTextMessageContentEvent: ({ event }) => {
        setMessages((prev) => {
          const updated = [...prev];
          for (let i = updated.length - 1; i >= 0; i--) {
            if (updated[i].role === 'assistant') {
              updated[i] = {
                ...updated[i],
                content: updated[i].content + event.delta,
              };
              break;
            }
          }
          return updated;
        });
      },

      onTextMessageEndEvent: () => {
        setMessages((prev) => {
          const updated = [...prev];
          for (let i = updated.length - 1; i >= 0; i--) {
            if (updated[i].role === 'assistant') {
              updated[i] = { ...updated[i], isStreaming: false };
              break;
            }
          }
          return updated;
        });
      },

      onToolCallStartEvent: ({ event }) => {
        setMessages((prev) => {
          const updated = [...prev];
          for (let i = updated.length - 1; i >= 0; i--) {
            if (updated[i].role === 'assistant') {
              updated[i] = {
                ...updated[i],
                toolCalls: [
                  ...updated[i].toolCalls,
                  {
                    id: event.toolCallId,
                    name: event.toolCallName,
                    args: '',
                    isComplete: false,
                  },
                ],
              };
              break;
            }
          }
          return updated;
        });
      },

      onToolCallArgsEvent: ({ event }) => {
        setMessages((prev) => {
          const updated = [...prev];
          for (let i = updated.length - 1; i >= 0; i--) {
            if (updated[i].role === 'assistant') {
              updated[i] = {
                ...updated[i],
                toolCalls: updated[i].toolCalls.map((tc) =>
                  tc.id === event.toolCallId
                    ? { ...tc, args: tc.args + event.delta }
                    : tc,
                ),
              };
              break;
            }
          }
          return updated;
        });
      },

      onToolCallEndEvent: ({ event }) => {
        setMessages((prev) => {
          const updated = [...prev];
          for (let i = updated.length - 1; i >= 0; i--) {
            if (updated[i].role === 'assistant') {
              updated[i] = {
                ...updated[i],
                toolCalls: updated[i].toolCalls.map((tc) =>
                  tc.id === event.toolCallId
                    ? { ...tc, isComplete: true }
                    : tc,
                ),
              };
              break;
            }
          }
          return updated;
        });
      },

      onRunErrorEvent: ({ event }) => {
        const errorMessage = event.message ?? 'An unknown error occurred';
        setError(errorMessage);
        setIsRunning(false);
        const errorChatMessage: ChatMessage = {
          id: crypto.randomUUID(),
          role: 'assistant',
          content: errorMessage,
          isStreaming: false,
          toolCalls: [],
          isError: true,
        };
        setMessages((prev) => [...prev, errorChatMessage]);
      },
    };

    agent
      .runAgent({}, subscriber)
      .then(() => {
        setIsRunning(false);
      })
      .catch((err: unknown) => {
        setIsRunning(false);
        const message = err instanceof Error ? err.message : 'An unknown error occurred';
        setError(message);
      });
  }, [messages]);

  const abortRun = useCallback(() => {
    agentRef.current.abortRun();
    setIsRunning(false);
  }, []);

  const clearChat = useCallback(() => {
    setMessages([]);
    setError(null);
    setIsRunning(false);
    agentRef.current = new HttpAgent({ url: agentUrl });
  }, [agentUrl]);

  return { messages, isRunning, error, sendMessage, abortRun, clearChat };
}
