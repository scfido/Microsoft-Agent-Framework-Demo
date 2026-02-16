import { useState } from "react";
import { Wrench, ChevronRight, Check, Loader2 } from "lucide-react";
import {
  Collapsible,
  CollapsibleTrigger,
  CollapsibleContent,
} from "@/components/ui/collapsible";
import type { ChatToolCall } from "@/hooks/useChat";

interface ToolCallDisplayProps {
  toolCall: ChatToolCall;
}

export function ToolCallDisplay({ toolCall }: ToolCallDisplayProps) {
  const [open, setOpen] = useState(false);

  return (
    <Collapsible open={open} onOpenChange={setOpen}>
      <CollapsibleTrigger className="flex w-full items-center gap-2 rounded-md border px-3 py-2 text-sm text-muted-foreground hover:bg-muted/50 transition-colors">
        <ChevronRight
          className={`h-4 w-4 shrink-0 transition-transform duration-200 ${open ? "rotate-90" : ""}`}
        />
        <Wrench className="h-4 w-4 shrink-0" />
        <span className="truncate font-medium">{toolCall.name}</span>
        <span className="ml-auto shrink-0">
          {toolCall.isComplete ? (
            <Check className="h-4 w-4 text-green-500" />
          ) : (
            <Loader2 className="h-4 w-4 animate-spin" />
          )}
        </span>
      </CollapsibleTrigger>
      <CollapsibleContent>
        {toolCall.args && (
          <pre className="mt-1 overflow-x-auto rounded-md bg-muted p-3 text-xs">
            <code>{toolCall.args}</code>
          </pre>
        )}
      </CollapsibleContent>
    </Collapsible>
  );
}
