# Personal Details Web

A **React 19 + TypeScript** frontend, built with **Vite**, for [PersonalDetailsApi](../PersonalDetailsApi/README.md).

| Page | What it does |
|---|---|
| **People** (`/`) | Search by name or e-mail. A collapsible filter panel covers gender, blood group, marital status, city, age range and sorting. Active filters show as removable chips and are kept in the URL. |
| **Person** (`/persons/:id`) | Profile header with contact links and quick facts. Employment and education are shown as timelines, alongside addresses, emergency contacts and identity documents (masked, with an expiry status). Edit or delete. |
| **Add / Edit** (`/persons/new`, `/persons/:id/edit`) | Sectioned form, including every child list, with a Save bar that stays visible while scrolling. Server validation errors appear under the matching field. |
| **Statistics** (`/statistics`) | Summary tiles and breakdowns by age group, gender, blood group and nationality. |

The layout adapts to phone widths (the sidebar becomes a top bar) and follows the operating system's light or dark mode. Text uses the Inter font from Google Fonts, falling back to the system font when offline.

## Run it

Requires **Node.js 20.19+ or 22.12+** (`node -v`). With nvm for Windows: `nvm use 22.22.2`.

```powershell
# 1. Start the API (from the repository root)
dotnet run --project PersonalDetailsApi --launch-profile http

# 2. In a second terminal, start the frontend
cd PersonalDetailsWeb
npm install
npm run dev
```

Open **http://localhost:5173**.

| Script | Purpose |
|---|---|
| `npm run dev` (or `npm start`) | Dev server with hot reload on port 5173 |
| `npm run build` | Type-check (`tsc`), then bundle to `dist/` |
| `npm run preview` | Serve the `dist/` build on port 4173 |

## How it talks to the API

The dev server proxies `/api/*` and `/health` to `http://localhost:5056` ([vite.config.ts](vite.config.ts)). The browser therefore sees a single origin, and no CORS configuration is involved.

To call the API directly instead, create `.env.local` with:

```
VITE_API_BASE_URL=http://localhost:5056
```

The API must then allow the frontend's origin in `Cors:AllowedOrigins`. `appsettings.Development.json` already allows `http://localhost:5173`.

## Layout

```
src/
├── api/          types.ts (mirrors the C# DTOs), client.ts (fetch + ProblemDetails), persons.ts (endpoints)
├── hooks/        useApi: loading/error state, aborts stale requests
├── components/   Icon, Avatar, Toast, Skeleton, ErrorMessage, Pagination, BarList
├── pages/        PersonListPage, PersonDetailPage, PersonFormPage, StatisticsPage
├── utils/        Enum labels and date formatting
├── App.tsx       Navigation and routes
└── main.tsx      Entry point
```

A detailed walkthrough, including how each frontend piece maps to the backend, is in [section 11 of the Project Structure Guide](../PersonalDetailsApi/docs/PROJECT_GUIDE.md#11-the-react-frontend).

## Notes

- **Editing identity documents:** the API only returns masked numbers (`****4567`), and `PUT` replaces the whole record. Document numbers must therefore be re-entered when saving an edit.
- **Deleting** is a soft delete on the server. The person disappears from the app but stays in the database.
