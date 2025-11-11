# PRS DATA
This folder contains data files used by the Portal Reporting System (PRS). These files include datasets, configuration files, and other resources necessary for generating reports and analytics within the PRS framework.

### How to scaffold this database

``dotnet ef dbcontext scaffold Name=PrsConnection Npgsql.EntityFrameworkCore.PostgreSQL --output-dir DataModels --context PrsDbContext --context-dir . --startup-project ../Portal.Server``


dotnet ef dbcontext scaffold Name=PrsConnection Npgsql.EntityFrameworkCore.PostgreSQL --output-dir Models --context-dir . --context PrsDbContext --startup-project ../Portal.Server --force