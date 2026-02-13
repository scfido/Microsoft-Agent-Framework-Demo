using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace MafDemo.Olds.ThreadDemo;

/// <summary>
/// 自定义的聊天消息存储，用于在会话线程中持久化消息。
/// </summary>
internal sealed class CustomeChatHistoryProvider : ChatHistoryProvider
{
    private List<ChatMessage> messages = [];
    public CustomeChatHistoryProvider(JsonElement serializedStoreState, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        if (serializedStoreState.ValueKind is JsonValueKind.String)
        {
            // Here we can deserialize the thread id so that we can access the same messages as before the suspension.
            ThreadDbKey = serializedStoreState.Deserialize<string>();
        }
    }

    public string? ThreadDbKey { get; private set; }
    
    protected override async ValueTask<IEnumerable<ChatMessage>> InvokingCoreAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        return messages;
    }

    protected override async ValueTask InvokedCoreAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        // Don't store messages if the request failed.
        if (context.InvokeException is not null)
        {
            return;
        }

        this.ThreadDbKey ??= Guid.NewGuid().ToString("N");

        // Add both request and response messages to the store
        // Optionally messages produced by the AIContextProvider can also be persisted (not shown).
        var newMessages = context.RequestMessages.Concat(context.RequestMessages ?? []).Concat(context.ResponseMessages ?? []);
        messages.AddRange(newMessages);
    }
   
    public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        // We have to serialize the thread id, so that on deserialization we can retrieve the messages using the same thread id.
        return JsonSerializer.SerializeToElement(this.ThreadDbKey);
    }
}
