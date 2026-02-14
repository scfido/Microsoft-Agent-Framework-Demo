// Copyright (c) Maf.AgentSkills Contributors. All rights reserved.
// Licensed under the MIT License.

using AgentSkills;
using AgentSkills.Models;
using Microsoft.Agents.AI;
using System.Text.Json;

namespace Microsoft.Extensions.AI;

/// <summary>
/// Extension methods for creating skills-enabled AI agents from ChatClient.
/// </summary>
public static class ChatClientExtensions
{
    /// <summary>
    /// Creates an AI Agent with skills support using the AIContextProviderFactory pattern.
    /// </summary>
    /// <param name="chatClient">The chat client.</param>
    /// <param name="configureSkills">Optional callback to configure skills options.</param>
    /// <param name="configureAgent">Optional callback to configure agent options.</param>
    /// <returns>An AI Agent with skills support enabled.</returns>
    /// <example>
    /// <code>
    /// var agent = chatClient.CreateSkillsAgent(
    ///     configureSkills: options =>
    ///     {
    ///         options.AgentName = "my-assistant";
    ///         options.ProjectRoot = Directory.GetCurrentDirectory();
    ///     },
    ///     configureAgent: options =>
    ///     {
    ///         options.ChatOptions = new() { Instructions = "You are a helpful assistant." };
    ///     });
    /// </code>
    /// </example>
    public static AIAgent AsSkillsAIAgent(
        this IChatClient chatClient,
        Action<SkillsOptions>? configureSkills = null,
        Action<ChatClientAgentOptions>? configureAgent = null)
    {
        ArgumentNullException.ThrowIfNull(chatClient);

        var skillsOptions = new SkillsOptions();
        configureSkills?.Invoke(skillsOptions);

        var agentOptions = new ChatClientAgentOptions
        {
            AIContextProviderFactory = (ctx, ct) =>
            {
                // Check if we're restoring from serialized state
                if (ctx.SerializedState.ValueKind != JsonValueKind.Undefined)
                {
                    return ValueTask.FromResult<AIContextProvider>(new SkillsContextProvider(
                        chatClient,
                        ctx.SerializedState,
                        ctx.JsonSerializerOptions));
                }

                // Create new instance
                return ValueTask.FromResult<AIContextProvider>(new SkillsContextProvider(
                    chatClient,
                    skillsOptions));
            }
        };

        configureAgent?.Invoke(agentOptions);

        return chatClient.AsAIAgent(agentOptions);
    }
}
