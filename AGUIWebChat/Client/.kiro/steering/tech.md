# Tech Stack

## Core
- React 19 with TypeScript (strict mode)
- Vite 7 (build tool and dev server)
- TanStack Router (file-based routing with auto-generated route tree)
- Tailwind CSS v4 (via `@tailwindcss/vite` plugin)
- `@ag-ui/client` (AG-UI protocol client for agent communication)

## Dev / Testing
- Vitest (test runner)
- React Testing Library (`@testing-library/react`, `@testing-library/dom`)
- TypeScript 5.7+ with strict compiler options

## Icons
- `lucide-react`

## Package Manager
- pnpm (workspace enabled via `pnpm-workspace.yaml`)

## Path Aliases
- `@/*` maps to `./src/*` (configured in both `tsconfig.json` and `vite.config.ts`)

## Common Commands
| Command | Description |
|---------|-------------|
| `pnpm dev` | Start dev server on port 3000 |
| `pnpm build` | Production build via Vite |
| `pnpm preview` | Preview production build |
| `pnpm test` | Run tests once (`vitest run`) |
