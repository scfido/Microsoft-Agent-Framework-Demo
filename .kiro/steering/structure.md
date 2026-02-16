# Project Structure

```
MafDemo.slnx                    # Solution file (XML .slnx format)
Directory.Packages.props         # Central NuGet package version management

GettingStarted/                  # Introductory samples — step-by-step agent tutorials
├── Agents/                      # Agent examples (Step01_Running, Step02_MultiturnConversation)
└── Program.cs                   # Entry point, selects which sample to run

Cowork/                          # Standalone single-agent console demo

A2A/TravelPlanner/               # Multi-agent A2A protocol demo
├── HotelAgentServer/            # ASP.NET Core A2A server — hotel recommendations
├── WeatherAgentServer/          # ASP.NET Core A2A server — weather queries
├── PlanAgentServer/             # ASP.NET Core A2A server — attraction/plan recommendations
├── TravelPlannerClient/         # Console client that orchestrates all A2A agents via a main AI agent
└── RunAllServer.ps1             # PowerShell script to launch all three servers

AgentSkills/                     # Agent Skills specification integration
├── AutoBot/                     # Library: skill engine, tool factory, context provider (packaged as Maf.AgentSkills)
│   ├── AutoBot/                 # Core: SkillEngine/, Tools/, options, state, prompt templates
│   └── Microsoft/               # Extension methods for MAF integration
└── ClientAgent/                 # Console agent that uses AutoBot skills

AGUIWebChat/                     # AG-UI protocol web chat demo
├── Server/                      # ASP.NET Core backend hosting AG-UI endpoint
└── Client/                      # React 19 + Vite + TailwindCSS + CopilotKit frontend

Olds/                            # Archived/legacy samples (AISlogan, CheckpointDemo, Judge, ThreadDemo, Test01)
```

## Conventions
- Each demo project is self-contained with its own `.csproj` and `Program.cs`
- ASP.NET Core web projects use `Microsoft.NET.Sdk.Web`; console apps use `Microsoft.NET.Sdk`
- A2A server agents follow the pattern: `Agent.cs` (logic + AgentCard) + `Program.cs` (host setup with MapA2A)
- Agent classes use a static factory pattern (`CreateAsync` / `Create`) returning the agent wrapper
- `appsettings.Development.json` is gitignored — secrets and endpoints go there
- The `Olds/` folder contains deprecated samples; avoid referencing them for new work
