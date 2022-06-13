## Migration Scripts

```bash

dotnet ef migrations add InitialCreate -c MessagePersistenceDbContext
dotnet ef database update -c MessagePersistenceDbContext

```
