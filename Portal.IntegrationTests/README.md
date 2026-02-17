# Portal Integration Tests

Integration tests for Portal.Server API endpoints using a **real PostgreSQL database** in Docker.

## How it works

- **Testcontainers**: Each test run starts a PostgreSQL container (PostGIS image for schema compatibility) and tears it down after tests.
- **WebApplicationFactory**: The ASP.NET Core app is hosted in-process with test configuration (auth disabled, test connection string).
- **Database schema**: `EnsureCreated()` is applied to the test database so all tables exist with no migrations required.

## Prerequisites

1. **Docker** must be running (Desktop or engine). Testcontainers will pull and start the image automatically.
2. **Stop Portal.Server** if it is running, so the build can copy DLLs.

## Run tests

```bash
# From repo root (stop the server first if it's running)
dotnet test Portal.IntegrationTests/Portal.IntegrationTests.csproj
```

To run with verbose output:

```bash
dotnet test Portal.IntegrationTests/Portal.IntegrationTests.csproj --logger "console;verbosity=detailed"
```

## Endpoints covered

| Group        | Routes tested |
|-------------|----------------|
| Contacts    | GET list, GET by id, GET contact jobs |
| Councils    | GET list, GET by id, GET council jobs |
| Jobs        | GET list, GET by id, POST, PUT, GET assignedUserNotes |
| Schedule    | GET slots, GET colours, PUT colours |
| Settings    | GET systemsettings, PUT systemsettings |
| Timesheet   | GET by userId |
| Users       | GET list |

Tests assert status codes (e.g. 200, 400, 404) and that endpoints respond without 5xx errors.

## Optional: use a fixed Postgres container

To run tests against a Postgres container you start yourself (e.g. for debugging), use `docker-compose.test.yml`:

```bash
docker-compose -f docker-compose.test.yml up -d
```

Then set the connection string when running tests (e.g. in environment or `appsettings.Test.json` in the test project) and switch the factory to use that connection string instead of Testcontainers. The current setup uses Testcontainers by default so no manual container is required.
