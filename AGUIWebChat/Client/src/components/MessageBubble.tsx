import { User, Bot } from "lucide-react";
import { cn } from "@/lib/utils";
import { Card, CardContent } from "@/components/ui/card";
import { MarkdownRenderer } from "@/components/MarkdownRenderer";
import { ToolCallDisplay } from "@/components/ToolCallDisplay";
import type { ChatMessage } from "@/hooks/useChat";

interface MessageBubbleProps {
  message: ChatMessage;
}

export function MessageBubble({ message }: MessageBubbleProps) {
  const isUser = message.role === "user";

  if (isUser) {
    return (
      <div className="flex items-start justify-end gap-2">
        <div className="max-w-[80%] rounded-2xl rounded-tr-sm bg-primary px-4 py-2.5 text-primary-foreground">
          <p className="whitespace-pre-wrap text-sm">{message.content}</p>
        </div>
        <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary/10">
          <User className="h-4 w-4 text-primary" />
        </div>
      </div>
    );
  }

  return (
    <div className="flex items-start gap-2">
      <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-muted">
        <Bot className="h-4 w-4 text-muted-foreground" />
      </div>
      <div className="max-w-[80%]">
        <Card
          className={cn(
            "gap-0 py-0",
            message.isError && "border-destructive bg-destructive/5"
          )}
        >
          <CardContent className="p-3">
            {message.isError ? (
              <p className="text-sm text-destructive">{message.content}</p>
            ) : (
              <MarkdownRenderer content={message.content} />
            )}
            {message.isStreaming && !message.content && (
              <span className="inline-block h-4 w-1.5 animate-pulse rounded-sm bg-muted-foreground/50" />
            )}
            {message.isStreaming && message.content && (
              <span className="ml-0.5 inline-block h-4 w-1.5 animate-pulse rounded-sm bg-muted-foreground/50 align-text-bottom" />
            )}
          </CardContent>
        </Card>
        {message.toolCalls.length > 0 && (
          <div className="mt-2 space-y-1.5">
            {message.toolCalls.map((toolCall) => (
              <ToolCallDisplay key={toolCall.id} toolCall={toolCall} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
