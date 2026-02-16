# Tech Stack

## Runtime & Language
- .NET 10 (net10.0) — all C# projects target this
- C# with nullable reference types enabled, implicit usings enabled
- TypeScript / React 19 for the AGUIWebChat client

## Build System
- Solution file: `MafDemo.slnx` (XML-based .slnx format)
- Central Package Management via `Directory.Packages.props` at repo root
- NuGet for .NET dependencies; pnpm for the JS/TS client

## Core Frameworks & Libraries

### Microsoft Agent Framework (MAF)
- `Microsoft.Agents.AI` — core agent abstractions and `AIAgent`
- `Microsoft.Agents.AI.OpenAI` — OpenAI/Azure OpenAI integration
- `Microsoft.Agents.AI.Workflows` — workflow orchestration (events, executors, checkpoints)
- `Microsoft.Agents.AI.Hosting.AGUI.AspNetCore` — AG-UI protocol hosting

### Microsoft.Extensions.AI
- `Microsoft.Extensions.AI` / `.Abstractions` — `IChatClient`, `AIFunction`, `AITool` abstractions
- `Microsoft.Extensions.AI.OpenAI` — OpenAI adapter for the Extensions.AI interface

### A2A Protocol
- `A2A.AspNetCore` — Agent-to-Agent protocol server hosting (TaskManager, AgentCard, AgentSkill)

### Frontend (AGUIWebChat/Client)
- React 19, Vite 7, TailwindCSS 4
- TanStack Router for file-based routing
- CopilotKit (`@copilotkit/react-core`, `react-ui`, `runtime`) for chat UI
- `@ag-ui/client` for AG-UI protocol client

### Other
- `Azure.Identity` / `Azure.AI.OpenAI` — Azure authentication
- `YamlDotNet` — YAML frontmatter parsing (AgentSkills)
- `Microsoft.SemanticKernel.Connectors.InMemory` — in-memory vector store

## Common Commands

```powershell
# Build entire solution
dotnet build MafDemo.slnx

# Run a specific project
dotnet run --project GettingStarted/GettingStarted.csproj
dotnet run --project Cowork/Cowork.csproj
dotnet run --project AGUIWebChat/Server/Server.csproj

# Run all A2A TravelPlanner servers (from A2A/TravelPlanner/)
.\RunAllServer.ps1

# AGUIWebChat client (from AGUIWebChat/Client/)
pnpm install
pnpm dev          # dev server on port 3000
pnpm build        # production build
pnpm test         # vitest run

# Restore NuGet packages
dotnet restore MafDemo.slnx
```

## Configuration
- OpenAI/LLM settings go in `appsettings.json` and `appsettings.Development.json` (gitignored)
- Config keys: `OpenAI:Endpoint`, `OpenAI:ApiKey`, `OpenAI:Model`
- All projects use `IConfiguration` with JSON + environment variable providers
