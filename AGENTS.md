# AGENTS.md

## Project overview

- This repository is a financial control system.
- Frontend: React/TypeScript app in `/client/finances-control-web`.
- Backend: .NET API in `/server/FinancesControl.API`.
- The `/docs` folder is the source of truth for project conventions. Read the relevant document before changing related code.

## Documentation map

- `/docs/dapper-usage-guidelines.md`: Dapper usage, PostgreSQL data access, repository SQL organization, parameterized queries, and database-generated IDs.
- `/docs/database-table-guidelines.md`: PostgreSQL table and script naming, quoted PascalCase identifiers, mandatory audit columns, UTC timestamp rules, currency decimals, and migration script location.
- `/docs/frontend-theme-palette.md`: centralized semantic theme tokens, avoiding isolated component palettes, and CSS variable guidance.
- `/docs/react-typescript-axios-structure.md`: frontend API layering, shared Axios client, feature API files, hooks, React Query-style query keys, and shared type placement.

## General development rules

- Read the relevant `/docs` file before implementing changes in that area.
- Do not invent project conventions when the docs are silent.
- Keep changes aligned with the current repository structure.
- If information is missing from the docs, call it `not documented yet` instead of guessing.
- Do not modify existing docs unless the task explicitly asks for documentation changes.

## Frontend conventions

- Follow the documented flow: `Component -> Custom Hook -> API Function -> Axios Client`.
- Components must not call Axios directly.
- Components must not know API endpoint URLs.
- Use the documented `src/api/axiosClient.ts`, `src/features`, and `src/types` patterns.
- Keep feature-specific request payload types near the feature API file unless they are reused elsewhere.
- Place reusable domain types in `src/types`.
- For frontend API, hook, Axios, or typing changes, read `/docs/react-typescript-axios-structure.md`.

## Styling and theme rules

- Use centralized theme tokens or synchronized semantic CSS variables for reusable colors.
- Avoid hardcoded reusable colors in components.
- Reuse existing theme colors before adding new ones.
- Add new colors only through semantic theme names.
- Base palette work on the current interface, especially `LoginPage`, as documented.
- For visual styling, theme, palette, or color changes, read `/docs/frontend-theme-palette.md`.

## Routing and layout rules

- Routing conventions are not documented yet.
- Layout conventions are not documented yet.
- Before changing routing or layout behavior, inspect the current frontend implementation and follow existing local patterns.

## API and data access rules

- Use Dapper for data access.
- Keep SQL out of repositories.
- Put SQL in module-level `SqlCommands` classes.
- Always use parameterized queries.
- Let PostgreSQL generate IDs.
- Check database table scripts before using table or column names.
- For backend repository or data access work, read `/docs/dapper-usage-guidelines.md`.

## Database rules

- Use `CREATE TABLE IF NOT EXISTS` for table scripts.
- Use quoted PascalCase table and column names.
- Include `Id`, `CreatedAt`, and `UpdatedAt` on tables.
- Store timestamps as UTC/GMT 0 without database time zones.
- Use `DECIMAL(18,2)` for currency and prices.
- Save table scripts under `/server/FinancesControl.API/Database/Tables/<version>/`.
- Follow the documented script filename format, such as `v1-0-0_create_orders_table.sql`.
- For database table or SQL migration work, read `/docs/database-table-guidelines.md`.

## Testing and checking commands

- Frontend commands from `/client/finances-control-web/package.json`:
  - `npm run build`
  - `npm run lint`
  - `npm run dev`
  - `npm run preview`
- Backend commands inferred from the solution and project files:
  - `dotnet build server/FinancesControl.sln`
  - `dotnet test server/FinancesControl.sln`
- A required test workflow is not documented yet.

## Instructions for future Codex agents

- For backend repository or data access work, read `/docs/dapper-usage-guidelines.md`.
- For database table or SQL migration work, read `/docs/database-table-guidelines.md`.
- For frontend API, hooks, Axios, or typing work, read `/docs/react-typescript-axios-structure.md`.
- For visual styling, theme, palette, or color work, read `/docs/frontend-theme-palette.md`.
- For routing or layout work, inspect the current app because no dedicated doc exists yet.
- Prefer concise implementation notes.
- Reference the relevant docs instead of duplicating their full content.
