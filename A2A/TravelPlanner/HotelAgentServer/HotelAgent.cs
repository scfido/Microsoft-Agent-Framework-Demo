using A2A;

namespace HotelAgentServer;

public class HotelAgent
{
    public void Attach(ITaskManager taskManager)
    {
        taskManager.OnMessageReceived = QueryHotelsAsync;
        taskManager.OnAgentCardQuery = GetAgentCardAsync;
    }

    private Task<A2AResponse> QueryHotelsAsync(MessageSendParams messageSendParams, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<A2AResponse>(cancellationToken);
        }

        // Process the message
        var messageText = messageSendParams.Message.Parts.OfType<TextPart>().First().Text;

        // Create and return an artifact
        var message = new AgentMessage()
        {
            Role = MessageRole.Agent,
            MessageId = Guid.NewGuid().ToString(),
            ContextId = messageSendParams.Message.ContextId,
            Parts = [new TextPart() {
                Text = $"""
                    🏨 **酒店推荐**

                    根据您的需求，为您推荐以下酒店：

                    **豪华型 ⭐⭐⭐⭐⭐**
                    1. 上海外滩华尔道夫酒店
                       - 📍 外滩核心位置，江景房
                       - 💰 ¥2,500/晚起
                       - ⭐ 评分 4.9/5.0

                    **舒适型 ⭐⭐⭐⭐**
                    2. 上海静安香格里拉大酒店
                       - 📍 静安寺商圈，交通便利
                       - 💰 ¥1,200/晚起
                       - ⭐ 评分 4.7/5.0

                    **经济型 ⭐⭐⭐**
                    3. 全季酒店（上海南京路店）
                       - 📍 南京路步行街旁
                       - 💰 ¥380/晚起
                       - ⭐ 评分 4.5/5.0

                    💡 提示：建议提前预订，周末和节假日价格可能上涨 20-50%。
                    """
            }]
        };

        return Task.FromResult<A2AResponse>(message);
    }

    private Task<AgentCard> GetAgentCardAsync(string agentUrl, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<AgentCard>(cancellationToken);
        }

        var capabilities = new AgentCapabilities()
        {
            Streaming = true,
            PushNotifications = false,
        };

        return Task.FromResult(new AgentCard()
        {
            Name = "hotel-a2a-agent",
            Description = "hotel information agent",
            Url = agentUrl,
            Version = "1.0.0",
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = capabilities,
            Skills = [
                new AgentSkill
                {
                    Id = "hotel-recommendation",
                    Name = "酒店推荐",
                    Description = "根据目的地和预算推荐合适的酒店，包括豪华型、舒适型、经济型",
                    Tags = ["hotel", "accommodation", "booking", "travel"],
                    Examples = ["推荐上海的酒店", "上海外滩附近有什么好酒店", "预算500以内的北京酒店"],
                    InputModes = ["text"],
                    OutputModes = ["text"]
                }
                ],
        });
    }
}