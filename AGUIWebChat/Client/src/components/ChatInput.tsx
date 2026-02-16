import { useState, type KeyboardEvent } from "react";
import { SendHorizontal, Square } from "lucide-react";
import { Textarea } from "@/components/ui/textarea";
import { Button } from "@/components/ui/button";

interface ChatInputProps {
  onSend: (content: string) => void;
  onAbort: () => void;
  isRunning: boolean;
  disabled: boolean;
}

export function ChatInput({
  onSend,
  onAbort,
  isRunning,
  disabled,
}: ChatInputProps) {
  const [value, setValue] = useState("");

  const trimmed = value.trim();
  const canSend = trimmed.length > 0 && !isRunning && !disabled;

  function handleSend() {
    if (!canSend) return;
    onSend(trimmed);
    setValue("");
  }

  function handleKeyDown(e: KeyboardEvent<HTMLTextAreaElement>) {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  }

  return (
    <div className="flex items-end gap-2">
      <Textarea
        value={value}
        onChange={(e) => setValue(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder="输入消息..."
        disabled={isRunning || disabled}
        className="min-h-10 max-h-40 resize-none"
        rows={1}
      />
      {isRunning ? (
        <Button
          variant="destructive"
          size="icon"
          onClick={onAbort}
          aria-label="中止"
        >
          <Square className="size-4" />
        </Button>
      ) : (
        <Button
          size="icon"
          onClick={handleSend}
          disabled={!canSend}
          aria-label="发送"
        >
          <SendHorizontal className="size-4" />
        </Button>
      )}
    </div>
  );
}
