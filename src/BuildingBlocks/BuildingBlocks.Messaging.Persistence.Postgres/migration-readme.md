## Migration Scripts

```bash

dotnet ef migrations add InitialCreate -c MessagePersistenceDbContext -o Migrations
dotnet ef database update -c MessagePersistenceDbContext

```
