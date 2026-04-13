# Ballast

A full-stack Todo List application built with .NET 10 and React.

## Architecture

```
ballast/
├── backend/Ballast/
│   ├── Ballast.Api/           # ASP.NET Core Web API
│   ├── Ballast.Application/   # Entities, interfaces, services, DTOs
│   ├── Ballast.Infrastructure/ # Repositories, database access
│   └── Ballast.Tests/         # xUnit unit tests
└── frontend/                  # React + Vite + Tailwind CSS
```

---

## Requirements

| Tool | Version |
|------|---------|
| .NET SDK | 10.0+ |
| SQL Server | Any edition (Express, Developer, LocalDB) |
| Node.js | 18+ |

---

## Backend

### Database setup

The API uses SQL Server with Windows Authentication. The default connection string targets a local instance:

```
Server=.;Database=BallastTodo;Trusted_Connection=True;TrustServerCertificate=True
```

To change it, edit `Ballast.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "<your connection string>"
}
```

**No manual schema setup is needed.** On first run the API automatically creates the `Users` and `TodoItems` tables and seeds a default user:

| Username | Password |
|----------|----------|
| `user`   | `password` |

### Configuration

JWT settings live in `Ballast.Api/appsettings.json`. Change the key before deploying to production:

```json
"Jwt": {
  "Key": "super-secret-key-change-in-production-32chars",
  "Issuer": "BallastApi",
  "Audience": "BallastClient",
  "ExpiryMinutes": 60
}
```

### Running the API

```bash
cd backend/Ballast/Ballast.Api
dotnet run
```

The API starts on:
- HTTP: `http://localhost:5161`
- HTTPS: `https://localhost:7033`

Interactive API docs (Scalar) are available at `/scalar` in Development mode.

### Running the tests

```bash
cd backend/Ballast
dotnet test
```

All 43 tests are pure unit tests — no database required.

---

## Frontend

### Setup

```bash
cd frontend
npm install
```

### Running the dev server

```bash
npm run dev
```

Opens at `http://localhost:5173`. All `/api/*` requests are proxied to `https://localhost:7033`, so the backend must be running first.

### Building for production

```bash
npm run build
```

Output is written to `frontend/dist/`.

---

## Usage

1. Start the API (`dotnet run`)
2. Start the frontend (`npm run dev`)
3. Open `http://localhost:5173`
4. Log in with `user` / `password`, or register a new account
5. Create, complete, and delete todo items
