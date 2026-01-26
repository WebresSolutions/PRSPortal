## Scaffold

### MySQL
> dotnet ef dbcontext scaffold 'Server=localhost;port=3306;Database=prs_database;User Id=adminuser;Password=j4clas&P+e@4lv@zwvtB;' Pomelo.EntityFrameworkCore.MySql --output-dir SourceDb --context SourceDBContext

### PostgreSQL
> dotnet ef dbcontext scaffold 'Host=localhost;Port=5433;Database=prs_database;Username=postgres;Password=145269;Include Error Detail=true;' Npgsql.EntityFrameworkCore.PostgreSQL --output-dir ../Migration/Models --context PrsDbContext --force --no-onconfiguring --context-namespace Migration --namespace Migration.Models


### To Create a persistent database in docker: 
	docker pull postgres
	docker run --name pgdev -e POSTGRES_PASSWORD=145269 -p 5433:5432 -v pgdev-data:/var/lib/postgresql -d postgres


	docker pull mysql
	docker run --name mysqldev -e MYSQL_ROOT_PASSWORD=145269 -p 3307:3306 -v mysqldev-data:/var/lib/mysql -d mysql