import { createFileRoute } from '@tanstack/react-router'
import { MessageSquarePlus, MessageCircle } from 'lucide-react'
import { useChat } from '@/hooks/useChat'
import { ChatMessageList } from '@/components/ChatMessageList'
import { ChatInput } from '@/components/ChatInput'
import { Button } from '@/components/ui/button'

export const Route = createFileRoute('/')({
  component: ChatPage,
})

function ChatPage() {
  const { messages, isRunning, sendMessage, abortRun, clearChat } = useChat()

  return (
    <div className="flex h-dvh flex-col bg-background">
      {/* Header */}
      <header className="flex items-center justify-between border-b px-4 py-3">
        <h1 className="text-lg font-semibold">AG-UI Chat</h1>
        <Button variant="ghost" size="sm" onClick={clearChat}>
          <MessageSquarePlus className="mr-2 size-4" />
          新对话
        </Button>
      </header>

      {/* Message area */}
      <div className="flex-1 overflow-hidden">
        {messages.length === 0 ? (
          <div className="flex h-full flex-col items-center justify-center gap-3 text-muted-foreground">
            <MessageCircle className="size-12 opacity-40" />
            <p className="text-lg">开始一段新的对话</p>
            <p className="text-sm">在下方输入消息，与 AI 助手开始交流</p>
          </div>
        ) : (
          <ChatMessageList messages={messages} />
        )}
      </div>

      {/* Input area */}
      <div className="border-t p-4">
        <ChatInput
          onSend={sendMessage}
          onAbort={abortRun}
          isRunning={isRunning}
          disabled={false}
        />
      </div>
    </div>
  )
}
