## How to scaffold:  

> dotnet ef dbcontext scaffold Name=PrsConnection Npgsql.EntityFrameworkCore.PostgreSQL --output-dir ../Portal.Data/Models --context-dir ../Portal.Data --context PrsDbContext --force --no-onconfiguring --context-namespace Portal.Data --namespace Portal.Data.Models

- If the build fails: clean the solution and rebuild.