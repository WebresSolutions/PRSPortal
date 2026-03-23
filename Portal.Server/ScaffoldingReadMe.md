## How to scaffold:  

Run From the Portal.Server directory.

This will create data objects in the 'Data' project and use the json setting from the server.

> dotnet ef dbcontext scaffold Name=PrsConnection Npgsql.EntityFrameworkCore.PostgreSQL --output-dir ../Portal.Data/Models --context-dir ../Portal.Data --context PrsDbContext --force --no-onconfiguring --context-namespace Portal.Data --namespace Portal.Data.Models

- If the build fails: clean the solution and rebuild.