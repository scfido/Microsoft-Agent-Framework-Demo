// Copyright (c) Microsoft. All rights reserved.

// This sample demonstrates how to use a HarnessAgent with the Harness AIContextProviders
// (TodoProvider and AgentModeProvider) for interactive research tasks with web search
// capabilities powered by Azure AI Foundry.
// The agent plans research tasks, creates a todo list, gets user approval,
// and then executes each step — all within an interactive conversation loop.
//
// Special commands:
//   /todos  — Display the current todo list without invoking the agent.
//   exit    — End the session.

#pragma warning disable MAAI001  // Suppress experimental API warnings for Agents AI experiments.

using Cowork;
using Harness.Shared.Console;
using Harness.Shared.Console.ToolFormatters;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;
using SampleApp;

const int MaxContextWindowTokens = 1_050_000;
const int MaxOutputTokens = 128_000;

// Load Configuration
var configuration = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true)
    .Build();

AIAgent agent = await CoworkAgent.CreateAsync(configuration);


// Run the interactive console session using the shared HarnessConsole helper.
await HarnessConsole.RunAgentAsync(
    agent,
    userPrompt: "Enter a research topic to get started.",
    new HarnessConsoleOptions
    {
        Observers = [
            .. HarnessConsoleOptions.BuildObserversWithPlanning(
                agent,
                planModeName: "plan",
                executionModeName: "execute",
                maxContextWindowTokens: MaxContextWindowTokens,
                maxOutputTokens: MaxOutputTokens,
                toolFormatters: [new DownloadUriToolFormatter(), .. ToolCallFormatter.BuildDefaultToolFormatters()])],
        CommandHandlers = HarnessConsoleOptions.BuildDefaultCommandHandlers(agent),
    });