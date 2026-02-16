# Project Structure

```
├── src/
│   ├── main.tsx              # App entry point — mounts RouterProvider
│   ├── router.tsx            # TanStack Router factory with config
│   ├── routeTree.gen.ts      # Auto-generated route tree (DO NOT EDIT)
│   ├── styles.css            # Global styles (Tailwind v4 import + base resets)
│   └── routes/
│       ├── __root.tsx        # Root layout route (renders <Outlet />)
│       └── index.tsx         # Home page route (`/`)
├── public/                   # Static assets served at root
├── index.html                # HTML shell
├── vite.config.ts            # Vite config (React, TanStack Router, Tailwind, tsconfig paths)
├── tsconfig.json             # TypeScript config (strict, bundler mode)
└── package.json
```

## Conventions

- **Routing**: File-based routing via TanStack Router. Add new routes as files under `src/routes/`. The route tree (`src/routeTree.gen.ts`) is auto-generated — never edit it manually.
- **Route files**: Each route file exports a `Route` constant created with `createFileRoute()` or `createRootRoute()`, and a component function.
- **Styling**: Use Tailwind CSS utility classes. Global styles go in `src/styles.css`.
- **Imports**: Use the `@/` alias for imports from `src/` (e.g., `import { Foo } from '@/components/Foo'`).
